using System;
using Celeste.Mods.XaphanHelper.Presentation.Outlines;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using Celeste.Core.Components;
using Celeste.Core.Display;
using Celeste.Core.Mathematics;
using Celeste.Core.Mathematics.Easing;
using Celeste.Core.Randomness;
using Celeste.Core.Audio;
using Celeste.Core.Rendering;
using Celeste.Core.Reflection.Runtime;
using Celeste.Core.Scheduling;
using Celeste.Everest.Modules.Entities.Metadata;
using Celeste.Formats.Map.Entities.Attributes;
using Celeste.Game.Content.Maps.Entities;
using Celeste.Game.Content.Maps.Levels;
using Celeste.Game.Execution.Levels;
using Celeste.Game.Execution.Processes;
using Celeste.Graphics.Animation;
using Celeste.Graphics.Content;
using Celeste.Graphics.Content.Atlas;
using Celeste.Graphics.Display.Colors;
using Celeste.Graphics.Rendering;
using Celeste.Input.Gameplay;
using Celeste.Mods.XaphanHelper.Runtime.Slopes;
using Celeste.Physics.Collision;
using Celeste.Physics.Collision.Shapes;
using Celeste.Scene.Entities;
using Celeste.Scene.Tracking;
using Celeste.Scene.Tracking.Attributes;
using Celeste.Simulation;
using Celeste.Simulation.Audio.Sources;
using Celeste.Simulation.Gameplay.Actors.Foundation;
using Celeste.Simulation.Gameplay.Platforms.Foundation;
using Celeste.Simulation.Gameplay.Platforms.StaticMovers;
using Celeste.Simulation.Presentation.Lighting;
using Celeste.Simulation.Presentation.Particles.Definitions;

namespace Celeste.Mods.XaphanHelper.Entities.Platforms.MoveBlocks
{
    public partial class CustomMoveBlock
    {
        [RuntimeTypeAlias("Celeste.Mod.XaphanHelper.Entities.CustomMoveBlock+Border")]
        private class Border : SceneEntityState
        {
            public CustomMoveBlock Parent;

            public Border(CustomMoveBlock parent) : base(((ISourceEntitySourceDataEnvironment)parent.Engine)
                .EntitySourceDataRegistration)
            {
                Parent = parent;
                base.Depth = parent.Depth + 1;
            }

            public override void Update()
            {
                if (Parent.Scene != base.Scene)
                {
                    RemoveSelf();
                }

                base.Update();
            }

            public override void Render(ISourceRenderContext context)
            {
                SourcePrimitiveDrawing.Rect(context, this, Parent.X + Parent.Shake.X - 1f,
                    Parent.Y + Parent.Shake.Y - 1f, Parent.Width + 2f, Parent.Height + 2f, PackedColor.Black);
            }
        }
    }
}
