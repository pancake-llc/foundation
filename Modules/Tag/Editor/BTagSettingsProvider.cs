using System;
using System.Collections.Generic;
using System.IO;
using Pancake.BTag;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Pancake.BTagEditor
{
    public static class BTagSettingsProvider
    {
        const string Version = "1.4.3";

        const string HelpFullRefresh =
            "A Full Refresh will find references in:\n - Open Scenes\n - Prefabs\n - ScriptableObjects\n - Scenes\nEquivalent to manually clicking the 'Find All References' button via the inspector.";

        const string HelpIterativeRefresh = "Similar to Full Refresh but only updates for files that have been updated since the last search. The default.";

        const string HelpCachedOnly =
            "This option will only display the previously found results held in cache. This is a great option for those who prefer to manually click the 'Find All References' button and effectively disable the automatic refresh upon selection.";

        static Label helpLabel;

        [SettingsProvider]
        public static SettingsProvider CreateMyCustomSettingsProvider()
        {
            var provider = new SettingsProvider("Preferences/BTagSettings", SettingsScope.User)
            {
                label = "BTag",
                // activateHandler is called when the user clicks on the Settings item in the Settings window.
                activateHandler = (searchContext, rootElement) =>
                {
                    var contents = new ScrollView();
                    contents.style.paddingBottom = contents.style.paddingLeft = contents.style.paddingRight = contents.style.paddingTop = 10;
                    var settings = new SerializedObject(BTagSetting.Instance);

                    var title = new Label("BTag Settings")
                    {
                        style = {fontSize = 18, unityFontStyleAndWeight = new StyleEnum<FontStyle>(FontStyle.Bold), paddingBottom = 10}
                    };
                    title.AddToClassList("title");
                    var version = new Label(Version);
                    version.style.position = new StyleEnum<Position>(Position.Absolute);
                    version.style.top = 12;
                    version.style.right = 10;
                    contents.Add(version);
                    contents.Add(title);

                    var pf = new PropertyField(settings.FindProperty("searchMode"), "Auto-Reference Finding");
                    pf.RegisterCallback<ChangeEvent<string>>(UpdateHelpTextLabel);
                    pf.Bind(settings);
                    Label desc = new Label("The extents of the project that are automatically searched upon Selecting a Tag asset in the project window.");
                    desc.style.marginBottom = desc.style.marginLeft = desc.style.marginRight = desc.style.marginTop = 4f;
                    desc.style.whiteSpace = new StyleEnum<WhiteSpace>(WhiteSpace.Normal);
                    contents.Add(desc);
                    contents.Add(pf);
                    helpLabel = new Label("");
                    helpLabel.style.marginBottom = helpLabel.style.marginLeft = helpLabel.style.marginRight = helpLabel.style.marginTop = 4f;
                    helpLabel.style.paddingBottom = helpLabel.style.paddingLeft = helpLabel.style.paddingRight = helpLabel.style.paddingTop = 8f;
                    helpLabel.style.borderTopLeftRadius = helpLabel.style.borderBottomLeftRadius =
                        helpLabel.style.borderBottomRightRadius = helpLabel.style.borderTopRightRadius = 5f;
                    helpLabel.style.backgroundColor = new StyleColor(new Color(0.5f, 0.5f, 0.5f, 0.2f));
                    helpLabel.style.whiteSpace = new StyleEnum<WhiteSpace>(WhiteSpace.Normal);
                    UpdateHelpTextLabel();
                    contents.Add(helpLabel);


                    Toggle editorCheckToggle = new Toggle("Enable Editor Safety Checks ");
                    editorCheckToggle.value = !settings.FindProperty("disableEditorChecks").boolValue;
                    editorCheckToggle.RegisterValueChangedCallback(ce =>
                    {
                        settings.FindProperty("disableEditorChecks").boolValue = !ce.newValue;
                        settings.ApplyModifiedProperties();
                    });
                    contents.Add(editorCheckToggle);
                    Label editorCheckDesc = new Label(
                        "It is possible for multiple GameObjects to have the same Tag. The Editor Safety Checks will warn when a query in code would return more than one match but is only requesting the first. This is because it is non-deterministc - which object you get back might vary. You can, for example, add another Tag to ensure the query is more specific or add .AnyIsFine() to the query.");
                    editorCheckDesc.style.marginBottom = editorCheckDesc.style.marginLeft = editorCheckDesc.style.marginRight = editorCheckDesc.style.marginTop = 4f;
                    editorCheckDesc.style.paddingBottom = editorCheckDesc.style.paddingLeft = editorCheckDesc.style.paddingRight = editorCheckDesc.style.paddingTop = 8f;
                    editorCheckDesc.style.borderTopLeftRadius = editorCheckDesc.style.borderBottomLeftRadius =
                        editorCheckDesc.style.borderBottomRightRadius = editorCheckDesc.style.borderTopRightRadius = 5f;
                    editorCheckDesc.style.backgroundColor = new StyleColor(new Color(0.5f, 0.5f, 0.5f, 0.2f));
                    editorCheckDesc.style.whiteSpace = new StyleEnum<WhiteSpace>(WhiteSpace.Normal);
                    contents.Add(editorCheckDesc);

                    Toggle showGroupLabelsToggle = new Toggle("Show Group Labels");
                    showGroupLabelsToggle.value = settings.FindProperty("showGroupNames").boolValue;
                    showGroupLabelsToggle.RegisterValueChangedCallback(ce =>
                    {
                        settings.FindProperty("showGroupNames").boolValue = ce.newValue;
                        settings.ApplyModifiedProperties();
                    });
                    contents.Add(showGroupLabelsToggle);

                    Toggle showHashesToggle = new Toggle("Show Hashes");
                    showHashesToggle.value = settings.FindProperty("showHashes").boolValue;
                    showHashesToggle.RegisterValueChangedCallback(ce =>
                    {
                        settings.FindProperty("showHashes").boolValue = ce.newValue;
                        settings.ApplyModifiedProperties();
                    });
                    contents.Add(showHashesToggle);

                    Button btn = new Button(() => CheckForCollisions(true));
                    btn.tooltip =
                        "In rare circumstances - such as a bad merge from source control - ScriptableBTag's may not have unique identifiers. If ever in doubt, click this to check all assets and note the output in the console.";
                    btn.text = "Check for Duplicated Assets";
                    contents.Add(btn);

                    rootElement.Add(contents);
                },

                // Populate the search keywords to enable smart search filtering and label highlighting:
                keywords = new HashSet<string>(new[] {"Auto-Reference", "Finding", "Search", "Refresh", "Tag"})
            };

            return provider;
        }

        /// <summary>
        /// Attempts to add a new #define constant to the Player Settings
        /// </summary>
        /// <param name="newDefineCompileConstant">constant to attempt to define</param>
        /// <param name="targetGroups">platforms to add this for (null will add to all platforms)</param>
        public static void AddCompileDefine(string newDefineCompileConstant, BuildTargetGroup[] targetGroups = null)
        {
            if (targetGroups == null)
                targetGroups = (BuildTargetGroup[]) Enum.GetValues(typeof(BuildTargetGroup));

            foreach (BuildTargetGroup grp in targetGroups)
            {
                if (grp == BuildTargetGroup.Unknown) //the unknown group does not have any constants location
                    continue;

                string defines = PlayerSettings.GetScriptingDefineSymbolsForGroup(grp);
                try
                {
                    if (!defines.Contains(newDefineCompileConstant))
                    {
                        if (defines.Length > 0) //if the list is empty, we don't need to append a semicolon first
                            defines += ";";

                        defines += newDefineCompileConstant;
                        PlayerSettings.SetScriptingDefineSymbolsForGroup(grp, defines);
                    }
                }
                catch
                {
                }
            }
        }

        /// <summary>
        /// Attempts to remove a #define constant from the Player Settings
        /// </summary>
        /// <param name="defineCompileConstant"></param>
        /// <param name="targetGroups"></param>
        public static void RemoveCompileDefine(string defineCompileConstant, BuildTargetGroup[] targetGroups = null)
        {
            if (targetGroups == null)
                targetGroups = (BuildTargetGroup[]) Enum.GetValues(typeof(BuildTargetGroup));

            foreach (BuildTargetGroup grp in targetGroups)
            {
                string defines = PlayerSettings.GetScriptingDefineSymbolsForGroup(grp);
                int index = defines.IndexOf(defineCompileConstant);
                if (index < 0)
                    continue; //this target does not contain the define
                else if (index > 0)
                    index -= 1; //include the semicolon before the define
                //else we will remove the semicolon after the define

                //Remove the word and it's semicolon, or just the word (if listed last in defines)
                int lengthToRemove = Math.Min(defineCompileConstant.Length + 1, defines.Length - index);

                //remove the constant and it's associated semicolon (if necessary)
                defines = defines.Remove(index, lengthToRemove);

                PlayerSettings.SetScriptingDefineSymbolsForGroup(grp, defines);
            }
        }

        public static bool willCheck = false;

        public static void CheckForCollisions(bool userInitiated = false)
        {
            if (willCheck) return;
            willCheck = true;
            EditorApplication.delayCall += () => CheckForDuplicates(userInitiated);
        }

        public static void CheckForDuplicates(bool userInitiated = false)
        {
            ScriptableBTagRegistry.FindAllAssets<BTagGroupBase, ScriptableBTag>();
            HashSet<BHash128> set = new HashSet<BHash128>();
            bool hasDuplicates = false;
            for (int i = 0; i < ScriptableBTagRegistry.AllAssets.Count; ++i)
            {
                var asset = ScriptableBTagRegistry.AllAssets[i].asset;
                if (asset != null && !asset.IsDefault)
                {
                    if (set.Contains(asset.Hash))
                    {
                        var dupHash = asset.Hash;
                        while (set.Contains(asset.Hash)) asset.EditorGenerateNewHash();
                        if (ScriptableBTagRegistry.AllAssets[i].group != null)
                            AssetDatabase.SetMainObject(ScriptableBTagRegistry.AllAssets[i].group, AssetDatabase.GetAssetPath(asset));
                        EditorUtility.SetDirty(asset);
                        hasDuplicates = true;
                        set.Add(asset.Hash);
                    }
                    else
                    {
                        set.Add(asset.Hash);
                    }
                }
            }

            willCheck = false;
            if (userInitiated && !hasDuplicates)
                Debug.Log("All assets inheriting from ScritpableBTag appear to have unique hashes. Checked " + set.Count + " assets and No Duplicates were detected.");
        }

        private static void UpdateHelpTextLabel(ChangeEvent<string> evt = null)
        {
            switch (BTagSetting.Instance.searchMode)
            {
                case SearchRegistryOption.FullRefresh:
                    helpLabel.text = HelpFullRefresh;
                    break;
                case SearchRegistryOption.IterativeRefresh:
                    helpLabel.text = HelpIterativeRefresh;
                    break;
                case SearchRegistryOption.CachedResultsOnly:
                    helpLabel.text = HelpCachedOnly;
                    break;
            }
        }
    }
}