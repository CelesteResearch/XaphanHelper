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
[RuntimeTypeAlias("Celeste.Mod.XaphanHelper.Entities.CustomExitBlock")]
[CustomEntity(new string[] { "XaphanHelper/CustomExitBlock" })]
public class CustomExitBlock : SourceSolidEntityState
{
    public enum Modes
    {
        Wall,
        Block
    }

    private Modes mode;

    private TileGrid tiles;

    private RoomTransitionListener tl;

    public EffectCutoutState cutout;

    private float startAlpha;

    private char fillTile;

    private char flagFillTile;

    private bool CloseSound;

    public int group;

    private string flag;

    public CustomExitBlock(ISourceEngineContext engine, EntityData data, Vector2 offset, ISourceSolidWorld world)
        : base(data.Position + offset, data.Width, data.Height, true, world)
    {
        this.engine = engine;
        playerKey = ((ISourceSceneCatalogEnvironment)engine).SceneCatalog.Find(typeof(RoomPlayerSession)).RegistryKey;
        this.Depth = -13000;
        mode = data.Enum<Modes>("mode", Modes.Block);
        flag = data.Attr("flag", "");
        flagFillTile = data.Char("flagTiletype", '3');
        fillTile = data.Char("tiletype", '3');
        CloseSound = data.Bool("closeSound", false);
        group = data.Int("group", 0);
        tl = new RoomTransitionListener();
        tl.OnOutBegin = OnTransitionOutBegin;
        tl.OnInBegin = OnTransitionInBegin;
        this.Add(tl);
        EffectCutoutState val = new EffectCutoutState();
        EffectCutoutState val2 = val;
        cutout = val;
        this.Add(val2);
        this.SurfaceSoundIndex = ((IOriginalTileSurfaceEnvironment)engine).SurfaceIndices.TileToIndex[fillTile];
        base.EnableAssistModeChecks = false;
    }

    private void OnTransitionOutBegin()
    {
        if (Collider.Collide(this.SceneAsLevel().Bounds))
        {
            tl.OnOut = OnTransitionOut;
            startAlpha = tiles.Alpha;
        }
    }

    private void OnTransitionOut(float percent)
    {
        cutout.Alpha = (tiles.Alpha = startAlpha + (0f - startAlpha) * percent);
        (cutout).Update();
    }

    private void OnTransitionInBegin()
    {
        if (Collider.Collide(this.SceneAsLevel().PreviousBounds.Value) && !this.CollideCheck(playerKey))
        {
            cutout.Alpha = 0f;
            tiles.Alpha = 0f;
            tl.OnIn = OnTransitionIn;
        }
    }

    private void OnTransitionIn(float percent)
    {
        cutout.Alpha = (tiles.Alpha = percent);
        (cutout).Update();
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
        int num = 0;
        foreach (CustomExitBlock item in FindExitBlocks())
        {
            if (item.group == group && item.CollideCheck(playerKey))
            {
                num++;
            }
        }

        if (num > 0)
        {
            cutout.Alpha = (tiles.Alpha = 0f);
            this.Collidable = false;
        }
    }

    public override void Update()
    {
        base.Update();
        int num = 0;
        exitSnapshot.Clear();
        var all = ((ISourceScene)Scene).SourceScene.Entities.Entities;
        for (int i = 0; i < all.Count; i++)
            if (all[i] is CustomExitBlock exit)
                exitSnapshot.Add(exit);
        foreach (CustomExitBlock item in exitSnapshot)
        {
            if (item.group == group && item.CollideCheck(playerKey))
            {
                this.SceneAsLevel().Session.SetFlag(GroupFlag, true);
                num++;
            }
        }

        if (num == 0)
        {
            this.SceneAsLevel().Session.SetFlag(GroupFlag, false);
        }

        if (this.Collidable)
        {
            cutout.Alpha = (tiles.Alpha = ScalarMath.Approach(tiles.Alpha, 1f, engine.DeltaTime,
                ((ISourceExtremaEnvironment)engine).ExtremaArithmetic));
            {
                foreach (CustomFakeWall entity in FakeWalls())
                {
                    if (entity.group == group && SourceEntityCollision.Check(this, entity))
                    {
                        entity.fade = true;
                        entity.tiles.Alpha = (entity.cutout.Alpha = tiles.Alpha);
                    }
                }

                return;
            }
        }

        if (!this.SceneAsLevel().Session.GetFlag(GroupFlag))
        {
            this.Collidable = true;
            if (CloseSound)
            {
                ((ISourceSpatialAudioEnvironment)engine).SpatialAudio.Play("event:/game/general/passage_closed_behind",
                    this.Center);
            }
        }
    }

    public override void Render(ISourceRenderContext context)
    {
        if (tiles.Alpha >= 1f) SecretWallDrawing.RenderShakeMargins(this, tiles, (ISecretWallScene)Scene, context);
        base.Render(context);
    }

    private readonly List<CustomExitBlock> exitSnapshot = new List<CustomExitBlock>();
    private int flagGroup;
    private string groupFlag;

    private string GroupFlag
    {
        get
        {
            if (groupFlag == null || flagGroup != group)
            {
                flagGroup = group;
                groupFlag = "Inside_ExitBlock_" + group;
            }

            return groupFlag;
        }
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
        OriginalLevelEntityData data) => new CustomExitBlock(level.EngineContext, data, offset,
        level.SolidWorldFactory.Create(data.Position + offset, true));
}
