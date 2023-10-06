using System;
using Pancake.Apex;
using Pancake.Spine;

namespace Pancake.SceneFlow
{
    [Serializable]
    public class OutfitElement
    {
        [Guid] public string id;
        [ReadOnly] public bool isUnlocked;
        public OutfitUnlockType unlockType;
        public int value;
        [SpineSkinPickup("mix-and-match-pro_SkeletonData")] public string skinId;
    }
}