using System;
using System.Numerics;
using Celeste.Core.Display;
using Celeste.Core.Scheduling;
using Celeste.Graphics.Animation;
using Celeste.Graphics.Content;
using Celeste.Physics.Collision;
using Celeste.Physics.Collision.Shapes;
using Celeste.Scene.Components.Oscillation;
using Celeste.Simulation.Gameplay.Actors.Foundation;
using Celeste.Simulation.Gameplay.Platforms.Foundation;

namespace Celeste.Mods.XaphanHelper.Entities.Liquids
{
    public sealed class LiquidAirBubble : SourceActorEntityState
    {
        private readonly ISourceEngineContext engine;
        private readonly Liquid liquid;
        private readonly Sprite sprite;
        private readonly SineWaveState sine;
        private readonly SourceCollisionHandler onCollide;
        private readonly Action<string> onLastFrame;

        public LiquidAirBubble(ISourceEngineContext engine, Vector2 position, Liquid liquid)
            : base(new PixelActor(null, position), engine,
                ((ISourcePlatformTargetMotionEnvironment)engine).TargetMotionPolicy)
        {
            this.engine = engine;
            this.liquid = liquid;
            Collider = new HitboxColliderGeometry(4, 4);
            Add(sprite = new Sprite(engine, ((ISourceGameGraphicsEnvironment)engine).GameGraphics.GameAtlas,
                "objects/XaphanHelper/liquid/"));
            sprite.AddLoop("idle", "bubble", .08f, 0);
            sprite.AddLoop("break", "bubble", .08f, 1);
            sprite.Color = PackedColor.White * .7f;
            sprite.Play("idle");
            Add(sine = new SineWaveState(engine, .44f).Randomize());
            onCollide = OnCollide;
            onLastFrame = _ => RemoveSelf();
        }

        public override void Update()
        {
            base.Update();
            // Upstream explicitly advances the owned sine a second time.
            sine.Update();
            if (liquid != null && Top > liquid.Top) MoveV(-25 * engine.DeltaTime, onCollide);
            else Break();
        }

        private void OnCollide(PixelCollision _) => Break();

        private void Break()
        {
            sprite.Play("break");
            sprite.OnLastFrame = onLastFrame;
        }
    }
}
