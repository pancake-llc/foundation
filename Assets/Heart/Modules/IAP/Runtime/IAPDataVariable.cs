#if PANCAKE_IAP
using System;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Purchasing;

namespace Pancake.IAP
{
    [EditorIcon("so_blue_variable")]
    [CreateAssetMenu(fileName = "variable_IAPData.asset", menuName = "Pancake/IAP Data")]
    [Serializable]
    [Searchable]
    public class IAPDataVariable : ScriptableObject
    {
        [ReadOnly] public string id;
        [ReadOnly] public ProductType productType;
        [ReadOnly] public int price;
        [ReadOnly] public string localizedPrice;
        [ReadOnly] public string isoCurrencyCode;
        [ReadOnly] public string localizedDescription;
        [ReadOnly] public string localizedTitle;
        [HideInInspector] public string receipt;
        internal SubscriptionInfo subscriptionInfo;

        [Space] [SerializeField] private IAPPurchaseSuccess onPurchaseSuccess;
        [SerializeField] private IAPPurchaseFailed onPurchaseFailed;

        internal IAPPurchaseSuccess OnPurchaseSuccess => onPurchaseSuccess;
        internal IAPPurchaseFailed OnPurchaseFailed => onPurchaseFailed;

        [NonSerialized] internal Action purchaseSuccessCallback;
        [NonSerialized] internal Action<string> purchaseFailedCallback;
    }
}
#endif