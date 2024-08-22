using UnityEngine;

namespace Pancake.Game.UI
{
    [CreateAssetMenu(menuName = "Pancake/Game/Daily Reward Data", fileName = "daily_reward_data.asset")]
    [EditorIcon("icon_so_blue")]
    public class DailyRewardData : ScriptableObject
    {
        public int day;
        public DayReward[] rewards;
    }
}