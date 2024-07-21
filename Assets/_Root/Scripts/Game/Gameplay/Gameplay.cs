using Pancake.Sound;
using UnityEngine.SceneManagement;

namespace Pancake.Game
{
    using UnityEngine;

    [EditorIcon("icon_entry")]
    public class Gameplay : MonoBehaviour
    {
        public async void GotoMenu()
        {
            AudioStatic.StopAll();
            SceneManager.sceneLoaded += OnMenuSceneLoaded;
            Static.sceneHolder.Remove(Constant.Scene.GAMEPLAY);
            SceneManager.UnloadSceneAsync(Constant.Scene.GAMEPLAY);
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