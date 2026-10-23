using System;
using Celeste.Core.Components;
using System.Collections.Generic;
using System.Numerics;
using Celeste.Core.Display;
using Celeste.Core.Rendering;
using Celeste.Formats.Map.Entities.Structure;
using Celeste.Core.Geometry;
using Celeste.Core.Mathematics.Extrema;
using Celeste.Core.Scheduling;
using Celeste.Game.Execution.Areas;
using Celeste.Game.Execution.Levels;
using Celeste.Game.Execution.Processes;
using Celeste.Graphics.Rendering;
using Celeste.Physics.Collision;
using Celeste.Scene.Entities;
using Celeste.Scene.Tracking;
using Celeste.Simulation;

namespace Celeste.Mods.XaphanHelper.Entities.Liquids
{
    public partial class Liquid
    {
        private OriginalLevelSceneRoot Level => (OriginalLevelSceneRoot)Scene;
        private List<object> Tracked(Type type) => ((ISourceScene)Scene).SourceScene.Entities.GetTrackedEntities(type);

        private RoomPlayerSession FirstPlayer()
        {
            var players = Tracked(typeof(RoomPlayerSession));
            return players.Count == 0 ? null : (RoomPlayerSession)players[0];
        }

        private int ChapterIndex => ((IOriginalAreaKeyEnvironment)engine).AreaKeys.GetChapterIndex(
            new OriginalAreaKey
            {
                ID = Level.Session.AreaID, Mode = Level.Session.Mode, SID = Level.Session.AreaSID,
                SIDID = Level.Session.AreaID
            }, engine);

        private bool SolidAt(IntRect rectangle) =>
            SourceSceneCollision.CheckRect(((ISourceScene)Scene).SourceScene.Entities, "Celeste.Solid", rectangle);

        private bool VariaActive => module.LiquidFeatures.VariaJacketActive?.Invoke(Level) == true;
        private bool GravityActive => module.LiquidFeatures.GravityJacketActive?.Invoke(Level) == true;

        private bool IsInType(string type, bool contains = false)
        {
            var liquids = Tracked(typeof(Liquid));
            for (var i = 0; i < liquids.Count; i++)
            {
                var liquid = (Liquid)liquids[i];
                if ((contains ? liquid.LiquidType.Contains(type) : liquid.LiquidType == type) &&
                    liquid.PlayerInside()) return true;
            }

            return false;
        }

        private bool IsInLiquid()
        {
            if (IsInType("water") || IsInType("lava") || IsInType("acid", true)) return true;
            var sections = module.LiquidFeatures.WaterfallSections;
            for (var i = 0; i < sections.Count; i++)
                if (ReferenceEquals(sections[i].Entity.Scene, Scene) && sections[i].PlayerInside())
                    return true;
            return false;
        }

        private void RefreshDisplacementGrid()
        {
            var width = (int)(Collider.Width / 8f);
            var height = (int)(Collider.Height / 8f);
            if (DisplacementGrid.GetLength(0) != width || DisplacementGrid.GetLength(1) != height)
                DisplacementGrid = new bool[width, height];
            CheckSolidsForDisplacement();
        }

        private PackedColor GradientColor(float percent)
        {
            if (!(percent >= 1)) return gradientColors[0];
            if (percent < gradientColors.Length) return gradientColors[(int)percent];
            return CalculateGradient(PackedColor.FromHex(color), PackedColor.FromHex(poisonedColor), percent);
        }

        private static PackedColor[] CreateGradientPalette(PackedColor from, PackedColor to)
        {
            var colors = new PackedColor[103];
            for (var i = 0; i < colors.Length; i++) colors[i] = CalculateGradient(from, to, i);
            return colors;
        }

        private static PackedColor CalculateGradient(PackedColor from, PackedColor to, float percent)
        {
            // Original Utils accumulates channels and truncates; Lerp has different rounding.
            var red = (float)from.R;
            var green = (float)from.G;
            var blue = (float)from.B;
            var redStep = (to.R - from.R) / 100f;
            var greenStep = (to.G - from.G) / 100f;
            var blueStep = (to.B - from.B) / 100f;
            for (var i = 1; i <= percent; i++)
            {
                red += redStep;
                green += greenStep;
                blue += blueStep;
            }

            return new PackedColor((int)red, (int)green, (int)blue);
        }

        private Celeste.Core.Audio.ISourceAudioEventReference PlayAudio(string path, Vector2? position = null,
            string parameter = null, float value = 0)
        {
            var process = (IOriginalGameplayProcess)engine;
            return position.HasValue
                ? process.AudioSession.Play(path, position.Value, process.SaveContext, parameter, value)
                : parameter == null
                    ? process.AudioSession.Play(path)
                    : process.AudioSession.Play(path, parameter, value);
        }

        private void DrawLiquidSubrect(ISourceRenderContext context, Vector2 offset, IntRect rectangle)
            => liquidSprite.DrawSubrect(context, offset, rectangle,
                ((ISourceExtremaEnvironment)engine).ExtremaArithmetic);

        private void Rumble() =>
            ((Celeste.Input.Gameplay.Feedback.ISourceRumbleEnvironment)engine).RumbleSession.Rumble(0, 0);
    }
}
