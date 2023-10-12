using System;
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
        public override void Load()
        {
            Value.isClaimed = Data.Load(Guid, DefaultValue.isClaimed);
            base.Load();
        }

        public override void Save()
        {
            Data.Save(Guid, Value.isClaimed);
            base.Save();
        }
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