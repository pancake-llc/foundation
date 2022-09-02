#define DEBUG_WRAPPED_SCRIPT

using System;
using UnityEditor;
using UnityEngine;

namespace Pancake.Init
{
    [CustomEditor(typeof(Wrapper<>), true, isFallback=true), CanEditMultipleObjects]
    public class WrapperEditor : UnityEditor.Editor
    {
        private MonoScript wrappedScriptReference;
        private Type wrappedType;
        private RuntimeFieldsDrawer runtimeFieldsDrawer;

        private void OnEnable() => Setup();

        private void Setup()
        {
            var targetType = target.GetType();
            Type baseType;
            for(baseType = targetType.BaseType; baseType != null; baseType = baseType.BaseType)
            {
                if(baseType.IsGenericType && baseType.GetGenericTypeDefinition() == typeof(Wrapper<>))
                {
                    break;
                }
            }

            if(baseType == null)
            {
                return;
            }

            wrappedType = baseType.GetGenericArguments()[0];

            var wrappedObject = (target as IWrapper).WrappedObject;

            if(wrappedObject is null)
            {
                return;
            }

            runtimeFieldsDrawer = new RuntimeFieldsDrawer(wrappedObject, typeof(object));

            var wrappedTypeName = wrappedType.Name;

            string requiredSuffix = wrappedTypeName + ".cs";

            foreach(string guid in AssetDatabase.FindAssets(wrappedTypeName))
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(guid);

                if(!assetPath.EndsWith(requiredSuffix, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }
                char charBeforeSuffix = assetPath[assetPath.Length - requiredSuffix.Length - 1];
                if(charBeforeSuffix != '\\' && charBeforeSuffix != '/')
                {
                    continue;
                }

                bool multipleOptions = false;
                var script = AssetDatabase.LoadAssetAtPath<MonoScript>(assetPath);
                if(script != null)
                {
                    if(script.GetType() == wrappedType)
                    {
                        wrappedScriptReference = script;
                        return;
                    }

                    if(wrappedScriptReference != null)
                    {
                        if(wrappedScriptReference.GetType().Assembly == targetType.Assembly)
                        {
                            if(script.GetType().Assembly == targetType.Assembly)
                            {
                                #if DEV_MODE && DEBUG_WRAPPED_SCRIPT
                                Debug.Log("Can't determine wrappedScriptReference because more than one option in the same assembly as target.", script);
                                #endif

                                wrappedScriptReference = null;
                                return;
                            }

                            #if DEV_MODE && DEBUG_WRAPPED_SCRIPT
                            Debug.Log("Found two options for wrappedScriptReference but prioritizing one that is in same assemby as target.", script);
                            #endif

                            continue;
                        }
                        else if(script.GetType().Assembly == targetType.Assembly)
                        {
                            #if DEV_MODE && DEBUG_WRAPPED_SCRIPT
                            Debug.Log("Found two options for wrappedScriptReference but prioritizing one that is in same assemby as target.", script);
                            #endif

                            wrappedScriptReference = script;
                            continue;
                        }

                        multipleOptions = true;
                    }

                    #if DEV_MODE && DEBUG_WRAPPED_SCRIPT
                    Debug.Log("wrappedScriptReference = " + wrappedType.Name, script);
                    #endif

                    wrappedScriptReference = script;
                }

                if(multipleOptions)
                {
                    wrappedScriptReference = null;

                    #if DEV_MODE
                    Debug.Log($"Can't resolve {nameof(MonoScript)} for target {targetType.FullName} because more than one option exist in the same assembly as target.", script);
                    #endif
                }
            }
        }

        public override void OnInspectorGUI()
        {
            if(wrappedScriptReference == null)
            {
                var scriptReferenceProperty = serializedObject.GetIterator();
                scriptReferenceProperty.NextVisible(true);
                GUI.enabled = false;
                EditorGUILayout.PropertyField(scriptReferenceProperty, false);
                GUI.enabled = true;
            }
            else
            {
                GUI.enabled = false;
                EditorGUILayout.ObjectField("Script", wrappedScriptReference, typeof(MonoScript), false);
                GUI.enabled = true;
                GUILayout.Space(20f);
            }

            var wrappedObjectProperty = serializedObject.FindProperty("wrapped");
            if(wrappedObjectProperty != null && wrappedObjectProperty.NextVisible(true))
            {
                serializedObject.Update();

                do
                {
                    EditorGUILayout.PropertyField(wrappedObjectProperty, true);
                }
                while(wrappedObjectProperty.NextVisible(false));

                serializedObject.ApplyModifiedProperties();
            }

            runtimeFieldsDrawer?.Draw();
        }
    }
}