#if UNITY_EDITOR
using System;
using JetBrains.Annotations;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Sisus.ComponentNames.EditorOnly
{
    /// <summary>
    /// Utility class for getting and setting component tooltips.
    /// </summary>
    internal static class ComponentTooltip
    {
        private static readonly Dictionary<Object, string> tooltips = new();

        private static readonly Dictionary<Type, string> summaries = new()
        {
            {typeof(Transform), "Position, rotation and scale of an object."},
            {typeof(MeshFilter), "A class to access the Mesh of the mesh filter."},
            {typeof(Light), "Script interface for light components."},
            {typeof(MeshCollider), "A mesh collider allows you to do collision detection between meshes and primitives."}
        };

        private static readonly HashSet<Component> hasOverride = new();

        internal static bool HasOverride([NotNull] Component component) => hasOverride.Contains(component);

        internal static void Set([NotNull] Component component, [CanBeNull] string tooltip, ModifyOptions modifyOptions = ModifyOptions.Defaults)
        {
            if (string.IsNullOrEmpty(tooltip))
            {
                tooltips.Remove(component);
                hasOverride.Remove(component);

                NameContainer.TryGet(component,
                    nameContainer =>
                    {
                        if (ComponentName.IsNullEmptyOrDefault(component, nameContainer.NameOverride) && modifyOptions.IsRemovingNameContainerAllowed())
                        {
                            nameContainer.Remove(modifyOptions);
                        }
                        else
                        {
                            nameContainer.TooltipOverride = "";
                        }
                    },
                    modifyOptions);

                return;
            }

            hasOverride.Add(component);
            tooltips[component] = tooltip;

            NameContainer.TryGetOrCreate(component,
                nameContainer => { nameContainer.TooltipOverride = tooltip; },
                null,
                tooltip,
                modifyOptions);
        }

        internal static string Get([NotNull] Component component)
        {
            if (tooltips.TryGetValue(component, out string tooltip))
            {
                return tooltip;
            }

            tooltip = GetSummary(component);
            tooltips.Add(component, tooltip);
            return tooltip;
        }

        private static string GetSummary(Component component)
        {
            var type = component.GetType();
            if (summaries.TryGetValue(type, out string summary))
            {
                return summary;
            }

            if (component is not MonoBehaviour monoBehaviour || !MonoScriptSummaryParser.TryParseSummary(monoBehaviour, out summary))
            {
                DLLSummaryParser.TryParseSummary(type, out summary);
            }

            summary ??= "";
            summaries[type] = summary;
            return summary;
        }
    }
}
#endif