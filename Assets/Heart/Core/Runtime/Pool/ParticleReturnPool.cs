using UnityEngine;

namespace Pancake
{
    [RequireComponent(typeof(ParticleSystem))]
    public class ParticleReturnPool : MonoBehaviour
    {
        private ParticleSystem _particle;

        private void OnEnable()
        {
            if (_particle == null) _particle = GetComponent<ParticleSystem>();
            _particle.Play();
            App.Delay(_particle.main.duration, Despawn);
        }

        private void Despawn() { gameObject.Return(); }
    }
}