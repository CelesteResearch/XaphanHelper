// SPDX-License-Identifier: MIT

using System;
using System.Numerics;
using Celeste.Core.Components;
using Celeste.Core.Display;
using Celeste.Core.Mathematics;
using Celeste.Core.Rendering;
using Celeste.Core.Scheduling;
using Celeste.Game.Execution.Areas;
using Celeste.Game.Execution.Graphics;
using Celeste.Game.Execution.Levels;
using Celeste.Graphics.Content;
using Celeste.Graphics.Display;
using Celeste.Graphics.Rendering;
using Celeste.Mods.XaphanHelper.Modules;
using Celeste.Scene.Entities;
using Celeste.Scene.Scheduling.ExecutionTags;
using Celeste.Scene.Tracking.Attributes;
using Celeste.Simulation;

namespace Celeste.Mods.XaphanHelper.Entities.Liquids
{
    [SceneTracked(true)]
    public sealed class OxygenIndicator : SceneEntityState
    {
        private readonly ISourceEngineContext engine;
        private readonly XaphanHelperModule module;

        public OxygenIndicator(ISourceEngineContext engine, XaphanHelperModule module)
            : base(((ISourceEntitySourceDataEnvironment)engine).EntitySourceDataRegistration)
        {
            this.engine = engine;
            this.module = module;
            Depth = -20000;
            Visible = false;
            AddTag(((ISourcePersistentTransitionExecutionTagsEnvironment)engine).PersistentTransitionExecutionTags
                .Persistent.Value);
        }

        public override void Update()
        {
            base.Update();
            var entities = ((OriginalLevelSceneRoot)Scene).Scheduler.Entities;
            if (entities.GetTrackedEntity<RoomPlayerSession>() != null &&
                entities.GetTrackedEntity<BreathManager>() is { } manager)
                Visible = manager.IsVisible;
        }

        public override void Render(ISourceRenderContext context)
        {
            base.Render(context);
            if (!Visible) return;
            var level = (OriginalLevelSceneRoot)Scene;
            var keys = ((IOriginalAreaKeyEnvironment)engine).AreaKeys;
            int mode = keys.GetLevelSet(keys.GetSessionArea(level.Session)) == "Xaphan/0"
                ? module.BreathFeatures.SoCMOxygenIndicator
                : module.BreathFeatures.OxygenIndicator;
            if (mode == 0 || mode == 3) return;
            var player = level.Scheduler.Entities.GetTrackedEntity<RoomPlayerSession>();
            var manager = level.Scheduler.Entities.GetTrackedEntity<BreathManager>();
            if (player == null || manager == null || module.Features.IsControllingRemoteDrone) return;
            if (!player.Sprite.Visible && !level.Session.GetFlag("Xaphan_Helper_Ceiling") &&
                module.BreathFeatures.StartedScrewAttack?.Invoke(player) != true) return;
            var position = player.Center + new Vector2(player.Data.Facing == 1 ? player.Machine.State != 1 ? -14f : -12f
                : player.Machine.State != 1 ? 7f : 5f, -10f);
            if (module.BreathFeatures.StaminaIndicatorVisible?.Invoke(player) == true)
                position.X += player.Data.Facing == 1 ? -4f : 4f;
            var texture =
                ((OriginalGameGraphicsResources)((ISourceGameGraphicsEnvironment)engine).GameGraphics).GuiAtlas.Entries[
                    "upgrades/staminaindicator"];
            var draw = new AtlasTextureDraw(texture, position, Vector2.Zero, PackedColor.White, Vector2.One);
            ((ISourceRenderCommandSink<AtlasTextureDraw>)context).Draw(in draw, this);
            position += new Vector2(3, 3);
            SourcePrimitiveDrawing.Line(context, this, position + new Vector2(0, 10),
                position + new Vector2(0, 10 - (float)Math.Ceiling(10f * manager.GetAirPercent() / 100f)),
                manager.GetAirPercent() > 26.66f ? new PackedColor(0, 170, 255) : new PackedColor(0, 43, 254),
                ((ISourceVectorEnvironment)engine).VectorArithmetic);
        }
    }
}
