using Pancake.Scriptable;
using UnityEngine;

namespace Pancake.Sound
{
    [Searchable]
    [EditorIcon("so_blue_event")]
    [CreateAssetMenu(fileName = "event_audio_handle.asset", menuName = "Pancake/Sound/Event Audio Handle")]
    public class ScriptableEventAudioHandle : ScriptableEvent<AudioHandle>
    {
    }
}