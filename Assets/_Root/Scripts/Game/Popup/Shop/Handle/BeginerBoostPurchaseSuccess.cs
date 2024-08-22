using Pancake.IAP;
using UnityEngine;

namespace Pancake.Game.UI
{
    [CreateAssetMenu(fileName = "beginer_boost_purchase_success_event", menuName = "Pancake/Game/Shop/Beginer Boost Purchase Success Event", order = 32)]
    [EditorIcon("so_blue_event")]
    public class BeginerBoostPurchaseSuccess : IAPPurchaseSuccess
    {
        public override void Raise() { }
    }
}