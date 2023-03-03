using UnityEditor;
using UnityEngine;

namespace PancakeEditor.Scriptable
{
    public class ScriptableWizard
    {
        [MenuItem("Tools/create")]
        public static void Create(){
            PopupWindow.Show(new Rect(), new CreateTypeWindow(new Rect(500, 500, 0, 0)));
        }
    }
}