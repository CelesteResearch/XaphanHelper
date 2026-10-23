using System.Collections;
using Celeste.Core.Components;
using System.Numerics;
using Celeste.Core.Display;
using Celeste.Core.Maps;
using Celeste.Core.Mathematics;
using Celeste.Core.Reflection.Runtime;
using Celeste.Core.Rendering;
using Celeste.Core.Scheduling;
using Celeste.Everest.Modules.Entities.Metadata;
using Celeste.Formats.Map.Entities.Structure;
using Celeste.Game.Content.Maps.Entities;
using Celeste.Game.Execution.Levels;
using Celeste.Game.Execution.Processes;
using Celeste.Game.Execution.Startup;
using Celeste.Graphics.Animation;
using Celeste.Graphics.Content;
using Celeste.Mods.XaphanHelper.Entities.Platforms.MoveBlocks;
using Celeste.Mods.XaphanHelper.Runtime.MagneticCeilings;
using Celeste.Physics.Collision;
using Celeste.Physics.Collision.Shapes;
using Celeste.Scene.Entities;
using Celeste.Scene.Tracking;
using Celeste.Scene.Tracking.Attributes;
using Celeste.Simulation;
using Celeste.Simulation.Gameplay.Platforms.Foundation;
using Celeste.Simulation.Gameplay.Platforms.StaticMovers;
using Celeste.Simulation.Gameplay.Player.Collision.Components;

namespace Celeste.Mods.XaphanHelper.Entities.Platforms.MagneticCeilings;

[SceneTracked(true)]
[RuntimeTypeAlias("Celeste.Mod.XaphanHelper.Entities.MagneticCeiling")]
[CustomEntity("XaphanHelper/MagneticCeiling")]
public partial class MagneticCeiling : SceneEntityState
{
    private readonly ISourceEngineContext engine;
    private readonly MagneticCeilingRuntime runtime;
    private readonly string ceilingKey;
    private readonly Sprite spriteA;
    private readonly Sprite spriteB;
    private bool PlayerCollideSolid;
    public bool CanJump;
    public string Directory;
    public float AnimationSpeed;
    private bool playSfx;
    private readonly PlayerColliderComponentState pc;
    public MapEntityId ID;
    private readonly SourceStaticMoverState staticMover;
    private Vector2 imageOffset;
    public bool NoStaminaDrain;
    public bool JumpGracePeriod;
    public bool playerWasAttached;
    public string PlayerPose = "";
    private SimulationCoroutine JumpGraceTimerRoutine;
    public CustomMoveBlock attachedMoveBlock;
    public PackedColor EnabledColor = PackedColor.White;
    public PackedColor DisabledColor = PackedColor.White;
    public bool VisibleWhenDisabled;
    public bool ForceDetachPlayer;
    private OriginalLevelSceneRoot Level => (OriginalLevelSceneRoot)Scene;

    public MagneticCeiling(ISourceEngineContext engine, EntityData data, Vector2 offset, MapEntityId eid)
        : this(engine, data.Position, offset, eid, data.Width, data.Attr("directory", ""),
            data.Float("animationSpeed", 0f), data.Bool("canJump", false), data.Bool("noStaminaDrain", false))
    {
    }

    public MagneticCeiling(ISourceEngineContext engine, Vector2 position, Vector2 offset, MapEntityId eid,
        float width, string directory, float animationSpeed, bool canJump, bool noStaminaDrain)
        : base(((ISourceEntitySourceDataEnvironment)engine).EntitySourceDataRegistration)
    {
        this.engine = engine;
        runtime = MagneticCeilingRuntime.For(engine);
        ceilingKey = ((ISourceSceneCatalogEnvironment)engine).SceneCatalog.Find(typeof(MagneticCeiling)).RegistryKey;
        Position = position + offset;
        AddTag(((IOriginalGameTagsEnvironment)engine).Tags.TransitionUpdate.Value);
        ID = eid;
        Collider = new HitboxColliderGeometry(width, 4f);
        Directory = string.IsNullOrEmpty(directory) ? "objects/XaphanHelper/MagneticCeiling" : directory;
        AnimationSpeed = animationSpeed;
        CanJump = canJump;
        NoStaminaDrain = noStaminaDrain;
        JumpGraceTimerRoutine = new SimulationCoroutine(engine, true);
        var atlas = ((ISourceGameGraphicsEnvironment)engine).GameGraphics.GameAtlas;
        Add(spriteA = new Sprite(engine, atlas, Directory + "/"));
        spriteA.Position = Vector2.Zero;
        spriteA.AddLoop("idle", "idle_a", AnimationSpeed);
        spriteA.Play("idle");
        Add(spriteB = new Sprite(engine, atlas, Directory + "/"));
        spriteB.Position = new Vector2(8f, 0f);
        spriteB.AddLoop("idle", "idle_b", AnimationSpeed);
        spriteB.Play("idle");
        Add(pc = new PlayerColliderComponentState(OnCollide, new HitboxColliderGeometry(width, 5f)));
        Add(staticMover = new SourceStaticMoverState
        {
            OnShake = OnShake, SolidChecker = IsRiding, JumpThruChecker = IsRiding,
            OnEnable = OnEnable, OnDisable = OnDisable, OnMove = OnMove
        });
        Depth = -50;
        playSfx = true;
    }

    public override void Awake(object scene)
    {
        base.Awake(scene);
        foreach (CustomMoveBlock block in Level.Scheduler.Entities.GetTrackedEntities(typeof(CustomMoveBlock)))
            if (block.Left <= Left && block.Right >= Right && block.Bottom == Top)
            {
                attachedMoveBlock = block;
                break;
            }
    }

    public override void Update()
    {
        base.Update();
        var player = runtime.Player = Level.Scheduler.Entities.GetTrackedEntity<RoomPlayerSession>();
        if (player == null) return;
        PlayerCollideSolid = SolidAt(player, player.Position + Vector2.UnitX) ||
                             SolidAt(player, player.Position - Vector2.UnitX);
        if (attachedMoveBlock != null && attachedMoveBlock.steerSides.Contains("Bottom") &&
            (attachedMoveBlock.direction == CustomMoveBlock.Directions.Left ||
             attachedMoveBlock.direction == CustomMoveBlock.Directions.Right))
        {
            var position = Position;
            position.Y = playerWasAttached
                ? attachedMoveBlock.Position.Y + attachedMoveBlock.Height + attachedMoveBlock.magneticCeilingOffset +
                  attachedMoveBlock.buttonPressedOffset
                : attachedMoveBlock.Position.Y + attachedMoveBlock.Height + attachedMoveBlock.magneticCeilingOffset;
            Position = position;
        }

        if (playerWasAttached && (Level.Transitioning ||
                                  !SourceTrackedEntityCollision.CheckAt(Level.Scheduler.Entities,
                                      ceilingKey, player, player.Position - Vector2.UnitY)))
            DetachPlayer(player);
    }

    private IEnumerator JumpGraceTimer(RoomPlayerSession player)
    {
        float timer = .1f;
        JumpGracePeriod = true;
        while (timer > 0f)
        {
            timer -= engine.Frame.DeltaTime;
            yield return null;
            if (player.Input.Jump.Pressed && CanJump) timer = 0f;
        }

        if (!NoStaminaDrain && !player.DashAttacking) player.Data.Stamina -= 27.5f;
        JumpGracePeriod = false;
    }

    public void DetachPlayer(RoomPlayerSession player)
    {
        PlayerPose = "";
        Level.Session.SetFlag("Xaphan_Helper_Ceiling", false);
        Level.Session.SetFlag("Xaphan_Helper_Ceiling_Can_Jump", false);
        playSfx = true;
        playerWasAttached = ForceDetachPlayer = false;
    }

    private bool SolidAt(RoomPlayerSession player, Vector2 at) =>
        SourceTrackedEntityCollision.CheckAt(Level.Scheduler.Entities, "Celeste.Solid", player, at);

    private bool SpikesAt(RoomPlayerSession player) =>
        SourceTrackedEntityCollision.Check(Level.Scheduler.Entities, "Celeste.Spikes", player);

    private void OnCollide(RoomPlayerSession player)
    {
        PlayerPose = "";
        if (player.Holding.Holding != null || player.DashAttacking || player.Machine.State == 1) return;
        if (SourceEntityCollision.CheckAt(player, this, player.Position - Vector2.UnitY) &&
            (runtime.SpiderMagnetActive(Level) || !runtime.Module.LiquidFeatures.UseUpgrades) &&
            !runtime.Module.Features.IsControllingRemoteDrone && !ForceDetachPlayer)
        {
            if (player.Input.GrabCheck && (!CanJump || player.Speed.Y >= 0f) && Left - 6f < player.Left &&
                player.Right < Right + 6f && player.Top > Top && player.Data.Stamina > 0f && !player.Dead &&
                !SpikesAt(player))
            {
                playerWasAttached = true;
                if (playSfx)
                {
                    ((IOriginalGameplayProcess)engine).AudioSession.Play("event:/char/madeline/climb_ledge");
                    playSfx = false;
                }

                Level.Session.SetFlag("Xaphan_Helper_Ceiling", true);
                if (CanJump) Level.Session.SetFlag("Xaphan_Helper_Ceiling_Can_Jump", true);
                var speed = player.Speed;
                speed.Y = 0f;
                player.Speed = speed;
                player.MoveToY(Bottom + 11f, null);
                staticMover.TriggerPlatform();
                if (!NoStaminaDrain)
                {
                    if ((Vector2Math.SafeNormalize(player.Input.Aim.Value) == -Vector2.UnitX ||
                         Vector2Math.SafeNormalize(player.Input.Aim.Value) == Vector2.UnitX) &&
                        !SolidAt(player, player.Position + Vector2.UnitX) &&
                        !SolidAt(player, player.Position - Vector2.UnitX))
                        player.Data.Stamina -= 27.5f * engine.Frame.DeltaTime;
                    else player.Data.Stamina -= 10f * engine.Frame.DeltaTime;
                }

                PlayerPose = Vector2Math.SafeNormalize(player.Input.Aim.Value).X != 0f && !PlayerCollideSolid
                    ? "XaphanHelper_ceilingMove"
                    : "XaphanHelper_ceilingIdle";
                player.Sprite.Play(PlayerPose);
            }
            else if (SpikesAt(player))
            {
                DetachPlayer(player);
                player.Die(Vector2.UnitY, false, true);
            }
            else if (playerWasAttached)
            {
                if (CanJump && !JumpGraceTimerRoutine.Active)
                {
                    Add(JumpGraceTimerRoutine = new SimulationCoroutine(engine, JumpGraceTimer(player), true));
                    playerWasAttached = false;
                }

                DetachPlayer(player);
            }
        }
        else if (playerWasAttached) DetachPlayer(player);
    }

    public override void Render(ISourceRenderContext context)
    {
        float count = Width / 8f;
        for (int i = 0; i < count; i += 2)
        {
            spriteA.RenderPosition = Position + imageOffset + new Vector2(i * 8f, 0f);
            spriteA.Render(context);
        }

        spriteA.Visible = false;
        for (int i = 0; i < count - 1f; i += 2)
        {
            spriteB.RenderPosition = Position + imageOffset + new Vector2(8f, 0f) + new Vector2(i * 8f, 0f);
            spriteB.Render(context);
        }

        spriteB.Visible = false;
    }

    private void OnShake(Vector2 amount) => imageOffset += amount;

    private bool IsRiding(SourceSolidEntityState solid) => !SourceEntityCollision.Check(this, solid) &&
                                                           SourceEntityCollision.CheckAt(this, solid,
                                                               Position - Vector2.UnitY *
                                                               (attachedMoveBlock != null
                                                                   ? 1 + attachedMoveBlock.magneticCeilingOffset
                                                                   : 1));

    private bool IsRiding(ISourceJumpThruEntity jumpThru) =>
        SourceEntityCollision.CheckAt(this, jumpThru, Position - Vector2.UnitY);

    public void SetCeilingColor(PackedColor color)
    {
        foreach (var component in Components)
            if (component is Sprite sprite)
                sprite.Color = color;
    }

    private void OnEnable()
    {
        Visible = Collidable = true;
        SetCeilingColor(EnabledColor);
    }

    private void OnDisable()
    {
        Collidable = false;
        var player = Level.Scheduler.Entities.GetTrackedEntity<RoomPlayerSession>();
        if (player != null && Level.Session.GetFlag("Xaphan_Helper_Ceiling")) DetachPlayer(player);
        JumpGracePeriod = false;
        if (VisibleWhenDisabled) SetCeilingColor(DisabledColor);
        else Visible = false;
    }

    private void OnMove(Vector2 amount) => Position += amount;
}
