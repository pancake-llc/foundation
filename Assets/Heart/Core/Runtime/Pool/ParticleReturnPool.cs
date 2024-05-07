using Pancake.Common;
using UnityEngine;

namespace Pancake
{
    [RequireComponent(typeof(ParticleSystem))]
    public class ParticleReturnPool : MonoBehaviour
    {
        private ParticleSystem _particle;
        private DelayHandle _handle;

        private void OnEnable()
        {
            if (_particle == null) _particle = GetComponent<ParticleSystem>();
            _particle.Play();
            _handle = App.Delay(_particle.main.duration, Despawn);
        }

        private void Despawn() { gameObject.Return(); }

        private void OnDestroy()
        {
            if (_handle is {IsCompleted: false}) App.CancelDelay(_handle);
        }
    }
}