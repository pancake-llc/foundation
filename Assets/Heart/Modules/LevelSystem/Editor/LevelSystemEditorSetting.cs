using System;
using System.Collections.Generic;

namespace Pancake.LevelSystemEditor
{
    [EditorIcon("scriptable_editor_setting")]
    [Serializable]
    public class LevelSystemEditorSetting : ScriptableSettings<LevelSystemEditorSetting>
    {
        public List<string> whitelistPaths = new List<string>();
        public List<string> blacklistPaths = new List<string>();
    }
}