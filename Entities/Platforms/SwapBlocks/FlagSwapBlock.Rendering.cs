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
    public override void Render(ISourceRenderContext context)
    {
        Vector2 val = this.Position + this.Shake;
        if (lerp != (float)target && speed > 0f)
        {
            Vector2 val2 = Vector2Math.SafeNormalize(end - start);
            if (target == 1)
            {
                val2 *= -1f;
            }

            float num = speed / maxForwardSpeed;
            float num2 = 16f * num;
            for (int i = 2; (float)i < num2; i += 2)
            {
                DrawBlockStyle(context, val + val2 * (float)i, this.Width, this.Height, nineSliceGreen, middleGreen,
                    PackedColor.White * (1f - (float)i / num2));
            }
        }

        if (redAlpha < 1f)
        {
            DrawBlockStyle(context, val, this.Width, this.Height, nineSliceGreen, middleGreen, PackedColor.White);
        }

        if (redAlpha > 0f)
        {
            DrawBlockStyle(context, val, this.Width, this.Height, nineSliceRed, middleRed,
                PackedColor.White * redAlpha);
        }
    }

    private void DrawBlockStyle(ISourceRenderContext context, Vector2 pos, float width, float height,
        AtlasEntry[,] ninSlice, Sprite middle, PackedColor color)
    {
        int num = (int)(width / 8f);
        int num2 = (int)(height / 8f);
        SourceAtlasDrawing.Draw(context, this, ninSlice[0, 0], pos + new Vector2(0f, 0f), Vector2.Zero, color);
        SourceAtlasDrawing.Draw(context, this, ninSlice[2, 0], pos + new Vector2(width - 8f, 0f), Vector2.Zero, color);
        SourceAtlasDrawing.Draw(context, this, ninSlice[0, 2], pos + new Vector2(0f, height - 8f), Vector2.Zero, color);
        SourceAtlasDrawing.Draw(context, this, ninSlice[2, 2], pos + new Vector2(width - 8f, height - 8f), Vector2.Zero,
            color);
        for (int i = 1; i < num - 1; i++)
        {
            SourceAtlasDrawing.Draw(context, this, ninSlice[1, 0], pos + new Vector2((float)(i * 8), 0f), Vector2.Zero,
                color);
            SourceAtlasDrawing.Draw(context, this, ninSlice[1, 2], pos + new Vector2((float)(i * 8), height - 8f),
                Vector2.Zero, color);
        }

        for (int j = 1; j < num2 - 1; j++)
        {
            SourceAtlasDrawing.Draw(context, this, ninSlice[0, 1], pos + new Vector2(0f, (float)(j * 8)), Vector2.Zero,
                color);
            SourceAtlasDrawing.Draw(context, this, ninSlice[2, 1], pos + new Vector2(width - 8f, (float)(j * 8)),
                Vector2.Zero, color);
        }

        for (int k = 1; k < num - 1; k++)
        {
            for (int l = 1; l < num2 - 1; l++)
            {
                SourceAtlasDrawing.Draw(context, this, ninSlice[1, 1], pos + new Vector2((float)k, (float)l) * 8f,
                    Vector2.Zero, color);
            }
        }

        if (middle != null)
        {
            middle.Color = color;
            middle.RenderPosition = pos + new Vector2(width / 2f, height / 2f);
            middle.Render(context);
        }
    }
}
