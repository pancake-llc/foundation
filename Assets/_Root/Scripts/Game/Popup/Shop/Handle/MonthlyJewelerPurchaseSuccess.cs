using Pancake.IAP;
using UnityEngine;

namespace Pancake.Game.UI
{
    [CreateAssetMenu(fileName = "monthly_jeweler_purchase_success_event", menuName = "Pancake/Game/Shop/Monthly Jeweler Purchase Success Event", order = 34)]
    [EditorIcon("so_blue_event")]
    public class MonthlyJewelerPurchaseSuccess : IAPPurchaseSuccess
    {
        public override void Raise() { }
    }
}