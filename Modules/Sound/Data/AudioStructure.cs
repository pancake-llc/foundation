using Pancake.Apex;
using UnityEngine;

namespace Pancake.Sound
{
    [Searchable]
    [EditorIcon("scriptable_sfx")]
    [CreateAssetMenu(fileName = "AudioStructure", menuName = "Pancake/Sound/Audio Structure", order = 0)]
    public class AudioStructure : ScriptableObject
    {
        [SerializeField, Label("Audio")] private Audio au;
        [SerializeField] private AudioPlayEvent audioPlayChannel;
        [SerializeField] private AudioConfig audioConfig;

        public void Play() { audioPlayChannel.Raise(au, audioConfig, Vector3.zero); }
    }
}