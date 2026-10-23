// SPDX-License-Identifier: MIT

using System;
using System.Reflection;
using Celeste.Game.Execution.Processes;
using Celeste.Game.Gameplay.Holdables.Gliders;
using Celeste.Game.Gameplay.Holdables.TheoCrystal;
using Celeste.Mods.XaphanHelper.Entities.Slopes;
using Celeste.Physics.Collision;
using MonoMod.RuntimeDetour;

namespace Celeste.Mods.XaphanHelper.Modules
{
    public sealed partial class XaphanHelperModule
    {
        private void InstallHoldableCollisionHooks()
        {
            hooks.Add(new Hook(
                typeof(TheoCrystalState).GetMethod("OnCollideH", BindingFlags.Instance | BindingFlags.NonPublic),
                (Action<Action<TheoCrystalState, PixelCollision>, TheoCrystalState, PixelCollision>)((original, actor,
                    collision) =>
                {
                    if (Owns(actor) && collision.Hit is Slope) actor.Speed.X *= -0.4f;
                    else original(actor, collision);
                })));
            hooks.Add(new Hook(
                typeof(OriginalGliderState).GetMethod("OnCollideH", BindingFlags.Instance | BindingFlags.NonPublic),
                (Action<Action<OriginalGliderState, PixelCollision>, OriginalGliderState, PixelCollision>)((original,
                    actor, collision) =>
                {
                    if (!Owns(actor) || !(collision.Hit is Slope))
                    {
                        original(actor, collision);
                        return;
                    }

                    var process = (IOriginalGameplayProcess)SourceEngine;
                    if (actor.Speed.X < -50f)
                        process.AudioSession.Play("event:/new_content/game/10_farewell/glider_wallbounce_left",
                            actor.Position, process.SaveContext);
                    else if (actor.Speed.X > 50f)
                        process.AudioSession.Play("event:/new_content/game/10_farewell/glider_wallbounce_right",
                            actor.Position, process.SaveContext);
                    actor.Speed.X *= -1f;
                })));
        }
    }
}
