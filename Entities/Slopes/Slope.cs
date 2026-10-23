// SPDX-License-Identifier: MIT

using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Celeste.Core.Components;
using Celeste.Everest.Modules.Entities.Metadata;
using Celeste.Everest.Modules.Runtime;
using Celeste.Game.Content.Maps.Entities;
using Celeste.Game.Content.Maps.Levels;
using Celeste.Game.Execution.Levels;
using Celeste.Game.Execution.Startup;
using Celeste.Graphics.Content;
using Celeste.Mods.XaphanHelper.Geometry.Slopes;
using Celeste.Mods.XaphanHelper.Modules;
using Celeste.Mods.XaphanHelper.Runtime.Slopes;
using Celeste.Scene.Tracking;
using Celeste.Scene.Tracking.Attributes;
using Celeste.Simulation.Gameplay.Platforms.Foundation;
using Level = Celeste.Game.Execution.Levels.OriginalLevelSceneRoot;
using MTexture = Celeste.Graphics.Content.Atlas.AtlasEntry;
using Rectangle = Celeste.Core.Geometry.IntRect;

namespace Celeste.Mods.XaphanHelper.Entities.Slopes
{
    [SceneTracked(true)]
    [CustomEntity("XaphanHelper/Slope")]
    public sealed partial class Slope : SourceSolidEntityState
    {
        private readonly ISlopeGameplayFeatures features;
        public string Side;

        public string TilesTop;

        public string TilesBottom;

        public string Directory;

        public string FlagDirectory;

        public string Texture;

        public string FlagTexture;

        public bool Gentle;

        public bool CanSlide;

        public bool ForceSlide;

        public bool UpsideDown;

        public bool NoRender;

        public bool StickyDash;

        public int SoundIndex;

        public int SlopeHeight;

        public int[] variation = new int[35];

        public int[] variationInner = new int[35];

        private MTexture[,] BaseTextures;

        private MTexture[] SlopeTextures;

        public bool Rainbow;

        public bool CanJumpThrough;

        public bool VisualOnly;

        private bool AffectPlayerSpeed;

        public string Flag;

        public Celeste.Physics.Collision.Shapes.ColliderListGeometry colliderList;

        private List<LightOccludeBlock> lightOccludeBlocks = new();

        private string RenderMethod;

        private bool PreventRefillOnSliding;

        public Slope(Vector2 position, Vector2 offset, bool gentle, string side, int soundIndex, int slopeHeight,
            string tilesTop, string tilesBottom, string texture, string flagTexture, bool canSlide, bool forceSlide,
            string directory, string flagDirectory, bool upsideDown, bool noRender, bool stickyDash, bool rainbow,
            bool canJumpThrough, string flag, bool affectPlayerSpeed, string renderMethod, bool visualOnly,
            bool preventRefillOnSliding, ISourceSolidWorld world, ISlopeGameplayFeatures features) : base(
            position + offset, 0, 0, true, world)
        {
            this.features = features;
            AddTag(((IOriginalGameTagsEnvironment)SourceWorld.EngineContext).Tags.TransitionUpdate.Value);
            AllowStaticMovers = false;
            Collidable = false;
            Gentle = gentle;
            Side = side;
            SoundIndex = soundIndex;
            SlopeHeight = slopeHeight;
            TilesTop = tilesTop;
            TilesBottom = tilesBottom;
            Texture = texture;
            FlagTexture = flagTexture;
            CanSlide = canSlide;
            ForceSlide = forceSlide;
            Directory = directory;
            FlagDirectory = flagDirectory;
            UpsideDown = upsideDown;
            NoRender = noRender;
            StickyDash = stickyDash;
            Rainbow = rainbow;
            CanJumpThrough = canJumpThrough;
            VisualOnly = visualOnly;
            Flag = flag;
            AffectPlayerSpeed = affectPlayerSpeed;
            RenderMethod = renderMethod;
            PreventRefillOnSliding = preventRefillOnSliding;
            var shape = SlopeGeometry.CreateCollision(Gentle, Side, SlopeHeight, UpsideDown, CanJumpThrough,
                VisualOnly);
            Collider = shape.Collider;
            colliderList = Collider as Celeste.Physics.Collision.Shapes.ColliderListGeometry;
            foreach (var occluder in shape.LightOccluders)
                lightOccludeBlocks.Add(new LightOccludeBlock(
                    ((ISourceEntitySourceDataEnvironment)SourceWorld.EngineContext).EntitySourceDataRegistration,
                    Position + occluder.Position, occluder.Width, occluder.Height));
            if (SlopeHeight < 1)
            {
                SlopeHeight = 1;
            }
            else if (SlopeHeight > 30)
            {
                SlopeHeight = 30;
            }

            if (string.IsNullOrEmpty(Directory))
            {
                Directory = "objects/XaphanHelper/Slope";
            }

            if (string.IsNullOrEmpty(FlagDirectory))
            {
                FlagDirectory = "objects/XaphanHelper/Slope";
            }

            Depth = -10003;
        }

        public static Slope Load(OriginalLevelSceneRoot level, OriginalLevelRoomData room, Vector2 offset,
            OriginalLevelEntityData data)
        {
            var module = ((ISourceEverestModulesEnvironment)level.EngineContext).Modules.OfType<XaphanHelperModule>()
                .Single();
            return new Slope(data.Position, offset, data.Bool("gentle"), data.Attr("side"), data.Int("soundIndex"),
                data.Int("slopeHeight", 1), data.Attr("tilesTop"), data.Attr("tilesBottom"),
                data.Attr("texture", "cement"),
                data.Attr("flagTexture", ""), data.Bool("canSlide"), data.Bool("forceSlide"),
                data.Attr("customDirectory", ""),
                data.Attr("flagCustomDirectory", ""), data.Bool("upsideDown"), data.Bool("noRender"),
                data.Bool("stickyDash"),
                data.Bool("rainbow"), data.Bool("canJumpThrough"), data.Attr("flag", ""),
                data.Bool("affectPlayerSpeed"),
                data.Attr("renderMethod", "Type A"), data.Bool("visualOnly"), data.Bool("preventRefillOnSliding"),
                level.SolidWorldFactory.Create(data.Position + offset, true), module.Features);
        }

        public override void Added(object scene)
        {
            base.Added(scene);
            foreach (LightOccludeBlock lightOccludeBlock in lightOccludeBlocks)
            {
                ((ISourceScene)Scene).SourceScene.QueueSourceEntity(lightOccludeBlock);
            }

            MTexture mtexture = ((ISourceGameGraphicsEnvironment)SourceWorld.EngineContext).GameGraphics.GameAtlas[
                ((!string.IsNullOrEmpty(Flag) && ((Level)Scene).Session.GetFlag(Flag)) ? FlagDirectory : Directory) +
                "/" + ((!string.IsNullOrEmpty(Flag) && ((Level)Scene).Session.GetFlag(Flag)) ? FlagTexture : Texture)];
            BaseTextures = new MTexture[6, 15];
            for (int i = 0; i < 6; i++)
            {
                for (int j = 0; j < 15; j++)
                {
                    BaseTextures[i, j] = mtexture.GetSubtexture(new Rectangle(i * 8, j * 8, 8, 8));
                }
            }

            SlopeTextures = new MTexture[96];
            if (!UpsideDown)
            {
                for (int k = 0; k < 32; k++)
                {
                    SlopeTextures[k] = mtexture.GetSubtexture(new Rectangle(k, 0, 1, 8));
                }

                for (int k = 32; k < 64; k++)
                {
                    SlopeTextures[k] = mtexture.GetSubtexture(new Rectangle(k - 32, 88, 1, 8));
                }

                for (int k = 64; k < 96; k++)
                {
                    SlopeTextures[k] = mtexture.GetSubtexture(new Rectangle(k - 64, 96, 1, 8));
                }
            }
            else
            {
                for (int k = 0; k < 32; k++)
                {
                    SlopeTextures[k] = mtexture.GetSubtexture(new Rectangle(k, 8, 1, 8));
                }

                for (int k = 32; k < 64; k++)
                {
                    SlopeTextures[k] = mtexture.GetSubtexture(new Rectangle(k - 32, 104, 1, 8));
                }

                for (int k = 64; k < 96; k++)
                {
                    SlopeTextures[k] = mtexture.GetSubtexture(new Rectangle(k - 64, 112, 1, 8));
                }
            }

            int Variation;
            Math.DivRem((int)Position.X / 8, 4, out Variation);
            Variation = Math.Abs(Variation);
            variation[0] = Variation;

            for (int l = 1; l < 35; l++)
            {
                Variation++;
                if (Variation >= 4)
                {
                    Variation = 0;
                }

                variation[l] = Variation;
            }

            Math.DivRem((int)Position.X / 8, 12, out Variation);
            Variation = Math.Abs(Variation);
            variationInner[0] = Variation;

            for (int m = 1; m < 35; m++)
            {
                Variation++;
                if (Variation >= 12)
                {
                    Variation = 0;
                }

                variationInner[m] = Variation;
            }

            if (!VisualOnly)
            {
                var level = (Level)Scene;
                var layout = SlopeGeometry.CreatePlayerPlatformLayout(Position, Gentle, Side, SlopeHeight, UpsideDown);
                ((ISourceScene)Scene).SourceScene.QueueSourceEntity(new PlayerPlatform(layout.Position, layout.Width,
                    Gentle, Side, SoundIndex, SlopeHeight, CanSlide, ForceSlide, Top, AffectPlayerSpeed, UpsideDown,
                    StickyDash, CanJumpThrough, PreventRefillOnSliding,
                    level.SolidWorldFactory.Create(layout.Position, true), features));
                if (!UpsideDown)
                    ((ISourceScene)Scene).SourceScene.QueueSourceEntity(new FakePlayerPlatform(layout.Position,
                        layout.Width,
                        Gentle, Side, SoundIndex, SlopeHeight, Top, UpsideDown, StickyDash, CanJumpThrough,
                        level.SolidWorldFactory.Create(layout.Position, true), features));
            }

            (SlopeTop, SlopeBottom) = SlopeGeometry.GetSlopeEnds(Position, Gentle, Side, SlopeHeight, UpsideDown);
        }

        public Vector2 SlopeTop;
        public Vector2 SlopeBottom;

        public override void Update()
        {
            Collidable = features.HasDroneDebris && !CanJumpThrough;
        }
    }
}
