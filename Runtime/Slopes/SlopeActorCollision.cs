// SPDX-License-Identifier: MIT

using System.Numerics;
using Celeste.Mods.XaphanHelper.Entities.Slopes;
using Celeste.Physics.Collision;
using Celeste.Scene.Entities;
using Celeste.Scene.Tracking;

namespace Celeste.Mods.XaphanHelper.Runtime.Slopes
{
    public static class SlopeActorCollision
    {
        public static void BeforeUpdate(SceneEntityState actor)
        {
            var registry = ((ISourceScene)actor.Scene).SourceScene.Entities;
            foreach (Slope slope in registry.GetTrackedEntities(typeof(Slope)))
            {
                if (SourceEntityCollision.Check(slope, actor)) slope.Collidable = true;
                else if (slope.CanJumpThrough)
                {
                    Vector2 point = !slope.UpsideDown ? actor.BottomCenter :
                        slope.Side == "Right" ? actor.TopRight : actor.TopLeft;
                    float cross = (slope.SlopeBottom.X - slope.SlopeTop.X) * (point.Y - slope.SlopeTop.Y)
                                  - (slope.SlopeBottom.Y - slope.SlopeTop.Y) * (point.X - slope.SlopeTop.X);
                    if ((slope.Side == "Right" && cross >= 0) || (slope.Side == "Left" && cross <= 0))
                        slope.Collidable = true;
                }
                else if (!slope.UpsideDown ||
                         (slope.Side == "Right" && actor.Right < slope.Right) ||
                         (slope.Side == "Left" && actor.Left > slope.Left)) slope.Collidable = true;
            }

            foreach (PlayerPlatform platform in registry.GetTrackedEntities(typeof(PlayerPlatform)))
                platform.Collidable = false;
        }

        public static void AfterUpdate(SceneEntityState actor)
        {
            var registry = ((ISourceScene)actor.Scene).SourceScene.Entities;
            foreach (Slope slope in registry.GetTrackedEntities(typeof(Slope))) slope.Collidable = false;
            foreach (PlayerPlatform platform in registry.GetTrackedEntities(typeof(PlayerPlatform)))
                platform.RestoreCollisionForPlayer();
        }

        public static void ResolveUpsideDown(SceneEntityState actor)
        {
            foreach (Slope slope in ((ISourceScene)actor.Scene).SourceScene.Entities.GetTrackedEntities(typeof(Slope)))
                if (slope.UpsideDown && SourceEntityCollision.Check(actor, slope))
                    actor.Position += Vector2.UnitY;
        }
    }
}
