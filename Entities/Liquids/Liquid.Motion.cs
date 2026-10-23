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
        private IEnumerator ShakeLevel()
        {
            while (Level.Transitioning)
            {
                yield return null;
            }

            while (!Level.Paused && rising)
            {
                Level.DirectionalShake(new Vector2(0.5f, 0), 0.05f);
                Rumble();
                yield return 0.05f;
            }
        }

        public IEnumerator RiseRoutine()
        {
            if (Position != finalPosition)
            {
                if (riseShake)
                {
                    Add(new SimulationCoroutine(engine, ShakeLevel()));
                }

                while (Level.Transitioning)
                {
                    yield return null;
                }

                if (riseSound)
                {
                    RiseSoundSource = PlayAudio("event:/game/xaphan/liquid_rise");
                }

                while (riseDelay > 0)
                {
                    riseDelay -= engine.DeltaTime;
                    yield return null;
                }

                Player player = FirstPlayer();
                Vector2 start = Position;
                float coef = riseSpeed / 8;
                float num = Vector2.Distance(start, finalPosition) / 4f / coef;
                Tween tween = Tween.Create(engine.TweenPool, Tween.TweenMode.Oneshot,
                    ((ISourceEasingEnvironment)engine).EasingArithmetic.SineInOut, num, start: true);
                tween.OnUpdate = delegate(Tween t)
                {
                    if (player != null)
                    {
                        Position = Vector2.Lerp(start, finalPosition, t.Eased);
                    }
                };
                Add(tween);
                while (Position != finalPosition)
                {
                    if (LiquidType == "water" && Displacement != null)
                    {
                        RefreshDisplacementGrid();
                    }

                    if (Level.Transitioning)
                    {
                        tween.Stop();
                        StopRiseSound = true;
                        rising = false;
                        riseEnd = true;
                        break;
                    }

                    yield return null;
                }

                if (RiseSoundSource != null)
                {
                    RiseSoundSource.Stop(true);
                }

                rising = false;
                riseEnd = true;
            }
        }

        public void ReturnToOrigPosition()
        {
            Add(new SimulationCoroutine(engine, ReverseRiseRoutine()));
        }

        public IEnumerator ReverseRiseRoutine()
        {
            rising = true;
            Player player = FirstPlayer();
            Vector2 start = Position;
            float coef = riseSpeed * 10 / 8;
            float num = Vector2.Distance(start, startPosition) / 4f / coef;
            Tween tween = Tween.Create(engine.TweenPool, Tween.TweenMode.Oneshot,
                ((ISourceEasingEnvironment)engine).EasingArithmetic.SineInOut, num, start: true);
            tween.OnUpdate = delegate(Tween t)
            {
                if (player != null)
                {
                    Position = Vector2.Lerp(start, startPosition, t.Eased);
                }
            };
            Add(tween);
            while (Position != startPosition)
            {
                if (LiquidType == "water" && Displacement != null)
                {
                    RefreshDisplacementGrid();
                }

                if (Level.Transitioning)
                {
                    tween.Stop();
                    StopRiseSound = true;
                    rising = false;
                    riseEnd = true;
                    break;
                }

                yield return null;
            }

            if (RiseSoundSource != null)
            {
                RiseSoundSource.Stop(true);
            }

            rising = false;
            riseEnd = true;
        }

        public IEnumerator LevelBottomDelay()
        {
            float timer = 1.5f;
            while (timer > 0)
            {
                timer -= engine.DeltaTime;
                yield return null;
            }

            if (RiseSoundSource != null)
            {
                RiseSoundSource.Stop(true);
            }

            RemoveSelf();
        }

        public bool StopRiseSound;

        public override void Removed(object scene)
        {
            base.Removed(scene);
            if (RiseSoundSource != null)
            {
                RiseSoundSource.Stop(true);
            }
        }
    }
}
