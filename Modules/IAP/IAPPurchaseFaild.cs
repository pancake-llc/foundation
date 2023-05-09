namespace Pancake.IAP
{
    using UnityEngine;
    
    [EditorIcon("scriptable_event")]
    public class IAPPurchaseFaild : ScriptableObject
    {
        public virtual void Raise() { }
    }
}