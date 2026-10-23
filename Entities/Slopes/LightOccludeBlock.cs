// SPDX-License-Identifier: MIT

using System.Numerics;
using Celeste.Core.Components;
using Celeste.Core.Rendering;
using Celeste.Physics.Collision.Shapes;
using Celeste.Scene.Entities;
using Celeste.Simulation.Presentation.Lighting;

namespace Celeste.Mods.XaphanHelper.Entities.Slopes
{
    public sealed class LightOccludeBlock : SceneEntityState
    {
        public LightOccludeBlock(ISourceEntitySourceDataRegistration sourceData, Vector2 position, float width,
            float height)
            : base(sourceData, position)
        {
            Collider = new HitboxColliderGeometry(width, height);
            Add(new LightOccludeState());
            Depth = -10003;
        }

        public override void DebugRender(ISourceRenderContext context, object camera)
        {
        }
    }
}
