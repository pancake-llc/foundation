#if PANCAKE_IAP
using System.Collections.Generic;
using UnityEngine;
using System;

using UnityEngine.Purchasing;


// ReSharper disable InconsistentNaming
namespace Pancake.IAP
{
    [Serializable]
    public class IAPData
    {
        public string id;
        public ProductType productType;
    }


    [EditorIcon("scriptable_iap")]
    public class IAPSettings : ScriptableObject
    {
        [SerializeField] private List<IAPData> skusData = new();
        [SerializeField] private List<IAPDataVariable> products = new();
#if UNITY_EDITOR
        [SerializeField, TextArea] private string googlePlayStoreKey;
#endif

        public List<IAPDataVariable> Products => products;
    }
}
#endif