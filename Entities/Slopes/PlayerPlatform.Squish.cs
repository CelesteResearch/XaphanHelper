// SPDX-License-Identifier: MIT

using System.Numerics;
using Celeste.Physics.Collision;

namespace Celeste.Mods.XaphanHelper.Entities.Slopes
{
    public sealed partial class PlayerPlatform : IPusherSquishWigglePolicy
    {
        public bool TryHandleSquishWiggle(PixelActor actor, PixelCollision collision, int wiggleX, int wiggleY,
            out bool result)
        {
            result = true;
            if (!UpsideDown) return false;
            actor.Position = new Vector2(actor.Position.X, Bottom - actor.Hitbox.Y);
            return true;
        }
    }
}
