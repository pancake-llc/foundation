using System.Linq;
using PancakeEditor;
using UnityEditor;
using UnityEngine;

namespace Obvious.Soap.Editor
{
    public class SoapWindowBase : EditorWindow
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