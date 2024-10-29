using DebugUI;
using Pancake.DebugView;

namespace Pancake.Game
{
    public class DailyRewardDebugPage : DebugPageBase
    {
        public override void Configure(DebugUIBuilder builder)
        {
            builder.AddFoldout("Daily Reward", builder => { builder.AddButton("Next Day", UserData.NextDayDailyReward); });
        }
    }
}