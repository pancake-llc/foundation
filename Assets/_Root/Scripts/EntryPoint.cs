using Pancake.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Pancake.SceneFlow
{
    using Pancake;

    [EditorIcon("scene_manager")]
    public class EntryPoint : GameComponent
    {
        [SerializeField] private AssetReference launcher;

        private void Awake() { DontDestroyOnLoad(gameObject); }

        private async void Start()
        {
            await Addressables.LoadSceneAsync(launcher);
            Destroy(gameObject);
        }
    }
}