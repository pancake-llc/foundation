using System;
using System.Runtime.CompilerServices;
using Pancake.Apex;
using Pancake.Scriptable;

namespace Pancake.SceneFlow
{
    using UnityEngine;

    [Serializable]
    [CreateAssetMenu(menuName = "Pancake/Game/Daily Reward Data", fileName = "daily_reward_data.asset")]
    [EditorIcon("scriptable_variable")]
    public class DailyRewardVariable : ScriptableVariable<DailyRewardData>
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Claim()
        {
            Value.isClaimed = true;
            Save();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsClaimed() => Value.isClaimed;

        public override void Load()
        {
            base.Load();
            Value.isClaimed = Data.Load(Guid, InitialValue.isClaimed);
        }

        public override void Save()
        {
            Data.Save(Guid, Value.isClaimed);
            base.Save();
        }
        
#if UNITY_EDITOR
        protected override void OnValidate()
        {
            try
            {
                if (value == null || value.isClaimed == PreviousValue.isClaimed) return;
                ValueChanged();
            }
            catch (Exception)
            {
                // ignored
            }
        }
#endif
    }

    [Serializable]
    public class DailyRewardData
    {
        public int day;
        public TypeRewardDailyReward typeReward;
        public int amount;
        [ShowIf(nameof(typeReward), TypeRewardDailyReward.Coin)] public Sprite icon;
        [ShowIf(nameof(typeReward), TypeRewardDailyReward.Outfit)] public OutfitUnitVariable outfitUnit;
        public bool isClaimed;
    }


    public enum TypeRewardDailyReward
    {
        Coin,
        Outfit
    }
}