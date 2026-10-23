using System.Collections;
using Celeste.Scene.Entities;
using Celeste.Simulation;

namespace Celeste.Mods.XaphanHelper.Runtime.Liquids
{
    public interface ILiquidWaterfallSection
    {
        SceneEntityState Entity { get; }
        bool Purified { get; }
        bool PlayerInside();
    }
}
