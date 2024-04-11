using System;

namespace Pancake.SceneFlow
{
    using UnityEngine;

    [CreateAssetMenu(menuName = "Pancake/Game/Outfit/Outfit Container", fileName = "OutfitContainer.asset")]
    public class CharacterOutfitContainer : ScriptableObject
    {
        public CharacterOutfit[] outfits;
    }

    [Serializable]
    public class CharacterOutfit
    {
        public OutfitType type;
        public OutfitUnitVariable[] list;
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