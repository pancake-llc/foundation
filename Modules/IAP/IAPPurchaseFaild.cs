namespace Pancake.IAP
{
    using UnityEngine;

    [EditorIcon("scriptable_event")]
    public abstract class IAPPurchaseFaild : ScriptableObject
    {
        public abstract void Raise();
    }
}