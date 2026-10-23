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
        public override void Update()
        {
            base.Update();
            Player player = FirstPlayer();
            if (!Level.Transitioning)
            {
                if (player != null)
                {
                    if (!PlayerCompletelyInside())
                    {
                        currentTransparency = ScalarMath.Approach(currentTransparency, outsideTransparency,
                            engine.DeltaTime * 2f);
                    }
                    else
                    {
                        if (Group == -1 || GroupLeader)
                        {
                            currentTransparency = ScalarMath.Approach(currentTransparency, insideTransparency,
                                engine.DeltaTime * 2f);
                        }
                        else
                        {
                            foreach (Liquid liquid in Tracked(typeof(Liquid)))
                            {
                                if (leader != null && liquid.Group == Group)
                                {
                                    currentTransparency = leader.currentTransparency;
                                }
                            }
                        }
                    }
                }
            }

            liquidSprite.Color = waterSplashIn.Color =
                waterSplashOut.Color = GradientColor(gradientTimer * 100) * currentTransparency;
            if ((LiquidType == "lava" && IsInType("lava") && !GravityActive) ||
                (LiquidType.Contains("acid") && IsInType("acid", true)))
            {
                FlashingRed = true;
            }
            else
            {
                if (FlashingRed)
                {
                    FlashingRed = false;
                }
            }

            if (StopRiseSound)
            {
                if (RiseSoundSource != null)
                {
                    RiseSoundSource.Stop(true);
                }
            }

            if (Top > Level.Bounds.Bottom + (LiquidType == "lava" ? 8 : 0) && riseDistance < 0)
            {
                Visible = false;
                Add(new SimulationCoroutine(engine, LevelBottomDelay()));
                return;
            }

            if (!playerHasMoved && player != null && player.Speed != Vector2.Zero)
            {
                playerHasMoved = true;
            }

            if (rising)
            {
                Collider.Height = origLevelBottom - Position.Y;
            }

            if (playerHasMoved && !riseEnd && !rising && riseDistance != 0 &&
                (string.IsNullOrEmpty(riseFlag) || Level.Session.GetFlag(riseFlag)) &&
                (string.IsNullOrEmpty(riseEndFlag) || !Level.Session.GetFlag(riseEndFlag) || riseEndFlag == riseFlag))
            {
                rising = true;
                Add(new SimulationCoroutine(engine, RiseRoutine()));
            }
            else if (!rising && !waving && LowPosition != 0)
            {
                waving = true;
                Vector2 start = Position;
                Vector2 end = Position + new Vector2(0, LowPosition);
                float coef = LowPosition / 8;
                float num = Vector2.Distance(start, end) / 4f / coef;
                waveTween = Tween.Create(engine.TweenPool, Tween.TweenMode.YoyoLooping,
                    ((ISourceEasingEnvironment)engine).EasingArithmetic.SineInOut, num, start: true);
                waveTween.OnUpdate = delegate(Tween t) { Position = Vector2.Lerp(start, end, t.Eased); };
                Add(waveTween);
            }

            if (rising)
            {
                if (waveTween != null)
                {
                    waving = false;
                    waveTween.Stop();
                }
            }

            if (LiquidType == "water")
            {
                if (player != null && player.Left < Right && player.Right > Left)
                {
                    if ((upsideDown
                            ? (player.Top <= Bottom + 1f && player.Top >= Bottom - 1f && player.Speed.Y < 0)
                            : (player.Bottom >= Top - 1f && player.Bottom <= Top + 1f && player.Speed.Y > 0)) &&
                        !waterIn)
                    {
                        waterIn = true;
                        PlaySplashIn(new Vector2(player.Position.X - 12f, (upsideDown ? Bottom - 20f : Top - 21f)));
                    }

                    if ((upsideDown
                            ? (player.Top <= Bottom + 1f && player.Top >= Bottom - 1f && player.Speed.Y > 0)
                            : (player.Bottom >= Top - 1f && player.Bottom <= Top + 1f && player.Speed.Y < 0)) &&
                        !waterOut)
                    {
                        waterOut = true;
                        waterSplashOut.RenderPosition = new Vector2(player.Position.X - 12f,
                            (upsideDown ? Bottom - 20f : Top - 21f));
                        waterSplashOut.Play("splash", restart: true);
                        waterSplashOut.OnLastFrame = OnLastFrameOut;
                    }
                }

                var components =
                    ((ISourceScene)Scene).SourceScene.Components.GetTrackedComponents("Celeste.WaterInteraction");
                foreach (WaterInteraction item in components)
                {
                    Rectangle bounds = ((ISourceEntityCollider)item.Entity).Collider.Bounds;
                    bool flag = contains.Contains(item);
                    bool flag2 = SourceEntityCollision.CheckRect(this, bounds);
                    if (flag != flag2)
                    {
                        bool flag3 = item.IsDashing();
                        int num = (bounds.Center.Y < Center.Y && !SolidAt(bounds)) ? 1 : 0;
                        if (flag)
                        {
                            if (flag3)
                            {
                                PlayAudio("event:/char/madeline/water_dash_out",
                                    new Vector2(bounds.Center.X, bounds.Center.Y), "deep", num);
                            }
                            else if (!Level.Transitioning && player != null && player.Bottom >= Top)
                            {
                                PlayAudio("event:/char/madeline/water_out",
                                    new Vector2(bounds.Center.X, bounds.Center.Y), "deep", num);
                            }

                            item.DrippingTimer = 2f;
                        }
                        else
                        {
                            if (flag3 && num == 1)
                            {
                                PlayAudio("event:/char/madeline/water_dash_in",
                                    new Vector2(bounds.Center.X, bounds.Center.Y), "deep", num);
                            }
                            else if (!Level.Transitioning && player != null && player.Top < Top)
                            {
                                PlayAudio("event:/char/madeline/water_in",
                                    new Vector2(bounds.Center.X, bounds.Center.Y), "deep", num);
                            }

                            item.DrippingTimer = 0f;
                        }

                        if (flag)
                        {
                            contains.Remove(item);
                        }
                        else
                        {
                            contains.Add(item);
                        }
                    }
                }
            }

            if (LiquidType == "quicksand" && player != null && !module.Features.UseMetroidGameplay)
            {
                if (player.Speed.Y >= 10 && PlayerInside())
                {
                    player.Speed = new Vector2(player.Speed.X, 10);
                }

                if (player.Speed.X >= 13.5f && PlayerInside())
                {
                    player.Speed = new Vector2(13.5f, player.Speed.Y);
                }

                if (player.Speed.X <= -13.5f && PlayerInside())
                {
                    player.Speed = new Vector2(-13.5f, player.Speed.Y);
                }

                if (PlayerInside() && player.Top >= Top + 6)
                {
                    player.Die(Vector2.Zero);
                }
            }
        }

        public bool FlagRegiseredInSaveData()
        {
            var session = Level.Session;
            string Prefix = Level.AreaData.LevelSet;
            int chapterIndex = ChapterIndex;
            return module.LiquidSaveData.SavedFlags.Contains(Prefix + "_Ch" + chapterIndex + "_test");
        }

        public void PlaySplashIn(Vector2 position)
        {
            waterSplashIn.RenderPosition = position;
            waterSplashIn.Play("splash", restart: true);
            waterSplashIn.OnLastFrame = OnLastFrameIn;
        }
    }
}
