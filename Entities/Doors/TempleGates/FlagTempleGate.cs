using System;
using System.Numerics;
using Celeste.Core.Geometry;
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
using Celeste.Graphics.Animation;
using Celeste.Graphics.Content;
using Celeste.Scene.Components.Shaking;
using Celeste.Scene.Entities;
using Celeste.Simulation.Gameplay.Platforms.Foundation;

namespace Celeste.Mods.XaphanHelper.Entities.Doors.TempleGates
{
    [RuntimeTypeAlias("Celeste.Mod.XaphanHelper.Entities.FlagTempleGate")]
    [CustomEntity("XaphanHelper/FlagTempleGate")]
    public class FlagTempleGate : SourceSolidEntityState
    {
        private readonly ISourceEngineContext engine;
        private bool horizontal;

        private bool attachRight;

        private int closedHeight;

        private Sprite sprite;

        private ShakerComponentState shaker;

        private float drawHeight;

        private float drawHeightMoveSpeed;

        private Vector2 holdingCheckFrom;

        private bool open;

        private string spriteName;

        private string flag;

        public bool startOpen;

        private bool openOnHeartCollection;

        private bool silent;

        public FlagTempleGate(EntityData data, Vector2 offset, OriginalLevelSceneRoot level) : base(
            data.Position + offset, data.Width, data.Height, true,
            level.SolidWorldFactory.Create(data.Position + offset, true))
        {
            engine = level.EngineContext;
            horizontal = data.Bool("horizontal", false);
            attachRight = data.Bool("attachRight", false);
            flag = data.Attr("flag", "");
            startOpen = data.Bool("startOpen", false);
            openOnHeartCollection = data.Bool("openOnHeartCollection");
            spriteName = data.Attr("spriteName", "default");
            silent = data.Bool("silent", false);
            closedHeight = data.Height;
            Add(sprite =
                ((ISourceGameGraphicsEnvironment)engine).GameGraphics.SpriteFactory.Create("templegate_" + spriteName));
            if (horizontal)
            {
                sprite.Rotation = -(float)Math.PI / 2f;
                if (attachRight)
                {
                    sprite.Rotation = (float)Math.PI / 2f;
                }

                sprite.Position += new Vector2(0, 5);
            }

            if (horizontal)
            {
                if (attachRight)
                {
                    sprite.Position += new Vector2(48, -1);
                }
                else
                {
                    sprite.X = Collider.Left;
                }
            }
            else
            {
                sprite.X = Collider.Width / 2;
            }

            sprite.Play("idle");
            Add(shaker = new ShakerComponentState(engine, on: false));
            Depth = -9000;
            holdingCheckFrom = Position + (horizontal
                ? new Vector2(data.Height / 2, Width / 2f)
                : new Vector2(Width / 2f, data.Height / 2));
        }

        public override void Awake(object scene)
        {
            base.Awake(scene);
            drawHeight = Math.Max(4f, Height);
            if ((!startOpen && ((OriginalLevelSceneRoot)Scene).Session.GetFlag(flag)) ||
                (startOpen && !((OriginalLevelSceneRoot)Scene).Session.GetFlag(flag)) ||
                (openOnHeartCollection && ((OriginalLevelSceneRoot)Scene).Session.HeartGem))
            {
                StartOpen();
            }
            else
            {
                SetHeight(closedHeight);
                drawHeight = Math.Max(4f, horizontal ? Width : Height);
            }
        }

        public void Open()
        {
            Collidable = false;
            if (!silent)
            {
                ((Celeste.Core.Audio.ISourceSpatialAudioEnvironment)engine).SpatialAudio.Play(
                    "event:/game/05_mirror_temple/gate_main_open", Position);
            }

            drawHeightMoveSpeed = 200f;
            drawHeight = horizontal ? Width : Height;
            shaker.ShakeFor(0.2f, removeOnFinish: false);
            SetHeight(0);
            sprite.Play("open");
            open = true;
        }

        public void Close()
        {
            Collidable = true;
            if (!silent)
            {
                ((Celeste.Core.Audio.ISourceSpatialAudioEnvironment)engine).SpatialAudio.Play(
                    "event:/game/05_mirror_temple/gate_main_close", Position);
            }

            drawHeightMoveSpeed = 300f;
            drawHeight = Math.Max(4f, horizontal ? Width : Height);
            shaker.ShakeFor(0.2f, removeOnFinish: false);
            SetHeight(closedHeight);
            sprite.Play("hit");
            open = false;
        }

        public void StartOpen()
        {
            Collidable = false;
            SetHeight(0);
            drawHeight = 4f;
            open = true;
        }

        private void SetHeight(int height)
        {
            if (!horizontal)
            {
                ClosingSolidResize.SetHeight(this, height);
            }
            else if (height < Collider.Width)
            {
                ClosingSolidResize.SetWidth(this, height);
                return;
            }
            else
            {
                ClosingSolidResize.SetWidth(this, height);
                Collider.Height = 8f;
            }
        }

        public override void Update()
        {
            base.Update();
            float num = Math.Max(4f, horizontal ? Width : Height);
            if (drawHeight != num)
            {
                drawHeight = ScalarMath.Approach(drawHeight, num, drawHeightMoveSpeed * engine.DeltaTime,
                    ((ISourceExtremaEnvironment)engine).ExtremaArithmetic);
            }

            if (((!startOpen && (((OriginalLevelSceneRoot)Scene).Session.GetFlag(flag))) ||
                 (startOpen && (!((OriginalLevelSceneRoot)Scene).Session.GetFlag(flag))) ||
                 (openOnHeartCollection && ((OriginalLevelSceneRoot)Scene).Session.HeartGem)) && !open)
            {
                Open();
            }
            else if (!openOnHeartCollection)
            {
                if (((!startOpen && !((OriginalLevelSceneRoot)Scene).Session.GetFlag(flag)) ||
                     (startOpen && ((OriginalLevelSceneRoot)Scene).Session.GetFlag(flag))) && open)
                {
                    Close();
                }
            }
            else if (openOnHeartCollection)
            {
                if (((!startOpen && !((OriginalLevelSceneRoot)Scene).Session.GetFlag(flag)) ||
                     (startOpen && ((OriginalLevelSceneRoot)Scene).Session.GetFlag(flag))) &&
                    !((OriginalLevelSceneRoot)Scene).Session.HeartGem && open)
                {
                    Close();
                }
            }
        }

        public override void Render(ISourceRenderContext context)
        {
            if (horizontal)
            {
                Vector2 value = new(0f, Math.Sign(shaker.Value.Y));
                sprite.DrawSubrect(context, Vector2.Zero + value,
                    new IntRect(0, (int)(sprite.Height - drawHeight), (int)sprite.Width, (int)drawHeight),
                    ((ISourceExtremaEnvironment)engine).ExtremaArithmetic);
            }
            else
            {
                Vector2 value = new(Math.Sign(shaker.Value.X), 0f);
                sprite.DrawSubrect(context, Vector2.Zero + value,
                    new IntRect(0, (int)(sprite.Height - drawHeight), (int)sprite.Width, (int)drawHeight),
                    ((ISourceExtremaEnvironment)engine).ExtremaArithmetic);
            }
        }

        public override void DebugRender(ISourceRenderContext context, object camera)
        {
            if (Collidable || !horizontal)
            {
                base.DebugRender(context, camera);
            }
        }

        public static SceneEntityState Load(OriginalLevelSceneRoot level, OriginalLevelRoomData room, Vector2 offset,
            OriginalLevelEntityData data) => new FlagTempleGate(data, offset, level);
    }
}
