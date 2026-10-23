// SPDX-License-Identifier: MIT

using System.Collections.Generic;
using Celeste.Physics.Collision.Shapes;

namespace Celeste.Mods.XaphanHelper.Geometry.Slopes
{
    public sealed class SlopeCollisionShape
    {
        public ColliderGeometry Collider { get; }
        public IReadOnlyList<SlopeLightOccluder> LightOccluders { get; }
        public int Height { get; }

        public SlopeCollisionShape(ColliderGeometry collider, IReadOnlyList<SlopeLightOccluder> lightOccluders,
            int height)
        {
            Collider = collider;
            LightOccluders = lightOccluders;
            Height = height;
        }
    }
}
