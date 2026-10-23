using System;
using System.Numerics;
using Celeste.Core.Audio;
using Celeste.Core.Components;
using Celeste.Core.Display;
using Celeste.Core.Geometry;
using Celeste.Core.Mathematics;
using Celeste.Core.Mathematics.Extrema;
using Celeste.Core.Mathematics.Framework;
using Celeste.Core.Randomness;
using Celeste.Core.Reflection.Runtime;
using Celeste.Core.Rendering;
using Celeste.Core.Scheduling;
using Celeste.Everest.Modules.Entities.Metadata;
using Celeste.Formats.Map.Entities.Structure;
using Celeste.Game.Content.Maps.Entities;
using Celeste.Game.Content.Maps.Levels;
using Celeste.Game.Execution.Levels;
using Celeste.Graphics.Animation;
using Celeste.Graphics.Content;
using Celeste.Graphics.Content.Atlas;
using Celeste.Graphics.Display.Colors;
using Celeste.Graphics.Rendering.Drawing;
using Celeste.Graphics.Rendering;
using Celeste.Scene.Entities;
using Celeste.Scene.Tracking;
using Celeste.Simulation.Gameplay.Platforms.Foundation;
using Celeste.Simulation.Gameplay.Player.Dash;
using Celeste.Simulation.Presentation.Lighting;
using Celeste.Simulation.Presentation.Particles.Definitions;
using Celeste.Simulation.Presentation.PostProcessing.Displacement;

namespace Celeste.Mods.XaphanHelper.Entities.Platforms.SwapBlocks;

[CustomEntity(new string[] { "XaphanHelper/FlagSwapBlock", "XaphanHelper/CustomSwapBlock" })]
[RuntimeTypeAlias("Celeste.Mod.XaphanHelper.Entities.FlagSwapBlock")]
public partial class FlagSwapBlock : SourceSolidEntityState
{
    public ParticleTypePresentation P_Move;

    private const float ReturnTime = 0.8f;

    public Vector2 Direction;

    public bool Swapping;

    private Vector2 start;

    private Vector2 end;

    private float lerp;

    private int target;

    private IntRect moveRect;

    private float setSpeed;

    private float speed;

    private float maxForwardSpeed;

    private float maxBackwardSpeed;

    private float returnTimer;

    private float redAlpha = 1f;

    private AtlasEntry[,] nineSliceGreen;

    private AtlasEntry[,] nineSliceRed;

    private AtlasEntry[,] nineSliceTarget;

    private Sprite middleGreen;

    private Sprite middleRed;

    private PathRenderer path;

    private ISourceAudioEventReference moveSfx;

    private ISourceAudioEventReference returnSfx;

    private DisplacementBurst burst;

    private float particlesRemainder;

    public string flag;

    public string directory = "objects/swapblock";

    public bool isActive;

    public bool toggle;

    private bool renderBG;

    public FlagSwapBlock(EntityData data, Vector2 offset, ISourceSolidWorld world)
        : base(data.Position + offset, data.Width, data.Height, false, world)
    {
        engine = world.EngineContext;
        directory = data.Attr("directory", "objects/swapblock");
        if (directory == "objects/swapblock")
        {
            directory = data.Attr("sprite", "objects/swapblock");
        }

        flag = data.Attr("flag", "");
        toggle = data.Bool("toggle", false);
        renderBG = data.Bool("renderBG", true);
        P_Move = new ParticleTypePresentation
        {
            Size = 1f,
            Color = SourceHexColor.HexToColor(data.Attr("particleColor1", "fbf236")),
            Color2 = SourceHexColor.HexToColor(data.Attr("particleColor2", "6abe30")),
            ColorMode = (ParticleTypePresentation.ColorModes)2,
            DirectionRange = 0.6981317f,
            SpeedMin = 10f,
            SpeedMax = 20f,
            SpeedMultiplier = 0.3f,
            LifeMin = 0.3f,
            LifeMax = 0.5f
        };
        Vector2 val = data.Nodes[0] + offset;
        redAlpha = 1f;
        start = this.Position;
        end = val;
        setSpeed = data.Float("speed", 0f);
        maxForwardSpeed = data.Float("speed", 360f) / Vector2Math.Distance(start, end);
        maxBackwardSpeed = maxForwardSpeed * 0.4f;
        Direction.X = Math.Sign(end.X - start.X);
        Direction.Y = Math.Sign(end.Y - start.Y);
        this.Add(new DashListenerState
        {
            OnDash = OnDash
        });
        int num = (int)SourceFrameworkMath.Min(this.X, val.X);
        int num2 = (int)SourceFrameworkMath.Min(this.Y, val.Y);
        int num3 = (int)SourceFrameworkMath.Max(this.X + this.Width, val.X + this.Width);
        int num4 = (int)SourceFrameworkMath.Max(this.Y + this.Height, val.Y + this.Height);
        moveRect = new IntRect(num, num2, num3 - num, num4 - num2);
        AtlasEntry val2 = ((ISourceGameGraphicsEnvironment)engine).GameGraphics.GameAtlas[directory + "/block"];
        AtlasEntry val3 = ((ISourceGameGraphicsEnvironment)engine).GameGraphics.GameAtlas[directory + "/blockRed"];
        AtlasEntry val4 = ((ISourceGameGraphicsEnvironment)engine).GameGraphics.GameAtlas[directory + "/target"];
        nineSliceGreen = new AtlasEntry[3, 3];
        nineSliceRed = new AtlasEntry[3, 3];
        nineSliceTarget = new AtlasEntry[3, 3];
        for (int i = 0; i < 3; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                nineSliceGreen[i, j] = val2.GetSubtexture(i * 8, j * 8, 8, 8);
                nineSliceRed[i, j] = val3.GetSubtexture(i * 8, j * 8, 8, 8);
                nineSliceTarget[i, j] = val4.GetSubtexture(i * 8, j * 8, 8, 8);
            }
        }

        middleGreen = new Sprite(engine, ((ISourceGameGraphicsEnvironment)engine).GameGraphics.GameAtlas,
            directory + "/midBlock");
        middleGreen.AddLoop("idle", "", 0.08f);
        middleGreen.Justify = new Vector2(0.5f, 0.5f);
        middleGreen.Play("idle", false, false);
        this.Add(middleGreen);
        middleRed = new Sprite(engine, ((ISourceGameGraphicsEnvironment)engine).GameGraphics.GameAtlas,
            directory + "/midBlockRed");
        middleRed.AddLoop("idle", "", 0.08f);
        middleRed.Justify = new Vector2(0.5f, 0.5f);
        middleRed.Play("idle", false, false);
        this.Add(middleRed);
        this.Add(new LightOccludeState(0.2f));
        this.Depth = -9999;
    }

    public override void Awake(object scene)
    {
        base.Awake(scene);
        ((ISourceScene)scene).SourceScene.QueueSourceEntity(path = new PathRenderer(this));
    }

    public override void Removed(object scene)
    {
        base.Removed(scene);
        ((ISourceSpatialAudioEnvironment)engine).SpatialAudio.Stop(moveSfx, true);
        ((ISourceSpatialAudioEnvironment)engine).SpatialAudio.Stop(returnSfx, true);
    }

    public override void SceneEnd(object scene)
    {
        base.SceneEnd(scene);
        ((ISourceSpatialAudioEnvironment)engine).SpatialAudio.Stop(moveSfx, true);
        ((ISourceSpatialAudioEnvironment)engine).SpatialAudio.Stop(returnSfx, true);
    }

    private readonly ISourceEngineContext engine;
    private OriginalLevelSceneRoot SceneAsLevel() => (OriginalLevelSceneRoot)Scene;

    public static SceneEntityState Load(OriginalLevelSceneRoot level, OriginalLevelRoomData room, Vector2 offset,
        OriginalLevelEntityData data) =>
        new FlagSwapBlock(data, offset, level.SolidWorldFactory.Create(data.Position + offset, false));
}
