// SPDX-License-Identifier: MIT

using System;
using Celeste.Game.Content.Maps.Levels;
using Celeste.Game.Execution.Levels;
using Celeste.Mods.XaphanHelper.Entities.Slopes;
using Celeste.Scene.Tracking;
using Celeste.Simulation;
using Celeste.Simulation.Gameplay.Player.Intro;

namespace Celeste.Mods.XaphanHelper.Modules
{
    public sealed partial class XaphanHelperModule
    {
        private void OnLevelLoad(Action<OriginalLevelSceneRoot, PlayerIntroType, bool> original,
            OriginalLevelSceneRoot level, PlayerIntroType intro, bool isFromLoader)
        {
            original(level, intro, isFromLoader);
            if (ReferenceEquals(level.EngineContext, SourceEngine)) onSlope = false;
        }

        private void OnLevelEnd(Action<OriginalLevelSceneRoot> original, OriginalLevelSceneRoot level)
        {
            original(level);
            if (ReferenceEquals(level.EngineContext, SourceEngine)) onSlope = false;
        }

        private void OnLevelUpdate(Action<OriginalLevelSceneRoot> original, OriginalLevelSceneRoot level)
        {
            original(level);
            if (!ReferenceEquals(level.EngineContext, SourceEngine)) return;
            var player =
                ((ISourceScene)level).SourceScene.Entities.GetTrackedEntity<RoomPlayerSession>(typeof(RoomPlayerSession)
                    .FullName);
            foreach (PlayerPlatform platform in ((ISourceScene)level).SourceScene.Entities.GetTrackedEntities(
                         typeof(PlayerPlatform)))
            {
                if (player != null && platform.HasPlayerOnTop())
                {
                    onSlope = true;
                    onSlopeDir = platform.Side == "Right" ? 1 : -1;
                    onSlopeGentle = platform.Gentle;
                    onSlopeTop = platform.slopeTop;
                    onSlopeAffectPlayerSpeed = platform.AffectPlayerSpeed;
                    break;
                }

                onSlope = false;
                onSlopeDir = 0;
                onSlopeGentle = false;
                onSlopeTop = 0;
                onSlopeAffectPlayerSpeed = false;
            }
        }

        private void OnPlayerUpdate(Action<RoomPlayerSession> original, RoomPlayerSession player)
        {
            if (!Owns(player))
            {
                original(player);
                return;
            }

            if (onSlope && player.Bottom != onSlopeTop && player.Speed.X != 0 && onSlopeAffectPlayerSpeed)
                maxRunSpeed += 0.025f;
            else
            {
                if (player.Speed.X == 0) maxRunSpeed = 0;
                if (maxRunSpeed != 0) maxRunSpeed -= 0.025f;
            }

            if (maxRunSpeed > 0.4f) maxRunSpeed = 0.4f;
            if (maxRunSpeed < 0) maxRunSpeed = 0;
            original(player);
        }

        private float DetermineSpeedOffset(RoomPlayerSession player)
        {
            float offset = 0;
            bool wasAccelerating = false;
            if (onSlope && player.Bottom != onSlopeTop)
            {
                if ((onSlopeDir == -1 && player.Speed.X < 0) || (onSlopeDir == 1 && player.Speed.X > 0))
                    offset = -(onSlopeGentle ? maxRunSpeed / 2 : maxRunSpeed);
                else if ((onSlopeDir == -1 && player.Speed.X > 0) || (onSlopeDir == 1 && player.Speed.X < 0))
                {
                    wasAccelerating = true;
                    offset = onSlopeGentle ? maxRunSpeed / 2 : maxRunSpeed;
                }
            }
            else if (maxRunSpeed > 0) offset = wasAccelerating ? maxRunSpeed : -maxRunSpeed;

            return offset;
        }
    }
}
