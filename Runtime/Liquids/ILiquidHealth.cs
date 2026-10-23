using System.Collections;
using Celeste.Scene.Entities;
using Celeste.Simulation;

namespace Celeste.Mods.XaphanHelper.Runtime.Liquids
{
    public interface ILiquidHealth
    {
        int CurrentHealth { get; set; }
        void PlayDamageSound();
        void StopDamageSound();
        void GetEnergyTanks();
    }
}
