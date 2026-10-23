using System.Runtime.CompilerServices;
using Celeste.Core.Scheduling;
using Celeste.Game.Execution.Areas;
using Celeste.Game.Execution.Levels;
using Celeste.Mods.XaphanHelper.Modules;
using Celeste.Simulation;

namespace Celeste.Mods.XaphanHelper.Runtime.MagneticCeilings;

internal sealed class MagneticCeilingRuntime
{
    private static readonly ConditionalWeakTable<ISourceEngineContext, MagneticCeilingRuntime> instances = new();
    private readonly ISourceEngineContext engine;
    internal XaphanHelperModule Module;
    internal RoomPlayerSession Player;
    private MagneticCeilingRuntime(ISourceEngineContext engine) => this.engine = engine;

    internal static MagneticCeilingRuntime For(ISourceEngineContext engine) =>
        instances.GetValue(engine, key => new MagneticCeilingRuntime(key));

    internal bool SpiderMagnetActive(OriginalLevelSceneRoot level)
    {
        if (!Module.ModSettings.SpiderMagnet) return false;
        var keys = ((IOriginalAreaKeyEnvironment)engine).AreaKeys;
        return !Module.LiquidSaveData.SpiderMagnetInactive.Contains(
            keys.GetLevelSet(keys.GetSessionArea(level.Session)));
    }
}
