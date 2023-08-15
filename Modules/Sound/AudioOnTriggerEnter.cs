namespace Pancake.Sound
{
    using UnityEngine;

    /// <summary>
    /// Class to activate AudioCues when a GameObject (i.e. the Player) enters the trigger Collider on this same GameObject.
    /// This component is mostly used for testing purposes.
    /// </summary>
    [RequireComponent(typeof(AudioComponent))]
    public class AudioOnTriggerEnter : CacheGameComponent<AudioComponent>
    {
        [SerializeField] private string tagDetect;

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag(tagDetect)) component.Play();
        }
    }
}