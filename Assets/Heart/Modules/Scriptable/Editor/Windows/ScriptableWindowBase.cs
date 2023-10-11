using System.Linq;
using Pancake.ExLibEditor;
using UnityEditor;
using UnityEngine;

namespace Pancake.ScriptableEditor
{
    public class ScriptableWindowBase : EditorWindow
    {
        protected virtual string HeaderTitle { get; }

        protected virtual void OnEnable() { Init(); }

        protected virtual void Init() { }

        protected virtual void OnGUI()
        {
            Uniform.DrawHeader(HeaderTitle);
            GUILayout.Space(2);
        }
    }
}