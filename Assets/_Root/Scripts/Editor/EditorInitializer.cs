#if UNITY_EDITOR
using System.Reflection;
using Pancake;
using Pancake.ExLibEditor;
using Pancake.Linq;
using Pancake.SceneFlow;
using Pancake.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.SceneManagement;

namespace PancakeEditor
{
    internal static class EditorInitializer
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        public static async void Init()
        {
            string startScene = SceneManager.GetActiveScene().name;
            switch (startScene)
            {
                case Constant.LAUNCHER_SCENE: return;
                case Constant.PERSISTENT_SCENE:
                case Constant.MENU_SCENE:
                case Constant.GAMEPLAY_SCENE:

                    await Addressables.LoadSceneAsync(Constant.LAUNCHER_SCENE);
                    if (startScene.Equals(Constant.PERSISTENT_SCENE))
                    {
                        var poolCleaner = ProjectDatabase.FindAll<ScriptableObject>().Map(o => o as IPoolCleaner).Filter(c => c != null);
                        foreach (var cleaner in poolCleaner)
                        {
                            cleaner.InternalClearPool();
                        }
                    }

                    break;
            }
        }
    }
}

#endif