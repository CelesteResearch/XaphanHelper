// SPDX-License-Identifier: MIT

using Celeste.Graphics.Animation;
using Celeste.Simulation.Gameplay.Actors.Foundation;

namespace Celeste.Mods.XaphanHelper.Runtime.Slopes
{
    public interface ISlopeGameplayFeatures
    {
        bool HasDrone { get; }
        bool HasDroneDebris { get; }
        SourceActorEntityState FakePlayer { get; }
        Sprite FakePlayerSprite { get; }
        bool FakePlayerIsDucking { get; }
        bool DroneDisablesPlayerCollision { get; }
        bool IsControllingRemoteDrone { get; }
        bool UiOpened { get; }
        bool UseMetroidGameplay { get; }
        bool Shinesparking { get; }
        bool ShowCompleteSlopesHitboxes { get; }
    }
}
