using System;
using Celeste.Mods.XaphanHelper.Runtime.Liquids;
using Celeste.Mods.XaphanHelper.Persistence;

namespace Celeste.Mods.XaphanHelper.Modules
{
    public sealed partial class XaphanHelperModule
    {
        public LiquidGameplayFeatures LiquidFeatures { get; } = new();
        public override Type SaveDataType => typeof(XaphanHelperSaveData);
        public XaphanHelperSaveData LiquidSaveData => (XaphanHelperSaveData)_SaveData;
    }
}
