#if UNITY_EDITOR
using System.Reflection;
using Pancake.ExLibEditor;
using Pancake.SceneFlow;
using Pancake.Sound;
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
                        var poolSoundEmitters = ProjectDatabase.FindAll<SoundEmitterPool>();
                        foreach (var pool in poolSoundEmitters)
                        {
                            var method = pool.GetType().BaseType.BaseType.GetMethod("InternalClearPool", BindingFlags.Instance | BindingFlags.NonPublic);
                            Debug.Log(method);
                            method.Invoke(pool, null);
                        }
                    }

                    break;
            }
        }
    }
}

#endif