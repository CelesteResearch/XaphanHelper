using System;
using Celeste.Core.Components;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using Celeste.Core.Components.Animation;
using Celeste.Core.Display;
using Celeste.Core.Rendering;
using Celeste.Formats.Map.Entities.Structure;
using Celeste.Core.Geometry;
using Celeste.Core.Mathematics;
using Celeste.Core.Mathematics.Easing;
using Celeste.Core.Mathematics.Extrema;
using Celeste.Core.Scheduling;
using Celeste.Everest.Modules.Entities.Metadata;
using Celeste.Everest.Gameplay.Visibility;
using Celeste.Game.Content.Maps.Entities;
using Celeste.Game.Execution.Areas;
using Celeste.Game.Execution.Levels;
using Celeste.Game.Execution.Processes;
using Celeste.Game.Execution.Startup;
using Celeste.Graphics.Animation;
using Celeste.Graphics.Content;
using Celeste.Graphics.Rendering;
using Celeste.Mods.XaphanHelper.Modules;
using Celeste.Mods.XaphanHelper.Runtime.Liquids;
using Celeste.Physics.Collision;
using Celeste.Physics.Collision.Shapes;
using Celeste.Scene.Entities;
using Celeste.Scene.Tracking;
using Celeste.Scene.Tracking.Attributes;
using Celeste.Simulation;
using Celeste.Simulation.Audio.Sources;
using Celeste.Simulation.Gameplay.Environment.Water;
using Celeste.Simulation.Gameplay.Player.Collision.Components;
using Celeste.Simulation.Presentation.PostProcessing.Displacement;
using Color = Celeste.Core.Display.PackedColor;
using Rectangle = Celeste.Core.Geometry.IntRect;
using Player = Celeste.Simulation.RoomPlayerSession;
using Tween = Celeste.Core.Components.Animation.SourceTweenState;
using SoundSource = Celeste.Simulation.Audio.Sources.SoundSourceComponentState;
using WaterInteraction = Celeste.Simulation.Gameplay.Environment.Water.WaterInteractionState;

namespace Celeste.Mods.XaphanHelper.Entities.Liquids
{
    public partial class Liquid : SceneEntityState
    {
        public override void Render(ISourceRenderContext context)
        {
            if (LiquidType == "water")
            {
                if (waterSplashIn != null)
                {
                    waterSplashIn.Render(context);
                }

                if (waterSplashOut != null)
                {
                    waterSplashOut.Render(context);
                }
            }

            if (upsideDown)
            {
                for (int i = 0; i < Width / liquidSprite.Width; i++)
                {
                    liquidSprite.RenderPosition =
                        Position + new Vector2(i * liquidSprite.Width, Height - liquidSprite.Height + 8);
                    if (EverestCullHelper.IsRectangleVisible(engine, liquidSprite.RenderPosition.X,
                            liquidSprite.RenderPosition.Y, liquidSprite.Width, liquidSprite.Height))
                    {
                        if ((i + 1) * liquidSprite.Width <= Width && liquidSprite.Height <= Height)
                        {
                            liquidSprite.Render(context);
                        }
                        else if ((i + 1) * liquidSprite.Width > Width && liquidSprite.Height <= Height)
                        {
                            DrawLiquidSubrect(context, Vector2.Zero,
                                new Rectangle(0, 0,
                                    (int)liquidSprite.Width - ((i + 1) * (int)liquidSprite.Width - (int)Width),
                                    (int)liquidSprite.Height));
                        }
                        else if ((i + 1) * liquidSprite.Width <= Width && liquidSprite.Height > Height)
                        {
                            DrawLiquidSubrect(context, new Vector2(0, liquidSprite.Height - Height - 8),
                                new Rectangle(0, 0, (int)liquidSprite.Width,
                                    (int)liquidSprite.Height - ((int)liquidSprite.Height - (int)Height - 8)));
                        }
                        else
                        {
                            DrawLiquidSubrect(context, new Vector2(0, liquidSprite.Height - Height - 8),
                                new Rectangle(0, 0,
                                    (int)liquidSprite.Width - ((i + 1) * (int)liquidSprite.Width - (int)Width),
                                    (int)liquidSprite.Height - ((int)liquidSprite.Height - (int)Height - 8)));
                        }
                    }
                }

                int totalLines = (int)(liquidSprite.Height - SurfaceHeight) / 8;
                for (int i = 0; i < Width / liquidSprite.Width; i++)
                {
                    for (int j = 0; j < (Height + 8 - liquidSprite.Height) / 8; j++)
                    {
                        liquidSprite.RenderPosition = Position + new Vector2(i * liquidSprite.Width,
                            (Height + 8 - liquidSprite.Height) - j * 8 - 8);
                        if (EverestCullHelper.IsRectangleVisible(engine, liquidSprite.RenderPosition.X,
                                liquidSprite.RenderPosition.Y, liquidSprite.Width, liquidSprite.Height))
                        {
                            Math.DivRem(j, totalLines, out int Variation);
                            if ((i + 1) * liquidSprite.Width <= Width)
                            {
                                DrawLiquidSubrect(context, Vector2.Zero,
                                    new Rectangle(0, SurfaceHeight + 8 * Variation, (int)liquidSprite.Width, 8));
                            }
                            else
                            {
                                DrawLiquidSubrect(context, Vector2.Zero,
                                    new Rectangle(0, SurfaceHeight + 8 * Variation,
                                        (int)liquidSprite.Width - ((i + 1) * (int)liquidSprite.Width - (int)Width), 8));
                            }
                        }
                    }
                }
            }
            else
            {
                for (int i = 0; i < Width / liquidSprite.Width; i++)
                {
                    liquidSprite.RenderPosition = Position + new Vector2(i * liquidSprite.Width, -8f);
                    if (EverestCullHelper.IsRectangleVisible(engine, liquidSprite.RenderPosition.X,
                            liquidSprite.RenderPosition.Y, liquidSprite.Width, liquidSprite.Height))
                    {
                        if ((i + 1) * liquidSprite.Width <= Width && liquidSprite.Height <= Height)
                        {
                            liquidSprite.Render(context);
                        }
                        else if ((i + 1) * liquidSprite.Width > Width && liquidSprite.Height <= Height)
                        {
                            DrawLiquidSubrect(context, Vector2.Zero,
                                new Rectangle(0, 0,
                                    (int)liquidSprite.Width - ((i + 1) * (int)liquidSprite.Width - (int)Width),
                                    (int)liquidSprite.Height));
                        }
                        else if ((i + 1) * liquidSprite.Width <= Width && liquidSprite.Height > Height)
                        {
                            DrawLiquidSubrect(context, Vector2.Zero,
                                new Rectangle(0, 0, (int)liquidSprite.Width,
                                    (int)liquidSprite.Height - ((int)liquidSprite.Height - (int)Height - 8)));
                        }
                        else
                        {
                            DrawLiquidSubrect(context, Vector2.Zero,
                                new Rectangle(0, 0,
                                    (int)liquidSprite.Width - ((i + 1) * (int)liquidSprite.Width - (int)Width),
                                    (int)liquidSprite.Height - ((int)liquidSprite.Height - (int)Height - 8)));
                        }
                    }
                }

                for (int i = 0; i < Width / liquidSprite.Width; i++)
                {
                    for (int j = 0; j < (Height + 8 - liquidSprite.Height) / (liquidSprite.Height - SurfaceHeight); j++)
                    {
                        liquidSprite.RenderPosition = Position + new Vector2(i * liquidSprite.Width,
                            liquidSprite.Height - 8 + j * (liquidSprite.Height - SurfaceHeight));
                        if (EverestCullHelper.IsRectangleVisible(engine, liquidSprite.RenderPosition.X,
                                liquidSprite.RenderPosition.Y, liquidSprite.Width, liquidSprite.Height))
                        {
                            if ((i + 1) * liquidSprite.Width <= Width &&
                                liquidSprite.Height - 8 + j * (liquidSprite.Height - SurfaceHeight) <=
                                (Height + 8 - liquidSprite.Height))
                            {
                                DrawLiquidSubrect(context, Vector2.Zero,
                                    new Rectangle(0, SurfaceHeight, (int)liquidSprite.Width,
                                        (int)liquidSprite.Height - SurfaceHeight));
                            }
                            else if ((i + 1) * liquidSprite.Width > Width &&
                                     liquidSprite.Height - 8 + j * (liquidSprite.Height - SurfaceHeight) <=
                                     (Height + 8 - liquidSprite.Height))
                            {
                                DrawLiquidSubrect(context, Vector2.Zero,
                                    new Rectangle(0, SurfaceHeight,
                                        (int)liquidSprite.Width - ((i + 1) * (int)liquidSprite.Width - (int)Width),
                                        (int)liquidSprite.Height - SurfaceHeight));
                            }
                            else if ((i + 1) * liquidSprite.Width <= Width &&
                                     liquidSprite.Height - 8 + j * (liquidSprite.Height - SurfaceHeight) >
                                     (Height + 8 - liquidSprite.Height))
                            {
                                DrawLiquidSubrect(context, Vector2.Zero,
                                    new Rectangle(0, SurfaceHeight, (int)liquidSprite.Width,
                                        (int)(liquidSprite.Height - SurfaceHeight) -
                                        ((int)(liquidSprite.Height - SurfaceHeight +
                                               j * (liquidSprite.Height - SurfaceHeight)) -
                                         (int)(Height + 8 - liquidSprite.Height))));
                            }
                            else
                            {
                                DrawLiquidSubrect(context, Vector2.Zero,
                                    new Rectangle(0, SurfaceHeight,
                                        (int)liquidSprite.Width - ((i + 1) * (int)liquidSprite.Width - (int)Width),
                                        (int)(liquidSprite.Height - SurfaceHeight) -
                                        ((int)(liquidSprite.Height - SurfaceHeight +
                                               j * (liquidSprite.Height - SurfaceHeight)) -
                                         (int)(Height + 8 - liquidSprite.Height))));
                            }
                        }
                    }
                }
            }
        }

        public void CheckSolidsForDisplacement()
        {
            int i = 0;
            for (int length = DisplacementGrid.GetLength(0); i < length; i++)
            {
                int j = 0;
                for (int length2 = DisplacementGrid.GetLength(1); j < length2; j++)
                {
                    if (foreground)
                    {
                        DisplacementGrid[i, j] = true;
                    }
                    else
                    {
                        DisplacementGrid[i, j] = !SolidAt(new Rectangle((int)X + i * 8, (int)Y + j * 8, 8, 8));
                    }
                }
            }
        }

        public void RenderDisplacement() => Celeste.Graphics.Rendering.Primitives.WaterDisplacementGridDrawing.Draw(
            ((ISourceRenderEnvironment)engine).CurrentContext, this, DisplacementGrid, X, Y,
            new Color(.5f, .5f, .25f, 1f));
    }
}
