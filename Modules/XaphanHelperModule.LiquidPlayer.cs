// SPDX-License-Identifier: MIT

using Celeste.Mods.XaphanHelper.Runtime.Liquids;

namespace Celeste.Mods.XaphanHelper.Modules
{
    public sealed partial class XaphanHelperModule
    {
        private LiquidPlayerHooks liquidPlayerHooks;
        private LiquidGravityDashHooks liquidGravityHooks;

        private void LoadLiquidPlayerHooks()
        {
            liquidPlayerHooks = new LiquidPlayerHooks(SourceEngine);
            hooks.Add(liquidPlayerHooks);
            liquidPlayerHooks.Load();
            liquidGravityHooks = new LiquidGravityDashHooks(SourceEngine, LiquidFeatures);
            hooks.Add(liquidGravityHooks);
            hooks.Add(new LiquidGravityJumpHooks(liquidGravityHooks));
        }
    }
}
