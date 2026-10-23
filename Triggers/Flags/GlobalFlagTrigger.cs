using System;
using System.Numerics;
using Celeste.Core.Components;
using Celeste.Core.Mathematics.Extrema;
using Celeste.Core.Reflection.Runtime;
using Celeste.Core.Scheduling;
using Celeste.Everest.Modules.Entities.Metadata;
using Celeste.Everest.Modules.Runtime;
using Celeste.Formats.Map.Entities.Structure;
using Celeste.Game.Content.Maps.Entities;
using Celeste.Game.Content.Maps.Levels;
using Celeste.Game.Execution.Areas;
using Celeste.Game.Execution.Levels;
using Celeste.Mods.XaphanHelper.Modules;
using Celeste.Scene.Entities;
using Celeste.Simulation;
using Celeste.Simulation.Gameplay.Triggers.Foundation;

namespace Celeste.Mods.XaphanHelper.Triggers.Flags
{
    [RuntimeTypeAlias("Celeste.Mod.XaphanHelper.Triggers.GlobalFlagTrigger")]
    [CustomEntity("XaphanHelper/GlobalFlagTrigger")]
    public sealed class GlobalFlagTrigger : SourceTriggerEntityState
    {
        private readonly ISourceEngineContext engine;
        private readonly XaphanHelperModule module;
        public string flag;

        public bool state;

        public bool switchFlag;

        public string levelSet;

        public GlobalFlagTrigger(ISourceEngineContext engine, XaphanHelperModule module, EntityData data,
            Vector2 offset) : base(((ISourceEntitySourceDataEnvironment)engine).EntitySourceDataRegistration,
            data.Position + offset, data.Width, data.Height, ((ISourceExtremaEnvironment)engine).ExtremaArithmetic)
        {
            this.engine = engine;
            this.module = module;
            flag = data.Attr("flag");
            state = data.Bool("state", true);
            switchFlag = data.Bool("switchFlag");
            levelSet = data.Attr("levelSet");
        }

        public override void OnStay(RoomPlayerSession player)
        {
            base.OnStay(player);
            string Prefix = AreaKeys.GetLevelSet(AreaKeys.GetSessionArea(((OriginalLevelSceneRoot)Scene).Session));
            if (string.IsNullOrEmpty(levelSet))
            {
                levelSet = Prefix;
            }

            int chapterIndex =
                AreaKeys.GetChapterIndex(AreaKeys.GetSessionArea(((OriginalLevelSceneRoot)Scene).Session), engine) == -1
                    ? 0
                    : AreaKeys.GetChapterIndex(AreaKeys.GetSessionArea(((OriginalLevelSceneRoot)Scene).Session),
                        engine);
            if ((!switchFlag && state) || (switchFlag &&
                                           !module.LiquidSaveData.GlobalFlags.Contains(levelSet + "_Ch" + chapterIndex +
                                               "_" + flag)))
            {
                if (!module.LiquidSaveData.GlobalFlags.Contains(levelSet + "_Ch" + chapterIndex + "_" + flag))
                {
                    module.LiquidSaveData.GlobalFlags.Add(levelSet + "_Ch" + chapterIndex + "_" + flag);
                    if (levelSet == Prefix)
                    {
                        ((OriginalLevelSceneRoot)Scene).Session.SetFlag("Ch" + chapterIndex + "_" + flag, true);
                    }
                }
            }
            else
            {
                if (module.LiquidSaveData.GlobalFlags.Contains(levelSet + "_Ch" + chapterIndex + "_" + flag))
                {
                    module.LiquidSaveData.GlobalFlags.Remove(levelSet + "_Ch" + chapterIndex + "_" + flag);
                    if (levelSet == Prefix)
                    {
                        ((OriginalLevelSceneRoot)Scene).Session.SetFlag("Ch" + chapterIndex + "_" + flag, false);
                    }
                }
            }
        }

        private IOriginalAreaKeyPolicy AreaKeys => ((IOriginalAreaKeyEnvironment)engine).AreaKeys;

        public static SceneEntityState Load(OriginalLevelSceneRoot level, OriginalLevelRoomData room, Vector2 offset,
            OriginalLevelEntityData data)
        {
            foreach (var candidate in ((ISourceEverestModulesEnvironment)level.EngineContext).Modules)
                if (candidate is XaphanHelperModule owner)
                    return new GlobalFlagTrigger(level.EngineContext, owner, data, offset);
            throw new InvalidOperationException("XaphanHelper module has not loaded.");
        }
    }
}
