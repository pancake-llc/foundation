using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Pancake.SceneFlow
{
    using Pancake;

    public class StartPoint : GameComponent
    {
        [SerializeField] private AssetReference launcher;

        private void Awake() { DontDestroyOnLoad(gameObject); }

        private void Start() { Addressables.LoadSceneAsync(launcher); }
    }
}