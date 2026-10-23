using System.Collections;
using Celeste.Scene.Entities;
using Celeste.Simulation;

namespace Celeste.Mods.XaphanHelper.Runtime.Liquids
{
    public interface ILiquidDrone
    {
        bool Dead { get; }
        RoomPlayerSession FakePlayer { get; }
        IEnumerator Destroy(bool force = false, bool destroyFakePlayer = false);
    }
}
