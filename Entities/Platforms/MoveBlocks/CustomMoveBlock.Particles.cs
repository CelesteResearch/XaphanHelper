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
        private void ActivateParticles()
        {
            bool flag = direction == Directions.Down || direction == Directions.Up;
            bool flag2 = (steerSides == "None" || !flag) && !CheckTracked<RoomPlayerSession>(Position - Vector2.UnitX);
            bool flag3 = (steerSides == "None" || !flag) && !CheckTracked<RoomPlayerSession>(Position + Vector2.UnitX);
            bool flag4 = (steerSides == "None" || flag) && !CheckTracked<RoomPlayerSession>(Position - Vector2.UnitY);
            if (flag2)
            {
                ((OriginalLevelSceneRoot)Scene).ParticlesBG.Emit(P_Activate, (int)(base.Height / 2f), base.CenterLeft,
                    Vector2.UnitY * (base.Height - 4f) * 0.5f, MathF.PI);
            }

            if (flag3)
            {
                ((OriginalLevelSceneRoot)Scene).ParticlesBG.Emit(P_Activate, (int)(base.Height / 2f), base.CenterRight,
                    Vector2.UnitY * (base.Height - 4f) * 0.5f, 0f);
            }

            if (flag4)
            {
                ((OriginalLevelSceneRoot)Scene).ParticlesBG.Emit(P_Activate, (int)(base.Width / 2f), base.TopCenter,
                    Vector2.UnitX * (base.Width - 4f) * 0.5f, -MathF.PI / 2f);
            }

            ((OriginalLevelSceneRoot)Scene).ParticlesBG.Emit(P_Activate, (int)(base.Width / 2f), base.BottomCenter,
                Vector2.UnitX * (base.Width - 4f) * 0.5f, MathF.PI / 2f);
        }

        private void BreakParticles()
        {
            Vector2 center = base.Center;
            for (int i = 0; (float)i < base.Width; i += 4)
            {
                for (int j = 0; (float)j < base.Height; j += 4)
                {
                    Vector2 vector = Position + new Vector2(2 + i, 2 + j);
                    ((OriginalLevelSceneRoot)Scene).Particles.Emit(P_Break, 1, vector, Vector2.One * 2f,
                        Vector2Geometry.Angle(vector - center));
                }
            }
        }

        private void MoveParticles()
        {
            Vector2 position;
            Vector2 positionRange;
            float num;
            float num2;
            if (direction == Directions.Right)
            {
                position = base.CenterLeft + Vector2.UnitX;
                positionRange = Vector2.UnitY * (base.Height - 4f);
                num = MathF.PI;
                num2 = base.Height / 32f;
            }
            else if (direction == Directions.Left)
            {
                position = base.CenterRight;
                positionRange = Vector2.UnitY * (base.Height - 4f);
                num = 0f;
                num2 = base.Height / 32f;
            }
            else if (direction == Directions.Down)
            {
                position = base.TopCenter + Vector2.UnitY;
                positionRange = Vector2.UnitX * (base.Width - 4f);
                num = -MathF.PI / 2f;
                num2 = base.Width / 32f;
            }
            else
            {
                position = base.BottomCenter;
                positionRange = Vector2.UnitX * (base.Width - 4f);
                num = MathF.PI / 2f;
                num2 = base.Width / 32f;
            }

            particleRemainder += num2;
            int num3 = (int)particleRemainder;
            particleRemainder -= num3;
            positionRange *= 0.5f;
            if (num3 > 0)
            {
                ((OriginalLevelSceneRoot)Scene).ParticlesBG.Emit(P_Move, num3, position, positionRange, num);
            }
        }

        private void ScrapeParticles(Vector2 dir)
        {
            _ = Collidable;
            Collidable = false;
            if (dir.X != 0f)
            {
                float x = ((!(dir.X > 0f)) ? (base.Left - 1f) : base.Right);
                for (int i = 0; (float)i < base.Height; i += 8)
                {
                    Vector2 vector = new Vector2(x, base.Top + 4f + (float)i);
                    if (SolidPoint(vector))
                    {
                        ((OriginalLevelSceneRoot)Scene).ParticlesFG.Emit(
                            ((IOriginalGameplayProcess)Engine).ParticleTypes.ZipMover.P_Scrape, vector);
                    }
                }
            }
            else
            {
                float y = ((!(dir.Y > 0f)) ? (base.Top - 1f) : base.Bottom);
                for (int j = 0; (float)j < base.Width; j += 8)
                {
                    Vector2 vector2 = new Vector2(base.Left + 4f + (float)j, y);
                    if (SolidPoint(vector2))
                    {
                        ((OriginalLevelSceneRoot)Scene).ParticlesFG.Emit(
                            ((IOriginalGameplayProcess)Engine).ParticleTypes.ZipMover.P_Scrape, vector2);
                    }
                }
            }

            Collidable = true;
        }
    }
}
