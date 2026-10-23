// SPDX-License-Identifier: MIT

using System;
using Celeste.Simulation;

namespace Celeste.Mods.XaphanHelper.Runtime.Liquids
{
    public sealed class BreathFeatures
    {
        public int OxygenIndicator { get; set; }
        public int SoCMOxygenIndicator { get; set; }
        public Func<RoomPlayerSession, bool> TouchingAirBubbles { get; set; }
        public Func<RoomPlayerSession, bool> StartedScrewAttack { get; set; }
        public Func<RoomPlayerSession, bool> StaminaIndicatorVisible { get; set; }
    }
}
