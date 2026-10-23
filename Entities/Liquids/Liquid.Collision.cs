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
        public bool PlayerInside()
        {
            if (!VisualOnly)
            {
                foreach (Player player in Tracked(typeof(Player)))
                {
                    if (SourceEntityCollision.Check(this, player) && (player.Bottom >= Top + 3f))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public bool PlayerCompletelyInside(bool duringTransition = false)
        {
            foreach (Player player in Tracked(typeof(Player)))
            {
                if (Group == -1)
                {
                    if (SourceEntityCollision.Check(this, player) &&
                        (upsideDown
                            ? player.Bottom <= Bottom - 4 + (duringTransition ? 12 : 0)
                            : player.Top >= Top + 4 - (duringTransition ? 12 : 0)) &&
                        player.Left >= Left - (duringTransition ? 8 : 0) &&
                        player.Right <= Right + (duringTransition ? 8 : 0))
                    {
                        return true;
                    }
                }
                else
                {
                    foreach (Liquid liquid in Tracked(typeof(Liquid)))
                    {
                        if (SourceEntityCollision.Check(liquid, player) && (liquid.upsideDown
                                ? player.Bottom <= liquid.Bottom - 4 + (duringTransition ? 12 : 0)
                                : player.Top >= liquid.Top + 4 - (duringTransition ? 12 : 0)))
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        private void OnCollide(Player player)
        {
            if (!VisualOnly)
            {
                if (LiquidType == "water")
                {
                    if (module.Features.IsControllingRemoteDrone)
                    {
                        var drone = module.LiquidFeatures.FindDrone?.Invoke(Level);
                        if (drone != null && !drone.Dead && player != drone.FakePlayer)
                        {
                            Add(new SimulationCoroutine(engine, drone.Destroy()));
                        }
                    }
                    else if ((!string.IsNullOrEmpty(purifyFlags) && !purified) ||
                             (CheckIfCollideWaterfall() && !GetWaterfallPurifiedInfo()))
                    {
                        player.Die(new Vector2(0f, -1f));
                    }
                }

                if (LiquidType == "lava")
                {
                    if (!module.LiquidFeatures.UseUpgrades)
                    {
                        player.Die(new Vector2(0f, -1f));
                    }
                    else
                    {
                        if (!module.Features.UseMetroidGameplay
                                ? (VariaActive ? (!VariaPreventDying && !player.JustRespawned) : !player.JustRespawned)
                                : !GravityActive)
                        {
                            if (!module.Features.IsControllingRemoteDrone)
                            {
                                if (!module.Features.UiOpened)
                                {
                                    if (!module.Features.UseMetroidGameplay)
                                    {
                                        player.Die(new Vector2(0f, -1f));
                                    }
                                    else
                                    {
                                        if (player != null && player.CanRetry && !GravityActive &&
                                            !LiquidDamageRoutine.Active)
                                        {
                                            Add(LiquidDamageRoutine =
                                                new SimulationCoroutine(engine, LiquidDamage(LiquidType)));
                                        }
                                    }
                                }
                            }
                            /*else -- Kill the drone in dummy state ??? Why ?
                            {
                                var drone = module.LiquidFeatures.FindDrone?.Invoke(Level);
                                if (player.Machine.State == 11 && drone != null && !drone.Dead)
                                {
                                    player.Die(new Vector2(0f, -1f));
                                    Add(new SimulationCoroutine(engine, drone.Destroy(true, true)));
                                }
                            }*/
                        }
                    }
                }

                if (LiquidType.Contains("acid"))
                {
                    if (module.LiquidFeatures.UseUpgrades && module.Features.IsControllingRemoteDrone)
                    {
                        var drone = module.LiquidFeatures.FindDrone?.Invoke(Level);
                        if (player.Machine.State == 11 && drone != null && !drone.Dead)
                        {
                            player.Die(new Vector2(0f, -1f));
                            Add(new SimulationCoroutine(engine, drone.Destroy(true, true)));
                        }
                        else if (drone != null && !drone.Dead)
                        {
                            Add(new SimulationCoroutine(engine, drone.Destroy()));
                        }
                    }
                    else
                    {
                        if (!module.Features.UseMetroidGameplay)
                        {
                            if (VariaActive)
                            {
                                if (!VariaPreventDying && !player.JustRespawned)
                                {
                                    player.Die(new Vector2(0f, -1f));
                                }
                            }
                            else
                            {
                                if (!player.JustRespawned)
                                {
                                    player.Die(new Vector2(0f, -1f));
                                }
                            }
                        }
                        else
                        {
                            if (player != null && player.CanRetry && !LiquidDamageRoutine.Active)
                            {
                                Add(LiquidDamageRoutine = new SimulationCoroutine(engine, LiquidDamage(LiquidType)));
                            }
                        }
                    }
                }

                if (LiquidType == "quicksand")
                {
                    if (module.Features.IsControllingRemoteDrone)
                    {
                        var drone = module.LiquidFeatures.FindDrone?.Invoke(Level);
                        if (drone != null && !drone.Dead && player != drone.FakePlayer)
                        {
                            Add(new SimulationCoroutine(engine, drone.Destroy()));
                        }
                    }
                }
            }
        }

        private IEnumerator LiquidDamage(string LiquidType)
        {
            var healthDisplay = module.LiquidFeatures.FindHealth?.Invoke(Level);
            var heatDamageActive = module.LiquidFeatures.HeatDamageActive?.Invoke(Level) == true;
            if (!heatDamageActive)
            {
                healthDisplay.PlayDamageSound();
            }

            while (healthDisplay != null && healthDisplay.CurrentHealth > 0 && !Level.Transitioning &&
                   !Level.FrozenOrPaused && !module.Features.UiOpened && IsInLiquid())
            {
                healthDisplay.CurrentHealth -= 1;
                healthDisplay.GetEnergyTanks();
                float tickTimer = (0.066f / (LiquidType == "lava" ? 2 : 6)) *
                                  (VariaActive && GravityActive ? 4 : VariaActive ? 2 : 1);
                while (tickTimer > 0)
                {
                    tickTimer -= engine.DeltaTime;
                    yield return null;
                }
            }

            if (!heatDamageActive)
            {
                healthDisplay.StopDamageSound();
            }
        }
    }
}
