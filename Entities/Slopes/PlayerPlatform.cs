// SPDX-License-Identifier: MIT

using System;
using System.Collections;
using System.Numerics;
using Celeste.Core.Rendering;
using Celeste.Core.Scheduling;
using Celeste.Input.Gameplay;
using Celeste.Mods.XaphanHelper.Runtime.Slopes;
using Celeste.Physics.Collision;
using Celeste.Physics.Collision.Shapes;
using Celeste.Scene.Tracking;
using Celeste.Scene.Tracking.Attributes;
using Celeste.Simulation.Gameplay.Platforms.Foundation;
using Hitbox = Celeste.Physics.Collision.Shapes.HitboxColliderGeometry;
using Level = Celeste.Game.Execution.Levels.OriginalLevelSceneRoot;
using Player = Celeste.Simulation.RoomPlayerSession;

namespace Celeste.Mods.XaphanHelper.Entities.Slopes
{
    [SceneTracked(true)]
    public sealed partial class PlayerPlatform : SourceSolidEntityState
    {
        private readonly ISlopeGameplayFeatures features;
        private ISourceGameInput Input => ((ISourceGameInputEnvironment)SourceWorld.EngineContext).Input;

        private Player FirstPlayer =>
            ((ISourceScene)Scene).SourceScene.Entities.GetTrackedEntity<Player>(typeof(Player).FullName);

        private bool CollidePlayer(Player player) => player != null && SourceEntityCollision.Check(this, player);

        private bool CollidePlayer(Player player, Vector2 at) =>
            player != null && SourceEntityCollision.CheckAt(this, player, at);

        private Vector2 StartPosition;

        private Vector2 EndPosition;

        public string Side;

        public bool Gentle;

        private bool CanSlide;

        private bool ForceSlide;

        public bool UpsideDown;

        public bool StickyDash;

        public bool CanJumpThrough;

        private int SlopeHeight;

        public float platfromWidth;

        public float slopeTop;

        public bool AffectPlayerSpeed;

        public bool Sliding;

        public bool preventCollision;

        public string PlayerPose = "";

        public bool CollidableForPlayer;

        private bool PreventRefillOnSliding;

        public PlayerPlatform(Vector2 position, int width, bool gentle, string side, int soundIndex, int slopeHeight,
            bool canSlide, bool forceSlide, float top, bool affectPlayerSpeed, bool upsideDown, bool stickyDash,
            bool canJumpThrough, bool preventRefillOnSliding, ISourceSolidWorld world,
            ISlopeGameplayFeatures features) : base(position, width, 4, true, world)
        {
            this.features = features;
            AllowStaticMovers = false;
            Gentle = gentle;
            Side = side;
            UpsideDown = upsideDown;
            Collider = new Hitbox(width, 4, Gentle ? (Side == "Left" ? 0 : -8) : 0, UpsideDown ? 4 : 8);
            SurfaceSoundIndex = soundIndex;
            SlopeHeight = slopeHeight;
            platfromWidth = width;
            CanSlide = canSlide;
            ForceSlide = forceSlide;
            slopeTop = top;
            AffectPlayerSpeed = affectPlayerSpeed;
            StickyDash = stickyDash;
            CanJumpThrough = canJumpThrough;
            PreventRefillOnSliding = preventRefillOnSliding;
        }

        public override void MoveVExact(int move)
        {
            Player player = FirstPlayer;
            if (!UpsideDown)
            {
                if (player != null)
                {
                    if (move < 0)
                    {
                        if (player.IsRiding(this))
                        {
                            Collidable = false;
                            if (player.TreatNaive)
                            {
                                player.NaiveMove(Vector2.UnitY * move);
                            }
                            else
                            {
                                player.MoveVExact(move);
                            }

                            Collidable = true;
                        }
                        else if (!player.TreatNaive && CollidePlayer(player, Position + Vector2.UnitY * move) &&
                                 !CollidePlayer(player))
                        {
                            Collidable = false;
                            player.MoveVExact((int)(Top + move - player.Bottom));
                            Collidable = true;
                        }
                    }
                    else
                    {
                        if ((player.IsRiding(this) && (StickyDash || player.Machine.State != 2)))
                        {
                            Collidable = false;
                            if (player.TreatNaive)
                            {
                                player.NaiveMove(Vector2.UnitY * move);
                            }
                            else
                            {
                                player.MoveVExact(move);
                            }

                            Collidable = true;
                        }
                    }
                }

                Y += move;
                MoveStaticMovers(Vector2.UnitY * move);
            }
            else
            {
                base.MoveVExact(move);
            }
        }

        public override void Added(object scene)
        {
            base.Added(scene);
            StartPosition = Position;
        }

        public override void Update()
        {
            base.Update();
            PlayerPose = "";
            Player player = FirstPlayer;
            if (features.DroneDisablesPlayerCollision)
                Collidable = false;
            if (player != null)
            {
                if (Sliding && (player.Sprite.CurrentAnimationID != "duck" || player.Data.Motion.Normal.Speed.X == 0 ||
                                Input.Jump.Pressed))
                {
                    Sliding = false;
                }

                if (player.Right <= Left - 16 || player.Left >= Right + 16)
                {
                    Position = StartPosition;
                }
                else
                {
                    if (player.Right <= Left || player.Left >= Right)
                    {
                        Position = StartPosition;
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

                    if ((player.Sprite.Rate != 2 ||
                         (player.Sprite.Rate == 2 && player.Sprite.CurrentAnimationID == "wakeUp")))
                    {
                        if (!UpsideDown)
                        {
                            if (features.IsControllingRemoteDrone && CollidePlayer(player))
                            {
                                player.MoveToY(player.Position.Y - 1);
                            }

                            if (player.Bottom > StartPosition.Y + 4)
                            {
                                Collidable = false;
                            }
                            else
                            {
                                SetCollision(player);
                            }

                            if (Side == "Left")
                            {
                                if (CanSlide && !features.HasDrone && player.IsRiding(this) &&
                                    (ForceSlide || (Input.MoveY.Value == 1 && Input.MoveX.Value != -1)) &&
                                    player.Left >= Left && !features.UiOpened)
                                {
                                    Sliding = true;
                                    PlayerPose = "XaphanHelper_slopeSlide";
                                    player.Sprite.Scale = new Vector2(1f, 1f);
                                    player.Data.Motion.Ducking = false;
                                    if (player.Facing != 1)
                                    {
                                        player.Data.Facing = 1;
                                    }

                                    if (player.Hair.Facing != 1)
                                    {
                                        player.Hair.Facing = 1;
                                    }

                                    if (player.Speed.X <= 250f - (Gentle ? 15f : 18f))
                                    {
                                        player.Data.Motion.Normal.Speed.X +=
                                            ForceSlide ? (Gentle ? 30f : 28f) : (Gentle ? 15f : 18f);
                                    }
                                    else
                                    {
                                        player.Data.Motion.Normal.Speed.X = 250f;
                                    }
                                }

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
                                    Add(new SimulationCoroutine(SourceWorld.EngineContext, MoveSlope()));
                                }
                            }
                            else if (Side == "Right")
                            {
                                if (CanSlide && !features.HasDrone && player.IsRiding(this) &&
                                    (ForceSlide || (Input.MoveY.Value == 1 && Input.MoveX.Value != 1)) &&
                                    player.Right <= Right && !features.UiOpened)
                                {
                                    Sliding = true;
                                    PlayerPose = "XaphanHelper_slopeSlide";
                                    player.Sprite.Scale = new Vector2(1f, 1f);
                                    player.Data.Motion.Ducking = false;
                                    if (player.Facing != -1)
                                    {
                                        player.Data.Facing = -1;
                                    }

                                    if (player.Hair.Facing != -1)
                                    {
                                        player.Hair.Facing = -1;
                                    }

                                    if (player.Speed.X >= -250f + (Gentle ? 15f : 18f))
                                    {
                                        player.Data.Motion.Normal.Speed.X -=
                                            ForceSlide ? (Gentle ? 30f : 28f) : (Gentle ? 15f : 18f);
                                    }
                                    else
                                    {
                                        player.Data.Motion.Normal.Speed.X = -250f;
                                    }
                                }

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
                                    Add(new SimulationCoroutine(SourceWorld.EngineContext, MoveSlope()));
                                }
                            }
                        }
                        else if (!((Level)Scene).Session.GetFlag("Xaphan_Helper_Ceiling"))
                        {
                            if (features.IsControllingRemoteDrone && CollidePlayer(player))
                            {
                                player.MoveToY(player.Position.Y + 1);
                            }

                            if (player.Top < StartPosition.Y + 12)
                            {
                                Collidable = false;
                            }
                            else
                            {
                                SetCollision(player);
                            }

                            if (Side == "Left")
                            {
                                if (player.BottomCenter.X < Right && player.BottomCenter.X > Left + 7 &&
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
                                    Add(new SimulationCoroutine(SourceWorld.EngineContext, MoveSlope()));
                                }

                                if (player.BottomCenter.X < Left + 7)
                                {
                                    Y = StartPosition.Y + SlopeHeight * 8 + 4;
                                }
                            }
                            else if (Side == "Right")
                            {
                                if (player.BottomCenter.X > Left && player.BottomCenter.X < Right - 7 &&
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
                                    Add(new SimulationCoroutine(SourceWorld.EngineContext, MoveSlope()));
                                }

                                if (player.BottomCenter.X > Right - 7)
                                {
                                    Y = StartPosition.Y + SlopeHeight * 8 + 4;
                                }
                            }
                        }
                    }

                    if (Collidable && !((Level)Scene).Session.GetFlag("Xaphan_Helper_Ceiling"))
                    {
                        if (!UpsideDown && CollidePlayer(FirstPlayer))
                        {
                            player.Position -= Vector2.UnitY;
                        }

                        if (UpsideDown && CollidePlayer(FirstPlayer))
                        {
                            player.Position += Vector2.UnitY;
                        }
                    }
                }

                CollidableForPlayer = Collidable;
            }
        }

        public IEnumerator MoveSlope()
        {
            if (!((Level)Scene).Session.GetFlag("Xaphan_Helper_Ceiling"))
            {
                MoveToY(Math.Max(EndPosition.Y, StartPosition.Y - 8 * SlopeHeight - 4), 0);
            }

            yield return null;
        }

        public void TurnOffCollision(bool state)
        {
            preventCollision = state;
        }

        public bool InView()
        {
            var camera = ((Level)Scene).Camera;
            if (base.X > camera.X - 16f && base.Y > camera.Y - 16f && base.X < camera.X + 320f + 16f)
            {
                return base.Y < camera.Y + 180f + 16f;
            }

            return false;
        }

        public void SetCollision(Player player)
        {
            if (!preventCollision)
            {
                if (player != null)
                {
                    if (!UpsideDown)
                    {
                        if (CanJumpThrough)
                        {
                            if (player.Bottom <= Top + 2)
                            {
                                Collidable = true;
                            }
                            else
                            {
                                Collidable = false;
                            }
                        }
                        else
                        {
                            Collidable = true;
                        }
                    }
                    else
                    {
                        if (CanJumpThrough)
                        {
                            if (player.Top >= Bottom)
                            {
                                Collidable = true;
                            }
                            else
                            {
                                Collidable = false;
                            }
                        }
                        else
                        {
                            Collidable = true;
                        }
                    }
                }
                else
                {
                    Collidable = false;
                }
            }
            else
            {
                Collidable = false;
            }
        }
        // Remove debug render

        public override void DebugRender(ISourceRenderContext context, object camera)
        {
            if (features.ShowCompleteSlopesHitboxes)
            {
                base.DebugRender(context, camera);
            }
        }

        public void RestoreCollisionForPlayer()
        {
            Collidable = CollidableForPlayer;
        }
    }
}
