/* Modified exerpts from:
 * https://github.com/yasirkula/UnityAssetUsageDetector/
 * The code from the above repository has the following license and only pertains to partial code in this file.
 * MIT License
    Copyright (c) 2016 Süleyman Yasir KULA

    Permission is hereby granted, free of charge, to any person obtaining a copy
    of this software and associated documentation files (the "Software"), to deal
    in the Software without restriction, including without limitation the rights
    to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
    copies of the Software, and to permit persons to whom the Software is
    furnished to do so, subject to the following conditions:

    The above copyright notice and this permission notice shall be included in all
    copies or substantial portions of the Software.

    THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
    IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
    FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
    AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
    LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
    OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
    SOFTWARE.
 */

#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using System.Reflection;
using UnityEngine.Events;
using System.IO;
using System.Linq;
using UnityEngine.UIElements;
using UnityEngine.UI;
using System.Text.RegularExpressions;
using Pancake.BTag;
using UnityEditor.Events;

namespace Pancake.BTagEditor
{
    public class ScriptableBTagRegistry : MonoBehaviour
    {
        private static BindingFlags fieldModifiers = BindingFlags.Instance | BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.NonPublic;
        private static BindingFlags propertyModifiers = BindingFlags.Instance | BindingFlags.DeclaredOnly;

        private static List<VariableGetterHolder> validVariables = new List<VariableGetterHolder>(32);

        // An optimization to fetch & filter fields and properties of a class only once
        private static Dictionary<Type, VariableGetterHolder[]> typeToVariables = new Dictionary<Type, VariableGetterHolder[]>(4096);
        private static Dictionary<ScriptableBTag, bool> assetReferencesHadFullRefresh = new Dictionary<ScriptableBTag, bool>();
        private static Dictionary<ScriptableBTag, List<SceneObjectIDBundle>> cachedAssetReferences = new Dictionary<ScriptableBTag, List<SceneObjectIDBundle>>();
        static BHash128 WorkingHash = default;
        static DateTime TimeLastSearched = default;

        public static bool CancelSearch = false;
        public static Action<float, float> OnSearchProgress;
        public static Action<float, float> OnOpenSceneSearchProgress;

        public static List<(BTagGroupBase group, ScriptableBTag asset)> AllAssets = new List<(BTagGroupBase, ScriptableBTag)>();

        public static void FindAllAssets<GROUP, SO>() where GROUP : BTagGroupBase where SO : ScriptableBTag
        {
            AllAssets.Clear();

            string[] groupGuids = AssetDatabase.FindAssets("t:" + typeof(GROUP));
            for (int groupIndex = 0; groupIndex < groupGuids.Length; ++groupIndex)
            {
                string groupPath = AssetDatabase.GUIDToAssetPath(groupGuids[groupIndex]);
                GROUP g = AssetDatabase.LoadAssetAtPath<GROUP>(groupPath);
                if (g == null) continue;
                if (!AssetDatabase.IsMainAsset(g))
                {
                    // TODO - Unfortunately Unity also messes up the naming - would be great to recover this somehow but unsure how
                    Debug.LogWarning("Detected Unity error with created SriptableObject - likely created via Ctrl + drag to duplicate. Fixing: " + g);
                    AssetDatabase.SetMainObject(g, groupPath);
                    BTagEditorUtils.ForceRefresh(g);
                }

                UnityEngine.Object[] allAssets = AssetDatabase.LoadAllAssetRepresentationsAtPath(groupPath);

                for (int i = 0; i < allAssets.Length; ++i)
                {
                    if (allAssets[i] == null) continue;
                    if (allAssets[i] is SO)
                    {
                        SO asset = allAssets[i] as SO;
                        AllAssets.Add((g, asset));
                    }
                }
            }

            string[] individualSOGuids = AssetDatabase.FindAssets("t:" + typeof(SO));
            for (int i = 0; i < individualSOGuids.Length; ++i)
            {
                SO individualAsset = AssetDatabase.LoadMainAssetAtPath(AssetDatabase.GUIDToAssetPath(individualSOGuids[i])) as SO;
                if (individualAsset != null)
                {
                    if (!individualAsset.IsDefault)
                    {
                        AllAssets.Add((null, individualAsset));
                    }
                }
            }
        }

        public static void ReplaceSOUsage(ScriptableBTag target, SearchRegistryOption searchOption, ScriptableBTag replacement) =>
            FindSOUsage(target, searchOption, replacement);

        public static void FindSOUsage(ScriptableBTag target, SearchRegistryOption searchOption, ScriptableBTag replacement = null)
        {
            if (target == null) return;
            WorkingHash = target.Hash;

            string[] openScenePaths = new string[EditorSceneManager.sceneCount];
            for (int s = 0; s < openScenePaths.Length; ++s) openScenePaths[s] = EditorSceneManager.GetSceneAt(s).path;

            string selectedObjGUID = "";
            long fileID = 0;
            AssetDatabase.TryGetGUIDAndLocalFileIdentifier(target, out selectedObjGUID, out fileID);

            List<string> guids = new List<string>(AssetDatabase.FindAssets("t:ScriptableObject"));

            if (!assetReferencesHadFullRefresh.ContainsKey(target))
            {
                assetReferencesHadFullRefresh[target] = false;
            }

            if (searchOption == SearchRegistryOption.FullRefresh ||
                (!assetReferencesHadFullRefresh[target] && BTagSetting.Instance.searchMode != SearchRegistryOption.CachedResultsOnly))
            {
                assetReferencesHadFullRefresh[target] = true;
                guids.AddRange(AssetDatabase.FindAssets("t:Scene").Where(x => !openScenePaths.Contains(AssetDatabase.GUIDToAssetPath(x))));
                guids.AddRange(AssetDatabase.FindAssets("t:Prefab"));
            }

            bool hasCachedResults = cachedAssetReferences.ContainsKey(target);
            if (!hasCachedResults)
            {
                cachedAssetReferences[target] = new List<SceneObjectIDBundle>();
            }
            else
            {
                if (searchOption == SearchRegistryOption.FullRefresh) cachedAssetReferences[target].Clear();

                // If the cached results weren't cleared, check the current entries still exist and remove them if they don't
                for (int i = cachedAssetReferences[target].Count - 1; i >= 0; --i)
                {
                    if (!File.Exists(cachedAssetReferences[target][i].scenePath))
                    {
                        cachedAssetReferences[target].RemoveAt(i);
                    }
                    else if (!string.IsNullOrEmpty(cachedAssetReferences[target][i].objectName))
                    {
                        var obj = EditorUtility.InstanceIDToObject(cachedAssetReferences[target][i].id);
                        if (obj == null || !SearchFieldsAndPropertiesOf(obj, true))
                        {
                            cachedAssetReferences[target].RemoveAt(i);
                        }
                    }
                }
            }

            float startTime = Time.realtimeSinceStartup;
            for (int i = 0; i < guids.Count; ++i)
            {
                if (CancelSearch) return;
                if (Time.realtimeSinceStartup > startTime + 0.5f)
                {
                    startTime = Time.realtimeSinceStartup;
                    OnSearchProgress?.Invoke(i + 1, guids.Count);
                }

                string assetPath = AssetDatabase.GUIDToAssetPath(guids[i]);
                if (searchOption != SearchRegistryOption.FullRefresh && hasCachedResults && TimeLastSearched != default &&
                    File.GetLastWriteTime(assetPath) < TimeLastSearched)
                {
                    continue;
                }

                GameObject loadedAssetGO = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
                SceneAsset loadedAssetScene = loadedAssetGO != null ? null : AssetDatabase.LoadAssetAtPath<SceneAsset>(assetPath);
                ScriptableObject loadedAssetSO = loadedAssetGO != null || loadedAssetScene != null ? null : AssetDatabase.LoadAssetAtPath<ScriptableObject>(assetPath);

                if (loadedAssetSO != null)
                {
                    SearchFieldsAndPropertiesOf(loadedAssetSO, true, 0, replacement);
                    var representations = AssetDatabase.LoadAllAssetRepresentationsAtPath(assetPath);
                    for (int r = 0; r < representations.Length; ++r) SearchFieldsAndPropertiesOf(representations[r], true, 0, replacement);
                }
                else if (loadedAssetGO != null)
                {
                    try
                    {
                        GameObject prefabInstance = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
                        Component[] components = prefabInstance.GetComponentsInChildren<Component>(true);
                        for (int c = 0; c < components.Length; c++)
                        {
                            if (components[c] == null || components[c].Equals(null)) continue;

                            var found = SearchFieldsAndPropertiesOf(components[c], false, 0, replacement);
                            if (found && replacement != null)
                            {
                                EditorUtility.SetDirty(prefabInstance);
                            }
                        }
                    }
                    catch
                    {
                    }
                }
                else if (loadedAssetScene != null)
                {
                    string scenePath = AssetDatabase.GetAssetPath(loadedAssetScene);
                    int lineCount = 0;
                    bool didReplaceOccurances = false;
                    var lines = File.ReadLines(scenePath).ToArray();
                    string replacementGuid = default;
                    long replacementID = default;
                    if (replacement != null) AssetDatabase.TryGetGUIDAndLocalFileIdentifier(replacement, out replacementGuid, out replacementID);
                    for (var l = 0; l < lines.Length; ++l)
                    {
                        var line = lines[l];
                        if (line.Contains("guid:"))
                        {
                            if (line.Contains(selectedObjGUID))
                            {
                                if (line.Contains("fileID:") && line.Contains(fileID.ToString()))
                                {
                                    SceneObjectIDBundle sceneGOGUID = new SceneObjectIDBundle(scenePath, string.Empty, lineCount);
                                    Add(target, sceneGOGUID);
                                    if (replacement != null)
                                    {
                                        line = Regex.Replace(line, "((fileID:)\\s?)[-0-9]*", "fileID: " + replacementID);
                                        line = Regex.Replace(line, "((guid:)\\s?)[a-f-0-9]*", "guid: " + replacementGuid);
                                        lines[l] = line;
                                        didReplaceOccurances = true;
                                    }
                                }
                            }
                        }

                        lineCount++;
                    }

                    if (didReplaceOccurances) File.WriteAllLines(scenePath, lines);
                }
            }

            SearchOpenScenes(replacement);
            TimeLastSearched = DateTime.Now;
        }

        private static void SearchOpenScenes(ScriptableBTag replacement = null)
        {
            float startTime = Time.realtimeSinceStartup;
            for (int s = 0; s < SceneManager.sceneCount; ++s)
            {
                if (CancelSearch) return;
                if (Time.realtimeSinceStartup > startTime + 0.5f)
                {
                    startTime = Time.realtimeSinceStartup;
                    OnOpenSceneSearchProgress?.Invoke(s + 1, SceneManager.sceneCount);
                }

                Component[] components = Resources.FindObjectsOfTypeAll<Component>();
                for (int c = 0; c < components.Length; c++)
                {
                    if (components[c] == null || components[c].Equals(null)) continue;

                    SearchFieldsAndPropertiesOf(components[c], false, 0, replacement);
                }
            }
        }

        public static void CleanReferences(ScriptableBTag obj)
        {
            if (cachedAssetReferences.ContainsKey(obj))
            {
                for (int i = 0; i < cachedAssetReferences[obj].Count; i++)
                {
                    if (cachedAssetReferences[obj][i] == null)
                    {
                        cachedAssetReferences[obj].RemoveAt(i);
                        i--;
                    }
                }
            }
        }

        public static void Add(ScriptableBTag obj)
        {
            if (!cachedAssetReferences.ContainsKey(obj))
            {
                cachedAssetReferences.Add(obj, new List<SceneObjectIDBundle>());
            }

            FindSOUsage(obj, SearchRegistryOption.IterativeRefresh);
        }

        public static void Add(ScriptableBTag obj, SceneObjectIDBundle c)
        {
            if (obj == null) return;

            if (!cachedAssetReferences.ContainsKey(obj)) cachedAssetReferences[obj] = new List<SceneObjectIDBundle>();

            if (!cachedAssetReferences[obj].Contains(c))
            {
                cachedAssetReferences[obj].Add(c);
            }
            else
            {
                // TODO This can happen when there are multiple results for one component - might account for this in the future
                //Debug.LogWarning(obj + " already had " + c + " reference in list");
            }
        }

        public static void Remove(ScriptableBTag obj)
        {
            if (cachedAssetReferences.ContainsKey(obj))
            {
                cachedAssetReferences.Remove(obj);
            }
        }

        public static void Remove(ScriptableBTag obj, UnityEngine.Object targ)
        {
            SceneObjectIDBundle sceneGOGUID = new SceneObjectIDBundle(AssetDatabase.GetAssetOrScenePath(targ), targ.ToString(), targ.GetInstanceID());
            Remove(obj, sceneGOGUID);
        }

        public static void Remove(ScriptableBTag obj, SceneObjectIDBundle c)
        {
            if (obj == null) return;

            if (cachedAssetReferences.ContainsKey(obj))
            {
                cachedAssetReferences[obj].Remove(c);
            }
        }

        public static bool HasCheckedAssetsFor(ScriptableBTag obj) { return cachedAssetReferences.ContainsKey(obj); }

        public static bool HasCachedResults(ScriptableBTag obj) { return (cachedAssetReferences.ContainsKey(obj)); }

        public static void References(ScriptableBTag obj, ref List<SceneObjectIDBundle> val, SearchRegistryOption searchOption)
        {
            if (obj == null) return;
            CancelSearch = false;

            if (searchOption != SearchRegistryOption.CachedResultsOnly)
            {
                FindSOUsage(obj, searchOption);
            }

            if (cachedAssetReferences.ContainsKey(obj))
            {
                val = cachedAssetReferences[obj];
                return;
            }
        }

        private static bool SearchFieldsAndPropertiesOf(UnityEngine.Object component, bool recursive = false, int depth = 0, ScriptableBTag replacement = null)
        {
            // Get filtered variables for this object
            if (component == null) return false;

            if (depth > 3) return false;

            VariableGetterHolder[] variables = GetFilteredVariablesForType(component.GetType());

            bool didFind = false;

            for (int i = 0; i < variables.Length; i++)
            {
                try
                {
                    object variableValue = variables[i].Get(component);
                    if (variableValue == null) continue;

                    bool shouldUpdate = false;
                    if (variableValue is Array)
                    {
                        for (int j = 0; j < ((Array) variableValue).Length; ++j)
                        {
                            object arrayItem = ((Array) variableValue).GetValue(j);
                            var wasValid = AddIfTrackedOrReference(arrayItem,
                                component,
                                recursive,
                                depth,
                                replacement);
                            didFind |= wasValid;
                            if (wasValid && replacement != null)
                            {
                                shouldUpdate = true;
                                ((Array) variableValue).SetValue(replacement, j);
                                EditorUtility.SetDirty(component);
                            }
                            //AddIfTrackedOrReference(arrayItem, component);
                        }

                        if (shouldUpdate) variables[i].Set(component, variableValue);
                    }
                    else if (variableValue is IList)
                    {
                        shouldUpdate = false;
                        for (int j = 0; j < ((IList) variableValue).Count; ++j)
                        {
                            object arrayItem = ((IList) variableValue)[j];
                            var wasValid = AddIfTrackedOrReference(arrayItem,
                                component,
                                recursive,
                                depth,
                                replacement);
                            didFind |= wasValid;
                            if (wasValid && replacement != null)
                            {
                                shouldUpdate = true;
                                ((IList) variableValue)[j] = replacement;
                                EditorUtility.SetDirty(component);
                            }
                        }

                        if (shouldUpdate) variables[i].Set(component, variableValue);
                    }
                    else if (variableValue is UnityEventBase)
                    {
                        shouldUpdate = false;
                        var evt = (variableValue as UnityEventBase);
                        for (int j = evt.GetPersistentEventCount() - 1; j >= 0; --j)
                        {
                            var targ = evt.GetPersistentTarget(j);
                            if (!(targ is ScriptableBTag)) continue;
                            var wasValid = AddIfTrackedOrReference(targ,
                                component,
                                recursive,
                                depth,
                                replacement);
                            didFind |= wasValid;
                            if (wasValid && replacement != null)
                            {
                                SerializedObject so = new SerializedObject(component);
                                var sp = so.FindProperty(variables[i].name);
                                sp = sp.FindPropertyRelative("m_PersistentCalls");
                                sp = sp.FindPropertyRelative("m_Calls");
                                sp = sp.GetArrayElementAtIndex(j).FindPropertyRelative("m_Target");
                                sp.objectReferenceValue = replacement;
                                so.ApplyModifiedProperties();
                            }

                            if (shouldUpdate) variables[i].Set(component, variableValue);
                        }
                    }

                    var valid = AddIfTrackedOrReference(variableValue,
                        component,
                        recursive,
                        depth,
                        replacement);
                    didFind |= valid;
                    if (valid && replacement != null)
                    {
                        variables[i].Set(component, replacement);
                        EditorUtility.SetDirty(component);
                    }
                }
                catch (UnassignedReferenceException)
                {
                }
                catch (MissingReferenceException)
                {
                }
            }

            return didFind;
        }

        private static bool AddIfTrackedOrReference(
            object variableValue,
            UnityEngine.Object referenceNode,
            bool recursive = false,
            int depth = 0,
            ScriptableBTag replacement = null)
        {
            int newDepth = depth + 1;
            if (variableValue is ScriptableBTag)
            {
                var taggedSO = (ScriptableBTag) variableValue;
                if (taggedSO.Hash.Equals(WorkingHash))
                {
                    if (variableValue is ScriptableObject && !AssetDatabase.IsMainAsset(variableValue as ScriptableObject) &&
                        AssetDatabase.LoadMainAssetAtPath(AssetDatabase.GetAssetPath(taggedSO)) == referenceNode)
                    {
                        return false;
                    }

                    SceneObjectIDBundle sceneGOGUID = new SceneObjectIDBundle(AssetDatabase.GetAssetOrScenePath(referenceNode),
                        referenceNode.ToString(),
                        referenceNode.GetInstanceID());
                    Add(taggedSO, sceneGOGUID);
                    return true;
                }
            }

            if (!(variableValue is ScriptableBTag) && !(variableValue is Transform) && variableValue != null)
            {
                if (recursive)
                {
                    if (depth > 3)
                    {
                        return false;
                    }

                    if (variableValue != null && variableValue is UnityEngine.Object && (UnityEngine.Object) variableValue != referenceNode)
                    {
                        SearchFieldsAndPropertiesOf((UnityEngine.Object) variableValue, recursive, newDepth, replacement);
                    }
                }
                else
                {
                    VariableGetterHolder[] variables = GetFilteredVariablesForType(variableValue.GetType());
                    for (int i = 0; i < variables.Length; i++)
                    {
                        try
                        {
                            var subVariableValue = variables[i].Get(variableValue);
                            if (subVariableValue != null && (subVariableValue is ScriptableBTag))
                            {
                                if (AddIfTrackedOrReference(subVariableValue,
                                        referenceNode,
                                        recursive,
                                        newDepth,
                                        replacement) && replacement != null)
                                {
                                    variables[i].Set(variableValue, replacement);
                                }
                            }
                        }
                        catch (Exception)
                        {
                        }
                    }
                }
            }

            return false;
        }

        // Get filtered variables for a type
        private static VariableGetterHolder[] GetFilteredVariablesForType(Type type)
        {
            VariableGetterHolder[] result;
            if (typeToVariables.TryGetValue(type, out result)) return result;

            // This is the first time this type of object is seen, filter and cache its variables
            // Variable filtering process:
            // 1- skip Obsolete variables
            // 2- skip primitive types, enums and strings
            // 3- skip common Unity types that can't hold any references (e.g. Vector3, Rect, Color, Quaternion)

            validVariables.Clear();

            // Filter the fields
            if (fieldModifiers != (BindingFlags.Instance | BindingFlags.DeclaredOnly))
            {
                Type currType = type;
                while (currType != typeof(object))
                {
                    FieldInfo[] fields = currType.GetFields(fieldModifiers);
                    for (int i = 0; i < fields.Length; i++)
                    {
                        // Skip obsolete fields
                        if (Attribute.IsDefined(fields[i], typeof(ObsoleteAttribute)))
                            continue;

                        // Skip primitive types
                        Type fieldType = fields[i].FieldType;
                        if (fieldType.IsPrimitive || fieldType == typeof(string) || fieldType.IsEnum)
                            continue;

                        if (typeof(Transform).IsAssignableFrom(currType))
                            continue;
                        else if (typeof(VisualTreeAsset).IsAssignableFrom(currType))
                            continue;
                        else if (typeof(StyleSheet).IsAssignableFrom(currType))
                            continue;
                        else if (typeof(GUISkin).IsAssignableFrom(currType))
                            continue;
                        else if (typeof(Font).IsAssignableFrom(currType))
                            continue;
                        else if (typeof(Graphic).IsAssignableFrom(currType))
                            continue;
                        else if (typeof(RawImage).IsAssignableFrom(currType))
                            continue;
                        else if (typeof(MaskableGraphic).IsAssignableFrom(currType))
                            continue;

                        VariableGetVal getter = fields[i].GetValue;
                        VariableSetVal setter = fields[i].SetValue;
                        if (getter != null)
                        {
                            validVariables.Add(new VariableGetterHolder(fields[i], getter, setter));
                        }
                    }

                    currType = currType.BaseType;
                }
            }

            if (propertyModifiers != (BindingFlags.Instance | BindingFlags.DeclaredOnly))
            {
                Type currType = type;
                while (currType != typeof(object))
                {
                    PropertyInfo[] properties = currType.GetProperties(propertyModifiers);
                    for (int i = 0; i < properties.Length; i++)
                    {
                        // Skip obsolete properties
                        if (Attribute.IsDefined(properties[i], typeof(ObsoleteAttribute)))
                            continue;

                        // Skip primitive types
                        Type propertyType = properties[i].PropertyType;
                        if (propertyType.IsPrimitive || propertyType == typeof(string) || propertyType.IsEnum)
                            continue;

                        // Additional filtering for properties:
                        // 1- Ignore "gameObject", "transform", "rectTransform" and "attachedRigidbody" properties of Component's to get more useful results
                        // 2- Ignore "canvasRenderer" and "canvas" properties of Graphic components
                        // 3 & 4- Prevent accessing properties of Unity that instantiate an existing resource (causing leak)
                        string propertyName = properties[i].Name;
                        if (typeof(Component).IsAssignableFrom(currType) && (propertyName.Equals("gameObject") || propertyName.Equals("transform") ||
                                                                             propertyName.Equals("attachedRigidbody") || propertyName.Equals("rectTransform")))
                            continue;
                        else if (typeof(UnityEngine.UI.Graphic).IsAssignableFrom(currType) && (propertyName.Equals("canvasRenderer") || propertyName.Equals("canvas")))
                            continue;
                        else if (typeof(MeshFilter).IsAssignableFrom(currType) && propertyName.Equals("mesh"))
                            continue;
                        else if (typeof(VisualTreeAsset).IsAssignableFrom(currType))
                            continue;
                        else if (typeof(StyleSheet).IsAssignableFrom(currType))
                            continue;
                        else if (typeof(Texture2D).IsAssignableFrom(currType))
                            continue;
                        else if (typeof(Renderer).IsAssignableFrom(currType) && (propertyName.Equals("sharedMaterial") || propertyName.Equals("sharedMaterials")))
                            continue;
                        else if ((propertyName.Equals("material") || propertyName.Equals("materials")) && (typeof(Renderer).IsAssignableFrom(currType) ||
                                                                                                           typeof(Collider).IsAssignableFrom(currType) ||
                                                                                                           typeof(Collider2D).IsAssignableFrom(currType)))
                            continue;
                        else
                        {
                            VariableGetVal getter = properties[i].GetValue;
                            VariableSetVal setter = properties[i].SetValue;
                            if (getter != null) validVariables.Add(new VariableGetterHolder(properties[i], getter, setter));
                        }
                    }

                    currType = currType.BaseType;
                }
            }

            result = validVariables.ToArray();

            // Cache the filtered fields
            typeToVariables.Add(type, result);
            return result;
        }
    }

    // Delegate to get the value of a variable (either field or property)
    public delegate object VariableGetVal(object obj);

    public delegate void VariableSetVal(object obj, object obj2);

    // Custom struct to hold a variable, its important properties and its getter function
    public struct VariableGetterHolder
    {
        public readonly string name;
        public readonly bool isProperty;
        private readonly VariableGetVal getter;
        private readonly VariableSetVal setter;

        public VariableGetterHolder(FieldInfo fieldInfo, VariableGetVal getter, VariableSetVal setter)
        {
            name = fieldInfo.Name;
            isProperty = false;
            this.getter = getter;
            this.setter = setter;
        }

        public VariableGetterHolder(PropertyInfo propertyInfo, VariableGetVal getter, VariableSetVal setter)
        {
            name = propertyInfo.Name;
            isProperty = true;
            this.getter = getter;
            this.setter = setter;
        }

        public object Get(object obj) => getter(obj);
        public void Set(object obj, object obj2) => setter(obj, obj2);
    }

    public class SceneObjectIDBundle : IEquatable<SceneObjectIDBundle>
    {
        public string scenePath;
        public string objectName;
        public int id;

        public SceneObjectIDBundle(string scene, string name, int id)
        {
            scenePath = scene;
            objectName = name;
            this.id = id;
        }

        public override int GetHashCode()
        {
            int result = 1;
            result = result * 31 * scenePath.GetHashCode();
            result = result * 31 * objectName.GetHashCode();
            result += id;
            return result;
        }

        public static bool operator ==(SceneObjectIDBundle lhs, SceneObjectIDBundle rhs) { return lhs.Equals(rhs); }

        public static bool operator !=(SceneObjectIDBundle lhs, SceneObjectIDBundle rhs) { return !(lhs.Equals(rhs)); }
        public override bool Equals(object obj) { return this.Equals((SceneObjectIDBundle) obj); }
        public bool Equals(SceneObjectIDBundle obj) { return this.scenePath == obj.scenePath && this.objectName == obj.objectName && this.id == obj.id; }
    }

    // Credit: http://stackoverflow.com/questions/724143/how-do-i-create-a-delegate-for-a-net-property
    public interface IPropertyAccessor
    {
        object GetValue(object source);
    }

    // A wrapper class for properties to get their values more efficiently
    public class PropertyWrapper<TObject, TValue> : IPropertyAccessor where TObject : class
    {
        private readonly Func<TObject, TValue> getter;

        public PropertyWrapper(MethodInfo getterMethod) { getter = (Func<TObject, TValue>) Delegate.CreateDelegate(typeof(Func<TObject, TValue>), getterMethod); }

        public object GetValue(object obj)
        {
            try
            {
                return getter((TObject) obj);
            }
            catch
            {
                // Property getters may return various kinds of exceptions
                // if their backing fields are not initialized (yet)
                return null;
            }
        }
    }
}
#endif