using UnityEngine;

namespace Pancake.IAP
{
    [EditorIcon("so_blue_event")]
    public abstract class IAPPurchaseSuccess : ScriptableObject
    {
        public abstract void Raise();
    }
}