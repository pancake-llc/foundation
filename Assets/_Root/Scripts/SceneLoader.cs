using System.Collections.Generic;
using Pancake.Scriptable;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;

namespace Pancake.SceneFlow
{
    [EditorIcon("icon_default")]
    public class SceneLoader : GameComponent
    {
        [SerializeField] private ScriptableEventString changeSceneEvent;
        
        public static readonly Dictionary<string, AsyncOperationHandle<SceneInstance>> SceneHolder = new();
        
        private void Start() { changeSceneEvent.OnRaised += OnChangeScene; }

        private void OnChangeScene(string sceneName)
        {
            foreach (var scene in GetAllLoadedScene())
            {
                if (!scene.name.Equals(Constant.PERSISTENT_SCENE))
                {
                    Addressables.UnloadSceneAsync(SceneHolder[scene.name]);
                    SceneHolder.Remove(scene.name);
                }
            }

            Addressables.LoadSceneAsync(sceneName, LoadSceneMode.Additive).Completed += OnAdditiveSceneLoaded;
        }

        private void OnAdditiveSceneLoaded(AsyncOperationHandle<SceneInstance> handle)
        {
            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                string sceneName = handle.Result.Scene.name;
                SceneHolder.Add(sceneName, handle);
                SceneManager.SetActiveScene(SceneManager.GetSceneByName(sceneName));
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