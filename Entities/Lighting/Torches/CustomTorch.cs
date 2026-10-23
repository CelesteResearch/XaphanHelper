using System;
using System.Numerics;
using Celeste.Core.Display;
using Celeste.Core.Components;
using Celeste.Core.Components.Animation;
using Celeste.Core.Mathematics.Easing;
using Celeste.Core.Reflection.Runtime;
using Celeste.Core.Scheduling;
using Celeste.Core.Maps;
using Celeste.Everest.Modules.Entities.Metadata;
using Celeste.Formats.Map.Identifiers;
using Celeste.Game.Content.Maps.Entities;
using Celeste.Game.Content.Maps.Levels;
using Celeste.Game.Execution.Levels;
using Celeste.Graphics.Animation;
using Celeste.Graphics.Content;
using Celeste.Graphics.Display.Colors;
using Celeste.Physics.Collision.Shapes;
using Celeste.Scene.Entities;
using Celeste.Scene.Tracking.Attributes;
using Celeste.Simulation;
using Celeste.Simulation.Gameplay.Player.Collision.Components;
using Celeste.Simulation.Presentation.Lighting;
using Celeste.Simulation.Presentation.PostProcessing.Bloom;
using Celeste.Simulation.Presentation.Particles.Definitions;

namespace Celeste.Mods.XaphanHelper.Entities.Lighting.Torches
{
    [SceneTracked(true)]
    [RuntimeTypeAlias("Celeste.Mod.XaphanHelper.Entities.CustomTorch")]
    [CustomEntity("XaphanHelper/CustomTorch")]
    public sealed class CustomTorch : SceneEntityState
    {
        private readonly ISourceEngineContext engine;
        public ParticleTypePresentation P_OnLight;

        public PackedColor Color;

        private MapEntityId eid;

        private bool lit;

        private bool playLitSound;

        private VertexLightState light;

        private BloomPointState bloom;

        private bool startLit;

        private Sprite TorchSprite;

        private string sprite;

        private string flag;

        private string FlagName => "torch_" + EntityIdCodec.Format(eid);

        private float alpha;

        private int startFade;

        private int endFade;

        private string sound;

        private bool noParticles;

        public CustomTorch(ISourceEngineContext engine, Vector2 position, Vector2 offset, string color,
            bool playLitSound, bool startLit, string sprite, string flag, float alpha, int startFade, int endFade,
            string sound, bool noParticles) : base(
            ((ISourceEntitySourceDataEnvironment)engine).EntitySourceDataRegistration, position + offset)
        {
            this.engine = engine;
            this.playLitSound = playLitSound;
            this.startLit = startLit;
            this.flag = flag;
            this.sprite = sprite;
            this.alpha = alpha;
            this.noParticles = noParticles;
            if (alpha < 0)
            {
                alpha = 0;
            }
            else if (alpha > 1)
            {
                alpha = 1;
            }

            this.startFade = startFade;
            this.endFade = endFade;
            if (endFade < startFade)
            {
                endFade = startFade;
            }

            this.sound = sound;
            if (string.IsNullOrEmpty(sound))
            {
                sound = "event:/game/05_mirror_temple/torch_activate";
            }

            Color = PackedColor.Lerp(PackedColor.White, SourceHexColor.HexToColor(color), 0.5f);
            P_OnLight = new ParticleTypePresentation
            {
                Color = SourceHexColor.HexToColor(color),
                Color2 = PackedColor.White,
                ColorMode = ParticleTypePresentation.ColorModes.Blink,
                FadeMode = ParticleTypePresentation.FadeModes.Late,
                LifeMin = 0.6f,
                LifeMax = 0.8f,
                Size = 1f,
                SpeedMin = 80f,
                SpeedMax = 90f,
                SpeedMultiplier = 0.03f,
                DirectionRange = (float)Math.PI * 2f
            };
            Collider = new HitboxColliderGeometry(32f, 32f, -8f, -8f);
            Add(new PlayerColliderComponentState(OnPlayer));
            Add(light = new VertexLightState(engine, Color, alpha, startFade, endFade));
            Add(bloom = new BloomPointState(0.5f, 8f));
            bloom.Visible = false;
            light.Visible = false;
            bloom.Position += new Vector2(8, 8);
            light.Position += new Vector2(8, 8);
            light.Color = Color;
            Depth = 2000;
            if (!string.IsNullOrEmpty(sprite))
            {
                Add(TorchSprite = new Sprite(engine, ((ISourceGameGraphicsEnvironment)engine).GameGraphics.GameAtlas,
                    sprite));
                TorchSprite.AddLoop("off", "", 0.1f, 0);
                TorchSprite.AddLoop("turnOn", "", 0.08f, 1, 2);
                TorchSprite.AddLoop("on", "", 0.08f, 3, 4, 5, 6, 7);
                TorchSprite.CenterOrigin();
                TorchSprite.Position = TorchSprite.Position + new Vector2(8, 8);
                TorchSprite.Play("off");
            }
            else
            {
                Add(TorchSprite = ((ISourceGameGraphicsEnvironment)engine).GameGraphics.SpriteFactory.Create("torch"));
            }
        }

        public CustomTorch(ISourceEngineContext engine, OriginalLevelEntityData data, Vector2 position, MapEntityId ID)
            : this(engine, data.Position, position, data.Attr("color", "ffa500"), data.Bool("playLitSound"),
                data.Bool("startLit"), data.Attr("sprite"),
                data.Attr("flag"), data.Float("alpha", 1f), data.Int("startFade", 48), data.Int("endFade", 64),
                data.Attr("sound", "event:/game/05_mirror_temple/torch_activate"), data.Bool("noParticles"))
        {
            eid = ID;
        }

        public override void Added(object scene)
        {
            base.Added(scene);
            if (string.IsNullOrEmpty(EntityIdCodec.Format(eid)))
            {
                eid = new MapEntityId(((OriginalLevelSceneRoot)Scene).Session.Level, 0);
            }

            if (startLit || ((OriginalLevelSceneRoot)Scene).Session.GetFlag(FlagName) ||
                ((OriginalLevelSceneRoot)Scene).Session.GetFlag(flag))
            {
                bloom.Visible = (light.Visible = true);
                lit = true;
                Collidable = false;
                TorchSprite.Play("on");
            }

            if (!string.IsNullOrEmpty(flag))
            {
                if (!((OriginalLevelSceneRoot)Scene).Session.GetFlag(flag))
                {
                    lit = false;
                    bloom.Visible = false;
                    light.Visible = false;
                    ((OriginalLevelSceneRoot)Scene).Session.SetFlag(FlagName, false);
                    TorchSprite.Play("off");
                }
            }
        }

        public override void Update()
        {
            base.Update();
            if (!string.IsNullOrEmpty(flag) && ((OriginalLevelSceneRoot)Scene).Session.GetFlag(flag))
            {
                if (!lit)
                {
                    if (playLitSound)
                    {
                        ((Celeste.Core.Audio.ISourceSpatialAudioEnvironment)engine).SpatialAudio.Play(sound, Position);
                    }

                    lit = true;
                    bloom.Visible = true;
                    light.Visible = true;
                    Collidable = false;
                    TorchSprite.Play("turnOn");
                    TorchSprite.OnLastFrame = onLastFrame;
                    SourceTweenState tween = SourceTweenState.Create(engine.TweenPool,
                        SourceTweenState.TweenMode.Oneshot, ((ISourceEasingEnvironment)engine).EasingArithmetic.BackOut,
                        1f, start: true);
                    tween.OnUpdate = delegate(SourceTweenState t)
                    {
                        light.Color = PackedColor.Lerp(PackedColor.White, Color, t.Eased);
                        light.StartRadius = startFade + (1f - t.Eased) * 32f;
                        light.EndRadius = endFade + (1f - t.Eased) * 32f;
                        bloom.Alpha = alpha + 0.5f * (1f - t.Eased);
                    };
                    Add(tween);
                    ((OriginalLevelSceneRoot)Scene).Session.SetFlag(FlagName);
                    if (!noParticles)
                    {
                        ((OriginalLevelSceneRoot)Scene).ParticlesFG.Emit(P_OnLight, 12, Position + new Vector2(8, 8),
                            new Vector2(3f, 3f));
                    }
                }
            }
            else if (lit && !string.IsNullOrEmpty(flag) && !((OriginalLevelSceneRoot)Scene).Session.GetFlag(flag))
            {
                lit = false;
                bloom.Visible = false;
                light.Visible = false;
                ((OriginalLevelSceneRoot)Scene).Session.SetFlag(FlagName, false);
                if (!noParticles)
                {
                    ((OriginalLevelSceneRoot)Scene).ParticlesFG.Emit(P_OnLight, 12, Position + new Vector2(8, 8),
                        new Vector2(3f, 3f));
                }

                TorchSprite.Play("off");
            }
        }

        private void onLastFrame(string s)
        {
            if (((OriginalLevelSceneRoot)Scene).Session.GetFlag(FlagName))
            {
                TorchSprite.Play("on");
            }
        }

        private void OnPlayer(RoomPlayerSession player)
        {
            if (flag == "" && !lit)
            {
                if (playLitSound)
                {
                    ((Celeste.Core.Audio.ISourceSpatialAudioEnvironment)engine).SpatialAudio.Play(sound, Position);
                }

                lit = true;
                bloom.Visible = true;
                light.Visible = true;
                Collidable = false;
                TorchSprite.Play("turnOn");
                TorchSprite.OnLastFrame = onLastFrame;
                SourceTweenState tween = SourceTweenState.Create(engine.TweenPool, SourceTweenState.TweenMode.Oneshot,
                    ((ISourceEasingEnvironment)engine).EasingArithmetic.BackOut, 1f, start: true);
                tween.OnUpdate = delegate(SourceTweenState t)
                {
                    light.Color = PackedColor.Lerp(PackedColor.White, Color, t.Eased);
                    light.StartRadius = startFade + (1f - t.Eased) * 32f;
                    light.EndRadius = endFade + (1f - t.Eased) * 32f;
                    bloom.Alpha = alpha + 0.5f * (1f - t.Eased);
                };
                Add(tween);
                ((OriginalLevelSceneRoot)Scene).Session.SetFlag(FlagName);
                if (!noParticles)
                {
                    ((OriginalLevelSceneRoot)Scene).ParticlesFG.Emit(P_OnLight, 12, Position + new Vector2(8, 8),
                        new Vector2(3f, 3f));
                }
            }
        }

        public static SceneEntityState Load(OriginalLevelSceneRoot level, OriginalLevelRoomData room, Vector2 offset,
            OriginalLevelEntityData data) =>
            new CustomTorch(level.EngineContext, data, offset, new MapEntityId(room.Name, data.ID));
    }
}
