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
        [RuntimeTypeAlias("Celeste.Mod.XaphanHelper.Entities.CustomMoveBlock+Debris")]
        private class Debris : SourceActorEntityState
        {
            private SpriteImage sprite;

            private Vector2 speed;

            private bool shaking;

            private bool firstHit;

            private float alpha;

            private SourceCollisionHandler onCollideH;

            private SourceCollisionHandler onCollideV;

            private float spin;

            private float lifeTimer;

            private float fadeLerp;

            private string directory;
            private readonly ISourceEngineContext Engine;

            private bool Interval(float interval) =>
                ((ISourceScene)Scene).SourceScene.Clock.OnInterval(interval, Engine);

            private ISourceRandom Random => ((ISourceRandomEnvironment)Engine).Random;

            public Debris(ISourceEngineContext engine, IPlatformTargetMotionPolicy motion)
                : base(new PixelActor(null, Vector2.Zero), engine, motion)
            {
                Engine = engine;
                Tags =
                    ((Celeste.Scene.Scheduling.ExecutionTags.ISourcePersistentTransitionExecutionTagsEnvironment)Engine)
                    .PersistentTransitionExecutionTags.TransitionUpdate.Value;
                base.Collider = new HitboxColliderGeometry(4f, 4f, -2f, -2f);
                onCollideH =
                    (PixelCollision _) => { speed.X = (0f - speed.X) * 0.5f; };
                onCollideV =
                    (PixelCollision _) =>
                    {
                        if (firstHit || speed.Y > 50f)
                        {
                            ((ISourceSpatialAudioEnvironment)Engine).SpatialAudio.Play(
                                "event:/game/general/debris_stone", Position, "debris_velocity",
                                ScalarMath.ClampedMap(speed.Y, 0f, 600f));
                        }

                        if (speed.Y > 0f && speed.Y < 40f)
                        {
                            speed.Y = 0f;
                        }
                        else
                        {
                            speed.Y = (0f - speed.Y) * 0.25f;
                        }

                        firstHit = false;
                    };
            }

            protected override void OnSquish(PixelCollision data)
            {
            }

            public Debris Init(Vector2 position, Vector2 center, Vector2 returnTo, string directory)
            {
                Collidable = true;
                Position = position;
                speed = ((ISourceVectorEnvironment)Engine).VectorArithmetic.SafeNormalize(position - center,
                    60f + SourceRandomMath.NextFloat(Random, 60f));
                this.directory = directory;
                Add(sprite = new SpriteImage(RandomSelection.Choose(Random,
                    ((ISourceGameGraphicsEnvironment)Engine).GameGraphics.GameAtlas.GetAtlasSubtextures(directory +
                        "/debris"))));
                sprite.CenterOrigin();
                sprite.FlipX = SourceRandomMath.NextFloat(Random) < .5f;
                sprite.Position = Vector2.Zero;
                sprite.Rotation = SourceRandomMath.NextAngle(Random);
                shaking = false;
                sprite.Scale.X = 1f;
                sprite.Scale.Y = 1f;
                sprite.Color = PackedColor.White;
                alpha = 1f;
                firstHit = false;
                spin = SourceRandomMath.Range(Random, 3.4906585f, 10.471975f) *
                       (float)RandomSelection.Choose(Random, 1, -1);
                fadeLerp = 0f;
                lifeTimer = SourceRandomMath.Range(Random, 0.6f, 2.6f);
                return this;
            }

            public override void Update()
            {
                SlopeActorCollision.BeforeUpdate(this);
                base.Update();
                if (Collidable)
                {
                    speed.X = ScalarMath.Approach(speed.X, 0f, Engine.DeltaTime * 100f);
                    if (!OnGround())
                    {
                        speed.Y += 400f * Engine.DeltaTime;
                    }

                    MoveH(speed.X * Engine.DeltaTime, onCollideH);
                    MoveV(speed.Y * Engine.DeltaTime, onCollideV);
                }

                if (shaking && Interval(0.05f))
                {
                    sprite.Position.X = -1 + Random.Next(3);
                    sprite.Position.Y = -1 + Random.Next(3);
                }

                if ((base.Scene as OriginalLevelSceneRoot).Transitioning)
                {
                    alpha = ScalarMath.Approach(alpha, 0f, Engine.DeltaTime * 4f);
                    sprite.Color = PackedColor.White * alpha;
                }

                sprite.Rotation += spin * ScalarMath.ClampedMap(Math.Abs(speed.Y), 50f, 150f) * Engine.DeltaTime;
                if (lifeTimer > 0f)
                {
                    lifeTimer -= Engine.DeltaTime;
                }
                else if (alpha > 0f)
                {
                    alpha -= 4f * Engine.DeltaTime;
                    if (alpha <= 0f)
                    {
                        RemoveSelf();
                    }
                }

                if (fadeLerp < 1f)
                {
                    fadeLerp = ScalarMath.Approach(fadeLerp, 1f, 2f * Engine.DeltaTime);
                }

                sprite.Color = PackedColor.Lerp(PackedColor.White, PackedColor.Gray, fadeLerp) * alpha;
                SlopeActorCollision.AfterUpdate(this);
            }
        }
    }
}
