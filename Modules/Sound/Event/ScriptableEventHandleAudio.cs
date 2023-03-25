using Pancake.Attribute;
using Pancake.Scriptable;
using UnityEngine;

namespace Pancake.Sound
{
    [Searchable]
    [EditorIcon("scriptable_event")]
    [CreateAssetMenu(fileName = "scriptable_event_finish_audio.asset", menuName = "Pancake/Sound/FinishEvent")]
    public class ScriptableEventHandleAudio : ScriptableEventFunc<AudioHandle, bool>
    {
        
    }
}