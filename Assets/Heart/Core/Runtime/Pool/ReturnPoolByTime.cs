using UnityEngine;

namespace Pancake
{
    public class ReturnPoolByTime : MonoBehaviour
    {
        [SerializeField] private float time;
        private void OnEnable() { App.Delay(time, Despawn); }

        private void Despawn() => gameObject.Return();
    }
}