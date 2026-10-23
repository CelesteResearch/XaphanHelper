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
    [SceneTracked(true)]
    [RuntimeTypeAlias("Celeste.Mod.XaphanHelper.Entities.CustomMoveBlock")]
    [CustomEntity("XaphanHelper/CustomMoveBlock")]
    public partial class CustomMoveBlock : SourceSolidEntityState
    {
        private ISourceEngineContext Engine => SourceWorld.EngineContext;
        private ISourceRandom Random => ((ISourceRandomEnvironment)Engine).Random;

        public static CustomMoveBlock Load(OriginalLevelSceneRoot level, OriginalLevelRoomData room, Vector2 offset,
            OriginalLevelEntityData data) => new(level, data, offset);

        internal static IDisposable RegisterDebrisPool(ISourceEngineContext engine) =>
            engine.EntityPools.RegisterScoped(() =>
                new Debris(engine, ((ISourcePlatformTargetMotionEnvironment)engine).TargetMotionPolicy));

        [RuntimeTypeAlias("Celeste.Mod.XaphanHelper.Entities.CustomMoveBlock+Directions")]
        public enum Directions
        {
            Left,
            Right,
            Up,
            Down
        }

        [RuntimeTypeAlias("Celeste.Mod.XaphanHelper.Entities.CustomMoveBlock+MovementState")]
        private enum MovementState
        {
            Idling,
            Moving,
            Breaking
        }


        public static ParticleTypePresentation P_Activate;

        public static ParticleTypePresentation P_Break;

        public static ParticleTypePresentation P_Move;

        public string steerSides;

        public Directions direction;

        private float homeAngle;

        private int angleSteerSign;

        private Vector2 startPosition;

        private MovementState state;

        private bool leftPressed;

        private bool rightPressed;

        private bool topPressed;

        private bool bottomPressed;

        private float currentSpeed;

        private float targetSpeed;

        private float speed;

        private float acceleration;

        private float angle;

        private float targetAngle;

        private float respawnTime;

        private float respawnTimer;

        private RoomPlayerSession noSquish;

        private List<SpriteImage> body = new List<SpriteImage>();

        private List<SpriteImage> topButton = new List<SpriteImage>();

        private List<SpriteImage> bottomButton = new List<SpriteImage>();

        private List<SpriteImage> leftButton = new List<SpriteImage>();

        private List<SpriteImage> rightButton = new List<SpriteImage>();

        private List<AtlasEntry> arrows = new List<AtlasEntry>();

        private Border border;

        private PackedColor fillColor;

        private float flash;

        private SoundSourceComponentState moveSfx;

        private bool triggered;

        private PackedColor idleBgFill;

        private PackedColor pressedBgFill;

        private PackedColor breakingBgFill;

        private float particleRemainder;

        private string directory;

        private bool oneUse;

        private bool addBorder;

        private bool buttonIgnoreColors;

        public int buttonPressedOffset;

        public int magneticCeilingOffset;

        private bool glow;

        private float alpha = 0f;

        private List<OutlinePoint> outline;

        private float outlineColorStrength;

        private float outlineColorTimer;

        private bool Destroyed;

        private SimulationCoroutine outlineFader;

        public CustomMoveBlock(OriginalLevelSceneRoot level, OriginalLevelEntityData data, Vector2 offset)
            : base(data.Position + offset, data.Width, data.Height, false,
                level.SolidWorldFactory.Create(data.Position + offset, false))
        {
            Tags = ((Celeste.Scene.Scheduling.ExecutionTags.ISourcePersistentTransitionExecutionTagsEnvironment)Engine)
                .PersistentTransitionExecutionTags.TransitionUpdate.Value;
            startPosition = Position;
            directory = data.Attr("directory", "objects/moveBlock");
            idleBgFill = SourceHexColor.HexToColor(data.Attr("idleColor", "474070"));
            pressedBgFill = SourceHexColor.HexToColor(data.Attr("moveColor", "30b335"));
            breakingBgFill = SourceHexColor.HexToColor(data.Attr("breakColor", "cc2541"));
            addBorder = data.Bool("addBorder", defaultValue: true);
            fillColor = idleBgFill;
            steerSides = data.Attr("steerSides", "None");
            direction = data.Enum("direction", Directions.Right);
            speed = data.Float("speed", 60f);
            acceleration = data.Float("acceleration", 300f);
            respawnTime = data.Float("respawnTime", 3f);
            oneUse = data.Bool("oneUse");
            glow = data.Bool("glow");
            buttonIgnoreColors = data.Bool("buttonIgnoreColors");
            buttonPressedOffset = data.Int("buttonPressedOffset", 2);
            magneticCeilingOffset = (steerSides.Contains("Bottom") ? data.Int("magneticCeilingOffset", 2) : 0);
            switch (direction)
            {
                default:
                    homeAngle = (targetAngle = (angle = 0f));
                    angleSteerSign = 1;
                    break;
                case Directions.Left:
                    homeAngle = (targetAngle = (angle = MathF.PI));
                    angleSteerSign = -1;
                    break;
                case Directions.Up:
                    homeAngle = (targetAngle = (angle = -MathF.PI / 2f));
                    angleSteerSign = 1;
                    break;
                case Directions.Down:
                    homeAngle = (targetAngle = (angle = MathF.PI / 2f));
                    angleSteerSign = -1;
                    break;
            }

            int num = (int)base.Width / 8;
            int num2 = (int)base.Height / 8;
            AtlasEntry mTexture = ((ISourceGameGraphicsEnvironment)Engine).GameGraphics.GameAtlas[directory + "/base"];
            AtlasEntry mTexture2 =
                ((ISourceGameGraphicsEnvironment)Engine).GameGraphics.GameAtlas[directory + "/button"];
            if ((steerSides == "TopOnly" || steerSides == "BottomOnly" || steerSides == "TopAndBottom") &&
                (direction == Directions.Left || direction == Directions.Right))
            {
                if (steerSides.Contains("Top"))
                {
                    for (int i = 0; i < num; i++)
                    {
                        int num3 = ((i != 0) ? ((i < num - 1) ? 1 : 2) : 0);
                        AddImage(mTexture2.GetSubtexture(num3 * 8, 0, 8, 8), new Vector2(i * 8, -4f), 0f,
                            new Vector2(1f, 1f), topButton);
                    }
                }

                if (steerSides.Contains("Bottom"))
                {
                    for (int j = 0; j < num; j++)
                    {
                        int num4 = ((j != 0) ? ((j < num - 1) ? 1 : 2) : 0);
                        AddImage(mTexture2.GetSubtexture(num4 * 8, 0, 8, 8), new Vector2(j * 8, -4f), 0f,
                            new Vector2(1f, 1f), bottomButton);
                    }
                }

                string text = ((steerSides == "TopOnly") ? "" : ((steerSides == "BottomOnly") ? "b" : "tb"));
                mTexture = ((ISourceGameGraphicsEnvironment)Engine).GameGraphics
                    .GameAtlas[directory + "/base_h" + text];
            }
            else if ((steerSides == "LeftOnly" || steerSides == "RightOnly" || steerSides == "LeftAndRight") &&
                     (direction == Directions.Up || direction == Directions.Down))
            {
                if (steerSides.Contains("Left"))
                {
                    for (int k = 0; k < num2; k++)
                    {
                        int num5 = ((k != 0) ? ((k < num2 - 1) ? 1 : 2) : 0);
                        AddImage(mTexture2.GetSubtexture(num5 * 8, 0, 8, 8), new Vector2(-4f, k * 8), MathF.PI / 2f,
                            new Vector2(1f, -1f), leftButton);
                    }
                }

                if (steerSides.Contains("Right"))
                {
                    for (int l = 0; l < num2; l++)
                    {
                        int num6 = ((l != 0) ? ((l < num2 - 1) ? 1 : 2) : 0);
                        AddImage(mTexture2.GetSubtexture(num6 * 8, 0, 8, 8), new Vector2((num - 1) * 8 + 4, l * 8),
                            MathF.PI / 2f, new Vector2(1f, 1f), rightButton);
                    }
                }

                string text2 = ((steerSides == "LeftOnly") ? "l" : ((steerSides == "RightOnly") ? "r" : ""));
                mTexture =
                    ((ISourceGameGraphicsEnvironment)Engine).GameGraphics.GameAtlas[directory + "/base_v" + text2];
            }

            for (int m = 0; m < num; m++)
            {
                for (int n = 0; n < num2; n++)
                {
                    int num7 = ((m != 0) ? ((m < num - 1) ? 1 : 2) : 0);
                    int num8 = ((n != 0) ? ((n < num2 - 1) ? 1 : 2) : 0);
                    AddImage(mTexture.GetSubtexture(num7 * 8, num8 * 8, 8, 8), new Vector2(m, n) * 8f, 0f,
                        new Vector2(1f, 1f), body);
                }
            }

            arrows =
                ((ISourceGameGraphicsEnvironment)Engine).GameGraphics.GameAtlas.GetAtlasSubtextures(
                    directory + "/arrow");
            Add(moveSfx = new SoundSourceComponentState(Engine));
            Add(new SimulationCoroutine(Engine, Controller()));
            UpdateColors();
            Add(new LightOccludeState(0.5f));
            P_Activate = new ParticleTypePresentation
            {
                Size = 1f,
                Color = SourceHexColor.HexToColor(data.Attr("particlesColor", "000000")),
                FadeMode = ParticleTypePresentation.FadeModes.Late,
                DirectionRange = 0.34906584f,
                LifeMin = 0.4f,
                LifeMax = 0.6f,
                SpeedMin = 20f,
                SpeedMax = 40f,
                SpeedMultiplier = 0.25f
            };
            P_Break = new ParticleTypePresentation(P_Activate);
            P_Move = new ParticleTypePresentation(P_Activate);
        }

        public override void Awake(object scene)
        {
            base.Awake(scene);
            if (addBorder)
            {
                ((ISourceScene)scene).SourceScene.QueueSourceEntity(border = new Border(this));
            }
        }

        public override void Added(object scene)
        {
            base.Added(scene);
            outline = OutlinePoint.GenerateSolidOutline(this);
            outlineColorTimer = respawnTime / (float)outline.Count;
            if (!oneUse)
            {
                Add(outlineFader = new SimulationCoroutine(Engine));
                outlineFader.RemoveOnComplete = false;
            }
        }


        public override void Update()
        {
            SlopeActorCollision.BeforeUpdate(this);
            base.Update();
            alpha += Engine.DeltaTime * 4f;
            if (steerSides != "None")
            {
                bool flag = (direction == Directions.Up || direction == Directions.Down) &&
                            CheckTracked<RoomPlayerSession>(Position + new Vector2(-1f, 0f));
                bool flag2 = (direction == Directions.Up || direction == Directions.Down) &&
                             CheckTracked<RoomPlayerSession>(Position + new Vector2(1f, 0f));
                bool flag3 = (direction == Directions.Left || direction == Directions.Right) &&
                             CheckTracked<RoomPlayerSession>(Position + new Vector2(0f, -1f));
                bool flag4 = (direction == Directions.Left || direction == Directions.Right) &&
                             CheckTracked<RoomPlayerSession>(Position + new Vector2(0f,
                                 6f + (float)magneticCeilingOffset)) &&
                             ((OriginalLevelSceneRoot)Scene).Session.GetFlag("Xaphan_Helper_Ceiling");
                foreach (SpriteImage item in leftButton)
                {
                    item.Position.X = (flag ? buttonPressedOffset : 0);
                }

                foreach (SpriteImage item2 in rightButton)
                {
                    item2.Position.X = base.Width + (float)(flag2 ? (-buttonPressedOffset) : 0);
                }

                foreach (SpriteImage item3 in topButton)
                {
                    item3.Position.Y = (flag3 ? buttonPressedOffset : 0);
                }

                foreach (SpriteImage item4 in bottomButton)
                {
                    item4.Scale = new Vector2(1f, -1f);
                    item4.Position.Y = base.Height + (float)(flag4 ? buttonPressedOffset : 0);
                }

                if ((flag && !leftPressed) || (flag3 && !topPressed) || (flag2 && !rightPressed) ||
                    (flag4 && !bottomPressed))
                {
                    ((ISourceSpatialAudioEnvironment)Engine).SpatialAudio.Play(
                        "event:/game/04_cliffside/arrowblock_side_depress", Position);
                }

                if ((!flag && leftPressed) || (!flag3 && topPressed) || (!flag2 && rightPressed) || (!flag4 &&
                        bottomPressed && !((OriginalLevelSceneRoot)Scene).Session.GetFlag("Xaphan_Helper_Ceiling")))
                {
                    ((ISourceSpatialAudioEnvironment)Engine).SpatialAudio.Play(
                        "event:/game/04_cliffside/arrowblock_side_release", Position);
                }

                leftPressed = flag;
                rightPressed = flag2;
                topPressed = flag3;
                bottomPressed = flag4;
            }

            if (moveSfx != null && moveSfx.Playing)
            {
                int num = (int)Math.Floor(
                    (0f - Vector2Geometry.Angle(Vector2Geometry.AngleToVector(angle, 1f) * new Vector2(-1f, 1f)) +
                     MathF.PI * 2f) % (MathF.PI * 2f) / (MathF.PI * 2f) * 8f + 0.5f);
                moveSfx.Param("arrow_influence", num + 1);
            }

            if (border != null)
            {
                border.Visible = Visible;
            }

            flash = ScalarMath.Approach(flash, 0f, Engine.DeltaTime * 5f);
            UpdateColors();
            SlopeActorCollision.AfterUpdate(this);
        }

        public override void OnStaticMoverTrigger(SourceStaticMoverState sm)
        {
            triggered = true;
        }
    }
}
