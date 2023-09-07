#if PANCAKE_IAP
using System;
using Pancake.Apex;
using UnityEngine;
using UnityEngine.Purchasing;

namespace Pancake.IAP
{
    [EditorIcon("scriptable_variable")]
    [CreateAssetMenu(fileName = "scriptable_variable_IAPData.asset", menuName = "Pancake/IAP/Scriptable IAPData")]
    [Serializable]
    [HideMonoScript]
    public class IAPDataVariable : ScriptableObject
    {
        [ReadOnly] public bool isTest;
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