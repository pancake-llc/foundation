using Pancake.Game;
using UnityEngine.SceneManagement;

namespace PancakeEditor.Game
{
    using UnityEngine;

    internal static class EditorSceneInitializer
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        public static async void Init()
        {
            string startScene = SceneManager.GetActiveScene().name;
            switch (startScene)
            {
                case Constant.Scene.LAUNCHER: return;
                case Constant.Scene.PERSISTENT:
                case Constant.Scene.MENU:
                case Constant.Scene.GAMEPLAY:
                    await SceneManager.LoadSceneAsync(Constant.Scene.LAUNCHER);
                    break;
            }
        }
    }
}