using Pancake.Scriptable;
using UnityEngine;

namespace Pancake.Sound
{
    [Searchable]
    [EditorIcon("scriptable_event")]
    [CreateAssetMenu(fileName = "audio_handle_channel.asset", menuName = "Pancake/Sound/Audio Handle Event")]
    public class AudioHandleEvent : ScriptableEventFunc<AudioHandle, bool>
    {
    }
}