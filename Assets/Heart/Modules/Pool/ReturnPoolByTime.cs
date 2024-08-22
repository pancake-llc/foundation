using Pancake.Common;
using UnityEngine;

namespace Pancake.Pools
{
    public class ReturnPoolByTime : MonoBehaviour
    {
        [SerializeField] private float time;

        private DelayHandle _delayHandle;
        
        private void OnEnable() { _delayHandle = App.Delay(time, Despawn); }

        private void Despawn() => gameObject.Return();

        private void OnDestroy() { App.CancelDelay(_delayHandle); }
    }
}