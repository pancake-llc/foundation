#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Sisus.ComponentNames.EditorOnly
{
    /// <summary>
    /// Utility class for getting and setting component names.
    /// </summary>
    internal static class ComponentName
    {
        internal static event Action<Component, string> InspectorTitleChanged;

        private static readonly Dictionary<Type, string> defaults = new();
        private static readonly Dictionary<Object, string> overrides = new();
        private static readonly Dictionary<Type, string> defaultInspectorTitles = new();
        private static readonly Dictionary<Object, string> inspectorTitleOverrides = new();

        /// <summary>
        /// Should class name be added as suffix in the inspector by default?
        /// </summary>
        internal static bool AddClassNameAsInspectorSuffixByDefault
        {
            get => EditorPrefs.GetBool("ComponentNames.AddClassNameInspectorSuffix", true);
            set => EditorPrefs.SetBool("ComponentNames.AddClassNameInspectorSuffix", value);
        }

        /// <summary>
        /// The format for inspector suffixes.
        /// </summary>
        internal static string InspectorSuffixFormat
        {
            get => EditorPrefs.GetString("ComponentNames.InspectorSuffixFormat", " <color=grey>({0})</color>");
            set => EditorPrefs.SetString("ComponentNames.InspectorSuffixFormat", value);
        }

        /// <summary>
        /// Remove the default "(Script)" suffix from MonoBehaviour inspector titles?
        /// </summary>
        internal static bool RemoveDefaultScriptSuffix
        {
            get => EditorPrefs.GetBool("ComponentNames.RemoveDefaultScriptSuffix", true);
            set => EditorPrefs.GetBool("ComponentNames.RemoveDefaultScriptSuffix", value);
        }

        [return: NotNull]
        internal static string GetDefault([DisallowNull] Component component)
        {
            var componentType = component.GetType();
            if (!defaults.TryGetValue(componentType, out var defaultName))
            {
                defaultName = GetDefaultInspectorTitle(component, removeScriptSuffix: true);
                defaults.Add(componentType, defaultName);
            }

            return defaultName;
        }

        internal static bool HasOverride([DisallowNull] Component component) => overrides.ContainsKey(component);

        private static bool TryGetOverride([DisallowNull] Component component, out string nameOverride) => overrides.TryGetValue(component, out nameOverride);

        [return: NotNull]
        internal static string Get([DisallowNull] Component component) => TryGetOverride(component, out string nameOverride) ? nameOverride : GetDefault(component);

        internal static void Set([DisallowNull] Component component, [AllowNull] string name, ModifyOptions modifyOptions = ModifyOptions.Defaults)
        {
#if DEV_MODE
			Debug.Assert(component, name);
#endif

            if (IsNullEmptyOrDefault(component, name))
            {
                ResetToDefault(component, modifyOptions);
                return;
            }

            if (name.EndsWith(')'))
            {
                int i = name.IndexOf('(');
                if (i != -1)
                {
                    string suffix = name.Substring(i + 1, name.Length - i - 2);
                    name = name.Substring(0, i).Trim();
                    if (name.Length == 0)
                    {
                        name = GetDefault(component);
                    }

                    Set(component, name, suffix);
                    return;
                }
            }

            Set(component, name, AddClassNameAsInspectorSuffixByDefault);
        }

        internal static void Set(
            [DisallowNull] Component component,
            [AllowNull] string name,
            bool addTypeNameSuffix,
            ModifyOptions modifyOptions = ModifyOptions.Defaults)
        {
#if DEV_MODE
			Debug.Assert(component, name);
#endif

            if (IsNullEmptyOrDefault(component, name))
            {
                ResetToDefault(component, modifyOptions);
                return;
            }

            if (!addTypeNameSuffix)
            {
                Set(component, name, "");
                return;
            }

            string suffix = GetDefault(component);

            // Avoid situations like "Cube" => "Cube (Cube (Mesh Filter))"
            if (suffix.EndsWith(')'))
            {
                int i = suffix.IndexOf('(') + 1;
                if (i > 0)
                {
                    suffix = suffix.Substring(i, suffix.Length - i - 1);
                }
            }

            if (string.Equals(name, suffix))
            {
                Set(component, name, null);
                return;
            }

            Set(component, name, suffix);
        }

        /// <param name="component"> The component whose name and suffix are being joined. </param>
        /// <param name="name"> (Optional) The name for the component. </param>
        /// <param name="suffix">
        /// (Optional) The suffix to show after the component name inside parentheses.
        /// <para>
        /// If null, then default suffix, aka component type name, will be shown in the inspector.
        /// </para>
        /// <para>
        /// If empty, then no suffix will be shown in the inspector.
        /// </para>
        /// </param>
        /// <param name="inspectorTitle">
        /// When this method returns, contains the joined string, in the format that it should be shown in the Inspector,
        /// with rich text tags.
        /// </param>
        /// <returns>
        /// If both name and suffix are not null, empty or the default name, then returns name with suffix appended at the end inside parentheses, like so: "{name} ({suffix})".
        /// If the suffix is null or empty or the default name, then returns just the name.
        /// If both name and suffix are null, empty or the default name, then returns null.
        /// </returns>
        private static (string nameInContainer, string inspectorTile) Join([DisallowNull] Component component, [AllowNull] string name, [AllowNull] string suffix)
        {
#if DEV_MODE
			Debug.Assert(component, name);
#endif

            var type = component.GetType();

            string defaultName = GetDefault(component);

#if DEV_MODE
			Debug.Assert(defaultInspectorTitles.ContainsKey(type));
#endif

            bool useDefaultName = string.IsNullOrEmpty(name) || string.Equals(name, defaultName);
            // Null suffix value means "use default suffix".
            bool useDefaultSuffix = suffix is null || string.Equals(suffix, defaultName);

            if (useDefaultName)
            {
                if (useDefaultSuffix)
                {
                    return (null, null);
                }

                name = defaultName;
            }
            else if (useDefaultSuffix)
            {
                return (name, name + string.Format(InspectorSuffixFormat, defaultName));
            }

            // Empty suffix means "no suffix in the inspector".
            if (suffix.Length == 0)
            {
                return (name + " ()", name);
            }

            return (name + " (" + suffix + ")", name + string.Format(InspectorSuffixFormat, suffix));
        }

        internal static void Set(
            [DisallowNull] Component component,
            [AllowNull] string name,
            [AllowNull] string suffix,
            ModifyOptions modifyOptions = ModifyOptions.Defaults)
        {
#if DEV_MODE
			Debug.Assert(component, name);
#endif

            if (IsNullEmptyOrDefault(component, name, suffix))
            {
                ResetToDefault(component, modifyOptions);
                return;
            }

            (string setNameOverride, string inspectorTitle) = Join(component, name, suffix);
            if (setNameOverride is null)
            {
                overrides.Remove(component);
            }
            else
            {
                overrides[component] = setNameOverride;
            }

            if (inspectorTitle is null)
            {
                inspectorTitleOverrides.Remove(component);
            }
            else
            {
                inspectorTitleOverrides[component] = inspectorTitle;
            }

            NameContainer.TryGetOrCreate(component,
                nameContainer => { nameContainer.NameOverride = setNameOverride; },
                setNameOverride,
                null,
                modifyOptions);

            InspectorTitleChanged?.Invoke(component, inspectorTitle);
        }

        internal static string GetInspectorTitle([DisallowNull] Component component)
        {
            if (inspectorTitleOverrides.TryGetValue(component, out string inspectorTitle))
            {
                return inspectorTitle;
            }

            return GetDefaultInspectorTitle(component, RemoveDefaultScriptSuffix);
        }

        internal static string GetInspectorTitleAsPlainText([DisallowNull] Component component) { return WithoutRichTextTags(GetInspectorTitle(component)); }

        [return: NotNull]
        private static string GetDefaultInspectorTitle([DisallowNull] Component component, bool removeScriptSuffix)
        {
            var componentType = component.GetType();
            if (defaultInspectorTitles.TryGetValue(componentType, out string inspectorTitle))
            {
                return inspectorTitle;
            }

            if (TryGetCustomNameFromAddComponentMenuAttribute(componentType, out inspectorTitle))
            {
                defaultInspectorTitles.Add(componentType, inspectorTitle);
                return inspectorTitle;
            }

            inspectorTitle = ObjectNames.GetInspectorTitle(component);
            defaultInspectorTitles.Add(componentType, inspectorTitle);

            if (removeScriptSuffix && inspectorTitle.EndsWith(" (Script)"))
            {
                return inspectorTitle.Substring(0, inspectorTitle.Length - " (Script)".Length);
            }

            return inspectorTitle;
        }

        private static bool TryGetCustomNameFromAddComponentMenuAttribute(Type componentType, [MaybeNullWhen(false), NotNullWhen(true)] out string inspectorTitle)
        {
            foreach (AddComponentMenu addComponentMenu in componentType.GetCustomAttributes(typeof(AddComponentMenu), false))
            {
                string menuName = addComponentMenu.componentMenu;
                if (!string.IsNullOrEmpty(menuName))
                {
                    int lastCategoryEnd = menuName.LastIndexOf('/');
                    inspectorTitle = lastCategoryEnd != -1 ? menuName.Substring(lastCategoryEnd + 1) : menuName;
                    return true;
                }
            }

            inspectorTitle = "";
            return false;
        }

        /// <summary>
        /// Resets the name of the component to its default value.
        /// </summary>
        /// <param name="component"> The component whose name to reset. </param>
        /// <param name="modifyOptions">
        /// Use <see cref="ModifyOptions.Delayed"/> when calling during OnValidate or deserialization to avoid exceptions.
        /// <para>
        /// Use <see cref="ModifyOptions.Undoable"/> to make the action undoable.
        /// </para>
        /// <para>
        /// Both <see cref="ModifyOptions.Delayed"/> and <see cref="ModifyOptions.Undoable"/> flags are enabled by default.
        /// </para>
        /// </param>
        internal static void ResetToDefault([DisallowNull] Component component, ModifyOptions modifyOptions = ModifyOptions.Defaults)
        {
            inspectorTitleOverrides.Remove(component);
            bool changed = overrides.Remove(component);

            if (NameContainer.TryGet(component, out var nameContainer))
            {
                if (string.IsNullOrEmpty(nameContainer.TooltipOverride) && modifyOptions.IsRemovingNameContainerAllowed())
                {
                    nameContainer.Remove(modifyOptions);
                    changed = true;
                }
                else if (!string.IsNullOrEmpty(nameContainer.NameOverride))
                {
                    nameContainer.NameOverride = "";
                    changed = true;
                }
            }

            if (changed)
            {
                InspectorTitleChanged?.Invoke(component, "");
            }
        }

        internal static bool IsNullEmptyOrDefault(Component component, string name)
        {
            string defaultName = GetDefault(component);
            return string.IsNullOrEmpty(name) || string.Equals(name, defaultName);
        }

        private static bool IsNullEmptyOrDefault(Component component, string name, string suffix)
        {
            string defaultName = GetDefault(component);
            return (string.IsNullOrEmpty(name) || string.Equals(name, defaultName)) && (suffix is null || string.Equals(suffix, defaultName));
        }

        private static string WithoutRichTextTags(string text)
        {
            const int maxIterations = 10;
            for (int iteration = 1; iteration < maxIterations; iteration++)
            {
                int index = text.IndexOf("<color=", StringComparison.OrdinalIgnoreCase);
                if (index == -1)
                {
                    return text;
                }

                int endIndex = text.IndexOf('>', index + "<color=".Length);
                if (endIndex > 0)
                {
                    text = text.Remove(index, endIndex + 1 - index);
                }

                index = text.IndexOf("</color>", StringComparison.OrdinalIgnoreCase);
                if (index == -1)
                {
                    return text;
                }

                text = text.Remove(index, "</color>".Length);
            }

            return text;
        }
    }
}
#endif