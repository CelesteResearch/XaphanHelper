// SPDX-License-Identifier: MIT

using System;
using System.Collections.Generic;
using System.Numerics;
using Celeste.Core.Scheduling;
using Celeste.Game.Execution.Levels;
using Celeste.Mods.XaphanHelper.Entities.Liquids;
using Celeste.Physics.Collision;
using Celeste.Scene.Entities;
using Celeste.Simulation;
using Celeste.Simulation.Gameplay.Player.Movement;
using Mono.Cecil;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;

namespace Celeste.Mods.XaphanHelper.Runtime.Liquids
{
    internal sealed class LiquidPlayerHooks : IDisposable
    {
        private readonly ISourceEngineContext engine;
        private readonly List<IDisposable> detours = new();

        internal LiquidPlayerHooks(ISourceEngineContext engine)
        {
            this.engine = engine;
        }

        internal void Load()
        {
            detours.Add(new Hook(typeof(RoomPlayerSession).GetMethod(nameof(RoomPlayerSession.Update)),
                (Action<Action<RoomPlayerSession>, RoomPlayerSession>)PlayerUpdate));
            detours.Add(new Hook(typeof(RoomPlayerSession).GetMethod(nameof(RoomPlayerSession.ClimbJump)),
                (Action<Action<RoomPlayerSession>, RoomPlayerSession>)ClimbJump));
            HookSwim(nameof(RoomPlayerWorld.SwimCheck), SwimCheck);
            HookSwim(nameof(RoomPlayerWorld.SwimUnderwaterCheck), SwimUnderwaterCheck);
            HookSwim(nameof(RoomPlayerWorld.SwimJumpCheck), SwimJumpCheck);
            HookSwim(nameof(RoomPlayerWorld.SwimRiseCheck), SwimRiseCheck);
            detours.Add(new Hook(typeof(RoomPlayerSession).GetMethod(nameof(RoomPlayerSession.IsOverWater)),
                (Func<Func<RoomPlayerSession, bool>, RoomPlayerSession, bool>)IsOverWater));
            detours.Add(new ILHook(typeof(RoomPlayerSession).GetMethod(nameof(RoomPlayerSession.Jump)),
                il => JumpConstant(il, -105f, false)));
            detours.Add(new ILHook(typeof(RoomPlayerSession).GetMethod(nameof(RoomPlayerSession.SuperJump)), il =>
            {
                JumpConstant(il, 260f, true);
                JumpConstant(il, -105f, false);
            }));
            detours.Add(new ILHook(typeof(RoomPlayerSession).GetMethod(nameof(RoomPlayerSession.WallJump)), il =>
            {
                JumpConstant(il, 130f, true);
                JumpConstant(il, -105f, false);
            }));
            detours.Add(new ILHook(typeof(RoomPlayerSession).GetMethod(nameof(RoomPlayerSession.SuperWallJump)), il =>
            {
                JumpConstant(il, 170f, true);
                JumpConstant(il, -160f, false);
            }));
            detours.Add(new ILHook(typeof(ClimbSequence).GetMethod(nameof(ClimbSequence.Update)), ClimbInputs));
        }

        private void HookSwim(string method, Func<Func<RoomPlayerWorld, bool>, RoomPlayerWorld, bool> handler) =>
            detours.Add(new Hook(typeof(RoomPlayerWorld).GetMethod(method), handler));

        private bool Owns(RoomPlayerSession player) =>
            player.Scene is OriginalLevelSceneRoot level && ReferenceEquals(level.EngineContext, engine);

        private void PlayerUpdate(Action<RoomPlayerSession> original, RoomPlayerSession player)
        {
            if (!Owns(player))
            {
                original(player);
                return;
            }

            original(player);
            if (InQuicksand(player)) player.Data.CooldownTimer = engine.DeltaTime + .1f;
        }

        internal bool InQuicksand(RoomPlayerSession player)
        {
            if (!Owns(player) || engine.Frame.Scenes.CurrentScene is not OriginalLevelSceneRoot level) return false;
            var liquids = level.SourceScene.Entities.GetTrackedEntities(typeof(Liquid));
            for (int i = 0; i < liquids.Count; i++)
            {
                var liquid = (Liquid)liquids[i];
                if (liquid.PlayerInside() && liquid.LiquidType == "quicksand") return true;
            }

            return false;
        }

        private static Liquid FirstLiquid(RoomPlayerSession player)
        {
            var entities =
                ((OriginalLevelSceneRoot)player.Scene).SourceScene.Entities.GetTrackedEntities(typeof(Liquid));
            for (int i = 0; i < entities.Count; i++)
            {
                var liquid = (Liquid)entities[i];
                if (SourceEntityCollision.CheckAt(player, liquid, player.Position)) return liquid;
            }

            return null;
        }

        private bool SwimCheck(Func<RoomPlayerWorld, bool> original, RoomPlayerWorld world)
        {
            var player = world.SourcePlayer;
            if (!Owns(player)) return original(world);
            var liquid = FirstLiquid(player);
            return liquid != null && liquid.CanSwim &&
                   SourceEntityCollision.CheckAt(player, liquid, player.Position - Vector2.UnitY * 8f)
                ? SourceEntityCollision.CheckAt(player, liquid, player.Position)
                : original(world);
        }

        private bool SwimUnderwaterCheck(Func<RoomPlayerWorld, bool> original, RoomPlayerWorld world) =>
            SwimOffset(original, world, -9f, false);

        private bool SwimJumpCheck(Func<RoomPlayerWorld, bool> original, RoomPlayerWorld world) =>
            SwimOffset(original, world, -14f, true);

        private bool SwimRiseCheck(Func<RoomPlayerWorld, bool> original, RoomPlayerWorld world) =>
            SwimOffset(original, world, -18f, true);

        private bool SwimOffset(Func<RoomPlayerWorld, bool> original, RoomPlayerWorld world, float offset, bool invert)
        {
            var player = world.SourcePlayer;
            if (!Owns(player)) return original(world);
            var liquid = FirstLiquid(player);
            if (liquid == null || !liquid.CanSwim || !SourceEntityCollision.Check(player, liquid))
                return original(world);
            bool collision = SourceEntityCollision.CheckAt(player, liquid, player.Position + Vector2.UnitY * offset);
            return invert ? !collision : collision;
        }

        private bool IsOverWater(Func<RoomPlayerSession, bool> original, RoomPlayerSession player)
        {
            if (!Owns(player)) return original(player);
            var liquid = FirstLiquid(player);
            if (liquid == null || !liquid.CanSwim) return original(player);
            var bounds = player.Collider.Bounds;
            bounds.Height += 2;
            var liquids =
                ((OriginalLevelSceneRoot)player.Scene).SourceScene.Entities.GetTrackedEntities(typeof(Liquid));
            for (int i = 0; i < liquids.Count; i++)
                if (SourceSceneCollision.CheckRect(bounds, (Liquid)liquids[i]))
                    return true;
            return false;
        }

        private void ClimbJump(Action<RoomPlayerSession> original, RoomPlayerSession player)
        {
            if (!InQuicksand(player)) original(player);
        }

        private void JumpConstant(ILContext il, float constant, bool horizontal)
        {
            var cursor = new ILCursor(il);
            int count = 0;
            while (cursor.TryGotoNext(MoveType.After, instruction => instruction.MatchLdcR4(constant)))
            {
                cursor.Emit(OpCodes.Ldarg_0);
                cursor.EmitDelegate<Func<float, RoomPlayerSession, float>>((value, player) =>
                    InQuicksand(player) ? value * (horizontal ? .15f : .3f) : value);
                count++;
            }

            if (count == 0)
                throw new InvalidOperationException(
                    $"Liquid jump hook no longer matches {il.Method.FullName}: {constant}.");
        }

        private void ClimbInputs(ILContext il)
        {
            var cursor = new ILCursor(il);
            if (!cursor.TryGotoNext(MoveType.After,
                    instruction => (instruction.OpCode == OpCodes.Call || instruction.OpCode == OpCodes.Callvirt) &&
                                   instruction.Operand is MethodReference method && method.Name == "get_Pressed"))
                throw new InvalidOperationException("Liquid climb jump button hook no longer matches.");
            cursor.Emit(OpCodes.Ldarg_0);
            cursor.EmitDelegate<Func<bool, ClimbSequence, bool>>((pressed, climb) =>
                climb.World is RoomPlayerWorld world && InQuicksand(world.SourcePlayer) &&
                climb.Data.MoveX != -climb.Data.Facing
                    ? false
                    : pressed);
            cursor.Index = 0;
            int count = 0;
            while (cursor.TryGotoNext(MoveType.After,
                       instruction => (instruction.OpCode == OpCodes.Call || instruction.OpCode == OpCodes.Callvirt) &&
                                      instruction.Operand is MethodReference method && method.Name == "get_MoveY"))
            {
                if (!cursor.TryGotoNext(MoveType.After,
                        instruction => (instruction.OpCode == OpCodes.Call || instruction.OpCode == OpCodes.Callvirt) &&
                                       instruction.Operand is MethodReference method && method.Name == "get_Value"))
                    throw new InvalidOperationException("Liquid climb vertical input hook no longer matches.");
                cursor.Emit(OpCodes.Ldarg_0);
                cursor.EmitDelegate<Func<int, ClimbSequence, int>>((value, climb) =>
                    climb.World is RoomPlayerWorld world && InQuicksand(world.SourcePlayer) ? 0 : value);
                count++;
            }

            if (count == 0) throw new InvalidOperationException("Liquid climb vertical input hook has no matches.");
        }

        public void Dispose()
        {
            for (int i = detours.Count - 1; i >= 0; i--) detours[i].Dispose();
            detours.Clear();
        }
    }
}
