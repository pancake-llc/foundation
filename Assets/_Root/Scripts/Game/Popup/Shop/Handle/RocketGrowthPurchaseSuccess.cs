using Pancake.IAP;
using UnityEngine;

namespace Pancake.Game.UI
{
    [CreateAssetMenu(fileName = "rocket_growth_purchase_success_event", menuName = "Pancake/Game/Shop/Rocket Growth Purchase Success Event", order = 35)]
    [EditorIcon("so_blue_event")]
    public class RocketGrowthPurchaseSuccess : IAPPurchaseSuccess
    {
        public override void Raise() { }
    }
}