using UnityEngine;

#if PANCAKE_ADDRESSABLE_SUPPORT
using UnityEngine.AddressableAssets;
#endif


namespace Pancake
{
    public abstract class Initializer : MonoBehaviour
    {
        protected virtual void Awake() { }

        protected virtual void Start()
        {
#if PANCAKE_ADDRESSABLE_SUPPORT
            Addressables.InitializeAsync().WaitForCompletion();
#endif

#if UNITY_ANDROID || UNITY_IOS
            Application.targetFrameRate = 60;
#endif
        }
    }
}