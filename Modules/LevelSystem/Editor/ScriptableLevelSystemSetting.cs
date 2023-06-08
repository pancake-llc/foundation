using System.Collections.Generic;

namespace Pancake.LevelSystemEditor
{
    [EditorIcon("scriptable_editor_setting")]
    public class ScriptableLevelSystemSetting : ScriptableSettings<ScriptableLevelSystemSetting>
    {
        public List<string> whitelistPaths = new List<string>();
        public List<string> blacklistPaths = new List<string>();
    }
}