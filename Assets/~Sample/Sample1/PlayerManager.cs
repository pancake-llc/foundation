using System;
using Pancake;
using Pancake.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class PlayerManager : MonoBehaviour
{
    [SerializeField] private PlayerData playerData;
    [SerializeField] private AssetReference _assetContainer;

    private async void Awake()
    {
        _assetContainer = new AssetReference("66c1bc38545d23f41bcb62bc68ed8d68");
        // Archive.LoadFile("player-storage");
        // playerData = Archive.Load<PlayerData>("PlayerDataKey");
        //
        // playerData.ToString();
       //  await _assetContainer.LoadAssetAsync<GameObject>();
       //  Debug.Log(_assetContainer.Asset.name);
       // var go = Instantiate(_assetContainer.Asset);
       //  _assetContainer.ReleaseAsset();
       var go =  await Addressables.LoadAssetAsync<GameObject>("66c1bc38545d23f41bcb62bc68ed8d68");
       Instantiate(go);
       //_assetContainer.OperationHandle.Completed += LoadCompleted;
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