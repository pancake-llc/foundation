#if PANCAKE_IAP
using System;
using Alchemy.Inspector;
using UnityEngine;
using UnityEngine.Purchasing;

namespace Pancake.IAP
{
    [EditorIcon("so_blue_variable")]
    [CreateAssetMenu(fileName = "variable_IAPData.asset", menuName = "Pancake/IAP/Scriptable IAPData")]
    [Serializable]
    public class IAPDataVariable : ScriptableObject
    {
        [ReadOnly] public string id;
        [ReadOnly] public ProductType productType;

        [Space] [SerializeField] private IAPPurchaseSuccess onPurchaseSuccess;
        [SerializeField] private IAPPurchaseFailed onPurchaseFailed;

        internal IAPPurchaseSuccess OnPurchaseSuccess => onPurchaseSuccess;
        internal IAPPurchaseFailed OnPurchaseFailed => onPurchaseFailed;

        [NonSerialized] public Action purchaseSuccessCallback;
        [NonSerialized] public Action purchaseFailedCallback;
    }
}
#endif