using Pancake.Apex;
using Pancake.Scriptable;
using Pancake.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;

namespace Pancake.SceneFlow
{
    public class LauncherInitialize : GameComponent
    {
        [SerializeField] private BoolVariable loadingCompleted;
        [SerializeField] private BoolVariable remoteConfigFetchCompleted;
        [SerializeField] private AssetReference persistentScene;

        [Space] [SerializeField] private bool isWaitLevelLoad;
        [SerializeField, ShowIf("isWaitLevelLoad")] private BoolVariable loadLevelCompleted;

        private void Awake()
        {
            Application.targetFrameRate = (int) HeartSettings.TargetFrameRate;
            Input.multiTouchEnabled = HeartSettings.EnableMultipleTouch;
            LoadScene();
        }

        /// <summary>
        /// Because launcher scene run before persistent scene so something initalize not available in launcher
        /// </summary>
        private async void LoadScene()
        {
            await UniTask.WaitUntil(() => loadingCompleted.Value);
            await Addressables.LoadSceneAsync(persistentScene);
            if (remoteConfigFetchCompleted != null) await UniTask.WaitUntil(() => remoteConfigFetchCompleted.Value);
            if (isWaitLevelLoad) await UniTask.WaitUntil(() => loadLevelCompleted.Value);

            // TODO : wait something else before load menu

            // manual load menu scene not via event
            Addressables.LoadSceneAsync(Constant.MENU_SCENE, LoadSceneMode.Additive).Completed += OnMenuSceneLoaded;
        }

        private void OnMenuSceneLoaded(AsyncOperationHandle<SceneInstance> scene)
        {
            if (scene.Status == AsyncOperationStatus.Succeeded)
            {
                string sceneName = scene.Result.Scene.name;
                Static.sceneHolder.Add(sceneName, scene);
                SceneManager.SetActiveScene(SceneManager.GetSceneByName(sceneName));
            }
        }
    }
}