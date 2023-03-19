using System.Collections.Generic;
using Pancake.Attribute;
using Pancake.Scriptable;
using UnityEditor;
using UnityEngine;

namespace PancakeEditor.Scriptable
{
    /// <summary>
    /// This class is there to reset variables when you enable the "Editor Play Mode settings" to play faster.
    /// Because during the fast play mode, "OnEnable" on scriptable objects is not called.
    /// Therefore we need to manually reset them ! A bit annoying, but the speed gain from using the fast play mode outweigh the cost.
    /// </summary>
    [EditorIcon("scriptable_playmode_resetter")]
    [Searchable]
    [CreateAssetMenu(fileName = "PlayModeResetter.asset", menuName = "Pancake/Scriptable/PlayModeResetter")]
    public class PlayModeResetter : ScriptableObject
    {
        [Tooltip("Change this to the path where are located your scriptable variables & lists")] [SerializeField]
        private string path = "Assets/_Root/Storages/ScriptableVariables";

        [Space] [SerializeField] [Tooltip("If true, will automatically repopulate with all the variables at the path when you enter play mode.")]
        private bool autoPopulate = true;

        [Tooltip("Serialized list so that you can have a view of all your variables")] [SerializeField]
        private List<ScriptableObject> variablesToReset;
        
        private static PlayModeResetter[] instances;
        public static PlayModeResetter[] Instances => instances;
        
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        public static void Reset()
        {
            instances = Resources.LoadAll<PlayModeResetter>("PlayModeResetter") ;
            ResetManually();
        }

        public void GetAllIResetAtPath()
        {
            variablesToReset = Editor.FindAll<ScriptableObject>(path);

            for (int i = variablesToReset.Count - 1; i >= 0; i--)
            {
                var variable = variablesToReset[i];
                //filter variables that cannot be reset
                if (variable is IReset) continue;
                variablesToReset.RemoveAt(i);
            }

            EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssets();
        }
        
        private static void ResetManually()
        {
            var isFastPlayMode = EditorSettings.enterPlayModeOptionsEnabled;
            if (!isFastPlayMode) return;

            foreach (var playModeResetter in Instances)
            {
                if (playModeResetter.autoPopulate) playModeResetter.GetAllIResetAtPath();
                playModeResetter.variablesToReset.ForEach(i => (i as IReset)?.Reset());
            }
        }
    }
}