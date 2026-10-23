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

public partial class FlagSwapBlock : SourceSolidEntityState
{
    private void OnDash(Vector2 direction)
    {
        if (!isActive)
        {
            return;
        }

        Swapping = lerp < 1f;
        if (!toggle)
        {
            target = 1;
            returnTimer = 0.8f;
            object scene = this.Scene;
            burst = ((OriginalLevelSceneRoot)scene).Displacement.AddBurst(this.Center, 0.2f, 0f, 16f, 1f, null, null);
            if (lerp >= 0.2f)
            {
                speed = maxForwardSpeed;
            }
            else
            {
                speed = ScalarMath.Lerp(maxForwardSpeed * 0.333f, maxForwardSpeed, lerp / 0.2f,
                    ((ISourceScalarEnvironment)engine).ScalarArithmetic);
            }

            ((ISourceSpatialAudioEnvironment)engine).SpatialAudio.Stop(returnSfx, true);
            ((ISourceSpatialAudioEnvironment)engine).SpatialAudio.Stop(moveSfx, true);
            if (!Swapping)
            {
                ((ISourceSpatialAudioEnvironment)engine).SpatialAudio.Play(
                    "event:/game/05_mirror_temple/swapblock_move_end", this.Center);
            }
            else
            {
                moveSfx = ((ISourceSpatialAudioEnvironment)engine).SpatialAudio.Play(
                    "event:/game/05_mirror_temple/swapblock_move", this.Center);
            }

            return;
        }

        ((ISourceSpatialAudioEnvironment)engine).SpatialAudio.Stop(returnSfx, true);
        ((ISourceSpatialAudioEnvironment)engine).SpatialAudio.Stop(moveSfx, true);
        if (target == 1)
        {
            returnSfx = ((ISourceSpatialAudioEnvironment)engine).SpatialAudio.Play(
                "event:/game/05_mirror_temple/swapblock_move", this.Center);
            target = 0;
        }
        else
        {
            moveSfx = ((ISourceSpatialAudioEnvironment)engine).SpatialAudio.Play(
                "event:/game/05_mirror_temple/swapblock_move", this.Center);
            target = 1;
        }

        returnTimer = 0.8f;
        object scene2 = this.Scene;
        burst = ((OriginalLevelSceneRoot)scene2).Displacement.AddBurst(this.Center, 0.2f, 0f, 16f, 1f, null, null);
        if (lerp >= 0.2f)
        {
            speed = maxForwardSpeed;
        }
        else
        {
            speed = ScalarMath.Lerp(maxForwardSpeed * 0.333f, maxForwardSpeed, lerp / 0.2f,
                ((ISourceScalarEnvironment)engine).ScalarArithmetic);
        }

        if (!Swapping)
        {
            ((ISourceSpatialAudioEnvironment)engine).SpatialAudio.Play(
                "event:/game/05_mirror_temple/swapblock_move_end", this.Center);
        }
    }

    public override void Update()
    {
        base.Update();
        if (flag == "" || this.SceneAsLevel().Session.GetFlag(flag))
        {
            isActive = true;
        }
        else
        {
            isActive = false;
        }

        if (returnTimer > 0f)
        {
            returnTimer -= engine.DeltaTime;
            if (returnTimer <= 0f)
            {
                if (!toggle)
                {
                    target = 0;
                    returnSfx = ((ISourceSpatialAudioEnvironment)engine).SpatialAudio.Play(
                        "event:/game/05_mirror_temple/swapblock_return", this.Center);
                }

                speed = 0f;
            }
        }

        if (burst != null)
        {
            burst.Position = this.Center;
        }

        redAlpha = ScalarMath.Approach(redAlpha, (float)((target != 1) ? 1 : 0), engine.DeltaTime * 32f,
            ((ISourceExtremaEnvironment)engine).ExtremaArithmetic);
        if (target == 0 && lerp == 0f)
        {
            middleRed.SetAnimationFrame(0);
            middleGreen.SetAnimationFrame(0);
        }

        if (target == 1)
        {
            speed = ScalarMath.Approach(speed, maxForwardSpeed, maxForwardSpeed / 0.2f * engine.DeltaTime,
                ((ISourceExtremaEnvironment)engine).ExtremaArithmetic);
        }
        else
        {
            speed = ScalarMath.Approach(speed, maxBackwardSpeed, maxBackwardSpeed / 1.5f * engine.DeltaTime,
                ((ISourceExtremaEnvironment)engine).ExtremaArithmetic);
        }

        float num = lerp;
        lerp = ScalarMath.Approach(lerp, (float)target, speed * engine.DeltaTime,
            ((ISourceExtremaEnvironment)engine).ExtremaArithmetic);
        if (lerp != num)
        {
            Vector2 val = (end - start) * speed;
            Vector2 position = this.Position;
            if (target == 1)
            {
                val = (end - start) * maxForwardSpeed;
            }

            if (lerp < num)
            {
                val *= -1f;
            }

            if (target == 1 &&
                ((ISourceScene)Scene).SourceScene.Clock.OnInterval(
                    (setSpeed == 180f) ? 0.03f : ((setSpeed == 360f) ? 0.02f : 0.01f), engine))
            {
                MoveParticles(end - start);
            }

            if (target == 0 && toggle &&
                ((ISourceScene)Scene).SourceScene.Clock.OnInterval(
                    (setSpeed == 180f) ? 0.03f : ((setSpeed == 360f) ? 0.02f : 0.01f), engine))
            {
                MoveParticles(start - end);
            }

            this.MoveTo(Vector2Math.Lerp(start, end, lerp, ((ISourceScalarEnvironment)engine).ScalarArithmetic), val);
            if (position != this.Position)
            {
                ((ISourceSpatialAudioEnvironment)engine).SpatialAudio.Position(moveSfx, this.Center);
                ((ISourceSpatialAudioEnvironment)engine).SpatialAudio.Position(returnSfx, this.Center);
                if (this.Position == start && target == 0)
                {
                    returnSfx?.SetParameter("end", 1f);
                    ((ISourceSpatialAudioEnvironment)engine).SpatialAudio.Play(
                        "event:/game/05_mirror_temple/swapblock_return_end", this.Center);
                }
                else if (this.Position == end && target == 1)
                {
                    if (toggle)
                    {
                        moveSfx?.SetParameter("end", 1f);
                    }

                    ((ISourceSpatialAudioEnvironment)engine).SpatialAudio.Play(
                        "event:/game/05_mirror_temple/swapblock_move_end", this.Center);
                }
            }
        }

        if (Swapping && lerp >= 1f)
        {
            Swapping = false;
        }

        base.StopPlayerRunIntoAnimation = lerp <= 0f || lerp >= 1f;
    }

    private void MoveParticles(Vector2 normal)
    {
        Vector2 val;
        float num;
        float num2;
        Vector2 val2;
        if (normal.X > 0f)
        {
            val = this.CenterLeft;
            val2 = Vector2.UnitY * (this.Height - 6f);
            num = (float)Math.PI;
            num2 = Math.Max(2f, this.Height / 14f);
        }
        else if (normal.X < 0f)
        {
            val = this.CenterRight;
            val2 = Vector2.UnitY * (this.Height - 6f);
            num = 0f;
            num2 = Math.Max(2f, this.Height / 14f);
        }
        else if (normal.Y > 0f)
        {
            val = this.TopCenter;
            val2 = Vector2.UnitX * (this.Width - 6f);
            num = -(float)Math.PI / 2f;
            num2 = Math.Max(2f, this.Width / 14f);
        }
        else
        {
            val = this.BottomCenter;
            val2 = Vector2.UnitX * (this.Width - 6f);
            num = (float)Math.PI / 2f;
            num2 = Math.Max(2f, this.Width / 14f);
        }

        particlesRemainder += num2;
        int num3 = (int)particlesRemainder;
        particlesRemainder -= num3;
        val2 *= 0.5f;
        this.SceneAsLevel().Particles.Emit(P_Move, num3, val, val2, num);
    }
}
