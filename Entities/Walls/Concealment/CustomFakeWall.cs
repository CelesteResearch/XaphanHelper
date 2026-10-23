using System.Numerics;
using Celeste.Core.Components;
using System.Collections.Generic;
using Celeste.Core.Audio;
using Celeste.Core.Maps;
using Celeste.Core.Mathematics;
using Celeste.Core.Mathematics.Extrema;
using Celeste.Core.Reflection.Runtime;
using Celeste.Core.Rendering;
using Celeste.Core.Scheduling;
using Celeste.Everest.Modules.Entities.Metadata;
using Celeste.Formats.Map.Entities.Structure;
using Celeste.Game.Content.Maps.Entities;
using Celeste.Game.Content.Maps.Levels;
using Celeste.Game.Execution.Levels;
using Celeste.Game.Audio.Surfaces;
using Celeste.Graphics.Content.Tiles;
using Celeste.Physics.Collision;
using Celeste.Physics.Collision.Shapes;
using Celeste.Scene.Entities;
using Celeste.Scene.Tracking;
using Celeste.Scene.Tracking.Attributes;
using Celeste.Simulation;
using Celeste.Simulation.Gameplay.Platforms.Foundation;
using Celeste.Simulation.Gameplay.Tiles;
using Celeste.Simulation.Gameplay.Walls.Concealment;
using Celeste.Simulation.Presentation.Lighting;

namespace Celeste.Mods.XaphanHelper.Entities.Walls.Concealment;

[SceneTracked(true)]
[RuntimeTypeAlias("Celeste.Mod.XaphanHelper.Entities.CustomFakeWall")]
[CustomEntity(new string[] { "XaphanHelper/CustomFakeWall", "XaphanHelper/LinkedFakeWall" })]
public class CustomFakeWall : SceneEntityState
{
    public enum Modes
    {
        Wall,
        Block
    }

    private Modes mode;

    private char fillTile;

    private char flagFillTile;

    public TileGrid tiles;

    public bool fade;

    public EffectCutoutState cutout;

    private float transitionStartAlpha;

    private bool transitionFade;

    public MapEntityId eid;

    private bool playRevealWhenTransitionedInto;

    public int group;

    private string flag;

    private bool CheckedExitBlocks;

    private bool OverlapExitBlock;

    public CustomFakeWall(ISourceEngineContext engine, EntityData data, Vector2 position, MapEntityId eid)
        : base(((ISourceEntitySourceDataEnvironment)engine).EntitySourceDataRegistration)
    {
        this.engine = engine;
        playerKey = ((ISourceSceneCatalogEnvironment)engine).SceneCatalog.Find(typeof(RoomPlayerSession)).RegistryKey;
        Position = data.Position + position;
        mode = data.Enum<Modes>("mode", Modes.Wall);
        this.eid = eid;
        fillTile = data.Char("tiletype", '3');
        flagFillTile = data.Char("flagTiletype", '3');
        playRevealWhenTransitionedInto = data.Bool("playTransitionReveal", false);
        group = data.Int("group", 0);
        flag = data.Attr("flag", "");
        this.Collider = new HitboxColliderGeometry((float)data.Width, (float)data.Height, 0f, 0f);
        this.Depth = -13000;
        EffectCutoutState val = new EffectCutoutState();
        EffectCutoutState val2 = val;
        cutout = val;
        this.Add(val2);
    }

    public override void Added(object scene)
    {
        base.Added(scene);
        var catalog = ((ISourceSceneCatalogEnvironment)engine).SceneCatalog;
        fakeKey = catalog.Find(typeof(CustomFakeWall)).RegistryKey;
        exitKey = catalog.Find(typeof(CustomExitBlock)).RegistryKey;
        int num = (int)this.Width / 8;
        int num2 = (int)this.Height / 8;
        OriginalLevelSceneRoot val = this.SceneAsLevel();
        if (mode == Modes.Wall)
        {
            var tileBounds = ((ISecretWallScene)val).TileBounds;
            var solidsData = val.SolidsData;
            int num3 = (int)this.X / 8 - tileBounds.Left;
            int num4 = (int)this.Y / 8 - tileBounds.Top;
            tiles = ((ISecretWallScene)val).ForegroundAutotiler
                .GenerateOverlay((!string.IsNullOrEmpty(flag) && val.Session.GetFlag(flag)) ? flagFillTile : fillTile,
                    num3, num4, num, num2, solidsData).TileGrid;
        }
        else if (mode == Modes.Block)
        {
            tiles = ((ISecretWallScene)val).ForegroundAutotiler
                .GenerateBox((!string.IsNullOrEmpty(flag) && val.Session.GetFlag(flag)) ? flagFillTile : fillTile, num,
                    num2).TileGrid;
        }

        this.Add(tiles);
        this.Add(new TileInterceptorState(tiles, false));
    }

    public override void Awake(object scene)
    {
        base.Awake(scene);
        if (this.CollideCheck(playerKey))
        {
            if (playRevealWhenTransitionedInto)
            {
                ((ISourceSpatialAudioEnvironment)engine).SpatialAudio.Play("event:/game/general/secret_revealed",
                    this.Center);
            }

            {
                foreach (CustomFakeWall item in FindFakeWalls())
                {
                    if (item.group == group)
                    {
                        item.RevealWhenTransition();
                    }
                }

                return;
            }
        }

        RoomTransitionListener val = new RoomTransitionListener();
        val.OnOut = OnTransitionOut;
        val.OnOutBegin = OnTransitionOutBegin;
        val.OnIn = OnTransitionIn;
        val.OnInBegin = OnTransitionInBegin;
        this.Add(val);
    }

    public void RevealWhenTransition()
    {
        tiles.Alpha = 0f;
        fade = true;
        (cutout).Visible = false;
        bool flag = true;
        foreach (CustomExitBlock entity in ExitBlocks())
        {
            if (entity.group == group)
            {
                flag = false;
                break;
            }
        }

        if (flag)
        {
            this.SceneAsLevel().Session.DoNotLoad.Add(eid);
        }
    }

    private void OnTransitionOutBegin()
    {
        if (Collider.Collide(this.SceneAsLevel().Bounds))
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
        OriginalLevelSceneRoot val = this.SceneAsLevel();
        if (val.PreviousBounds.HasValue && Collider.Collide(val.PreviousBounds.Value))
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
            if (!CheckedExitBlocks)
            {
                List<object> list = new List<object>(ExitBlocks());
                foreach (CustomExitBlock item in list)
                {
                    item.Collidable = true;
                }

                foreach (CustomExitBlock entity in ExitBlocks())
                {
                    if (entity.group == group && SourceEntityCollision.Check(this, entity))
                    {
                        OverlapExitBlock = true;
                    }
                }

                foreach (CustomExitBlock item2 in list)
                {
                    item2.Collidable = item2.cutout.Alpha > 0f;
                }

                CheckedExitBlocks = true;
            }

            if (!OverlapExitBlock)
            {
                tiles.Alpha = ScalarMath.Approach(tiles.Alpha, 0f, 2f * engine.DeltaTime,
                    ((ISourceExtremaEnvironment)engine).ExtremaArithmetic);
                cutout.Alpha = tiles.Alpha;
                if (tiles.Alpha <= 0f)
                {
                    this.RemoveSelf();
                }
            }

            return;
        }

        RoomPlayerSession val = this.CollideFirst<RoomPlayerSession>(playerKey);
        if (val == null || val.Machine.State == 9)
        {
            return;
        }

        foreach (CustomFakeWall item3 in FindFakeWalls())
        {
            if (item3.group == group)
            {
                item3.Reveal();
            }
        }

        ((ISourceSpatialAudioEnvironment)engine).SpatialAudio.Play("event:/game/general/secret_revealed", this.Center);
    }

    public void Reveal()
    {
        this.SceneAsLevel().Session.DoNotLoad.Add(eid);
        fade = true;
    }

    public override void Render(ISourceRenderContext context)
    {
        if (mode == Modes.Wall) SecretWallDrawing.RenderShakeMargins(this, tiles, (ISecretWallScene)Scene, context);
        base.Render(context);
    }

    private readonly ISourceEngineContext engine;
    private readonly string playerKey;
    private string fakeKey;
    private string exitKey;
    private OriginalLevelSceneRoot SceneAsLevel() => (OriginalLevelSceneRoot)Scene;
    private List<object> ExitBlocks() => ((ISourceScene)Scene).SourceScene.Entities.GetTrackedEntities(exitKey);
    private List<object> FakeWalls() => ((ISourceScene)Scene).SourceScene.Entities.GetTrackedEntities(fakeKey);

    private List<CustomFakeWall> FindFakeWalls()
    {
        var result = new List<CustomFakeWall>();
        var all = ((ISourceScene)Scene).SourceScene.Entities.Entities;
        for (int i = 0; i < all.Count; i++)
            if (all[i] is CustomFakeWall wall)
                result.Add(wall);
        return result;
    }

    private List<CustomExitBlock> FindExitBlocks()
    {
        var result = new List<CustomExitBlock>();
        var all = ((ISourceScene)Scene).SourceScene.Entities.Entities;
        for (int i = 0; i < all.Count; i++)
            if (all[i] is CustomExitBlock wall)
                result.Add(wall);
        return result;
    }

    public static SceneEntityState Load(OriginalLevelSceneRoot level, OriginalLevelRoomData room, Vector2 offset,
        OriginalLevelEntityData data) =>
        new CustomFakeWall(level.EngineContext, data, offset, level.CreateEntityId(room, data));
}
