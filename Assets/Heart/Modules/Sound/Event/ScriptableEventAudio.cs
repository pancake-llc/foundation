using Pancake.Scriptable;
using UnityEngine;

namespace Pancake.Sound
{
    [Searchable]
    [EditorIcon("scriptable_event")]
    [CreateAssetMenu(fileName = "scriptable_event_audio.asset", menuName = "Pancake/Sound/Scriptable Event Audio")]
    public class ScriptableEventAudio : ScriptableEventFuncT_TResult<Audio, AudioHandle>
    {
    }
}