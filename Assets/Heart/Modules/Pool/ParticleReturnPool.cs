using UnityEngine;

namespace Pancake.Pools
{
    [RequireComponent(typeof(ParticleSystem))]
    public class ParticleReturnPool : MonoBehaviour
    {
        private void Awake()
        {
            var main = GetComponent<ParticleSystem>().main;
            main.stopAction = ParticleSystemStopAction.Callback;
        }

        private void OnParticleSystemStopped() { gameObject.Return(); }
    }
}