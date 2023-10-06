using System;
using Pancake.Apex;
using Pancake.Spine;

namespace Pancake.SceneFlow
{
    using UnityEngine;

    [CreateAssetMenu(menuName = "Pancake/Game/Outfit Container", fileName = "OutfitContainer.asset")]
    public class CharacterOutfitContainer : ScriptableObject
    {
        [Array] public CharacterOutfit[] outfits;
    }

    [Serializable]
    public class CharacterOutfit
    {
        [Guid] public string id;
        public OutfitType type;
        [SpineSkinPickup("mix-and-match-pro_SkeletonData")] public string skinId;
    }

    public enum OutfitType
    {
        Hat = 0,
        Shirt = 1,
        Shoe = 2
    }
}