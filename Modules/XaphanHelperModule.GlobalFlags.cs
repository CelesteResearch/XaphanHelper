using System;
using Celeste.Everest.Runtime.Events;
using Celeste.Game.Execution.Areas;
using Celeste.Game.Execution.Levels;
using Celeste.Simulation.Gameplay.Player.Intro;

namespace Celeste.Mods.XaphanHelper.Modules
{
    public sealed partial class XaphanHelperModule
    {
        private void LoadGlobalFlags() =>
            ((IEverestGameEventsEnvironment)SourceEngine).GameEvents.OnLoadLevel += RestoreGlobalFlags;

        private void UnloadGlobalFlags() =>
            ((IEverestGameEventsEnvironment)SourceEngine).GameEvents.OnLoadLevel -= RestoreGlobalFlags;

        private void RestoreGlobalFlags(OriginalLevelSceneRoot level, PlayerIntroType intro, bool isFromLoader)
        {
            if (!ReferenceEquals(level.EngineContext, SourceEngine)) return;
            var keys = ((IOriginalAreaKeyEnvironment)SourceEngine).AreaKeys;
            string prefix = keys.GetLevelSet(keys.GetSessionArea(level.Session));
            if (!LiquidSaveData.SavedFlags.Contains(prefix + "_teleporting"))
            {
                foreach (string flag in LiquidSaveData.GlobalFlags)
                {
                    if (flag.Contains(prefix))
                    {
                        string toRemove = prefix + "_";
                        string result = string.Empty;
                        int i = flag.IndexOf(toRemove);
                        if (i >= 0) result = flag.Remove(i, toRemove.Length);
                        level.Session.SetFlag(result, true);
                    }
                }
            }
        }
    }
}
