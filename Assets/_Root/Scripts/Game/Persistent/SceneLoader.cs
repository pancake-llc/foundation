using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Pancake.Game
{
    public static class SceneLoader
    {
        public static async UniTask LoadScene(string sceneName)
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
            foreach (var scene in GetAllLoadedScene())
            {
                if (!scene.name.Equals(Constant.Scene.PERSISTENT))
                {
                    Static.sceneHolder.Remove(scene.name);
                    await SceneManager.UnloadSceneAsync(scene.name);
                }
            }

            await SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
        }

        private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            Static.sceneHolder.Add(scene.name, scene);
            SceneManager.SetActiveScene(scene);
        }

        private static Scene[] GetAllLoadedScene()
        {
            int countLoaded = SceneManager.sceneCount;
            var loadedScenes = new Scene[countLoaded];

            for (var i = 0; i < countLoaded; i++)
            {
                loadedScenes[i] = SceneManager.GetSceneAt(i);
            }

            return loadedScenes;
        }
    }
}