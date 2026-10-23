using System;
using System.Numerics;
using Celeste.Core.Audio;
using Celeste.Core.Components;
using Celeste.Core.Display;
using Celeste.Core.Geometry;
using Celeste.Core.Mathematics;
using Celeste.Core.Mathematics.Extrema;
using Celeste.Core.Mathematics.Framework;
using Celeste.Core.Randomness;
using Celeste.Core.Reflection.Runtime;
using Celeste.Core.Rendering;
using Celeste.Core.Scheduling;
using Celeste.Everest.Modules.Entities.Metadata;
using Celeste.Formats.Map.Entities.Structure;
using Celeste.Game.Content.Maps.Entities;
using Celeste.Game.Content.Maps.Levels;
using Celeste.Game.Execution.Levels;
using Celeste.Graphics.Animation;
using Celeste.Graphics.Content;
using Celeste.Graphics.Content.Atlas;
using Celeste.Graphics.Display.Colors;
using Celeste.Graphics.Rendering.Drawing;
using Celeste.Graphics.Rendering;
using Celeste.Scene.Entities;
using Celeste.Scene.Tracking;
using Celeste.Simulation.Gameplay.Platforms.Foundation;
using Celeste.Simulation.Gameplay.Player.Dash;
using Celeste.Simulation.Presentation.Lighting;
using Celeste.Simulation.Presentation.Particles.Definitions;
using Celeste.Simulation.Presentation.PostProcessing.Displacement;

namespace Celeste.Mods.XaphanHelper.Entities.Platforms.SwapBlocks;

public partial class FlagSwapBlock : SourceSolidEntityState
{
    private class PathRenderer : SceneEntityState
    {
        private FlagSwapBlock block;
        private ISourceEngineContext engine => block.engine;
        private OriginalLevelSceneRoot SceneAsLevel() => (OriginalLevelSceneRoot)Scene;

        private AtlasEntry pathTexture;

        private AtlasEntry clipTexture = new AtlasEntry();

        private float timer = 0f;

        public bool isActive;

        public string flag;

        public PathRenderer(FlagSwapBlock block)
            : base(((ISourceEntitySourceDataEnvironment)block.engine).EntitySourceDataRegistration, block.Position)
        {
            this.block = block;
            flag = block.flag;
            this.Depth = 8999;
            pathTexture = (AtlasEntry)(block.renderBG
                ? ((object)((ISourceGameGraphicsEnvironment)engine).GameGraphics.GameAtlas[
                    block.directory + "/path" + ((block.start.X == block.end.X) ? "V" : "H")])
                : ((object)new AtlasEntry()));
            timer = SourceRandomMath.NextFloat(((ISourceRandomEnvironment)engine).Random);
        }

        public override void Update()
        {
            base.Update();
            if (flag == "" || this.SceneAsLevel().Session.GetFlag(flag))
            {
                isActive = true;
            }
            else
            {
                isActive = false;
            }

            timer += engine.DeltaTime * 4f;
        }

        public override void Render(ISourceRenderContext context)
        {
            if (block.renderBG)
            {
                for (int i = block.moveRect.Left; i < block.moveRect.Right; i += pathTexture.FrameWidth)
                {
                    for (int j = block.moveRect.Top; j < block.moveRect.Bottom; j += pathTexture.FrameHeight)
                    {
                        pathTexture.GetSubtexture(0, 0, Math.Min(pathTexture.FrameWidth, block.moveRect.Right - i),
                            Math.Min(pathTexture.FrameHeight, block.moveRect.Bottom - j), clipTexture);
                        SourceAtlasDrawing.DrawCentered(context, this, clipTexture,
                            new Vector2((float)(i + clipTexture.FrameWidth / 2),
                                (float)(j + clipTexture.FrameHeight / 2)), PackedColor.White);
                    }
                }
            }

            if (isActive)
            {
                float num = 0.5f * (0.5f + ((float)Math.Sin(timer) + 1f) * 0.25f);
                block.DrawBlockStyle(context, new Vector2((float)block.moveRect.X, (float)block.moveRect.Y),
                    block.moveRect.Width, block.moveRect.Height, block.nineSliceTarget, null, PackedColor.White * num);
            }
            else
            {
                block.DrawBlockStyle(context, new Vector2((float)block.moveRect.X, (float)block.moveRect.Y),
                    block.moveRect.Width, block.moveRect.Height, block.nineSliceTarget, null, PackedColor.White * 0.5f);
            }
        }
    }
}
