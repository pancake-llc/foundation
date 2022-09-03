using System.Reflection;
using Pancake.SOA;
using UnityEngine;
using UnityEditor;

namespace Pancake.Editor.SOA
{
    [CustomEditor(typeof(BaseGameEventListener<,>), true)]
    public class GameEventListenerEditor : BaseGameEventListenerEditor
    {
        private MethodInfo _raiseMethod;

        protected override void OnEnable()
        {
            base.OnEnable();

            _raiseMethod = target.GetType().BaseType.GetMethod("OnEventRaised");
        }
        protected override void DrawRaiseButton()
        {
            if (GUILayout.Button("Raise"))
            {
                _raiseMethod.Invoke(target, null);
            }
        }
    } 
}