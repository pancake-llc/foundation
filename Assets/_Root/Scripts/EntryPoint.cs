using Alchemy.Inspector;
using Pancake.Localization;
using Pancake.Notification;
using Pancake.Scriptable;
using Cysharp.Threading.Tasks;
using Pancake.Tracking;
using UnityEngine;
using UnityEngine.SceneManagement;
using VitalRouter;

namespace Pancake.SceneFlow
{
    [Routes]
    public partial class EntryPoint : GameComponent
    {
        [SerializeField] private BoolVariable loadingCompleted;
        [SerializeField] private ScriptableNotification dailyNotification;
        [SerializeField] private bool isWaitRemoteConfig;
        [SerializeField] private bool isWaitLevelLoaded;
        [SerializeField] private bool requireInitLocalization;
        [SerializeField, ShowIf("requireInitLocalization")] private BoolVariable localizationInitialized;

        private bool _isLevelLoadCompleted;

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

        public void OnLevelLoadedCompleted(LevelLoadedNoticeCommand cmd) { _isLevelLoadCompleted = true; }

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
            await SceneManager.LoadSceneAsync(Constant.PERSISTENT_SCENE);
            if (isWaitRemoteConfig) await UniTask.WaitUntil(() => RemoteConfig.IsFetchCompleted);
            if (isWaitLevelLoaded) await UniTask.WaitUntil(() => _isLevelLoadCompleted);

            // TODO : wait something else before load menu
            // Don't call the ScheduleDailyNotification function here because LoadSceneAsync may cause the Schedule to be inaccurate 

            // manual load menu scene not via event
            SceneManager.sceneLoaded += OnMenuSceneLoaded;
            SceneManager.LoadSceneAsync(Constant.MENU_SCENE, LoadSceneMode.Additive);
        }

        private void OnMenuSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            SceneManager.sceneLoaded -= OnMenuSceneLoaded;
            SceneManager.SetActiveScene(scene);
        }
    }

    public readonly struct LevelLoadedNoticeCommand : ICommand
    {
    }
}