#if PANCAKE_IAP
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Purchasing;


// ReSharper disable InconsistentNaming
namespace Pancake.IAP
{
    [Serializable]
    internal class IAPData
    {
        public string id;
        public ProductType productType;
    }


    [EditorIcon("so_blue_setting")]
    public class IAPSettings : ScriptableSettings<IAPSettings>
    {
        [SerializeField] private List<IAPData> skusData = new();
        [SerializeField] private List<IAPDataVariable> products = new();
#if UNITY_EDITOR
        [SerializeField, TextArea] private string googlePlayStoreKey;
#endif

        public static List<IAPDataVariable> Products => Instance.products;
    }
}
#endif