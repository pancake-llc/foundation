using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Pancake.LevelSystemEditor
{
    [EditorIcon("scriptable_editor_setting")]
    public class ScriptableLevelSystemSetting : ScriptableSettings<ScriptableLevelSystemSetting>
    {
        public List<string> whitelistPaths = new List<string>();
        public List<string> blacklistPaths = new List<string>();

        [NonSerialized] internal Vector2 _pickObjectScrollPosition;
        [NonSerialized] public Vector2 whiteScrollPosition;
        [NonSerialized] public Vector2 blackScrollPosition;
        [NonSerialized] internal PickObject _currentPickObject;
        [NonSerialized] private List<PickObject> _pickObjects;
        [NonSerialized] internal SerializedObject _pathFolderSerializedObject;
        [NonSerialized] internal SerializedProperty _pathFolderProperty;
        [NonSerialized] internal int _selectedSpawn;
        [NonSerialized] internal int _selectedMode;
        [NonSerialized] internal GameObject _rootSpawn;
        [NonSerialized] internal int _rootIndexSpawn;
        [NonSerialized] internal GameObject _previewPickupObject;
        [NonSerialized] internal UnityEngine.Object _previousObjectInpectorPreview;
        [NonSerialized] internal UnityEditor.Editor _editorInpsectorPreview;


        public static List<PickObject> PickObjects => Instance._pickObjects ?? (Instance._pickObjects = new List<PickObject>());
    }
}