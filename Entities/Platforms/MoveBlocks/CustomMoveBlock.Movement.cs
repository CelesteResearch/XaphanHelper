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
        public override void MoveHExact(int move)
        {
            if (noSquish != null && ((move < 0 && noSquish.X < base.X) || (move > 0 && noSquish.X > base.X)))
            {
                while (move != 0 &&
                       CheckEntityAt<SourceSolidEntityState>(noSquish, noSquish.Position + Vector2.UnitX * move))
                {
                    move -= Math.Sign(move);
                }
            }

            base.MoveHExact(move);
        }

        public override void MoveVExact(int move)
        {
            if (noSquish != null && move < 0 && noSquish.Y <= base.Y)
            {
                while (move != 0 &&
                       CheckEntityAt<SourceSolidEntityState>(noSquish, noSquish.Position + Vector2.UnitY * move))
                {
                    move -= Math.Sign(move);
                }
            }

            base.MoveVExact(move);
        }

        private bool MoveCheck(Vector2 speed)
        {
            if (speed.X != 0f)
            {
                if (MoveHCollideSolids(speed.X, thruDashBlocks: false))
                {
                    for (int i = 1; i <= 3; i++)
                    {
                        for (int num = 1; num >= -1; num -= 2)
                        {
                            Vector2 vector = new Vector2(Math.Sign(speed.X), i * num);
                            if (!CheckTracked<SourceSolidEntityState>(Position + vector))
                            {
                                MoveVExact(i * num);
                                MoveHExact(Math.Sign(speed.X));
                                return false;
                            }
                        }
                    }

                    return true;
                }

                return false;
            }

            if (speed.Y != 0f)
            {
                if (MoveVCollideSolids(speed.Y, thruDashBlocks: false))
                {
                    for (int j = 1; j <= 3; j++)
                    {
                        for (int num2 = 1; num2 >= -1; num2 -= 2)
                        {
                            Vector2 vector2 = new Vector2(j * num2, Math.Sign(speed.Y));
                            if (!CheckTracked<SourceSolidEntityState>(Position + vector2))
                            {
                                MoveHExact(j * num2);
                                MoveVExact(Math.Sign(speed.Y));
                                return false;
                            }
                        }
                    }

                    return true;
                }

                return false;
            }

            return false;
        }
    }
}
