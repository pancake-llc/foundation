using Pancake.Apex;
using Pancake.Localization;
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

        [Space] [SerializeField] private bool isWaitLevelLoaded;
        [SerializeField, ShowIf("isWaitLevelLoaded")] private BoolVariable loadLevelCompleted;

        [Space] [SerializeField] private bool requireInitLocalization;
        [SerializeField, ShowIf("requireInitLocalization")] private BoolVariable localizationInitialized;

        private void Awake()
        {
            Application.targetFrameRate = (int) HeartSettings.TargetFrameRate;
            Input.multiTouchEnabled = HeartSettings.EnableMultipleTouch;
            if (requireInitLocalization)
            {
                UserData.LoadLanguageSetting(LocaleSettings.DetectDeviceLanguage); // Do not add any other load process here to avoid slow loading
                localizationInitialized.Value = true;
            }

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
            if (isWaitLevelLoaded) await UniTask.WaitUntil(() => loadLevelCompleted.Value);

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