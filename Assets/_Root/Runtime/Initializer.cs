using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Pancake
{
    public class Initializer : MonoBehaviour
    {
        protected virtual void Awake() { }

        protected void Start()
        {
            Addressables.InitializeAsync().WaitForCompletion();
#if UNITY_ANDROID || UNITY_IOS
            Application.targetFrameRate = 60;
#endif
        }
    }
}