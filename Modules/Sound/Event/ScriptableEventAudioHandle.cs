using Pancake.Scriptable;
using UnityEngine;

namespace Pancake.Sound
{
    [Searchable]
    [EditorIcon("scriptable_event")]
    [CreateAssetMenu(fileName = "scriptable_audio_handle_event.asset", menuName = "Pancake/Sound/Scriptable Event Audio Handle")]
    public class ScriptableEventAudioHandle : ScriptableEvent<AudioHandle>
    {
    }
}