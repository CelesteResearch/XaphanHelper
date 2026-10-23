// SPDX-License-Identifier: MIT

using Celeste.Mods.XaphanHelper.Entities.Slopes;
using Celeste.Scene.Tracking;
using Celeste.Simulation;

namespace Celeste.Mods.XaphanHelper.Entities.Slopes
{
    public sealed partial class PlayerPlatform
    {
        internal void BeforeSolidUpdate()
        {
            if (InView() &&
                ((!HasPlayerRider() && !CollidePlayer(FirstPlayer, Position + System.Numerics.Vector2.UnitY) &&
                  !UpsideDown) || preventCollision))
                Collidable = false;
        }

        internal bool PreventsRefillFor(RoomPlayerSession player)
            => InView() && GetPlayerRider() == player && ForceSlide && Sliding && PreventRefillOnSliding;
    }
}
