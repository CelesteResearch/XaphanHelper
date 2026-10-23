using System;
using System.Collections.Generic;
using System.Reflection;
using Celeste.Core.Scheduling;
using Celeste.Game.Execution.Levels;
using Celeste.Mods.XaphanHelper.Entities.Liquids;
using Celeste.Simulation;
using Celeste.Simulation.Gameplay.Player.Dash;
using Celeste.Simulation.Gameplay.Player.State;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using MonoMod.Utils;

namespace Celeste.Mods.XaphanHelper.Runtime.Liquids
{
    internal sealed class LiquidGravityDashHooks : IDisposable
    {
        private readonly ISourceEngineContext engine;
        private readonly LiquidGameplayFeatures features;
        private readonly List<IDisposable> hooks = new();

        internal LiquidGravityDashHooks(ISourceEngineContext engine, LiquidGameplayFeatures features)
        {
            this.engine = engine;
            this.features = features;
            try
            {
                hooks.Add(new ILHook(typeof(DashSequence).GetMethod(nameof(DashSequence.Begin)), PatchBegin));
                var iterator = typeof(DashSequence).GetMethod(nameof(DashSequence.Coroutine)).GetStateMachineTarget();
                var owner = iterator.DeclaringType.GetField("<>4__this",
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                if (owner == null) throw new InvalidOperationException("DashSequence coroutine owner is missing.");
                hooks.Add(new ILHook(iterator, il => PatchCoroutine(il, owner)));
            }
            catch
            {
                Dispose();
                throw;
            }
        }

        private void PatchBegin(ILContext il)
        {
            var cursor = new ILCursor(il);
            while (cursor.TryGotoNext(MoveType.After, i => i.MatchLdcR4(.3f) || i.MatchLdcR4(.15f)))
            {
                cursor.Emit(OpCodes.Ldarg_0);
                cursor.EmitDelegate<Func<float, DashSequence, float>>(ModifyLength);
            }
        }

        private void PatchCoroutine(ILContext il, FieldInfo owner)
        {
            var cursor = new ILCursor(il);
            while (cursor.TryGotoNext(MoveType.After, i => i.MatchLdcR4(.3f) || i.MatchLdcR4(.15f)))
            {
                cursor.Emit(OpCodes.Ldarg_0);
                cursor.Emit(OpCodes.Ldfld, owner);
                cursor.EmitDelegate<Func<float, DashSequence, float>>(ModifyLength);
            }

            cursor.Index = 0;
            while (cursor.TryGotoNext(MoveType.AfterLabel,
                       i => i.MatchStfld<PlayerState>(nameof(PlayerState.TrailCounter))))
            {
                cursor.Emit(OpCodes.Ldarg_0);
                cursor.Emit(OpCodes.Ldfld, owner);
                cursor.EmitDelegate<Func<int, DashSequence, int>>(ModifyTrail);
                cursor.Index++;
            }
        }

        private float ModifyLength(float value, DashSequence dash)
            => IsSlowed(dash) ? value * .5f : value;

        private int ModifyTrail(int value, DashSequence dash)
            => IsSlowed(dash) ? (int)Math.Round((dash.Settings.SuperDashing ? .3f : .15f) * 4) - 1 : value;

        private bool IsSlowed(DashSequence dash)
        {
            if (!ReferenceEquals(dash.EngineContext, engine) ||
                engine.Frame.Scenes.CurrentScene is not OriginalLevelSceneRoot level)
                return false;
            // The original checks liquid contact before consulting GravityJacket.Active.
            return InWater(level) || InType(level, "lava", false) || InType(level, "acid", true)
                ? features.GravityJacketActive?.Invoke(level) != true
                : false;
        }

        internal float JumpHeightFactor(RoomPlayerSession player)
        {
            if (!Owns(player, out var level)) return 1f;
            if ((InWater(level) || InType(level, "lava", false) || InType(level, "acid", true)) &&
                features.GravityJacketActive?.Invoke(level) != true)
                return .7f;
            return InWaterfall(level) ? .7f : 1f;
        }

        internal float JumpSpeedFactor(RoomPlayerSession player)
        {
            if (!Owns(player, out var level)) return 1f;
            return (InWater(level) || InType(level, "lava", false) || InType(level, "acid", true)) &&
                   features.GravityJacketActive?.Invoke(level) != true
                ? .6f
                : 1f;
        }

        private bool Owns(RoomPlayerSession player, out OriginalLevelSceneRoot level)
        {
            level = engine.Frame.Scenes.CurrentScene as OriginalLevelSceneRoot;
            return level != null && ReferenceEquals(player.Scene, level) &&
                   ReferenceEquals(level.EngineContext, engine);
        }

        private bool InWater(OriginalLevelSceneRoot level)
        {
            if (InType(level, "water", false)) return true;
            return InWaterfall(level);
        }

        private bool InWaterfall(OriginalLevelSceneRoot level)
        {
            var sections = features.WaterfallSections;
            for (int i = 0; i < sections.Count; i++)
                if (ReferenceEquals(sections[i].Entity.Scene, level) && sections[i].PlayerInside())
                    return true;
            return false;
        }

        private static bool InType(OriginalLevelSceneRoot level, string type, bool contains)
        {
            var liquids = level.SourceScene.Entities.GetTrackedEntities(typeof(Liquid));
            for (int i = 0; i < liquids.Count; i++)
            {
                var liquid = (Liquid)liquids[i];
                if (liquid.PlayerInside() && (contains ? liquid.LiquidType.Contains(type) : liquid.LiquidType == type))
                    return true;
            }

            return false;
        }

        public void Dispose()
        {
            for (int i = hooks.Count - 1; i >= 0; i--) hooks[i].Dispose();
            hooks.Clear();
        }
    }
}
