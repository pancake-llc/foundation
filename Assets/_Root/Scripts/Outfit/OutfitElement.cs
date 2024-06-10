using System;
using Pancake.Spine;
using UnityEngine;

namespace Pancake.SceneFlow
{
    [CreateAssetMenu(fileName = "outfit_variable.asset", menuName = "Pancake/Game/Outfit/Outfit Element")]
    [EditorIcon("so_blue_variable")]
    [Serializable]
    public class OutfitElement : ScriptableObject
    {
        [Guid] public string id;
        public bool isUnlocked;
        public OutfitUnlockType unlockType;
        public int value;
        public Vector2 viewPosition;
        [SpineSkinPickup("mix-and-match-pro_SkeletonData")] public string skinId;
    }
}