using System;
using Celeste.Mods.XaphanHelper.Entities.Platforms.MoveBlocks;

namespace Celeste.Mods.XaphanHelper.Modules
{
    public sealed partial class XaphanHelperModule
    {
        private IDisposable customMoveBlockDebrisPool;

        private void LoadCustomMoveBlockPooling() =>
            customMoveBlockDebrisPool = CustomMoveBlock.RegisterDebrisPool(SourceEngine);

        private void UnloadCustomMoveBlockPooling()
        {
            customMoveBlockDebrisPool?.Dispose();
            customMoveBlockDebrisPool = null;
        }
    }
}
