using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Sisus.Init.ValueProviders;
using UnityEditor;
using UnityEngine;

namespace Sisus.Init.EditorOnly
{
    internal static class ValueProviderEditorUtility
    {
        /// <summary>
        /// NOTE: Slow method; should not be called during every OnGUI.
        /// </summary>
        public static bool IsSingleSharedInstance(ScriptableObject valueProvider)
        {
            if(string.IsNullOrEmpty(AssetDatabase.GetAssetPath(valueProvider)))
            {
                return false;
            }

            var type = valueProvider.GetType();

            var guidsOfPotentialInstancesInProject = AssetDatabase.FindAssets("t:" + type.Name);
            if(guidsOfPotentialInstancesInProject.Length == 0)
            {
                return false;
            }

            var attribute = type.GetCustomAttribute<ValueProviderMenuAttribute>();
            var instancesInProject = guidsOfPotentialInstancesInProject
                .Select(guid => AssetDatabase.LoadAssetAtPath<ScriptableObject>(AssetDatabase.GUIDToAssetPath(guid)))
                .Where(obj => obj.GetType() == type && obj.name == attribute.ItemName)
                .ToList();

            if(instancesInProject.Count != 1 || instancesInProject[0] != valueProvider)
            {
                return false;
            }

            using(var serializedObject = new SerializedObject(valueProvider))
            {
                var firstProperty = serializedObject.GetIterator();
                if(firstProperty.NextVisible(true) && !firstProperty.NextVisible(false))
                {
                    return true;
                }
            }

            return false;
        }

        public static IEnumerable<Type> GetAllValueProviderMenuItemTargetTypes()
            => TypeCache.GetTypesWithAttribute<ValueProviderMenuAttribute>()
                .Where(t => !t.IsAbstract && typeof(ScriptableObject).IsAssignableFrom(t) && ValueProviderUtility.IsValueProvider(t));
    }
}