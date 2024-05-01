using System;
using System.Collections.Generic;
using UnityEngine;

namespace Pancake.LevelSystemEditor
{
    [EditorIcon("so_dark_setting")]
    [Serializable]
    public class LevelSystemEditorSetting : ScriptableSettings<LevelSystemEditorSetting>
    {
        public readonly string[] optionsSpawn = {"Default", "Index", "Custom"};
        public readonly string[] optionsMode = {"Renderer", "Ignore"};
        public int SelectedSpawn { get; set; }
        public int SelectedMode { get; set; }
        public float GizmosRadius { get; set; } = 2f;
        public float ElementSize { get; set; } = 65f;
        public GameObject RootSpawn { get; set; }
        public int RootIndexSpawn { get; set; }
        public List<string> whitelistPaths = new List<string>();
        public List<string> blacklistPaths = new List<string>();

        public Vector2 WhitelistScrollPosition { get; set; }
        public Vector2 BlacklistScrollPosition { get; set; }
        public Vector2 PickObjectScrollPosition { get; set; }

        public List<PickObject> PickObjects { get; set; } = new List<PickObject>();
        public PickObject CurrentPickObject { get; set; }
        public UnityEditor.Editor EditorInpsectorPreview { get; set; }
        public UnityEngine.Object PreviousObjectInpectorPreview { get; set; }
        public UnityEngine.GameObject PreviewPickupObject { get; set; }
    }
}