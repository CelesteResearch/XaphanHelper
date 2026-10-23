// SPDX-License-Identifier: MIT

using System;
using System.Collections.Generic;
using System.Numerics;
using Collider = Celeste.Physics.Collision.Shapes.ColliderGeometry;
using ColliderList = Celeste.Physics.Collision.Shapes.ColliderListGeometry;
using Hitbox = Celeste.Physics.Collision.Shapes.HitboxColliderGeometry;

namespace Celeste.Mods.XaphanHelper.Geometry.Slopes
{
    public static class SlopeGeometry
    {
        public static SlopeCollisionShape CreateCollision(bool gentle, string side, int height, bool upsideDown,
            bool canJumpThrough, bool visualOnly)
        {
            Vector2 Position = Vector2.Zero;
            var lightOccludeBlocks = new List<SlopeLightOccluder>();
            ColliderList colliderList = null;
            Collider collider = new Hitbox(0, 0);
            if (!visualOnly)
            {
                if (!upsideDown)
                {
                    if (side == "Left")
                    {
                        colliderList = new ColliderList(new Hitbox(4, 1, 0, 0));
                        if (!canJumpThrough)
                        {
                            lightOccludeBlocks.Add(new SlopeLightOccluder(Position, 8, 2));
                        }

                        for (int i = 0; i <= height - 1; i++)
                        {
                            if (i > 0)
                            {
                                colliderList.Add(new Hitbox(4, 1, (gentle ? i * 16 : i * 8), i * 8));
                            }

                            colliderList.Add(new Hitbox(gentle ? 6 : 5, 1, (gentle ? i * 16 : i * 8), 1 + i * 8));
                            colliderList.Add(new Hitbox(gentle ? 8 : 6, 1, (gentle ? i * 16 : i * 8), 2 + i * 8));
                            colliderList.Add(new Hitbox(gentle ? 10 : 7, 1, (gentle ? i * 16 : i * 8), 3 + i * 8));
                            colliderList.Add(new Hitbox(gentle ? 12 : 8, 1, (gentle ? i * 16 : i * 8), 4 + i * 8));
                            colliderList.Add(new Hitbox(gentle ? 14 : 9, 1, (gentle ? i * 16 : i * 8), 5 + i * 8));
                            colliderList.Add(new Hitbox(gentle ? 16 : 10, 1, (gentle ? i * 16 : i * 8), 6 + i * 8));
                            colliderList.Add(new Hitbox(gentle ? 18 : 11, 1, (gentle ? i * 16 : i * 8), 7 + i * 8));
                            if (!canJumpThrough)
                            {
                                lightOccludeBlocks.Add(new SlopeLightOccluder(
                                    Position + new Vector2((gentle ? i * 16 : i * 8), 1 + i * 8), 4 + (gentle ? 6 : 5),
                                    2));
                                lightOccludeBlocks.Add(new SlopeLightOccluder(
                                    Position + new Vector2((gentle ? i * 16 : i * 8), 3 + i * 8), 4 + (gentle ? 10 : 7),
                                    2));
                                lightOccludeBlocks.Add(new SlopeLightOccluder(
                                    Position + new Vector2((gentle ? i * 16 : i * 8), 5 + i * 8), 4 + (gentle ? 14 : 9),
                                    2));
                                lightOccludeBlocks.Add(new SlopeLightOccluder(
                                    Position + new Vector2((gentle ? i * 16 : i * 8), 7 + i * 8),
                                    4 + (gentle ? 18 : 11), 2));
                            }
                        }

                        collider = colliderList;
                    }
                    else
                    {
                        colliderList = new ColliderList(new Hitbox(4, 1, 20, 0));
                        if (!canJumpThrough)
                        {
                            lightOccludeBlocks.Add(new SlopeLightOccluder(Position + new Vector2(16, 0), 8, 2));
                        }

                        for (int i = 0; i <= height - 1; i++)
                        {
                            if (i > 0)
                            {
                                colliderList.Add(new Hitbox(4, 1, 20 + (gentle ? i * -16 : i * -8), i * 8));
                            }

                            colliderList.Add(new Hitbox(gentle ? 6 : 5, 1, (gentle ? 18f - i * 16 : 19f - i * 8),
                                1 + i * 8));
                            colliderList.Add(new Hitbox(gentle ? 8 : 6, 1, (gentle ? 16f - i * 16 : 18f - i * 8),
                                2 + i * 8));
                            colliderList.Add(new Hitbox(gentle ? 10 : 7, 1, (gentle ? 14f - i * 16 : 17f - i * 8),
                                3 + i * 8));
                            colliderList.Add(new Hitbox(gentle ? 12 : 8, 1, (gentle ? 12f - i * 16 : 16f - i * 8),
                                4 + i * 8));
                            colliderList.Add(new Hitbox(gentle ? 14 : 9, 1, (gentle ? 10f - i * 16 : 15f - i * 8),
                                5 + i * 8));
                            colliderList.Add(new Hitbox(gentle ? 16 : 10, 1, (gentle ? 8f - i * 16 : 14f - i * 8),
                                6 + i * 8));
                            colliderList.Add(new Hitbox(gentle ? 18 : 11, 1, (gentle ? 6f - i * 16 : 13f - i * 8),
                                7 + i * 8));
                            if (!canJumpThrough)
                            {
                                lightOccludeBlocks.Add(new SlopeLightOccluder(
                                    Position + new Vector2((gentle ? 14f - i * 16 : 15f - i * 8), 1 + i * 8),
                                    4 + (gentle ? 6 : 5), 2));
                                lightOccludeBlocks.Add(new SlopeLightOccluder(
                                    Position + new Vector2((gentle ? 10f - i * 16 : 13f - i * 8), 3 + i * 8),
                                    4 + (gentle ? 10 : 7), 2));
                                lightOccludeBlocks.Add(new SlopeLightOccluder(
                                    Position + new Vector2((gentle ? 6f - i * 16 : 11f - i * 8), 5 + i * 8),
                                    4 + (gentle ? 14 : 9), 2));
                                lightOccludeBlocks.Add(new SlopeLightOccluder(
                                    Position + new Vector2((gentle ? 2f - i * 16 : 9f - i * 8), 7 + i * 8),
                                    4 + (gentle ? 18 : 11), 2));
                            }
                        }

                        collider = colliderList;
                    }
                }
                else
                {
                    if (side == "Left")
                    {
                        colliderList = new ColliderList(new Hitbox(8, 1, 0, 15));
                        if (!canJumpThrough)
                        {
                            lightOccludeBlocks.Add(new SlopeLightOccluder(Position + new Vector2(0, 14), 8, 2));
                        }

                        for (int i = 0; i <= height - 1; i++)
                        {
                            if (i > 0)
                            {
                                colliderList.Add(new Hitbox(8, 1, (gentle ? i * 16 : i * 8), 15 + i * -8));
                            }

                            colliderList.Add(new Hitbox(gentle ? 10 : 9, 1, (gentle ? i * 16 : i * 8), 7 + 7 - i * 8));
                            colliderList.Add(new Hitbox(gentle ? 12 : 10, 1, (gentle ? i * 16 : i * 8), 7 + 6 - i * 8));
                            colliderList.Add(new Hitbox(gentle ? 14 : 11, 1, (gentle ? i * 16 : i * 8), 7 + 5 - i * 8));
                            colliderList.Add(new Hitbox(gentle ? 16 : 12, 1, (gentle ? i * 16 : i * 8), 7 + 4 - i * 8));
                            colliderList.Add(new Hitbox(gentle ? 18 : 13, 1, (gentle ? i * 16 : i * 8), 7 + 3 - i * 8));
                            colliderList.Add(new Hitbox(gentle ? 20 : 14, 1, (gentle ? i * 16 : i * 8), 7 + 2 - i * 8));
                            colliderList.Add(new Hitbox(gentle ? 22 : 15, 1, (gentle ? i * 16 : i * 8), 7 + 1 - i * 8));
                            if (!canJumpThrough)
                            {
                                lightOccludeBlocks.Add(new SlopeLightOccluder(
                                    Position + new Vector2((gentle ? i * 16 : i * 8), 7 + 6 - i * 8), gentle ? 10 : 9,
                                    2));
                                lightOccludeBlocks.Add(new SlopeLightOccluder(
                                    Position + new Vector2((gentle ? i * 16 : i * 8), 7 + 4 - i * 8), gentle ? 14 : 11,
                                    2));
                                lightOccludeBlocks.Add(new SlopeLightOccluder(
                                    Position + new Vector2((gentle ? i * 16 : i * 8), 7 + 2 - i * 8), gentle ? 18 : 13,
                                    2));
                                lightOccludeBlocks.Add(new SlopeLightOccluder(
                                    Position + new Vector2((gentle ? i * 16 : i * 8), 7 + 0 - i * 8), gentle ? 22 : 15,
                                    2));
                            }
                        }

                        collider = colliderList;
                    }
                    else
                    {
                        colliderList = new ColliderList(new Hitbox(8, 1, 16, 15));
                        if (!canJumpThrough)
                        {
                            lightOccludeBlocks.Add(new SlopeLightOccluder(Position + new Vector2(16, 14), 8, 2));
                        }

                        for (int i = 0; i <= height - 1; i++)
                        {
                            if (i > 0)
                            {
                                colliderList.Add(new Hitbox(8, 1, 16 + (gentle ? i * -16 : i * -8), 15 + i * -8));
                            }

                            colliderList.Add(new Hitbox(gentle ? 10 : 9, 1, (gentle ? 14f - i * 16 : 15f - i * 8),
                                7 + 7 - i * 8));
                            colliderList.Add(new Hitbox(gentle ? 12 : 10, 1, (gentle ? 12f - i * 16 : 14f - i * 8),
                                7 + 6 - i * 8));
                            colliderList.Add(new Hitbox(gentle ? 14 : 11, 1, (gentle ? 10f - i * 16 : 13f - i * 8),
                                7 + 5 - i * 8));
                            colliderList.Add(new Hitbox(gentle ? 16 : 12, 1, (gentle ? 8f - i * 16 : 12f - i * 8),
                                7 + 4 - i * 8));
                            colliderList.Add(new Hitbox(gentle ? 18 : 13, 1, (gentle ? 6f - i * 16 : 11f - i * 8),
                                7 + 3 - i * 8));
                            colliderList.Add(new Hitbox(gentle ? 20 : 14, 1, (gentle ? 4f - i * 16 : 10f - i * 8),
                                7 + 2 - i * 8));
                            colliderList.Add(new Hitbox(gentle ? 22 : 15, 1, (gentle ? 2f - i * 16 : 9f - i * 8),
                                7 + 1 - i * 8));
                            if (!canJumpThrough)
                            {
                                lightOccludeBlocks.Add(new SlopeLightOccluder(
                                    Position + new Vector2((gentle ? 14f - i * 16 : 15f - i * 8), 7 + 6 - i * 8),
                                    gentle ? 10 : 9, 2));
                                lightOccludeBlocks.Add(new SlopeLightOccluder(
                                    Position + new Vector2((gentle ? 10f - i * 16 : 13f - i * 8), 7 + 4 - i * 8),
                                    gentle ? 14 : 11, 2));
                                lightOccludeBlocks.Add(new SlopeLightOccluder(
                                    Position + new Vector2((gentle ? 6f - i * 16 : 11f - i * 8), 7 + 2 - i * 8),
                                    gentle ? 18 : 13, 2));
                                lightOccludeBlocks.Add(new SlopeLightOccluder(
                                    Position + new Vector2((gentle ? 2f - i * 16 : 9f - i * 8), 7 + 0 - i * 8),
                                    gentle ? 22 : 15, 2));
                            }
                        }

                        collider = colliderList;
                    }
                }
            }

            return new SlopeCollisionShape(collider, lightOccludeBlocks, Math.Max(1, Math.Min(30, height)));
        }

        public static SlopePlayerPlatformLayout CreatePlayerPlatformLayout(Vector2 position, bool gentle,
            string side, int height, bool upsideDown)
        {
            Vector2 at = position + new Vector2(
                side == "Right" ? ((gentle ? -(height - 1) * 16 : -(height - 1) * 8) + 8) * (upsideDown ? -1 : 1) : 0,
                8 * (height - 1) + 4) * (upsideDown ? -1 : 1);
            int width = gentle ? 8 + 16 * height : 8 + 8 * height;
            return new SlopePlayerPlatformLayout(at, width, gentle ? (side == "Left" ? 0 : -8) : 0,
                upsideDown ? 4 : 8);
        }

        public static (Vector2 Top, Vector2 Bottom) GetSlopeEnds(Vector2 position, bool gentle,
            string side, int height, bool upsideDown)
        {
            if (side != "Left" && side != "Right") return (Vector2.Zero, Vector2.Zero);
            int step = gentle ? 16 : 8;
            if (!upsideDown)
                return side == "Left"
                    ? (position + new Vector2(7, 0), position + new Vector2(7 + step * height, 8 * height))
                    : (position + new Vector2(17, 0), position + new Vector2(17 - step * height, 8 * height));
            return side == "Left"
                ? (position + new Vector2(7 + step * height, -16), position + new Vector2(7, -16 + 8 * height))
                : (position + new Vector2(17 - step * height, -16), position + new Vector2(17, -16 + 8 * height));
        }
    }
}
