// SPDX-License-Identifier: MIT

using System.Collections;
using System.Numerics;
using Celeste.Core.Components;
using Celeste.Core.Scheduling;
using Celeste.Game.Execution.Levels;
using Celeste.Mods.XaphanHelper.Modules;
using Celeste.Scene.Entities;
using Celeste.Scene.Scheduling.ExecutionTags;
using Celeste.Scene.Tracking.Attributes;
using Celeste.Simulation;

namespace Celeste.Mods.XaphanHelper.Entities.Liquids
{
    [SceneTracked(true)]
    public sealed class BreathManager : SceneEntityState
    {
        private readonly ISourceEngineContext engine;
        private readonly XaphanHelperModule module;
        private SimulationCoroutine airRoutine;
        public bool Flashing;
        public bool ForceRechargeAir;
        public float Air = -1f;
        public Liquid CurrentLiquid;
        public float CurrentMaxAir;
        public bool IsVisible;

        public BreathManager(ISourceEngineContext engine, XaphanHelperModule module)
            : base(((ISourceEntitySourceDataEnvironment)engine).EntitySourceDataRegistration)
        {
            this.engine = engine;
            this.module = module;
            AddTag(((ISourcePersistentTransitionExecutionTagsEnvironment)engine).PersistentTransitionExecutionTags
                .Persistent.Value);
            Depth = -20000;
            airRoutine = new SimulationCoroutine(engine);
        }

        public float GetAirPercent() => Air * 100f / CurrentMaxAir;

        public override void Update()
        {
            var level = (OriginalLevelSceneRoot)Scene;
            var player = level.Scheduler.Entities.GetTrackedEntity<RoomPlayerSession>();
            if (player == null || module.Features.UiOpened || player.Machine.State == 11) return;
            ForceRechargeAir = module.BreathFeatures.TouchingAirBubbles?.Invoke(player) == true;
            bool inside = false;
            foreach (Liquid liquid in level.Scheduler.Entities.GetTrackedEntities(typeof(Liquid)))
            {
                if (!liquid.PlayerCompletelyInside() || !liquid.CanDrown || liquid.VisualOnly) continue;
                inside = true;
                if (!ReferenceEquals(liquid, CurrentLiquid))
                {
                    CurrentLiquid = liquid;
                    Air = Air == -1f ? liquid.AirTimer : GetAirPercent() * liquid.AirTimer / 100f;
                    CurrentMaxAir = liquid.AirTimer;
                }

                break;
            }

            if (inside)
            {
                IsVisible = true;
                if (CurrentLiquid != null)
                {
                    if (!player.Dead)
                    {
                        if (level.OnInterval(GetAirPercent() > 24f ? 1.5f : .75f))
                            level.Scheduler.QueueSourceEntity(new LiquidAirBubble(engine,
                                player.Data.Facing == -1 ? player.TopLeft : player.TopRight - new Vector2(4, 0),
                                CurrentLiquid));
                        if (GetAirPercent() < 4f * (100f / 15f) && !module.Features.IsControllingRemoteDrone &&
                            !ForceRechargeAir)
                        {
                            if (level.OnRawInterval(.06f)) Flashing = !Flashing;
                        }
                        else Flashing = false;
                    }

                    if (!ForceRechargeAir)
                    {
                        if (airRoutine.Active) airRoutine.Cancel();
                        if (player.CanRetry && !module.Features.IsControllingRemoteDrone) Air -= engine.DeltaTime;
                        if (Air <= 0f)
                        {
                            Air = 0f;
                            if (!player.Dead) player.Die(Vector2.Zero);
                        }
                    }
                    else if (!airRoutine.Active)
                    {
                        Flashing = false;
                        Add(airRoutine = new SimulationCoroutine(engine, RechargeAir(false)));
                    }
                }
            }
            else if (CurrentLiquid != null)
            {
                CurrentLiquid = null;
                Flashing = false;
                if (!airRoutine.Active) Add(airRoutine = new SimulationCoroutine(engine, RechargeAir()));
            }

            airRoutine.Update();
        }

        private IEnumerator RechargeAir(bool hideAtEnd = true)
        {
            Flashing = false;
            while (Air < CurrentMaxAir)
            {
                Air += engine.DeltaTime * CurrentMaxAir;
                yield return null;
            }

            Air = CurrentMaxAir;
            if (hideAtEnd)
            {
                yield return 1.5f;
                IsVisible = false;
            }
        }
    }
}
