// SPDX-License-Identifier: MIT

using System;
using System.Runtime.CompilerServices;
using Celeste.Game.Execution.Levels;
using Celeste.Core.Scheduling;
using Celeste.Simulation;
using Celeste.Simulation.Gameplay.Player.Movement;
using Mono.Cecil;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;

namespace Celeste.Mods.XaphanHelper.Runtime.Movement
{
    internal sealed class NormalMotionHooks : IDisposable
    {
        private delegate float ValueModifier(float value, ref NormalMotionState state);

        private readonly ISourceEngineContext engine;
        private readonly Func<RoomPlayerSession, float, float> modifySlope;
        private readonly Func<RoomPlayerSession, bool> inQuicksand;
        private readonly ILHook horizontal;
        private readonly ILHook fall;

        internal NormalMotionHooks(ISourceEngineContext engine, Func<RoomPlayerSession, float, float> modifySlope,
            Func<RoomPlayerSession, bool> inQuicksand)
        {
            this.engine = engine;
            this.modifySlope = modifySlope;
            this.inQuicksand = inQuicksand;
            horizontal = new ILHook(typeof(NormalMotion).GetMethod(nameof(NormalMotion.UpdateHorizontal)), Horizontal);
            try
            {
                fall = new ILHook(typeof(NormalMotion).GetMethod(nameof(NormalMotion.UpdateFallLimit)), Fall);
            }
            catch
            {
                horizontal.Dispose();
                throw;
            }
        }

        private RoomPlayerSession Player(ref NormalMotionState state)
        {
            if (engine.Frame.Scenes.CurrentScene is not OriginalLevelSceneRoot level ||
                !ReferenceEquals(level.EngineContext, engine)) return null;
            var player = level.Scheduler.Entities.GetTrackedEntity<RoomPlayerSession>();
            return player != null && Unsafe.AreSame(ref state, ref player.Data.Motion.Normal) ? player : null;
        }

        private float Run(float maximum, ref NormalMotionState state)
        {
            var player = Player(ref state);
            if (player == null) return maximum;
            maximum = modifySlope(player, maximum);
            return inQuicksand(player) ? maximum * .15f : maximum;
        }

        private float FallSpeed(float maximum, ref NormalMotionState state)
        {
            var player = Player(ref state);
            return player != null && inQuicksand(player) ? maximum * .1f : maximum;
        }

        private void Horizontal(ILContext il)
        {
            var c = new ILCursor(il);
            if (!c.TryGotoNext(MoveType.After, i => i.MatchLdcR4(90f)) || !c.Next.MatchStloc(out int maximum))
                throw new InvalidOperationException("Xaphan normal maximum local is missing.");
            if (!c.TryGotoNext(MoveType.Before, i => i.MatchLdarg(0),
                    i => i.MatchLdflda<NormalMotionState>(nameof(NormalMotionState.Speed)),
                    i => i.MatchLdfld<System.Numerics.Vector2>(nameof(System.Numerics.Vector2.X)),
                    i => i.MatchCall(typeof(Math), nameof(Math.Abs))))
                throw new InvalidOperationException("Xaphan normal maximum comparison is missing.");
            c.MoveAfterLabels();
            c.Emit(OpCodes.Ldloc, il.Body.Variables[maximum]);
            c.Emit(OpCodes.Ldarg_0);
            c.EmitDelegate<ValueModifier>(Run);
            c.Emit(OpCodes.Stloc, il.Body.Variables[maximum]);
        }

        private void Fall(ILContext il)
        {
            var c = new ILCursor(il);
            int count = 0;
            while (c.TryGotoNext(MoveType.After, i => i.MatchLdcR4(160f) || i.MatchLdcR4(240f)))
            {
                if (!c.TryGotoNext(MoveType.Before, i => i.MatchStloc(out _)))
                    throw new InvalidOperationException("Xaphan fall limit local is missing.");
                c.Emit(OpCodes.Ldarg_0);
                c.EmitDelegate<ValueModifier>(FallSpeed);
                count++;
            }

            if (count != 2) throw new InvalidOperationException("Xaphan normal fall limits no longer match.");
        }

        public void Dispose()
        {
            fall.Dispose();
            horizontal.Dispose();
        }
    }
}
