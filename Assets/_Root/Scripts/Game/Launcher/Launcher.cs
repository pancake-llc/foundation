using Cysharp.Threading.Tasks;
using Pancake.Game.Interfaces;
using Pancake.Localization;
using Pancake.Notification;
using Sisus.Init;
using UnityEngine.SceneManagement;

namespace Pancake.Game
{
    using UnityEngine;

    [EditorIcon("icon_entry")]
    public class Launcher : MonoBehaviour<ILoading>
    {
        [SerializeField] private ScriptableNotification dailyNotification;
        [SerializeField] private bool requireInitLocalization;

        private ILoading _loading;

        protected override void Init(ILoading argument) { _loading = argument; }

        private void Awake()
        {
#if UNITY_EDITOR
            Application.targetFrameRate = (int) HeartEditorSettings.TargetFrameRate;
#else
            Application.targetFrameRate = (int) HeartSettings.TargetFrameRate;
#endif

            Input.multiTouchEnabled = HeartSettings.EnableMultipleTouch;

            if (requireInitLocalization) UserData.LoadLanguageSetting(LocaleSettings.DetectDeviceLanguage);
        }

        private void Start()
        {
            LitMotion.LMotion.Create(0, 0, 0).RunWithoutBinding();
            if (dailyNotification != null) dailyNotification.Schedule();
            LoadScene();
        }

        private async void LoadScene()
        {
            await UniTask.WaitUntil(() => _loading.IsLoadingCompleted);
            await SceneManager.LoadSceneAsync(Constant.Scene.PERSISTENT, LoadSceneMode.Single);
            SceneManager.sceneLoaded += OnMenuSceneLoaded;
            await SceneManager.LoadSceneAsync(Constant.Scene.MENU, LoadSceneMode.Additive);
        }

        private void OnMenuSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            SceneManager.sceneLoaded -= OnMenuSceneLoaded;
            Static.sceneHolder.Add(scene.name, scene);
            SceneManager.SetActiveScene(scene);
        }
    }
}