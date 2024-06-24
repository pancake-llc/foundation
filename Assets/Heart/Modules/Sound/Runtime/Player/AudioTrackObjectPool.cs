using UnityEngine;
using UnityEngine.Audio;

namespace Pancake.Sound
{
    internal class AudioTrackObjectPool : ObjectPool<AudioMixerGroup>
    {
        private readonly AudioMixerGroup[] _audioMixerGroups;
        private int _usedTrackCount;
        private readonly bool _isDominator;

        public AudioTrackObjectPool(AudioMixerGroup[] audioMixerGroups, bool isDominator = false)
            : base(null, audioMixerGroups.Length)
        {
            _audioMixerGroups = audioMixerGroups;
            _isDominator = isDominator;
        }

        protected override AudioMixerGroup CreateObject()
        {
            if (_usedTrackCount >= _audioMixerGroups.Length)
            {
                if (_isDominator)
                {
                    Debug.LogWarning(AudioConstant.LOG_HEADER +
                               "You have used up all the [Dominator] tracks. If you need more tracks, please click the [Add Dominator Track] button in settings at Audio in Wizard");
                }
                else
                {
                    Debug.LogWarning(AudioConstant.LOG_HEADER + "You have reached the limit of Audio tracks count, which is way beyond the MaxRealVoices count. " +
                                    "That means the sound will be inaudible, and also uncontrollable. For more infomation, please check the documentation");
                }

                return null;
            }

            var audioMixerGroup = _audioMixerGroups[_usedTrackCount];
            _usedTrackCount++;
            return audioMixerGroup;
        }

        protected override void DestroyObject(AudioMixerGroup track)
        {
            // max pool size equals to track count. there is no track will be destroyed.
        }
    }
}