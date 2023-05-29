using System.Collections.Generic;
using Pancake.Scriptable;
using UnityEngine;

namespace Pancake.UI
{
    [Searchable]
    [EditorIcon("scriptable_event")]
    [CreateAssetMenu(fileName = "popup_display_chanel.asset", menuName = "Pancake/UI/PopupShowEvent")]
    public class PopupShowEvent : ScriptableEventBase, IDrawObjectsInInspector
    {
        public List<Object> GetAllObjects()
        {
            return null;
        }
    }
}