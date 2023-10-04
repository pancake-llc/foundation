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

        private void Start() { Addressables.LoadSceneAsync(launcher); }
    }
}