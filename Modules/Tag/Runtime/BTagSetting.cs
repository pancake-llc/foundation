using UnityEngine;

namespace Pancake.BTag
{
    public enum SearchRegistryOption
    {
        FullRefresh,
        IterativeRefresh,
        CachedResultsOnly
    }

    public enum InclusionRule
    {
        Any,
        MustInclude,
        MustExclude
    }

    [EditorIcon("scriptable_editor_setting")]
    public class BTagSetting : ScriptableSettings<BTagSetting>
    {
        [SerializeField] public bool showAssetReferences;
        [SerializeField] public SearchRegistryOption searchMode;
        [SerializeField] public bool disableEditorChecks;
        [SerializeField] public bool showHashes;
        [SerializeField] public bool showGroupNames;
    }
}