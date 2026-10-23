// SPDX-License-Identifier: MIT

using System;
using System.Collections.Generic;
using Celeste.Core.Display;
using Celeste.Core.Rendering;
using Celeste.Game.Execution.Levels;
using Celeste.Mods.XaphanHelper.Entities.Liquids;
using Celeste.Mods.XaphanHelper.Runtime.Liquids;
using Celeste.Simulation;
using Celeste.Simulation.Gameplay.Player.Intro;
using MonoMod.RuntimeDetour;

namespace Celeste.Mods.XaphanHelper.Modules
{
    public sealed partial class XaphanHelperModule
    {
        public BreathFeatures BreathFeatures { get; } = new();
        private readonly List<IDisposable> breathHooks = new();

        private void LoadBreathHooks()
        {
            try
            {
                breathHooks.Add(new Hook(
                    typeof(OriginalLevelSceneRoot).GetMethod(nameof(OriginalLevelSceneRoot.Update)),
                    (Action<Action<OriginalLevelSceneRoot>, OriginalLevelSceneRoot>)UpdateBreath));
                breathHooks.Add(new Hook(
                    typeof(OriginalLevelSceneRoot).GetMethod(nameof(OriginalLevelSceneRoot.LoadLevel)),
                    (Action<Action<OriginalLevelSceneRoot, PlayerIntroType, bool>, OriginalLevelSceneRoot,
                        PlayerIntroType,
                        bool>)LoadOxygenIndicator));
                breathHooks.Add(new Hook(typeof(RoomPlayerSession).GetMethod(nameof(RoomPlayerSession.Render)),
                    (Action<Action<RoomPlayerSession, ISourceRenderContext>, RoomPlayerSession, ISourceRenderContext>)
                    RenderBreath));
            }
            catch
            {
                UnloadBreathHooks();
                throw;
            }
        }

        private void UnloadBreathHooks()
        {
            for (int i = breathHooks.Count - 1; i >= 0; i--) breathHooks[i].Dispose();
            breathHooks.Clear();
            if (SourceEngine.Frame.Scenes.CurrentScene is OriginalLevelSceneRoot level)
            {
                foreach (BreathManager manager in level.Scheduler.Entities.GetTrackedEntities(typeof(BreathManager)))
                    manager.RemoveSelf();
                foreach (OxygenIndicator indicator in level.Scheduler.Entities.GetTrackedEntities(
                             typeof(OxygenIndicator)))
                    indicator.RemoveSelf();
            }
        }

        private void UpdateBreath(Action<OriginalLevelSceneRoot> orig, OriginalLevelSceneRoot level)
        {
            if (ReferenceEquals(level.EngineContext, SourceEngine) && level.Scheduler.Entities
                                                                       .GetTrackedEntities(typeof(Liquid)).Count > 0
                                                                   && level.Scheduler.Entities
                                                                       .GetTrackedEntity<BreathManager>() == null)
                level.Scheduler.QueueSourceEntity(new BreathManager(SourceEngine, this));
            orig(level);
        }

        private void LoadOxygenIndicator(Action<OriginalLevelSceneRoot, PlayerIntroType, bool> orig,
            OriginalLevelSceneRoot level, PlayerIntroType intro, bool loader)
        {
            orig(level, intro, loader);
            if (ReferenceEquals(level.EngineContext, SourceEngine) &&
                level.Scheduler.Entities.GetTrackedEntity<OxygenIndicator>() == null)
                level.Scheduler.QueueSourceEntity(new OxygenIndicator(SourceEngine, this));
        }

        private void RenderBreath(Action<RoomPlayerSession, ISourceRenderContext> orig, RoomPlayerSession player,
            ISourceRenderContext context)
        {
            orig(player, context);
            if (!Owns(player) || player.Dead) return;
            var manager = ((OriginalLevelSceneRoot)player.Scene).Scheduler.Entities.GetTrackedEntity<BreathManager>();
            if (manager is { Flashing: true, ForceRechargeAir: false })
                player.Sprite.Color = new PackedColor(0, 32, 187);
        }
    }
}
