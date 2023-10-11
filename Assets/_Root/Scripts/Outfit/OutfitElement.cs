using System;
using Pancake.Apex;
using Pancake.Spine;
using UnityEngine;

namespace Pancake.SceneFlow
{
    [Serializable]
    public class OutfitElement
    {
        [Guid] public string id;
        [ReadOnly] public bool isUnlocked;
        public OutfitUnlockType unlockType;
        public int value;
        public Vector2 viewPosition;
        [SpineSkinPickup("mix-and-match-pro_SkeletonData")] public string skinId;
    }
}