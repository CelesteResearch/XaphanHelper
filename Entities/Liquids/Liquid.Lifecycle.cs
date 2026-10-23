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
        private void OnLastFrameIn(string s)
        {
            waterSplashIn.Stop();
            waterSplashIn.Visible = false;
            waterIn = false;
        }

        private void OnLastFrameOut(string s)
        {
            waterSplashOut.Stop();
            waterSplashOut.Visible = false;
            waterOut = false;
        }

        public override void Added(object scene)
        {
            base.Added(scene);
            if (!string.IsNullOrEmpty(appearFlags))
            {
                string[] flags = appearFlags.Split(',');
                foreach (string flag in flags)
                {
                    if (!Level.Session.GetFlag(flag))
                    {
                        RemoveSelf();
                        break;
                    }
                }
            }

            if (!string.IsNullOrEmpty(removeFlags))
            {
                string[] flags = removeFlags.Split(',');
                foreach (string flag in flags)
                {
                    if (Level.Session.GetFlag(flag))
                    {
                        RemoveSelf();
                        break;
                    }
                }
            }

            origLevelBottom = Level.Bounds.Bottom;
            if (LiquidType == "lava")
            {
                sfx.Play("event:/env/local/09_core/lavagate_idle");
                if (Width > 320)
                {
                    sfx2.Play("event:/env/local/09_core/lavagate_idle");
                }
            }

            if (LiquidType == "water")
            {
                CheckSolidsForDisplacement();
            }

            if (!string.IsNullOrEmpty(riseEndFlag) && Level.Session.GetFlag(riseEndFlag))
            {
                Position -= new Vector2(0, riseDistance);
                Collider.Height = origLevelBottom - Position.Y;
                if (Position.Y > Level.Bounds.Bottom)
                {
                    RemoveSelf();
                }
            }

            if (Group != -1)
            {
                GroupLeader = true;
                foreach (Liquid liquid in Tracked(typeof(Liquid)))
                {
                    if (liquid != this && liquid.Group == Group)
                    {
                        GroupLeader = false;
                    }
                }
            }
        }

        private bool CheckIfPurified()
        {
            bool purified = true;
            if (!CheckIfCollideWaterfall())
            {
                if (string.IsNullOrEmpty(purifyFlags))
                {
                    return true;
                }

                string[] flags = purifyFlags.Split(',');
                foreach (string flag in flags)
                {
                    if (invertPurifyFlags ? Level.Session.GetFlag(flag) : !Level.Session.GetFlag(flag))
                    {
                        purified = false;
                        break;
                    }
                }
            }
            else
            {
                purified = GetWaterfallPurifiedInfo();
            }

            return purified;
        }

        private bool CheckIfCollideWaterfall()
        {
            var sections = module.LiquidFeatures.WaterfallSections;
            for (var i = 0; i < sections.Count; i++)
                if (ReferenceEquals(sections[i].Entity.Scene, Scene) &&
                    SourceEntityCollision.CheckAt(this, sections[i].Entity, Position - Vector2.UnitY))
                    return true;
            return false;
        }

        private bool GetWaterfallPurifiedInfo()
        {
            var total = 0;
            var purifiedCount = 0;
            var sections = module.LiquidFeatures.WaterfallSections;
            for (var i = 0; i < sections.Count; i++)
            {
                var section = sections[i];
                if (!ReferenceEquals(section.Entity.Scene, Scene) ||
                    !SourceEntityCollision.CheckAt(this, section.Entity, Position - Vector2.UnitY)) continue;
                total++;
                if (section.Purified) purifiedCount++;
            }

            return purifiedCount >= total - purifiedCount;
        }

        public IEnumerator PoisonedRoutine()
        {
            string Prefix = Level.AreaData.LevelSet;
            int chapterIndex = ChapterIndex;
            while (true)
            {
                if (!string.IsNullOrEmpty(purifyFlags) || CheckIfCollideWaterfall())
                {
                    bool skip = false;
                    if (purified)
                    {
                        gradientTimer = 0f;
                    }
                    else
                    {
                        while (!CheckIfPurified())
                        {
                            yield return null;
                        }

                        while (gradientTimer > 0f)
                        {
                            gradientTimer -= engine.DeltaTime;
                            yield return null;
                            if (gradientTimer <= 0.5f)
                            {
                                purified = true;
                                if (!string.IsNullOrEmpty(purifiedFlag))
                                {
                                    Level.Session.SetFlag(purifiedFlag, true);
                                    if (!module.LiquidSaveData.SavedFlags.Contains(Prefix + "_Ch" + chapterIndex + "_" +
                                            purifiedFlag))
                                    {
                                        module.LiquidSaveData.SavedFlags.Add(Prefix + "_Ch" + chapterIndex + "_" +
                                                                             purifiedFlag);
                                    }
                                }
                            }

                            if (Level.Transitioning || !CheckIfPurified())
                            {
                                skip = true;
                                break;
                            }
                        }

                        if (!skip)
                        {
                            gradientTimer = 0f;
                        }
                    }

                    while (CheckIfPurified())
                    {
                        yield return null;
                    }

                    skip = false;
                    while (gradientTimer < 1f)
                    {
                        gradientTimer += engine.DeltaTime;
                        yield return null;
                        if (gradientTimer >= 0.5f)
                        {
                            purified = false;
                            if (!string.IsNullOrEmpty(purifiedFlag))
                            {
                                if (FlagRegiseredInSaveData())
                                {
                                    Level.Session.SetFlag(purifiedFlag, false);
                                    module.LiquidSaveData.SavedFlags.Remove(Prefix + "_Ch" + chapterIndex + "_" +
                                                                            purifiedFlag);
                                }
                            }
                        }

                        if (Level.Transitioning || CheckIfPurified())
                        {
                            skip = true;
                            break;
                        }
                    }

                    if (!skip)
                    {
                        gradientTimer = 1f;
                    }
                }

                yield return null;
            }
        }

        public override void Awake(object scene)
        {
            Add(new SimulationCoroutine(engine, PoisonedRoutine()));
            liquidSprite.Color = waterSplashIn.Color = waterSplashOut.Color = GradientColor(gradientTimer) *
                                                                              (PlayerCompletelyInside()
                                                                                  ? insideTransparency
                                                                                  : outsideTransparency);
            purified = CheckIfPurified();
            if (purified)
            {
                gradientTimer = 0f;
            }

            base.Awake(scene);
            if (!GroupLeader)
            {
                foreach (Liquid liquid in Tracked(typeof(Liquid)))
                {
                    if (liquid.Group == Group && liquid.GroupLeader)
                    {
                        leader = liquid;
                        break;
                    }
                }
            }

            Player player = FirstPlayer();
            if (player != null)
            {
                if (!PlayerCompletelyInside(true))
                {
                    currentTransparency = outsideTransparency;
                }
                else
                {
                    if (Group == -1 || GroupLeader)
                    {
                        currentTransparency = insideTransparency;
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
    }
}
