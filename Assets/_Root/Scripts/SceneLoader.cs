using Pancake.Scriptable;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;

namespace Pancake.SceneFlow
{
    [EditorIcon("csharp")]
    public class SceneLoader : GameComponent
    {
        [SerializeField] private ScriptableEventString changeSceneEvent;


        private void Start() { changeSceneEvent.OnRaised += OnChangeScene; }

        private void OnChangeScene(string sceneName)
        {
            foreach (var scene in GetAllLoadedScene())
            {
                if (!scene.name.Equals(Constant.PERSISTENT_SCENE))
                {
                    Addressables.UnloadSceneAsync(Static.sceneHolder[scene.name]);
                    Static.sceneHolder.Remove(scene.name);
                }
            }

            Addressables.LoadSceneAsync(sceneName, LoadSceneMode.Additive).Completed += OnAdditiveSceneLoaded;
        }

        private void OnAdditiveSceneLoaded(AsyncOperationHandle<SceneInstance> handle)
        {
            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                string sceneName = handle.Result.Scene.name;
                Static.sceneHolder.Add(sceneName, handle);
                App.Delay(0.016f, () => SceneManager.SetActiveScene(SceneManager.GetSceneByName(sceneName)));
            }
        }

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