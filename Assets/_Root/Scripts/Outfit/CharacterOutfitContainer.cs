using System;
using Pancake.Apex;

namespace Pancake.SceneFlow
{
    using UnityEngine;

    [CreateAssetMenu(menuName = "Pancake/Game/Outfit/Outfit Container", fileName = "OutfitContainer.asset")]
    public class CharacterOutfitContainer : ScriptableObject
    {
        [Array] public CharacterOutfit[] outfits;
    }

    [Serializable]
    public class CharacterOutfit
    {
        public OutfitType type;
        [Array] public OutfitUnitVariable[] list;
    }


    public enum OutfitType
    {
        Hat = 0,
        Shirt = 1,
        Shoe = 2
    }

    public enum OutfitUnlockType
    {
        Coin,
        Rewarded,
        BeginnerGift,
        Event,
        DailyReward
    }
}