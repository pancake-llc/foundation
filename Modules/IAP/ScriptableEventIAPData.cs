using Pancake.Scriptable;
using UnityEngine;

namespace Pancake.IAP
{
    [EditorIcon("scriptable_event")]
    [CreateAssetMenu(fileName = "scriptable_event_IAPData.asset", menuName = "Pancake/Scriptable/ScriptableEvents/IAPData")]
    public class ScriptableEventIAPData : ScriptableEvent<IAPDataVariable>
    {
    }
}