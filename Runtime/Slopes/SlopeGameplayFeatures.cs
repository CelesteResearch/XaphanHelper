// SPDX-License-Identifier: MIT

using Celeste.Graphics.Animation;
using Celeste.Simulation.Gameplay.Actors.Foundation;

namespace Celeste.Mods.XaphanHelper.Runtime.Slopes
{
    public sealed class SlopeGameplayFeatures : ISlopeGameplayFeatures
    {
        public bool HasDrone { get; set; }
        public bool HasDroneDebris { get; set; }
        public SourceActorEntityState FakePlayer { get; set; }
        public Sprite FakePlayerSprite { get; set; }
        public bool FakePlayerIsDucking { get; set; }
        public bool DroneDisablesPlayerCollision { get; set; }
        public bool IsControllingRemoteDrone { get; set; }
        public bool UiOpened { get; set; }
        public bool UseMetroidGameplay { get; set; }
        public bool Shinesparking { get; set; }
        public bool ShowCompleteSlopesHitboxes { get; set; }
    }
}
