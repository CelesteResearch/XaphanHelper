// SPDX-License-Identifier: MIT

using System;
using System.Collections;
using System.Numerics;
using Celeste.Core.Rendering;
using Celeste.Core.Scheduling;
using Celeste.Mods.XaphanHelper.Runtime.Slopes;
using Celeste.Physics.Collision;
using Celeste.Scene.Tracking.Attributes;
using Celeste.Simulation.Gameplay.Actors.Foundation;
using Celeste.Simulation.Gameplay.Platforms.Foundation;
using Hitbox = Celeste.Physics.Collision.Shapes.HitboxColliderGeometry;
using Level = Celeste.Game.Execution.Levels.OriginalLevelSceneRoot;

namespace Celeste.Mods.XaphanHelper.Entities.Slopes
{
    [SceneTracked(true)]
    public sealed class FakePlayerPlatform : SourceSolidEntityState
    {
        private readonly ISlopeGameplayFeatures features;
        private Vector2 StartPosition;

        private Vector2 EndPosition;

        public string Side;

        private bool Gentle;

        public bool UpsideDown;

        public bool StickyDash;

        public bool CanJumpThrough;

        private int SlopeHeight;

        public float platfromWidth;

        public float slopeTop;

        public FakePlayerPlatform(Vector2 position, int width, bool gentle, string side, int soundIndex,
            int slopeHeight, float top, bool upsideDown, bool stickyDash, bool canJumpThrough, ISourceSolidWorld world,
            ISlopeGameplayFeatures features) : base(position, width, 4, true, world)
        {
            this.features = features;
            AllowStaticMovers = false;
            Gentle = gentle;
            Side = side;
            UpsideDown = upsideDown;
            Collidable = false;
            Collider = new Hitbox(width, 4, Gentle ? (Side == "Left" ? 0 : -8) : 0, UpsideDown ? 4 : 8);
            SurfaceSoundIndex = soundIndex;
            SlopeHeight = slopeHeight;
            platfromWidth = width;
            slopeTop = top;
            StickyDash = stickyDash;
            CanJumpThrough = canJumpThrough;
        }

        public override void Added(object scene)
        {
            base.Added(scene);
            StartPosition = Position;
        }

        public override void Update()
        {
            base.Update();
            var player = features.FakePlayer;
            if (player != null)
            {
                if (player.Right <= Left - 16 || player.Left >= Right + 16)
                {
                    Position = StartPosition;
                    return;
                }

                if (!UpsideDown)
                {
                    if (Position.Y > StartPosition.Y)
                    {
                        Y = StartPosition.Y;
                    }
                }
                else
                {
                    if (Position.Y < StartPosition.Y)
                    {
                        Y = StartPosition.Y;
                    }
                }

                if ((features.FakePlayerSprite.Rate != 2 || (features.FakePlayerSprite.Rate == 2 &&
                                                             features.FakePlayerSprite.CurrentAnimationID == "wakeUp")))
                {
                    Collidable = false;
                    if (!UpsideDown)
                    {
                        if (features.IsControllingRemoteDrone && SourceEntityCollision.Check(this, player))
                        {
                            player.MoveToY(player.Position.Y - 1);
                        }

                        if (Side == "Left")
                        {
                            if (player.BottomCenter.X < Right + 16 &&
                                Position.Y >= StartPosition.Y - 8 * SlopeHeight - 4)
                            {
                                EndPosition = new Vector2(StartPosition.X,
                                    StartPosition.Y - (Right - (Gentle ? -4 : 0) - player.BottomCenter.X +
                                                       (((features.UseMetroidGameplay && features.Shinesparking) ||
                                                         (!features.UseMetroidGameplay &&
                                                          ((Level)Scene).Session.GetFlag(
                                                              "Xaphan_Helper_Shinesparking")))
                                                           ? 16f
                                                           : 4f)) / (Gentle ? 2 : 1));
                                Add(new SimulationCoroutine(SourceWorld.EngineContext, MoveSlope(player)));
                            }
                        }
                        else if (Side == "Right")
                        {
                            if (player.BottomCenter.X > Left - 16 &&
                                Position.Y >= StartPosition.Y - 8 * SlopeHeight - 4)
                            {
                                EndPosition = new Vector2(StartPosition.X,
                                    StartPosition.Y + (Left + (Gentle ? -4 : 0) - player.BottomCenter.X -
                                                       (((features.UseMetroidGameplay && features.Shinesparking) ||
                                                         (!features.UseMetroidGameplay &&
                                                          ((Level)Scene).Session.GetFlag(
                                                              "Xaphan_Helper_Shinesparking")))
                                                           ? 16f
                                                           : 4f)) / (Gentle ? 2 : 1));
                                Add(new SimulationCoroutine(SourceWorld.EngineContext, MoveSlope(player)));
                            }
                        }

                        if (player.Top > StartPosition.Y && !features.FakePlayerIsDucking)
                        {
                            Y = StartPosition.Y;
                        }
                    }
                    else
                    {
                        if (features.IsControllingRemoteDrone && SourceEntityCollision.Check(this, player))
                        {
                            player.MoveToY(player.Position.Y + 1);
                        }

                        if (Side == "Left")
                        {
                            if (player.BottomCenter.X < Right && player.BottomCenter.X > Left &&
                                Position.Y >= StartPosition.Y - 8 * SlopeHeight - 4)
                            {
                                EndPosition = new Vector2(StartPosition.X,
                                    StartPosition.Y - (Right - (Gentle ? -4 : 0) - player.BottomCenter.X +
                                                       (((features.UseMetroidGameplay && features.Shinesparking) ||
                                                         (!features.UseMetroidGameplay &&
                                                          ((Level)Scene).Session.GetFlag(
                                                              "Xaphan_Helper_Shinesparking")))
                                                           ? (Gentle ? 16f : 8f)
                                                           : (Gentle ? 8f : 4f))) / (Gentle ? 2 : 1) * -1 -
                                    (Gentle ? 2 : 0));
                                Add(new SimulationCoroutine(SourceWorld.EngineContext, MoveSlope(player)));
                            }
                        }
                        else if (Side == "Right")
                        {
                            if (player.BottomCenter.X > Left && player.BottomCenter.X < Right &&
                                Position.Y >= StartPosition.Y - 8 * SlopeHeight - 4)
                            {
                                EndPosition = new Vector2(StartPosition.X,
                                    StartPosition.Y + (Left + (Gentle ? -4 : 0) - player.BottomCenter.X -
                                                       (((features.UseMetroidGameplay && features.Shinesparking) ||
                                                         (!features.UseMetroidGameplay &&
                                                          ((Level)Scene).Session.GetFlag(
                                                              "Xaphan_Helper_Shinesparking")))
                                                           ? (Gentle ? 16f : 8f)
                                                           : (Gentle ? 8f : 4f))) / (Gentle ? 2 : 1) * -1 -
                                    (Gentle ? 2 : 0));
                                Add(new SimulationCoroutine(SourceWorld.EngineContext, MoveSlope(player)));
                            }
                        }

                        if (Position.Y > StartPosition.Y + SlopeHeight * 8 + 4)
                        {
                            Y = StartPosition.Y + SlopeHeight * 8 + 4;
                        }
                    }
                }
            }
        }

        public IEnumerator MoveSlope(SourceActorEntityState player)
        {
            MoveToY(Math.Max(EndPosition.Y, StartPosition.Y - 8 * SlopeHeight - 4), 0);
            yield return null;
        }

        // Remove debug render

        public override void DebugRender(ISourceRenderContext context, object camera)
        {
        }
    }
}
