using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Obvious.Soap.Editor
{
    /// <summary>
    /// This class is there to reset variables when you enable the "Editor Play Mode settings" to play faster.
    /// Because during the fast play mode, "OnEnable" on scriptable objects is not called.
    /// Therefore we need to manually reset them ! A bit annoying, but the speed gain from using the fast play mode outweigh the cost.
    /// </summary>
    [CreateAssetMenu(fileName = "PlayModeResetter.asset",
        menuName = "Soap/PlayModeResetter")]
    public class PlayModeResetter : ScriptableObject
    {
        [Tooltip("Change this to the path where are located your scriptable variables & lists")] 
        [SerializeField]
        private string _path = "Assets/Obvious/Soap/Examples/Content/ScriptableVariables";

        [Space]
        [SerializeField][Tooltip("If true, will automatically repopulate with all the variables at the path when you enter play mode.")]
        private bool _autoPopulate = true;

        [Tooltip("Serialized list so that you can have a view of all your variables")] 
        [SerializeField]
        private List<ScriptableObject> _variablesToReset = null;

        public void GetAllIResetAtPath()
        {
            _variablesToReset = PancakeEditor.Editor.FindAll<ScriptableObject>(_path);

            for (int i = _variablesToReset.Count - 1; i >= 0; i--)
            {
                var variable = _variablesToReset[i];
                //filter variables that cannot be reset
                if (variable is IReset)
                    continue;
                _variablesToReset.RemoveAt(i);
            }

            //Marks the object as dirty so it is visible in Version control.
            EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssets();
        }

        private static void ResetManually()
        {
            var isFastPlayMode = EditorSettings.enterPlayModeOptionsEnabled;
            if (!isFastPlayMode)
                return;

            foreach (var playModeResetter in Instances)
            {
                if (playModeResetter._autoPopulate)
                    playModeResetter.GetAllIResetAtPath();
                playModeResetter._variablesToReset.ForEach(i => (i as IReset)?.ResetToInitialValue());
            }
        }

        #region Initialization

        private static PlayModeResetter[] _instances;
        public static PlayModeResetter[] Instances => _instances;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        public static void Reset()
        {
            _instances = Resources.LoadAll<PlayModeResetter>("PlayModeResetter");
            ResetManually();
        }

        #endregion
    }
}