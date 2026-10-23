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
        private IEnumerator Controller()
        {
            while (true)
            {
                triggered = false;
                state = MovementState.Idling;
                while (!triggered && !HasPlayerRider())
                {
                    yield return null;
                }

                ((ISourceSpatialAudioEnvironment)Engine).SpatialAudio.Play(
                    "event:/game/04_cliffside/arrowblock_activate", Position);
                state = MovementState.Moving;
                StartShaking(0.2f);
                ActivateParticles();
                yield return 0.2f;
                targetSpeed = speed;
                moveSfx.Play("event:/game/04_cliffside/arrowblock_move");
                moveSfx.Param("arrow_stop", 0f);
                StopPlayerRunIntoAnimation = false;
                float crashTimer = 0.15f;
                float crashResetTimer = 0.1f;
                float noSteerTimer = 0.2f;
                while (true)
                {
                    if (steerSides != "None")
                    {
                        targetAngle = homeAngle;
                        bool playerLeft = CheckTracked<RoomPlayerSession>(Position + Vector2.UnitX * -1f);
                        bool playerRight = CheckTracked<RoomPlayerSession>(Position + Vector2.UnitX);
                        bool playerBottom =
                            CheckTracked<RoomPlayerSession>(Position +
                                                            Vector2.UnitY * (6f + (float)magneticCeilingOffset));
                        bool ceilingFlag = ((OriginalLevelSceneRoot)Scene).Session.GetFlag("Xaphan_Helper_Ceiling");
                        bool flag = ((direction != Directions.Up && direction != Directions.Down)
                            ? ((!(steerSides == "TopOnly"))
                                ? ((steerSides == "BottomOnly")
                                    ? (playerBottom && ceilingFlag)
                                    : (steerSides == "TopAndBottom" &&
                                       (HasPlayerOnTop() || (playerBottom && ceilingFlag))))
                                : (HasPlayerOnTop() && !ceilingFlag))
                            : ((!(steerSides == "LeftOnly"))
                                ? ((!(steerSides == "RightOnly"))
                                    ? (steerSides == "LeftAndRight" && HasPlayerClimbing())
                                    : (playerRight && HasPlayerClimbing()))
                                : (playerLeft && HasPlayerClimbing())));
                        if (flag && noSteerTimer > 0f)
                        {
                            noSteerTimer -= Engine.DeltaTime;
                        }

                        if (flag)
                        {
                            if (noSteerTimer <= 0f)
                            {
                                if (direction == Directions.Right || direction == Directions.Left)
                                {
                                    targetAngle = homeAngle + MathF.PI / 4f * (float)angleSteerSign *
                                        (float)((ISourceGameInputEnvironment)Engine).Input.MoveY.Value;
                                }
                                else
                                {
                                    targetAngle = homeAngle + MathF.PI / 4f * (float)angleSteerSign *
                                        (float)((ISourceGameInputEnvironment)Engine).Input.MoveX.Value;
                                }
                            }
                        }
                        else
                        {
                            noSteerTimer = 0.2f;
                        }
                    }

                    if (Interval(0.02f))
                    {
                        MoveParticles();
                    }

                    currentSpeed = ScalarMath.Approach(currentSpeed, targetSpeed, acceleration * Engine.DeltaTime);
                    angle = ScalarMath.Approach(angle, targetAngle, MathF.PI * 16f * Engine.DeltaTime);
                    Vector2 vector = Vector2Geometry.AngleToVector(angle, currentSpeed);
                    Vector2 vec = vector * Engine.DeltaTime;
                    bool flag2;
                    if (direction == Directions.Right || direction == Directions.Left)
                    {
                        flag2 = MoveCheck(new Vector2(vec.X, 0));
                        noSquish = ((ISourceScene)Scene).SourceScene.Entities.GetTrackedEntity<RoomPlayerSession>();
                        MoveVCollideSolids(vec.Y, thruDashBlocks: false);
                        noSquish = null;
                        LiftSpeed = vector;
                        if (Interval(0.03f))
                        {
                            if (vec.Y > 0f)
                            {
                                ScrapeParticles(Vector2.UnitY);
                            }
                            else if (vec.Y < 0f)
                            {
                                ScrapeParticles(-Vector2.UnitY);
                            }
                        }
                    }
                    else
                    {
                        flag2 = MoveCheck(new Vector2(0, vec.Y));
                        noSquish = ((ISourceScene)Scene).SourceScene.Entities.GetTrackedEntity<RoomPlayerSession>();
                        MoveHCollideSolids(vec.X, thruDashBlocks: false);
                        noSquish = null;
                        LiftSpeed = vector;
                        if (Interval(0.03f))
                        {
                            if (vec.X > 0f)
                            {
                                ScrapeParticles(Vector2.UnitX);
                            }
                            else if (vec.X < 0f)
                            {
                                ScrapeParticles(-Vector2.UnitX);
                            }
                        }

                        if (direction == Directions.Down &&
                            Top > (float)(((OriginalLevelSceneRoot)Scene).Bounds.Bottom + 32))
                        {
                            flag2 = true;
                        }
                    }

                    if (flag2)
                    {
                        moveSfx.Param("arrow_stop", 1f);
                        crashResetTimer = 0.1f;
                        if (!(crashTimer > 0f))
                        {
                            break;
                        }

                        crashTimer -= Engine.DeltaTime;
                    }
                    else
                    {
                        moveSfx.Param("arrow_stop", 0f);
                        if (crashResetTimer > 0f)
                        {
                            crashResetTimer -= Engine.DeltaTime;
                        }
                        else
                        {
                            crashTimer = 0.15f;
                        }
                    }

                    OriginalLevelSceneRoot level = Scene as OriginalLevelSceneRoot;
                    if (Left < (float)level.Bounds.Left || Top < (float)level.Bounds.Top ||
                        Right > (float)level.Bounds.Right)
                    {
                        break;
                    }

                    yield return null;
                }

                if (!oneUse)
                {
                    outlineFader.Replace(OutlineFade(1f));
                }

                ((ISourceSpatialAudioEnvironment)Engine).SpatialAudio.Play("event:/game/04_cliffside/arrowblock_break",
                    Position);
                moveSfx.Stop();
                state = MovementState.Breaking;
                currentSpeed = (targetSpeed = 0f);
                angle = (targetAngle = homeAngle);
                StartShaking(0.2f);
                StopPlayerRunIntoAnimation = true;
                yield return 0.2f;
                BreakParticles();
                List<Debris> debris = new List<Debris>();
                for (int i = 0; (float)i < Width; i += 8)
                {
                    for (int j = 0; (float)j < Height; j += 8)
                    {
                        Vector2 vector2 = new Vector2((float)i + 4f, (float)j + 4f);
                        Debris debris2 = Engine.EntityPools.Create<Debris>().Init(Position + vector2, Center,
                            startPosition + vector2, directory);
                        debris.Add(debris2);
                        ((ISourceScene)Scene).SourceScene.QueueSourceEntity(debris2);
                    }
                }

                CustomMoveBlock moveBlock = this;
                Vector2 amount = startPosition - Position;
                DisableStaticMovers();
                moveBlock.MoveStaticMovers(amount);
                Position = startPosition;
                Collidable = false;
                Destroyed = true;
                if (!oneUse)
                {
                    respawnTimer = respawnTime;
                    while (respawnTimer > 0f)
                    {
                        respawnTimer -= Engine.DeltaTime;
                        yield return null;
                    }

                    while (CheckTracked<SourceActorEntityState>() || CheckTracked<SourceSolidEntityState>())
                    {
                        yield return null;
                    }

                    Collidable = true;
                    outlineFader.Replace(OutlineFade(0f));
                    ISourceAudioEventReference instance =
                        ((ISourceSpatialAudioEnvironment)Engine).SpatialAudio.Play(
                            "event:/game/04_cliffside/arrowblock_reform_begin", debris[0].Position);
                    CustomMoveBlock moveBlock2 = this;
                    SimulationCoroutine coroutine;
                    SimulationCoroutine component = (coroutine =
                        new SimulationCoroutine(Engine, SoundFollowsDebrisCenter(instance, debris)));
                    SimulationCoroutine routine = coroutine;
                    moveBlock2.Add(component);
                    routine.RemoveSelf();
                    ((ISourceSpatialAudioEnvironment)Engine).SpatialAudio.Play(
                        "event:/game/04_cliffside/arrowblock_reappear", Position);
                    Destroyed = false;
                    EnableStaticMovers();
                    currentSpeed = (targetSpeed = 0f);
                    angle = (targetAngle = homeAngle);
                    noSquish = null;
                    fillColor = idleBgFill;
                    UpdateColors();
                    flash = 1f;
                }
                else
                {
                    RemoveSelf();
                }
            }
        }

        private IEnumerator SoundFollowsDebrisCenter(ISourceAudioEventReference instance, List<Debris> debris)
        {
            while (true)
            {
                var pLAYBACK_STATE = instance.PlaybackState;
                if (pLAYBACK_STATE == SourceAudioPlaybackState.Stopped)
                {
                    break;
                }

                Vector2 zero = Vector2.Zero;
                foreach (Debris debri in debris)
                {
                    zero += debri.Position;
                }

                zero /= (float)debris.Count;
                ((ISourceSpatialAudioEnvironment)Engine).SpatialAudio.Position(instance, zero);
                yield return null;
            }
        }

        private IEnumerator OutlineFade(float to)
        {
            float from = 1f - to;
            for (float t = 0f; t < 1f; t += Engine.DeltaTime * 2f)
            {
                outlineColorStrength =
                    from + (to - from) * ((ISourceEasingEnvironment)Engine).EasingArithmetic.CubeInOut(t);
                yield return null;
            }
        }
    }
}
