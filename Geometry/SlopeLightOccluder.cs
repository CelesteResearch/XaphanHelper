// SPDX-License-Identifier: MIT

using System.Numerics;

namespace Celeste.Mods.XaphanHelper.Geometry.Slopes
{
    public readonly struct SlopeLightOccluder
    {
        public Vector2 Position { get; }
        public float Width { get; }
        public float Height { get; }

        public SlopeLightOccluder(Vector2 position, float width, float height)
        {
            Position = position;
            Width = width;
            Height = height;
        }
    }
}
