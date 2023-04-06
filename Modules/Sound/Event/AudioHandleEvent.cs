using Pancake.Attribute;
using Pancake.Scriptable;
using UnityEngine;

namespace Pancake.Sound
{
    [Searchable]
    [EditorIcon("scriptable_event")]
    [CreateAssetMenu(fileName = "audio_handle_channel.asset", menuName = "Pancake/Sound/AudioHandleEvent")]
    public class AudioHandleEvent : ScriptableEventFunc<AudioHandle, bool>
    {
        
    }
}