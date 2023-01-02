#if PANCAKE_ADS
using Pancake.Editor;
using UnityEditor;

namespace Pancake.Monetization.Editor
{
    public class OnPostDeleted : AssetModificationProcessor
    {
        private static AssetDeleteResult OnWillDeleteAsset(string assetPath, RemoveAssetOptions options)
        {
            if (!SettingManager.IsAdmobSdkImported())
            {
                InEditor.ScriptingDefinition.RemoveDefineSymbolOnAllPlatforms(AdsEditorUtil.SCRIPTING_DEFINITION_ADMOB);
            }

            if (!SettingManager.IsMaxSdkImported())
            {
                InEditor.ScriptingDefinition.RemoveDefineSymbolOnAllPlatforms(AdsEditorUtil.SCRIPTING_DEFINITION_APPLOVIN);
            }

            return AssetDeleteResult.DidNotDelete;
        }
    }
}
#endif