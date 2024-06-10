using System;
using System.Runtime.CompilerServices;
using Alchemy.Inspector;
using Pancake.Common;

namespace Pancake.SceneFlow
{
    using UnityEngine;

//     [Serializable]
//     [CreateAssetMenu(menuName = "Pancake/Game/Daily Reward Data", fileName = "daily_reward_data.asset")]
//     [EditorIcon("so_blue_variable")]
//     public class DailyRewardVariable : ScriptableVariable<DailyRewardData>
//     {
//         [MethodImpl(MethodImplOptions.AggressiveInlining)]
//         public void Claim()
//         {
//             Value.isClaimed = true;
//             Save();
//         }
//
//         [MethodImpl(MethodImplOptions.AggressiveInlining)]
//         public bool IsClaimed() => Value.isClaimed;
//
//         public override void Load()
//         {
//             base.Load();
//             Value.isClaimed = Data.Load(Guid, InitialValue.isClaimed);
//         }
//
//         public override void Save()
//         {
//             Data.Save(Guid, Value.isClaimed);
//             base.Save();
//         }
//
// #if UNITY_EDITOR
//         protected override void OnValidate()
//         {
//             try
//             {
//                 if (value == null || value.isClaimed == PreviousValue.isClaimed) return;
//                 ValueChanged();
//             }
//             catch (Exception)
//             {
//                 // ignored
//             }
//         }
// #endif
//     }

    [Serializable]
    public class DailyRewardData
    {
        public int day;
        public TypeRewardDailyReward typeReward;
        public int amount;
#if UNITY_EDITOR
        private bool IsRewardCoin => typeReward == TypeRewardDailyReward.Coin;
        private bool IsRewardOutfit => typeReward == TypeRewardDailyReward.Outfit;
#endif
        [ShowIf("IsRewardCoin")] public Sprite icon;
        //[ShowIf("IsRewardOutfit")] public OutfitUnitVariable outfitUnit;
        public bool isClaimed;
    }


    public enum TypeRewardDailyReward
    {
        Coin,
        Outfit
    }
}