using UnityEngine;

namespace Pancake.Pools
{
    [RequireComponent(typeof(ParticleSystem))]
    public class ParentParticleReturnPool : MonoBehaviour
    {
        private void Awake()
        {
            var main = GetComponent<ParticleSystem>().main;
            main.stopAction = ParticleSystemStopAction.Callback;
        }

        private void OnParticleSystemStopped() { transform.parent.gameObject.Return(); }
    }
}