using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

namespace Pancake.Loader
{
    public static class LoadingUtility
    {
        public static AsyncOperation LoadAsync(int scene, LoadSceneMode mode = LoadSceneMode.Single) { return SceneManager.LoadSceneAsync(scene, mode); }

        public static AsyncOperation LoadAsync(string scene, LoadSceneMode mode = LoadSceneMode.Single) { return SceneManager.LoadSceneAsync(scene, mode); }

        public static AsyncOperation UnloadAsync(int scene) { return SceneManager.UnloadSceneAsync(scene); }

        public static AsyncOperation UnloadAsync(string scene) { return SceneManager.UnloadSceneAsync(scene); }

        public static void RegisterOnLoaded(UnityAction<Scene, LoadSceneMode> onSceneLoaded) { SceneManager.sceneLoaded += onSceneLoaded; }

        public static void UnRegisterOnLoaded(UnityAction<Scene, LoadSceneMode> onSceneLoaded) { SceneManager.sceneLoaded -= onSceneLoaded; }
    }
}