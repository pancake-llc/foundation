using UnityEngine;
using UnityEngine.SceneManagement;

namespace Pancake
{
    internal static class EditorInitializer
    {
#if UNITY_EDITOR
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        public static void Init()
        {
            string startScene = SceneManager.GetActiveScene().name;
            if (startScene.Equals(Invariant.LAUNCHER_SCENE)) return;
            if (startScene.Equals(Invariant.PERSISTENT_SCENE))
            {
                SceneManager.LoadScene(Invariant.MENU_SCENE, LoadSceneMode.Additive);
                App.Delay(0.016f, () => SceneManager.SetActiveScene(SceneManager.GetSceneByName(Invariant.MENU_SCENE)));
                return;
            }

            SceneManager.LoadScene(Invariant.PERSISTENT_SCENE, LoadSceneMode.Single);
            string activeSceneName = SceneManager.GetActiveScene().name;
            SceneManager.LoadScene(activeSceneName, LoadSceneMode.Additive);
            App.Delay(0.016f, () => SceneManager.SetActiveScene(SceneManager.GetSceneByName(activeSceneName)));
        }
#endif
    }
}