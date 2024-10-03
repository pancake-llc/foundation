using Pancake.IAP;
using UnityEngine;

namespace Pancake.Game.UI
{
    [CreateAssetMenu(fileName = "all_purchase_failed_event", menuName = "Pancake/Game/Shop/All Purchase Failed Event", order = 31)]
    [EditorIcon("so_blue_event")]
    public class AllPurchaseFailed : IAPPurchaseFailed
    {
        public override void Raise(string reason) { }
    }
}