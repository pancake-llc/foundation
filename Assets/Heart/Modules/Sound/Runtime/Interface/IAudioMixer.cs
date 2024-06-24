using UnityEngine.Audio;

namespace Pancake.Sound
{
    public interface IAudioMixer
    {
#if !UNITY_WEBGL
        AudioMixer Mixer { get; }
#endif

        internal AudioMixerGroup GetTrack(EAudioTrackType trackType);
        internal void ReturnTrack(EAudioTrackType trackType, AudioMixerGroup track);
    }
}