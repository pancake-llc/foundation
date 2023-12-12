using Pancake.Apex;
using Pancake.Scriptable;
using Pancake.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;

namespace Pancake.SceneFlow
{
    public class LauncherInitialize : GameComponent
    {
        [SerializeField] private ScriptableEventString changeSceneEvent;
        [SerializeField] private BoolVariable loadingCompleted;
        [SerializeField] private BoolVariable remoteConfigFetchCompleted;
        [SerializeField] private AssetReference persistentScene;

        [Space] [SerializeField] private bool isWaitLevelLoad;
        [SerializeField, ShowIf("isWaitLevelLoad")] private BoolVariable loadLevelCompleted;

        private bool _flagLoadedPersistent;

        private void Awake()
        {
            Application.targetFrameRate = (int) HeartSettings.TargetFrameRate;
            Input.multiTouchEnabled = HeartSettings.EnableMultipleTouch;
            LoadScene();
        }

        private async void LoadScene()
        {
            await UniTask.WaitUntil(() => loadingCompleted.Value);
            Addressables.LoadSceneAsync(persistentScene).Completed += SceneLoadCompleted;
            await UniTask.WaitUntil(() => _flagLoadedPersistent);
            if (remoteConfigFetchCompleted != null) await UniTask.WaitUntil(() => remoteConfigFetchCompleted.Value);
            if (isWaitLevelLoad) await UniTask.WaitUntil(() => loadLevelCompleted.Value);

            // TODO : wait something else before load menu

            changeSceneEvent.Raise(Constant.MENU_SCENE);
        }

        private void SceneLoadCompleted(AsyncOperationHandle<SceneInstance> handle) { _flagLoadedPersistent = true; }
    }
}