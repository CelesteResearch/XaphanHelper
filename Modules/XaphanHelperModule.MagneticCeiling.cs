using System;
using Celeste.Mods.XaphanHelper.Runtime.MagneticCeilings;
using Celeste.Mods.XaphanHelper.Settings;

namespace Celeste.Mods.XaphanHelper.Modules;

public sealed partial class XaphanHelperModule
{
    public override Type SettingsType => typeof(XaphanHelperSettings);
    public XaphanHelperSettings ModSettings => (XaphanHelperSettings)_Settings;
    private void BindMagneticCeilings() => MagneticCeilingRuntime.For(SourceEngine).Module = this;

    private void LoadMagneticCeilings() =>
        hooks.Add(new MagneticCeilingHooks(SourceEngine, MagneticCeilingRuntime.For(SourceEngine)));
}
