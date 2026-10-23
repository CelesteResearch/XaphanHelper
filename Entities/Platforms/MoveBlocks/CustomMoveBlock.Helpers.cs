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
        private bool Interval(float interval) => ((ISourceScene)Scene).SourceScene.Clock.OnInterval(interval, Engine);

        private bool SolidPoint(Vector2 at)
        {
            var s = ((ISourceScene)Scene).SourceScene;
            return SourceSceneCollision.CheckPoint(s.Entities,
                s.Catalog.Find(typeof(SourceSolidEntityState)).RegistryKey, at);
        }

        private bool CheckTracked<T>() => CheckTracked<T>(Position);
        private bool CheckTracked<T>(Vector2 at) => CheckEntityAt<T>(this, at);

        private static bool CheckEntityAt<T>(SceneEntityState entity, Vector2 at)
        {
            foreach (var candidate in ((ISourceScene)entity.Scene).SourceScene.Entities.GetTrackedEntities(typeof(T)))
                if (SourceEntityCollision.CheckAt(entity, (ISourceEntityCollider)candidate, at))
                    return true;
            return false;
        }

        private static void
            DrawCentered(ISourceRenderContext context, object entity, AtlasEntry entry, Vector2 position) =>
            SourceAtlasDrawing.Draw(context, entity, entry, position,
                new Vector2(entry.FrameWidth, entry.FrameHeight) / 2f, PackedColor.White, Vector2.One);
    }
}
