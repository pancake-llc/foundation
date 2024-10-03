using UnityEngine;

namespace Pancake.IAP
{
    [EditorIcon("so_blue_event")]
    public abstract class IAPPurchaseFailed : ScriptableObject
    {
        public abstract void Raise(string reason);
    }
}