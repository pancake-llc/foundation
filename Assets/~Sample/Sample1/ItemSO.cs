using MessagePack;
using Pancake;
using UnityEngine;
using UnityEngine.AddressableAssets;

[CreateAssetMenu, MessagePackFormatter(typeof(AssetFormatter<ItemSO>))]
// ReSharper disable once InconsistentNaming
public class ItemSO : ScriptableObject
{
   [SerializeField] private string displayName;
   [SerializeField] private int power;

   public int Power => power;
   public string DisplayName => displayName;
}
