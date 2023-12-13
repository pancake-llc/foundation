using System.Linq;
using Pancake.Greenery;
using UnityEditor;
using UnityEngine;

namespace Pancake.GreeneryEditor
{
    public static class GreeneryEditorUtilities
    {
        public static GreeneryManager GetActiveManager()
        {
            if (Selection.activeGameObject != null && Selection.activeGameObject.TryGetComponent(out LocalGreeneryManager localGreeneryManager))
            {
                return localGreeneryManager;
            }
            else
            {
                return GetFirstNonLocalManager();
            }
        }

        public static GreeneryManager GetFirstNonLocalManager()
        {
            GreeneryManager[] greeneryManagersInScene = GameObject.FindObjectsOfType<GreeneryManager>();
            if (greeneryManagersInScene.Length > 0 && greeneryManagersInScene.Except(GameObject.FindObjectsOfType<LocalGreeneryManager>()).ToList().Count > 0)
            {
                return greeneryManagersInScene.Except(GameObject.FindObjectsOfType<LocalGreeneryManager>()).ToList()[0];
            }
            else
            {
                return null;
            }
        }

        public static bool IsLeftClicking(Event currentEvent) { return (currentEvent.type == EventType.MouseDown && currentEvent.button == 0); }
    }
}