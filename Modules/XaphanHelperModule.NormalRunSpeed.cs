// SPDX-License-Identifier: MIT

using Celeste.Mods.XaphanHelper.Runtime.Movement;
using Celeste.Simulation;

namespace Celeste.Mods.XaphanHelper.Modules
{
    public sealed partial class XaphanHelperModule
    {
        private void LoadNormalMotionHooks() =>
            hooks.Add(new NormalMotionHooks(SourceEngine, ModifyMaximumRunSpeed, liquidPlayerHooks.InQuicksand));

        internal float ModifyMaximumRunSpeed(RoomPlayerSession player, float maximum)
            => maximum * liquidGravityHooks.JumpSpeedFactor(player) * (1f + DetermineSpeedOffset(player));
    }
}
