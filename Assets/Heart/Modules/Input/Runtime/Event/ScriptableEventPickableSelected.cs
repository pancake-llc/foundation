using Pancake.Scriptable;
using UnityEngine;

namespace Pancake.MobileInput
{
    [EditorIcon("scriptable_event")]
    [CreateAssetMenu(fileName = "event_pickable_selected.asset", menuName = "Pancake/Input/Events/pickable selected")]
    public class ScriptableEventPickableSelected : ScriptableEvent<PickableSelected>
    {
    }
}