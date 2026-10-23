using System;
using System.Collections.Generic;
using Celeste.Simulation;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;

namespace Celeste.Mods.XaphanHelper.Runtime.Liquids
{
    internal sealed class LiquidGravityJumpHooks : IDisposable
    {
        private readonly LiquidGravityDashHooks contact;
        private readonly List<IDisposable> hooks = new();

        internal LiquidGravityJumpHooks(LiquidGravityDashHooks contact)
        {
            this.contact = contact;
            try
            {
                Add(nameof(RoomPlayerSession.Jump), 0f, -105f);
                Add(nameof(RoomPlayerSession.SuperJump), 260f, -105f);
                Add(nameof(RoomPlayerSession.WallJump), 130f, -105f);
                Add(nameof(RoomPlayerSession.SuperWallJump), 170f, -160f);
            }
            catch
            {
                Dispose();
                throw;
            }
        }

        private void Add(string name, float horizontal, float vertical)
            => hooks.Add(new ILHook(typeof(RoomPlayerSession).GetMethod(name), il =>
            {
                var cursor = new ILCursor(il);
                if (horizontal != 0f)
                    while (cursor.TryGotoNext(MoveType.After, i => i.MatchLdcR4(horizontal)))
                    {
                        cursor.Emit(OpCodes.Ldarg_0);
                        cursor.EmitDelegate<Func<float, RoomPlayerSession, float>>(ModifyHorizontal);
                    }

                while (cursor.TryGotoNext(MoveType.After, i => i.MatchLdcR4(vertical)))
                {
                    cursor.Emit(OpCodes.Ldarg_0);
                    cursor.EmitDelegate<Func<float, RoomPlayerSession, float>>(ModifyVertical);
                }
            }));

        private float ModifyHorizontal(float value, RoomPlayerSession player)
            => value * contact.JumpSpeedFactor(player);

        private float ModifyVertical(float value, RoomPlayerSession player)
            => value * contact.JumpHeightFactor(player);

        public void Dispose()
        {
            for (int i = hooks.Count - 1; i >= 0; i--) hooks[i].Dispose();
            hooks.Clear();
        }
    }
}
