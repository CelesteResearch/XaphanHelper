using Celeste.Everest.Modules.Settings;
using Celeste.Everest.Settings.Metadata;

namespace Celeste.Mods.XaphanHelper.Settings;

public sealed class XaphanHelperSettings : EverestModuleSettings
{
    [SettingIgnore] public bool SpiderMagnet { get; set; }
}
