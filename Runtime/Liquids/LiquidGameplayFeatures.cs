using System;
using Celeste.Core.Components;
using System.Collections;
using System.Collections.Generic;
using Celeste.Game.Execution.Levels;
using Celeste.Scene.Entities;
using Celeste.Simulation;

namespace Celeste.Mods.XaphanHelper.Runtime.Liquids
{
    public sealed class LiquidGameplayFeatures
    {
        public bool UseUpgrades { get; set; }
        public Func<OriginalLevelSceneRoot, bool> VariaJacketActive { get; set; }
        public Func<OriginalLevelSceneRoot, bool> GravityJacketActive { get; set; }
        public Func<OriginalLevelSceneRoot, ILiquidDrone> FindDrone { get; set; }
        public Func<OriginalLevelSceneRoot, ILiquidHealth> FindHealth { get; set; }
        public Func<OriginalLevelSceneRoot, bool> HeatDamageActive { get; set; }
        public List<ILiquidWaterfallSection> WaterfallSections { get; } = new();
    }
}
