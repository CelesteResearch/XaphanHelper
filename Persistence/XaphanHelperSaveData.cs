using System.Collections.Generic;
using Celeste.Everest.Modules.Persistence;

namespace Celeste.Mods.XaphanHelper.Persistence
{
    public sealed class XaphanHelperSaveData : EverestModuleSaveData
    {
        public HashSet<string> SpiderMagnetInactive = new();

        public HashSet<string> GlobalFlags = new();

        public HashSet<string> SavedFlags { get; set; } = new();
    }
}
