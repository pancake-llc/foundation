using UnityEngine.SceneManagement;

namespace Pancake.SceneFlow
{
    public sealed class SceneLoader
    {
        public void ChangeScene(string sceneName)
        {
            foreach (var scene in GetAllLoadedScene())
            {
                if (!scene.name.Equals(Constant.PERSISTENT_SCENE)) SceneManager.UnloadSceneAsync(scene);
            }

            SceneManager.sceneLoaded += OnAdditiveSceneLoaded;
            SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
        }

        private void OnAdditiveSceneLoaded(Scene scene, LoadSceneMode mode) { SceneManager.SetActiveScene(scene); }

        private Scene[] GetAllLoadedScene()
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