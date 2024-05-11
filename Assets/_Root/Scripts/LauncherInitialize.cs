using Alchemy.Inspector;
using Pancake.Localization;
using Pancake.Notification;
using Pancake.Scriptable;
using Cysharp.Threading.Tasks;
using Pancake.Tracking;
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
        [SerializeField] private bool isWaitRemoteConfig;
        [SerializeField] private ScriptableNotification dailyNotification;
        [SerializeField] private AssetReference persistentScene;
        [Space] [SerializeField] private bool isWaitLevelLoaded;
        [Space] [SerializeField] private bool requireInitLocalization;
        [SerializeField, ShowIf("requireInitLocalization")] private BoolVariable localizationInitialized;

        private void Awake()
        {
#if UNITY_EDITOR
            Application.targetFrameRate = (int) HeartEditorSettings.TargetFrameRate;
#else
            Application.targetFrameRate = (int) HeartSettings.TargetFrameRate;
#endif

            Input.multiTouchEnabled = HeartSettings.EnableMultipleTouch;
            if (requireInitLocalization)
            {
                UserData.LoadLanguageSetting(LocaleSettings.DetectDeviceLanguage); // Do not add any other load process here to avoid slow loading
                localizationInitialized.Value = true;
            }

            ScheduleDailyNotification();
            LoadScene();
        }

        private void ScheduleDailyNotification()
        {
            if (dailyNotification != null) dailyNotification.Schedule();
        }

        /// <summary>
        /// Because launcher scene run before persistent scene so something initalize not available in launcher
        /// </summary>
        private async void LoadScene()
        {
            await UniTask.WaitUntil(() => loadingCompleted.Value);
            await Addressables.LoadSceneAsync(persistentScene);
            if (isWaitRemoteConfig) await UniTask.WaitUntil(() => RemoteConfig.IsFetchCompleted);
            if (isWaitLevelLoaded) await EventBus<LevelLoadedNoticeEvent>.GetAwaiter().Async();

            // TODO : wait something else before load menu
            // Don't call the ScheduleDailyNotification function here because LoadSceneAsync may cause the Schedule to be inaccurate 

            // manual load menu scene not via event
            Addressables.LoadSceneAsync(Constant.MENU_SCENE, LoadSceneMode.Additive).Completed += OnMenuSceneLoaded;
        }

        private void OnMenuSceneLoaded(AsyncOperationHandle<SceneInstance> scene)
        {
            if (scene.Status == AsyncOperationStatus.Succeeded)
            {
                string sceneName = scene.Result.Scene.name;
                SceneLoader.SceneHolder.Add(sceneName, scene);
                SceneManager.SetActiveScene(SceneManager.GetSceneByName(sceneName));
            }
        }
    }

    public struct LevelLoadedNoticeEvent : IEvent
    {
    }
}