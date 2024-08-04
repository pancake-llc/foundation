using System;

namespace Pancake.Game.UI
{
    [Serializable]
    public class DayReward
    {
        public EDailyRewardType typeReward;
        public int amount;
    }

    public enum EDailyRewardType
    {
        Coin = 0,
        Gem = 1,
        Chest = 2,
    }

    public enum EDailyRewardDayStatus
    {
        Claimable = 0,
        Claimed = 1,
        Locked = 2,
    }
}