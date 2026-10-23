using System.Numerics;
using Celeste.Core.Audio;
using Celeste.Core.Collections;
using Celeste.Core.Maps;
using Celeste.Core.Mathematics;
using Celeste.Core.Mathematics.Extrema;
using Celeste.Core.Reflection.Runtime;
using Celeste.Core.Rendering;
using Celeste.Core.Scheduling;
using Celeste.Everest.Modules.Entities.Metadata;
using Celeste.Formats.Map.Entities.Structure;
using Celeste.Formats.Map.Entities.Attributes;
using Celeste.Game.Content.Maps;
using Celeste.Game.Content.Maps.Entities;
using Celeste.Game.Content.Maps.Levels;
using Celeste.Game.Execution.Levels;
using Celeste.Graphics.Content.Tiles;
using Celeste.Graphics.Content.Tiles.Autotiling;
using Celeste.Physics.Collision.Shapes;
using Celeste.Scene.Entities;
using Celeste.Scene.Tracking;
using Celeste.Scene.Tracking.Attributes;
using Celeste.Simulation;
using Celeste.Simulation.Gameplay.Actors.Debris;
using Celeste.Simulation.Gameplay.Platforms.Foundation;
using Celeste.Simulation.Gameplay.Tiles;
using Celeste.Simulation.Gameplay.Walls.Concealment;
using Celeste.Simulation.Presentation.Lighting;

namespace Celeste.Mods.XaphanHelper.Entities.Walls.Breakable
{
    [SceneTracked(true)]
    [RuntimeTypeAlias("Celeste.Mod.XaphanHelper.Entities.FlagBlock")]
    [CustomEntity("XaphanHelper/FlagBlock")]
    public class FlagBlock : SourceSolidEntityState
    {
        public enum Modes
        {
            Wall,
            Block
        }

        private Modes mode;

        private char fillTile;

        private char flagFillTile;

        private TileGrid tiles;

        private bool fade;

        private EffectCutoutState cutout;

        private float transitionStartAlpha;

        private bool transitionFade;

        public MapEntityId eid;

        public bool breaking;

        private string[] removeFlags;

        private bool permanent;

        private string flag;

        public FlagBlock(ISourceEngineContext engine, EntityData data, Vector2 position, MapEntityId ID,
            ISourceSolidWorld world) : base(data.Position + position, data.Width, data.Height, true, world)
        {
            this.engine = engine;
            playerRegistryKey = ((ISourceSceneCatalogEnvironment)engine).SceneCatalog.Find(typeof(RoomPlayerSession))
                .RegistryKey;
            mode = data.Enum<Modes>("mode");
            flag = data.Attr("flag");
            eid = ID;
            fillTile = data.Char("tiletype", '3');
            flagFillTile = data.Char("flagTiletype", '3');
            permanent = data.Bool("permanent", true);
            removeFlags = data.Attr("removeFlags").Split(',');
            Collider = new HitboxColliderGeometry(data.Width, data.Height);
            Depth = -13000;
            Add(cutout = new EffectCutoutState());
            Add(new LightOccludeState(0.5f));
        }

        public override void Added(object scene)
        {
            base.Added(scene);
            foreach (string flag in removeFlags)
            {
                if ((Scene as OriginalLevelSceneRoot).Session.GetFlag(flag))
                {
                    RemoveSelf();
                    break;
                }
            }

            if (CollideCheck(playerRegistryKey))
            {
                RemoveSelf();
                (Scene as OriginalLevelSceneRoot).Session.DoNotLoad.Add(eid);
            }
            else
            {
                Collidable = (Visible = true);
            }

            int tilesX = (int)Width / 8;
            int tilesY = (int)Height / 8;
            OriginalLevelSceneRoot level = (Scene as OriginalLevelSceneRoot);
            if (mode == Modes.Wall)
            {
                var tileBounds = ((OriginalChapterMapData)level.Session.MapData).TileBounds;
                VirtualMap<char> solidsData = level.SolidsData;
                int x = (int)X / 8 - tileBounds.Left;
                int y = (int)Y / 8 - tileBounds.Top;
                tiles = ((ISourceAutotilingResources)engine).ForegroundAutotiler
                    .GenerateOverlay(
                        (!string.IsNullOrEmpty(flag) && level.Session.GetFlag(flag)) ? flagFillTile : fillTile, x, y,
                        tilesX, tilesY, solidsData).TileGrid;
            }
            else if (mode == Modes.Block)
            {
                tiles = ((ISourceAutotilingResources)engine).ForegroundAutotiler
                    .GenerateBox((!string.IsNullOrEmpty(flag) && level.Session.GetFlag(flag)) ? flagFillTile : fillTile,
                        tilesX, tilesY).TileGrid;
            }

            Add(tiles);
            Add(new TileInterceptorState(tiles, highPriority: false));
        }

        public override void Awake(object scene)
        {
            base.Awake(scene);
            if (CollideCheck(playerRegistryKey))
            {
                RevealWhenTransition();
            }
            else
            {
                RoomTransitionListener transitionListener = new();
                transitionListener.OnOut = OnTransitionOut;
                transitionListener.OnOutBegin = OnTransitionOutBegin;
                transitionListener.OnIn = OnTransitionIn;
                transitionListener.OnInBegin = OnTransitionInBegin;
                Add(transitionListener);
            }
        }

        public void RevealWhenTransition()
        {
            tiles.Alpha = 0f;
            fade = true;
            cutout.Visible = false;
            (Scene as OriginalLevelSceneRoot).Session.DoNotLoad.Add(eid);
        }

        private void OnTransitionOutBegin()
        {
            if (Collider.Collide((Scene as OriginalLevelSceneRoot).Bounds))
            {
                transitionFade = true;
                transitionStartAlpha = tiles.Alpha;
            }
            else
            {
                transitionFade = false;
            }
        }

        private void OnTransitionOut(float percent)
        {
            if (transitionFade)
            {
                tiles.Alpha = transitionStartAlpha * (1f - percent);
            }
        }

        private void OnTransitionInBegin()
        {
            OriginalLevelSceneRoot level = (Scene as OriginalLevelSceneRoot);
            if (level.PreviousBounds.HasValue && Collider.Collide(level.PreviousBounds.Value))
            {
                transitionFade = true;
                tiles.Alpha = 0f;
            }
            else
            {
                transitionFade = false;
            }
        }

        private void OnTransitionIn(float percent)
        {
            if (transitionFade)
            {
                tiles.Alpha = percent;
            }
        }

        public override void Update()
        {
            base.Update();
            if (fade)
            {
                tiles.Alpha = ScalarMath.Approach(tiles.Alpha, 0f, 2f * engine.DeltaTime,
                    ((ISourceExtremaEnvironment)engine).ExtremaArithmetic);
                cutout.Alpha = tiles.Alpha;
                if (tiles.Alpha <= 0f)
                {
                    RemoveSelf();
                }

                return;
            }

            if (!breaking)
            {
                foreach (string flag in removeFlags)
                {
                    if ((Scene as OriginalLevelSceneRoot).Session.GetFlag(flag))
                    {
                        Break();
                        break;
                    }
                }
            }
        }

        public void Break(bool playSound = true, bool playDebrisSound = true)
        {
            breaking = true;
            OriginalLevelSceneRoot level = (Scene as OriginalLevelSceneRoot);
            if (playSound)
            {
                if (((!string.IsNullOrEmpty(flag) && level.Session.GetFlag(flag)) ? flagFillTile : fillTile) == '1')
                {
                    ((ISourceSpatialAudioEnvironment)engine).SpatialAudio.Play("event:/game/general/wall_break_dirt",
                        Position);
                }
                else if (((!string.IsNullOrEmpty(flag) && level.Session.GetFlag(flag)) ? flagFillTile : fillTile) ==
                         '3')
                {
                    ((ISourceSpatialAudioEnvironment)engine).SpatialAudio.Play("event:/game/general/wall_break_ice",
                        Position);
                }
                else if (((!string.IsNullOrEmpty(flag) && level.Session.GetFlag(flag)) ? flagFillTile : fillTile) ==
                         '9')
                {
                    ((ISourceSpatialAudioEnvironment)engine).SpatialAudio.Play("event:/game/general/wall_break_wood",
                        Position);
                }
                else
                {
                    ((ISourceSpatialAudioEnvironment)engine).SpatialAudio.Play("event:/game/general/wall_break_stone",
                        Position);
                }
            }

            for (int i = 0; i < Width / 8f; i++)
            {
                for (int j = 0; j < Height / 8f; j++)
                {
                    ((ISourceScene)Scene).SourceScene.QueueSourceEntity(engine.EntityPools.Create<DebrisState>()
                        .Init(Position + new Vector2(4 + i * 8, 4 + j * 8),
                            (!string.IsNullOrEmpty(flag) && level.Session.GetFlag(flag)) ? flagFillTile : fillTile,
                            playDebrisSound).BlastFrom(Center));
                }
            }

            Collidable = false;
            RemoveSelf();
            if (permanent)
            {
                level.Session.DoNotLoad.Add(eid);
            }
        }


        public override void Render(ISourceRenderContext context)
        {
            if (mode == Modes.Wall) SecretWallDrawing.RenderShakeMargins(this, tiles, (ISecretWallScene)Scene, context);
            base.Render(context);
        }

        public override void Removed(object scene)
        {
            base.Removed(scene);
            DestroyStaticMovers();
        }

        private readonly ISourceEngineContext engine;
        private readonly string playerRegistryKey;

        public static SceneEntityState Load(OriginalLevelSceneRoot level, OriginalLevelRoomData room, Vector2 offset,
            OriginalLevelEntityData data) => new FlagBlock(level.EngineContext, data, offset,
            level.CreateEntityId(room, data), level.SolidWorldFactory.Create(data.Position + offset, true));
    }
}
