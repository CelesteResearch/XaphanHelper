using System;
using Celeste.Core.Components;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using Celeste.Core.Components.Animation;
using Celeste.Core.Display;
using Celeste.Core.Rendering;
using Celeste.Formats.Map.Entities.Structure;
using Celeste.Core.Geometry;
using Celeste.Core.Mathematics;
using Celeste.Core.Mathematics.Easing;
using Celeste.Core.Mathematics.Extrema;
using Celeste.Core.Scheduling;
using Celeste.Everest.Modules.Entities.Metadata;
using Celeste.Everest.Gameplay.Visibility;
using Celeste.Game.Content.Maps.Entities;
using Celeste.Game.Execution.Areas;
using Celeste.Game.Execution.Levels;
using Celeste.Game.Execution.Processes;
using Celeste.Game.Execution.Startup;
using Celeste.Graphics.Animation;
using Celeste.Graphics.Content;
using Celeste.Graphics.Rendering;
using Celeste.Mods.XaphanHelper.Modules;
using Celeste.Mods.XaphanHelper.Runtime.Liquids;
using Celeste.Physics.Collision;
using Celeste.Physics.Collision.Shapes;
using Celeste.Scene.Entities;
using Celeste.Scene.Tracking;
using Celeste.Scene.Tracking.Attributes;
using Celeste.Simulation;
using Celeste.Simulation.Audio.Sources;
using Celeste.Simulation.Gameplay.Environment.Water;
using Celeste.Simulation.Gameplay.Player.Collision.Components;
using Celeste.Simulation.Presentation.PostProcessing.Displacement;
using Color = Celeste.Core.Display.PackedColor;
using Rectangle = Celeste.Core.Geometry.IntRect;
using Player = Celeste.Simulation.RoomPlayerSession;
using Tween = Celeste.Core.Components.Animation.SourceTweenState;
using SoundSource = Celeste.Simulation.Audio.Sources.SoundSourceComponentState;
using WaterInteraction = Celeste.Simulation.Gameplay.Environment.Water.WaterInteractionState;

namespace Celeste.Mods.XaphanHelper.Entities.Liquids
{
    [Celeste.Core.Reflection.Runtime.RuntimeTypeAlias("Celeste.Mod.XaphanHelper.Entities.Liquid")]
    [SceneTracked(true)]
    [CustomEntity("XaphanHelper/Liquid = Load")]
    public partial class Liquid : SceneEntityState
    {
        public static Liquid Load(OriginalLevelSceneRoot level,
            Celeste.Game.Content.Maps.Levels.OriginalLevelRoomData room, Vector2 offset, OriginalLevelEntityData data)
        {
            var modules = ((Celeste.Everest.Modules.Runtime.ISourceEverestModulesEnvironment)level.EngineContext)
                .Modules;
            foreach (var candidate in modules)
                if (candidate is XaphanHelperModule owner)
                    return new Liquid(level.EngineContext, owner, data, offset);
            throw new InvalidOperationException("XaphanHelper module has not loaded.");
        }

        private readonly ISourceEngineContext engine;
        private readonly XaphanHelperModule module;
        private readonly PackedColor[] gradientColors;
        public ISourceEngineContext EngineContext => engine;
        public bool CanSwim { get; private set; }
        public int LowPosition;

        private Sprite liquidSprite;

        private Sprite waterSplashIn;

        private Sprite waterSplashOut;

        public string LiquidType;

        private PlayerColliderComponentState pc;

        private SoundSource sfx;

        private SoundSource sfx2;


        private HashSet<WaterInteraction> contains = new();

        private bool waterIn;

        private bool waterOut;

        public bool[,] DisplacementGrid;


        private float delay;

        private string color;

        private string poisonedColor;

        private float outsideTransparency;

        private float insideTransparency;

        private float currentTransparency;

        private bool foreground;

        private bool waving;

        private bool rising;

        private bool riseEnd;

        private float riseDelay;

        private int riseDistance;

        private float riseSpeed;

        private bool riseShake;

        private string riseFlag;

        private string riseEndFlag;

        private string appearFlags;

        private string removeFlags;

        private string purifyFlags;

        private int origLevelBottom;

        private bool playerHasMoved;

        private Vector2 startPosition;

        private Vector2 finalPosition;

        private bool riseSound;

        public Celeste.Core.Audio.ISourceAudioEventReference RiseSoundSource;

        public SimulationCoroutine LiquidDamageRoutine;

        public bool FlashingRed;

        public int SurfaceHeight;

        public string Directory;

        public int CustomSurfaceHeight;

        private Tween waveTween;

        public bool VisualOnly;


        private bool upsideDown;

        public DisplacementRenderHookState Displacement;

        public int Group;

        public bool GroupLeader;

        private Liquid leader;

        public bool CanDrown;

        public float AirTimer;

        public bool VariaPreventDying;

        private float gradientTimer = 1f;

        private bool purified;

        private bool invertPurifyFlags;

        private string purifiedFlag;

        public Liquid(ISourceEngineContext engine, XaphanHelperModule module, EntityData data, Vector2 position) : base(
            ((ISourceEntitySourceDataEnvironment)engine).EntitySourceDataRegistration, data.Position + position)
        {
            this.engine = engine;
            this.module = module;
            LiquidDamageRoutine = new SimulationCoroutine(engine);
            AddTag(((IOriginalGameTagsEnvironment)engine).Tags.TransitionUpdate.Value);
            Collider = new HitboxColliderGeometry(data.Width, data.Height, 0f, 0f);
            LiquidType = data.Attr("liquidType", "acid");
            LowPosition = data.Int("lowPosition");
            delay = data.Float("frameDelay");
            color = data.Attr("color");
            poisonedColor = data.Attr("poisonedColor", "4c9a42");
            purifiedFlag = data.Attr("purifiedFlag");
            outsideTransparency = data.Float("transparency");
            insideTransparency = data.Float("insideTransparency", outsideTransparency);
            foreground = data.Bool("foreground");
            riseDelay = data.Float("riseDelay");
            riseDistance = data.Int("riseDistance");
            riseSpeed = data.Float("riseSpeed");
            riseShake = data.Bool("riseShake");
            riseFlag = data.Attr("riseFlag");
            riseEndFlag = data.Attr("riseEndFlag");
            appearFlags = data.Attr("appearFlags");
            removeFlags = data.Attr("removeFlags");
            purifyFlags = data.Attr("purifyFlags");
            invertPurifyFlags = data.Bool("invertPurifyFlags");
            riseSound = data.Bool("riseSound");
            Directory = data.Attr("directory");
            CustomSurfaceHeight = data.Int("surfaceHeight", 0);
            VisualOnly = data.Bool("visualOnly", false);
            CanSwim = data.Bool("canSwim", false);
            upsideDown = data.Bool("upsideDown", false);
            Group = data.Int("group", -1);
            CanDrown = data.Bool("canDrown", false);
            AirTimer = data.Float("airTimer", 15f);
            VariaPreventDying = data.Bool("variaPreventDying", false);
            startPosition = Position;
            finalPosition = Position - new Vector2(0, riseDistance);
            if (delay == 0)
            {
                delay = 0.15f;
            }

            if (riseSpeed <= 0)
            {
                riseSpeed = 1;
            }

            if (string.IsNullOrEmpty(color))
            {
                switch (LiquidType)
                {
                    case "acid":
                        color = "88C098";
                        break;
                    case "acid_b":
                        color = "88C098";
                        break;
                    case "lava":
                        color = "F85818";
                        break;
                    case "quicksand":
                        color = "C8B078";
                        break;
                    case "water":
                        color = "669CEE";
                        break;
                    default:
                        color = "FFFFFF";
                        break;
                }
            }

            gradientColors = CreateGradientPalette(Color.FromHex(color), Color.FromHex(poisonedColor));
            if (outsideTransparency <= 0f)
            {
                outsideTransparency = 0.65f;
            }

            if (outsideTransparency >= 1f)
            {
                outsideTransparency = 1f;
            }

            if (insideTransparency <= 0f || insideTransparency >= outsideTransparency)
            {
                insideTransparency = outsideTransparency;
            }

            Depth = foreground ? -19999 : -9999;
            if (LiquidType == "lava")
            {
                Add(sfx = new SoundSource(engine));
                if (data.Width <= 320)
                {
                    sfx.Position = new Vector2(data.Width, data.Height) / 2f;
                }
                else
                {
                    Add(sfx2 = new SoundSource(engine));
                    sfx.Position = new Vector2(data.Width / 4f, data.Height / 2f);
                    sfx2.Position = new Vector2(data.Width / 4f * 3f, data.Height / 2f);
                }
            }

            if (LiquidType == "water")
            {
                DisplacementGrid = new bool[(int)(Collider.Width / 8f), (int)(Collider.Height / 8f)];
                Add(Displacement = new DisplacementRenderHookState(RenderDisplacement));
            }

            if (CustomSurfaceHeight == 0)
            {
                if (LiquidType == "lava")
                {
                    SurfaceHeight = 8;
                }
                else if (LiquidType == "water" || LiquidType == "quicksand")
                {
                    SurfaceHeight = 16;
                }
                else if (LiquidType.Contains("acid"))
                {
                    SurfaceHeight = 24;
                }
            }
            else
            {
                SurfaceHeight = CustomSurfaceHeight * 8;
            }

            Add(liquidSprite = new Sprite(engine, ((ISourceGameGraphicsEnvironment)engine).GameGraphics.GameAtlas,
                (string.IsNullOrEmpty(Directory) ? "objects/XaphanHelper/liquid" : Directory) + "/" + LiquidType +
                "/"));
            liquidSprite.AddLoop("liquid", "liquid", delay);
            liquidSprite.Play("liquid");
            Add(waterSplashIn = new Sprite(engine, ((ISourceGameGraphicsEnvironment)engine).GameGraphics.GameAtlas,
                "objects/XaphanHelper/liquid/water/"));
            waterSplashIn.AddLoop("splash", "splash", 0.04f);
            Add(waterSplashOut = new Sprite(engine, ((ISourceGameGraphicsEnvironment)engine).GameGraphics.GameAtlas,
                "objects/XaphanHelper/liquid/water/"));
            waterSplashOut.AddLoop("splash", "splash", 0.04f);
            Add(pc = new PlayerColliderComponentState(OnCollide));
            if (upsideDown)
            {
                liquidSprite.FlipY = true;
                waterSplashIn.FlipY = true;
                waterSplashOut.FlipY = true;
            }
        }
    }
}
