using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Pancake
{
    public abstract class Initializer : MonoBehaviour
    {
        protected virtual void Awake() { }

        protected virtual void Start()
        {
            Addressables.InitializeAsync().WaitForCompletion();
#if UNITY_ANDROID || UNITY_IOS
            Application.targetFrameRate = 60;
#endif
        }
    }
}