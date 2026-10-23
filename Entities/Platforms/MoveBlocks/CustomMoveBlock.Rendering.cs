using System;
using Celeste.Mods.XaphanHelper.Presentation.Outlines;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using Celeste.Core.Components;
using Celeste.Core.Display;
using Celeste.Core.Mathematics;
using Celeste.Core.Mathematics.Easing;
using Celeste.Core.Randomness;
using Celeste.Core.Audio;
using Celeste.Core.Rendering;
using Celeste.Core.Reflection.Runtime;
using Celeste.Core.Scheduling;
using Celeste.Everest.Modules.Entities.Metadata;
using Celeste.Formats.Map.Entities.Attributes;
using Celeste.Game.Content.Maps.Entities;
using Celeste.Game.Content.Maps.Levels;
using Celeste.Game.Execution.Levels;
using Celeste.Game.Execution.Processes;
using Celeste.Graphics.Animation;
using Celeste.Graphics.Content;
using Celeste.Graphics.Content.Atlas;
using Celeste.Graphics.Display.Colors;
using Celeste.Graphics.Rendering;
using Celeste.Input.Gameplay;
using Celeste.Mods.XaphanHelper.Runtime.Slopes;
using Celeste.Physics.Collision;
using Celeste.Physics.Collision.Shapes;
using Celeste.Scene.Entities;
using Celeste.Scene.Tracking;
using Celeste.Scene.Tracking.Attributes;
using Celeste.Simulation;
using Celeste.Simulation.Audio.Sources;
using Celeste.Simulation.Gameplay.Actors.Foundation;
using Celeste.Simulation.Gameplay.Platforms.Foundation;
using Celeste.Simulation.Gameplay.Platforms.StaticMovers;
using Celeste.Simulation.Presentation.Lighting;
using Celeste.Simulation.Presentation.Particles.Definitions;

namespace Celeste.Mods.XaphanHelper.Entities.Platforms.MoveBlocks
{
    public partial class CustomMoveBlock
    {
        private void UpdateColors()
        {
            PackedColor value = idleBgFill;
            if (state == MovementState.Moving)
            {
                value = pressedBgFill;
            }
            else if (state == MovementState.Breaking)
            {
                value = breakingBgFill;
            }

            fillColor = PackedColor.Lerp(fillColor, value, 10f * Engine.DeltaTime);
            if (buttonIgnoreColors)
            {
                return;
            }

            foreach (SpriteImage item in topButton)
            {
                item.Color = fillColor;
            }

            foreach (SpriteImage item2 in bottomButton)
            {
                item2.Color = fillColor;
            }

            foreach (SpriteImage item3 in leftButton)
            {
                item3.Color = fillColor;
            }

            foreach (SpriteImage item4 in rightButton)
            {
                item4.Color = fillColor;
            }
        }

        private void AddImage(AtlasEntry tex, Vector2 position, float rotation, Vector2 scale, List<SpriteImage> addTo)
        {
            SpriteImage image = new SpriteImage(tex);
            image.Position = position + new Vector2(4f, 4f);
            image.CenterOrigin();
            image.Rotation = rotation;
            image.Scale = scale;
            Add(image);
            addTo?.Add(image);
        }

        public override void Render(ISourceRenderContext context)
        {
            if (!Destroyed)
            {
                float num = (glow ? (0.9f * (0.9f + ((float)Math.Sin(alpha) + 1f) * 0.125f)) : 1f);
                Vector2 position = Position;
                Position += base.Shake;
                SourcePrimitiveDrawing.Rect(context, this, base.X + 2f, base.Y + 2f, base.Width - 4f, base.Height - 4f,
                    PackedColor.Black);
                foreach (SpriteImage item in leftButton)
                {
                    item.Render(context);
                }

                foreach (SpriteImage item2 in rightButton)
                {
                    item2.Render(context);
                }

                foreach (SpriteImage item3 in topButton)
                {
                    item3.Render(context);
                }

                foreach (SpriteImage item4 in bottomButton)
                {
                    item4.Render(context);
                }

                float num2 = 2f;
                if (directory == "objects/moveBlock")
                {
                    num2 = 3f;
                }

                SourcePrimitiveDrawing.Rect(context, this, base.X + num2, base.Y + num2, base.Width - num2 * 2f,
                    base.Height - num2 * 2f, fillColor * num);
                foreach (SpriteImage item5 in body)
                {
                    item5.Render(context);
                }

                SourcePrimitiveDrawing.Rect(context, this, base.Center.X - 4f, base.Center.Y - 4f, 8f, 8f,
                    fillColor * num);
                if (state != MovementState.Breaking)
                {
                    int value = (int)Math.Floor((0f - angle + MathF.PI * 2f) % (MathF.PI * 2f) / (MathF.PI * 2f) * 8f +
                                                0.5f);
                    DrawCentered(context, this, arrows[ScalarMath.Clamp(value, 0, 7)], Center);
                }
                else
                {
                    DrawCentered(context, this,
                        ((ISourceGameGraphicsEnvironment)Engine).GameGraphics.GameAtlas[directory + "/x"], Center);
                }

                float num3 = flash * 4f;
                SourcePrimitiveDrawing.Rect(context, this, base.X - num3, base.Y - num3, base.Width + num3 * 2f,
                    base.Height + num3 * 2f, PackedColor.White * flash);
                Position = position;
                return;
            }

            for (int i = 0; i < outline.Count; i++)
            {
                if (respawnTimer < (float)i * outlineColorTimer)
                {
                    SourcePrimitiveDrawing.Point(context, this, Position + new Vector2(outline[i].x, outline[i].y),
                        idleBgFill * (outline[i].visible ? outlineColorStrength : 0f));
                }
                else
                {
                    SourcePrimitiveDrawing.Point(context, this, Position + new Vector2(outline[i].x, outline[i].y),
                        PackedColor.White * (outline[i].visible ? outlineColorStrength : 0f));
                }
            }
        }
    }
}
