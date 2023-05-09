using System;
using Pancake.Apex;
using UnityEngine;
using UnityEngine.Purchasing;

namespace Pancake.IAP
{
    [EditorIcon("scriptable_variable")]
    [CreateAssetMenu(fileName = "scriptable_variable_IAPData.asset", menuName = "Pancake/Scriptable/ScriptableVariables/IAPData")]
    [System.Serializable]
    [HideMonoScript]
    public class IAPDataVariable : ScriptableObject
    {
        [ReadOnly] public bool isTest;
        [ReadOnly] public string id;
        [ReadOnly] public ProductType productType;

        [Space] [SerializeField] private IAPPurchaseSuccess onPurchaseSuccess;
        [SerializeField] private IAPPurchaseFaild onPurchaseFaild;

        internal IAPPurchaseSuccess OnPurchaseSuccess => onPurchaseSuccess;
        internal IAPPurchaseFaild OnPurchaseFaild => onPurchaseFaild;

        [NonSerialized] public Action purchaseSuccessCallback;
        [NonSerialized] public Action purchaseFaildCallback;
    }
}