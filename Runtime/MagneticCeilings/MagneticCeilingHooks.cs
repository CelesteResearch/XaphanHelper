using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Celeste.Core.Scheduling;
using Celeste.Everest.Runtime.Events.Player;
using Celeste.Game.Execution.Graphics;
using Celeste.Game.Execution.Levels;
using Celeste.Graphics.Animation;
using Celeste.Graphics.Content;
using Celeste.Mods.XaphanHelper.Entities.Platforms.MagneticCeilings;
using Celeste.Mods.XaphanHelper.Entities.Platforms.MoveBlocks;
using Celeste.Physics.Collision;
using Celeste.Scene.Entities;
using Celeste.Simulation;
using Celeste.Simulation.Gameplay.Platforms.Foundation;
using Celeste.Simulation.Gameplay.Player.Movement;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;

namespace Celeste.Mods.XaphanHelper.Runtime.MagneticCeilings;

internal sealed class MagneticCeilingHooks : IDisposable
{
    private delegate float GravityModifier(ref NormalMotionState state);

    private readonly ISourceEngineContext engine;
    private readonly MagneticCeilingRuntime runtime;
    private readonly List<IDisposable> hooks = new();

    internal MagneticCeilingHooks(ISourceEngineContext engine, MagneticCeilingRuntime runtime)
    {
        this.engine = engine;
        this.runtime = runtime;
        try
        {
            hooks.Add(new Hook(typeof(RoomPlayerSession).GetMethod(nameof(RoomPlayerSession.IsRiding),
                    new[] { typeof(ISourceEntityCollider) }),
                (Func<Func<RoomPlayerSession, ISourceEntityCollider, bool>, RoomPlayerSession, ISourceEntityCollider,
                    bool>)IsRiding));
            hooks.Add(new Hook(typeof(SourceSolidEntityState).GetMethod(nameof(SourceSolidEntityState.GetPlayerOnTop)),
                (Func<Func<SourceSolidEntityState, RoomPlayerSession>, SourceSolidEntityState, RoomPlayerSession>)
                GetPlayerOnTop));
            hooks.Add(new Hook(typeof(Sprite).GetMethod(nameof(Sprite.Play)),
                (Action<Action<Sprite, string, bool, bool>, Sprite, string, bool, bool>)Play));
            hooks.Add(new ILHook(typeof(NormalMotion).GetMethod(nameof(NormalMotion.UpdateGravityAndJump)), Gravity));
            hooks.Add(new Hook(
                typeof(OriginalGameGraphicsResources).GetMethod(nameof(OriginalGameGraphicsResources
                    .CreateFramesMetadata)),
                (Action<Action<OriginalGameGraphicsResources, string>, OriginalGameGraphicsResources, string>)
                CreateMetadata));
            EverestPlayerSourceEvents.OnSpawn += OnSpawn;
        }
        catch
        {
            Dispose();
            throw;
        }
    }

    private bool Owns(SceneEntityState entity) => entity.Scene is OriginalLevelSceneRoot level &&
                                                  ReferenceEquals(level.EngineContext, engine);

    private void OnSpawn(RoomPlayerSession player)
    {
        if (ReferenceEquals(player.EngineContext, engine) && player.Scene is OriginalLevelSceneRoot level &&
            level.Session.GetFlag("Xaphan_Helper_Ceiling")) level.Session.SetFlag("Xaphan_Helper_Ceiling", false);
    }

    private bool IsRiding(Func<RoomPlayerSession, ISourceEntityCollider, bool> original,
        RoomPlayerSession player, ISourceEntityCollider solid)
    {
        if (Owns(player) && ((OriginalLevelSceneRoot)player.Scene).Session.GetFlag("Xaphan_Helper_Ceiling"))
            return SourceEntityCollision.CheckAt(player, solid, player.Position - new System.Numerics.Vector2(0f,
                solid is CustomMoveBlock block ? 6 + block.magneticCeilingOffset : 6));
        return original(player, solid);
    }

    private RoomPlayerSession GetPlayerOnTop(Func<SourceSolidEntityState, RoomPlayerSession> original,
        SourceSolidEntityState solid)
    {
        if (Owns(solid) && ((OriginalLevelSceneRoot)solid.Scene).Session.GetFlag("Xaphan_Helper_Ceiling") &&
            runtime.Player != null)
            return SourceEntityCollision.CheckAt(solid, runtime.Player,
                solid.Position + System.Numerics.Vector2.UnitY * 5f)
                ? runtime.Player
                : null;
        return original(solid);
    }

    private void Play(Action<Sprite, string, bool, bool> original, Sprite sprite, string id, bool restart,
        bool randomizeFrame)
    {
        if (sprite.Entity is RoomPlayerSession player && ReferenceEquals(player.Sprite, sprite) && Owns(player) &&
            player.Machine.State != 2 && !runtime.Module.Features.IsControllingRemoteDrone)
            foreach (MagneticCeiling ceiling in ((OriginalLevelSceneRoot)player.Scene).Scheduler.Entities
                     .GetTrackedEntities(typeof(MagneticCeiling)))
                if (ceiling.PlayerPose != "" && ceiling.Active)
                {
                    id = ceiling.PlayerPose;
                    break;
                }

        original(sprite, id, restart, randomizeFrame);
    }

    private float GravityFactor(ref NormalMotionState state)
    {
        if (engine.Frame.Scenes.CurrentScene is not OriginalLevelSceneRoot level ||
            !ReferenceEquals(level.EngineContext, engine)) return 1f;
        var player = level.Scheduler.Entities.GetTrackedEntity<RoomPlayerSession>();
        if (player == null || !Unsafe.AreSame(ref state, ref player.Data.Motion.Normal)) return 1f;
        return level.Session.GetFlag("Xaphan_Helper_Ceiling") && !runtime.SpiderMagnetActive(level) &&
               runtime.Module.LiquidFeatures.UseUpgrades
            ? 0f
            : 1f;
    }

    private void Gravity(ILContext il)
    {
        var cursor = new ILCursor(il);
        int count = 0;
        while (cursor.TryGotoNext(MoveType.After, instruction => instruction.MatchLdcR4(900f)))
        {
            cursor.Emit(OpCodes.Ldarg_0);
            cursor.EmitDelegate<GravityModifier>(GravityFactor);
            cursor.Emit(OpCodes.Mul);
            count++;
        }

        if (count != 1) throw new InvalidOperationException("Xaphan SpiderMagnet gravity instruction is missing.");
    }

    private void CreateMetadata(Action<OriginalGameGraphicsResources, string> original,
        OriginalGameGraphicsResources graphics, string name)
    {
        original(graphics, name);
        if (!ReferenceEquals(graphics, ((ISourceGameGraphicsEnvironment)engine).GameGraphics) || name == null ||
            name.StartsWith("XaphanHelper_Extend_")) return;
        string extension = "XaphanHelper_Extend_" + name;
        if (!graphics.SpriteBank.Has(extension)) extension = "XaphanHelper_Extend_player";
        var source = graphics.SpriteBank.SpriteData[extension].Sprite;
        var target = graphics.SpriteBank.SpriteData[name].Sprite;
        var animations = new Dictionary<string, Sprite.Animation>(source.Animations);
        foreach (var animation in animations)
            if (!target.Animations.ContainsKey(animation.Key))
                target.Animations.Add(animation.Key, animation.Value);
        if (extension == "XaphanHelper_Extend_" + name) graphics.CreateFramesMetadata(extension);
    }

    public void Dispose()
    {
        EverestPlayerSourceEvents.OnSpawn -= OnSpawn;
        for (int i = hooks.Count - 1; i >= 0; i--) hooks[i].Dispose();
        hooks.Clear();
    }
}
