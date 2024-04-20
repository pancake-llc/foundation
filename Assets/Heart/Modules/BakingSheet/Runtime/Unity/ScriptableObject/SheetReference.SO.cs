using System;
using Pancake.BakingSheet.Unity;
using UnityEngine;

namespace Pancake.BakingSheet
{
    public interface IUnitySheetReference : ISheetReference
    {
        public SheetRowScriptableObject Asset { get; set; }
    }

    internal class UnitySheetReferenceAttribute : PropertyAttribute
    {
    }

    public partial class Sheet<TKey, TValue>
    {
        [Serializable]
        public partial struct Reference : IUnitySheetReference
        {
            [SerializeField, UnitySheetReference] private SheetRowScriptableObject asset;

            SheetRowScriptableObject IUnitySheetReference.Asset { get => asset; set => asset = value; }

            partial void EnsureLoadReference()
            {
                if (asset == null)
                    return;

                reference = asset.GetRow<TValue>();
                Id = reference.Id;
            }
        }
    }
}