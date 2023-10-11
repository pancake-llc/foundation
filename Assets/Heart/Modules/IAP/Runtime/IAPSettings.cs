#if PANCAKE_IAP
using System.Collections.Generic;
using Pancake.Apex;
using UnityEngine;
using System;

using UnityEngine.Purchasing;


// ReSharper disable InconsistentNaming
namespace Pancake.IAP
{
    [Serializable]
    public class IAPData
    {
        public bool isTest;
        public string id;
        public ProductType productType;
    }

    [HideMonoScript]
    [EditorIcon("scriptable_iap")]
    public class IAPSettings : ScriptableObject
    {
        [SerializeField] private List<IAPData> skusData = new List<IAPData>();
        [SerializeField] private List<IAPDataVariable> products = new List<IAPDataVariable>();
#if UNITY_EDITOR
        [SerializeField, TextArea] private string googlePlayStoreKey;
#endif

        public List<IAPDataVariable> Products => products;
    }
}
#endif