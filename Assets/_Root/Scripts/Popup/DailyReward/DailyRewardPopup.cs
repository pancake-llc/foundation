using UnityEngine;

namespace Pancake.UI
{
    [RequireComponent(typeof(DailyRewardView))]
    [EditorIcon("icon_popup")]
    public sealed class DailyRewardPopup : Popup<DailyRewardView>
    {
    }
}
