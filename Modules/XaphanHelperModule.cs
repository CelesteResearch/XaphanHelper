// SPDX-License-Identifier: MIT

using System;
using System.Collections.Generic;
using System.Numerics;
using System.Reflection;
using Celeste.Core.Scheduling;
using Celeste.Everest.Modules.Runtime;
using Celeste.Game.Execution.Levels;
using Celeste.Game.Gameplay.Enemies.Seekers;
using Celeste.Game.Gameplay.Hazards.Puffer;
using Celeste.Game.Gameplay.Holdables.Gliders;
using Celeste.Game.Gameplay.Holdables.TheoCrystal;
using Celeste.Game.Gameplay.Platforms.MoveBlocks;
using Celeste.Graphics.Animation;
using Celeste.Mods.XaphanHelper.Entities.Slopes;
using Celeste.Mods.XaphanHelper.Runtime.Slopes;
using Celeste.Physics.Collision;
using Celeste.Scene.Entities;
using Celeste.Scene.Tracking;
using Celeste.Simulation;
using Celeste.Simulation.Gameplay.Actors.Debris;
using Celeste.Simulation.Gameplay.Actors.Foundation;
using Celeste.Simulation.Gameplay.Platforms.Foundation;
using Celeste.Simulation.Gameplay.Player.Intro;
using MonoMod.RuntimeDetour;

namespace Celeste.Mods.XaphanHelper.Modules
{
    public sealed partial class XaphanHelperModule : SourceEverestModule
    {
        private readonly List<IDisposable> hooks = new();
        public SlopeGameplayFeatures Features { get; } = new();
        private bool onSlope;
        private int onSlopeDir;
        private bool onSlopeGentle;
        private float onSlopeTop;
        private bool onSlopeAffectPlayerSpeed;
        private float maxRunSpeed;

        public XaphanHelperModule(ISourceEngineContext engine) : base(engine)
        {
            BindMagneticCeilings();
        }

        private bool Owns(SceneEntityState entity) => entity.Scene is OriginalLevelSceneRoot level &&
                                                      ReferenceEquals(level.EngineContext, SourceEngine);

        public override void Load()
        {
            LoadCustomMoveBlockPooling();
            LoadGlobalFlags();
            LoadLiquidPlayerHooks();
            LoadNormalMotionHooks();
            hooks.Add(new Hook(typeof(SourceActorEntityState).GetMethod(nameof(SourceActorEntityState.MoveH)),
                (Func<Func<SourceActorEntityState, float, SourceCollisionHandler, ISourceEntityCollider, bool>,
                    SourceActorEntityState, float, SourceCollisionHandler, ISourceEntityCollider, bool>)OnActorMoveH));
            HookActorUpdate<TheoCrystalState>(true, actor => actor.Hold.IsHeld);
            HookActorUpdate<OriginalGliderState>(true, actor => actor.Hold.IsHeld);
            HookActorUpdate<OriginalPufferState>(false);
            HookActorUpdate<OriginalSeekerState>(false);
            HookActorUpdate<DebrisState>(false);
            hooks.Add(new Hook(typeof(MoveBlockState).GetMethod(nameof(MoveBlockState.Update)),
                (Action<Action<MoveBlockState>, MoveBlockState>)OnMoveBlockUpdate));
            hooks.Add(new Hook(typeof(SourcePlatformEntityState).GetMethod(nameof(SourcePlatformEntityState.Update)),
                (Action<Action<SourcePlatformEntityState>, SourcePlatformEntityState>)OnSolidUpdate));
            hooks.Add(new Hook(typeof(RoomPlayerSession).GetMethod(nameof(RoomPlayerSession.RefillDash)),
                (Func<Func<RoomPlayerSession, bool>, RoomPlayerSession, bool>)OnRefillDash));
            hooks.Add(new Hook(typeof(Sprite).GetMethod(nameof(Sprite.Play)),
                (Action<Action<Sprite, string, bool, bool>, Sprite, string, bool, bool>)OnSpritePlay));
            hooks.Add(new Hook(typeof(RoomPlayerSession).GetMethod(nameof(RoomPlayerSession.Update)),
                (Action<Action<RoomPlayerSession>, RoomPlayerSession>)OnPlayerUpdate));
            hooks.Add(new Hook(typeof(OriginalLevelSceneRoot).GetMethod(nameof(OriginalLevelSceneRoot.Update)),
                (Action<Action<OriginalLevelSceneRoot>, OriginalLevelSceneRoot>)OnLevelUpdate));
            hooks.Add(new Hook(typeof(OriginalLevelSceneRoot).GetMethod(nameof(OriginalLevelSceneRoot.LoadLevel)),
                (Action<Action<OriginalLevelSceneRoot, PlayerIntroType, bool>, OriginalLevelSceneRoot, PlayerIntroType,
                    bool>)OnLevelLoad));
            hooks.Add(new Hook(typeof(OriginalLevelSceneRoot).GetMethod(nameof(OriginalLevelSceneRoot.End)),
                (Action<Action<OriginalLevelSceneRoot>, OriginalLevelSceneRoot>)OnLevelEnd));
            InstallHoldableCollisionHooks();
            LoadBreathHooks();
            LoadMagneticCeilings();
        }

        public override void Unload()
        {
            UnloadCustomMoveBlockPooling();
            UnloadGlobalFlags();
            UnloadBreathHooks();
            for (int i = hooks.Count - 1; i >= 0; i--) hooks[i].Dispose();
            hooks.Clear();
        }

        private void HookActorUpdate<T>(bool exactType, Func<T, bool> held = null) where T : SourceActorEntityState
        {
            hooks.Add(new Hook(typeof(T).GetMethod(nameof(SourceActorEntityState.Update)),
                (Action<Action<T>, T>)((original, actor) =>
                {
                    if (!Owns(actor) || (exactType && actor.GetType() != typeof(T)))
                    {
                        original(actor);
                        return;
                    }

                    SlopeActorCollision.BeforeUpdate(actor);
                    original(actor);
                    if (held == null || !held(actor)) SlopeActorCollision.ResolveUpsideDown(actor);
                    SlopeActorCollision.AfterUpdate(actor);
                })));
        }

        private bool OnActorMoveH(
            Func<SourceActorEntityState, float, SourceCollisionHandler, ISourceEntityCollider, bool> original,
            SourceActorEntityState actor, float amount, SourceCollisionHandler collision, ISourceEntityCollider pusher)
        {
            if (Owns(actor) && actor.GetType() != typeof(RoomPlayerSession))
                foreach (Slope slope in ((ISourceScene)actor.Scene).SourceScene.Entities.GetTrackedEntities(
                             typeof(Slope)))
                    if (SourceEntityCollision.CheckAt(actor, slope, actor.Position + Vector2.UnitY))
                    {
                        amount = 0;
                        break;
                    }

            return original(actor, amount, collision, pusher);
        }

        private void OnMoveBlockUpdate(Action<MoveBlockState> original, MoveBlockState block)
        {
            if (!Owns(block) || block.GetType() != typeof(MoveBlockState))
            {
                original(block);
                return;
            }

            SlopeActorCollision.BeforeUpdate(block);
            original(block);
            SlopeActorCollision.ResolveUpsideDown(block);
            SlopeActorCollision.AfterUpdate(block);
        }

        private void OnSolidUpdate(Action<SourcePlatformEntityState> original, SourcePlatformEntityState solid)
        {
            if (solid is SourceSolidEntityState && Owns(solid))
                foreach (PlayerPlatform platform in ((ISourceScene)solid.Scene).SourceScene.Entities.GetTrackedEntities(
                             typeof(PlayerPlatform)))
                    platform.BeforeSolidUpdate();
            original(solid);
        }

        private bool OnRefillDash(Func<RoomPlayerSession, bool> original, RoomPlayerSession player)
        {
            if (Owns(player))
                foreach (PlayerPlatform platform in
                         ((ISourceScene)player.Scene).SourceScene.Entities.GetTrackedEntities(typeof(PlayerPlatform)))
                    if (platform.PreventsRefillFor(player))
                        return false;
            return original(player);
        }

        private void OnSpritePlay(Action<Sprite, string, bool, bool> original, Sprite sprite, string id, bool restart,
            bool randomizeFrame)
        {
            if (sprite.Entity is RoomPlayerSession player && ReferenceEquals(player.Sprite, sprite) && Owns(player)
                && player.Machine.State != 2 && !Features.IsControllingRemoteDrone)
                foreach (PlayerPlatform platform in
                         ((ISourceScene)player.Scene).SourceScene.Entities.GetTrackedEntities(typeof(PlayerPlatform)))
                    if (platform.PlayerPose != "" && platform.Active)
                    {
                        id = platform.PlayerPose;
                        break;
                    }

            original(sprite, id, restart, randomizeFrame);
        }
    }
}
