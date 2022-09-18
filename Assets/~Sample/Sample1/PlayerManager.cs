using System;
using System.Collections.Generic;
using Pancake;
using Pancake.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.AddressableAssets.ResourceLocators;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceLocations;
using Object = UnityEngine.Object;

internal class PlayerManager : MonoBehaviour
{
    [SerializeField] private PlayerData playerData;
    [SerializeField] private AssetReference _assetContainer;

    public void AAA()
    {
        
        //_assetContainer = new AssetReference("66c1bc38545d23f41bcb62bc68ed8d68");
        // Archive.LoadFile("player-storage");
        // playerData = Archive.Load<PlayerData>("PlayerDataKey");
        //
        // playerData.ToString();
       //  await _assetContainer.LoadAssetAsync<GameObject>();
       //  Debug.Log(_assetContainer.Asset.name);
       // var go = Instantiate(_assetContainer.Asset);
       //  _assetContainer.ReleaseAsset();
       // var go =  await Addressables.LoadAssetAsync<Object>("4769b653e88f47b4da1e5c5aef65848e");
       // var dict = new Dictionary<Type, Action>();
       // Debug.Log(go.GetType());
       // dict.Add(typeof(ScriptableObject),
       //     () =>
       //     {
       //         Debug.Log((go as ItemSO).DisplayName);
       //         Debug.Log((go as ItemSO).Power);
       //     });
       //
       // dict[go.GetType()].Invoke();
       //_assetContainer.OperationHandle.Completed += LoadCompleted;

       Archive.Save("PlayerDataKey", playerData);
       Archive.SaveFile("player-storage");
       C.LocationMap();
    }

    private void LoadCompleted(AsyncOperationHandle obj)
    {
        Debug.Log(_assetContainer.Asset.name);
        Instantiate(_assetContainer.Asset);
        //_assetContainer.ReleaseAsset();
    }

    private void OnApplicationQuit()
    {
        //Archive.Save("PlayerDataKey", playerData);
        //Archive.SaveFile("player-storage");
    }
    
    
}