using Cysharp.Threading.Tasks;
using Pancake.Common;
using UnityEngine;
using UnityEngine.SceneManagement;
using VContainer.Unity;
using VitalRouter;

namespace Pancake.SceneFlow
{
    public class EntryPoint : IInitializable
    {
        private bool _loadedCompleted;

        /// <summary>
        /// Because launcher scene run before persistent scene so something initalize not available in launcher
        /// </summary>
        private async void LoadScene()
        {
            await UniTask.WaitForSeconds(1f);
            await SceneManager.LoadSceneAsync(Constant.PERSISTENT_SCENE);

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

        public void Initialize()
        {
#if UNITY_EDITOR
            Application.targetFrameRate = (int) HeartEditorSettings.TargetFrameRate;
#else
            Application.targetFrameRate = (int) HeartSettings.TargetFrameRate;
#endif

            Input.multiTouchEnabled = HeartSettings.EnableMultipleTouch;
            LoadScene();
        }
    }
}