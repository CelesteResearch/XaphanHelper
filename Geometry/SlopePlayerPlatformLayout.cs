// SPDX-License-Identifier: MIT

using System.Numerics;

namespace Celeste.Mods.XaphanHelper.Geometry.Slopes
{
    public readonly struct SlopePlayerPlatformLayout
    {
        public Vector2 Position { get; }
        public int Width { get; }
        public float ColliderX { get; }
        public float ColliderY { get; }

        public SlopePlayerPlatformLayout(Vector2 position, int width, float colliderX, float colliderY)
        {
            Position = position;
            Width = width;
            ColliderX = colliderX;
            ColliderY = colliderY;
        }
    }
}
