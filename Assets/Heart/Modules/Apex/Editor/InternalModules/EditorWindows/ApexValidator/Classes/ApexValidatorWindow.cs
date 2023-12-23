using Pancake.ExLib.Reflection;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using System.IO;

namespace Pancake.ApexEditor
{
    sealed class ApexValidatorWindow : EditorWindow
    {
        private const string AUTO_CHECK_KEY = "Apex Validator Key";

        private struct Entry : IComparable<Entry>
        {
            public readonly GUIContent content;
            public readonly Type target;
            public readonly MonoScript editorScript;
            public readonly bool subclasses;

            public Entry(Type target, MonoScript editorScript, bool subclasses)
            {
                this.target = target;
                this.editorScript = editorScript;
                this.subclasses = subclasses;

                content = new GUIContent(
                    $"File: <b>{editorScript.name}.cs</b>  <i>(Override: {target.Name}{(subclasses ? " (Including subclasses)" : string.Empty)})</i>");
                if (target == typeof(UnityEngine.Object) && subclasses)
                {
                    content.image = EditorGUIUtility.IconContent("console.erroricon.sml").image;
                }
            }

            public int CompareTo(Entry other)
            {
                if (target == typeof(UnityEngine.Object) && subclasses)
                {
                    return -1;
                }

                return 1;
            }
        }

        private GUIStyle messageStyle;
        private GUIStyle itemEvenStyle;
        private GUIStyle itemOddStyle;
        private Vector2 scrollPos;
        private Type projectBrowserType;

        private void OnEnable()
        {
            if (projectBrowserType == null)
            {
                Assembly assembly = typeof(Editor).Assembly;
                projectBrowserType = assembly.GetType("UnityEditor.ProjectBrowser");
            }
        }

        private void OnGUI()
        {
            if (messageStyle == null)
            {
                messageStyle = new GUIStyle(EditorStyles.centeredGreyMiniLabel);
                messageStyle.richText = true;
                messageStyle.wordWrap = true;

                itemEvenStyle = new GUIStyle(ApexStyles.BoxEntryEven);
                itemEvenStyle.richText = true;
                itemEvenStyle.wordWrap = true;

                itemOddStyle = new GUIStyle(ApexStyles.BoxEntryOdd);
                itemOddStyle.richText = true;
                itemOddStyle.wordWrap = true;
            }

            Rect pos = new Rect(10, 10, 480, 50);
            GUI.Label(pos,
                "<b>Apex Validator</b> found that a custom editor was written for the following components. If you want to use the Apex editor for these components, click the <i>Fix</i> button.\n<i>This does not apply to other components, Apex will work without changes for them.</i>\nOtherwise, ignore this message and close the window.",
                messageStyle);

            pos.x = (position.width / 2) - 240;
            pos.y = pos.yMax + 10;
            pos.height = 121;
            GUI.Box(pos, GUIContent.none, ApexStyles.BoxEntryBkg);

            float height = 0;
            for (int i = 0; i < Entries.Count; i++)
            {
                Entry entry = Entries[i];
                height += Mathf.Max(25, itemEvenStyle.CalcHeight(entry.content, 480));
            }

            height -= Entries.Count - 1;

            Rect viewPos = new Rect(0, 0, pos.width, height);
            if (pos.height < height)
            {
                viewPos.width -= 43;
            }

            scrollPos = GUI.BeginScrollView(pos, scrollPos, viewPos);
            {
                viewPos.height = 25;

                float x = viewPos.x;
                float width = viewPos.width;
                if (pos.height < height)
                {
                    width += 30;
                }

                for (int i = 0; i < Entries.Count; i++)
                {
                    Entry entry = Entries[i];

                    viewPos.width = width - 30;
                    if (GUI.Button(viewPos, entry.content, (i % 2) == 0 ? itemEvenStyle : itemOddStyle))
                    {
                        if (projectBrowserType != null)
                        {
                            FocusWindowIfItsOpen(projectBrowserType);
                        }

                        EditorGUIUtility.PingObject(entry.editorScript);
                    }

                    viewPos.x = viewPos.xMax;
                    viewPos.width = 30;
                    if (GUI.Button(viewPos, "Fix", ApexStyles.BoxCenteredButton))
                    {
                        FixEntry(entry);
                        AssetDatabase.SaveAssets();
                        AssetDatabase.Refresh();
                        EditorApplication.delayCall += () => { AEditor.RepaintAll(); };
                    }

                    viewPos.x = x;
                    viewPos.y = viewPos.yMax - 1;
                }
            }
            GUI.EndScrollView();

            pos.width = 480;
            pos.x = pos.xMax - 70;
            pos.y = pos.yMax + 10;
            pos.width = 70;
            pos.height = 25;
            if (GUI.Button(pos, "Fix All", ApexStyles.BoxCenteredButton))
            {
                for (int i = 0; i < Entries.Count; i++)
                {
                    FixEntry(Entries[i]);
                }

                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                AEditor.RepaintAll();
            }

            pos.x = 10;
            pos.width = 200;
            pos.height = EditorGUIUtility.singleLineHeight;
            bool autoCheck = EditorPrefs.GetBool(AUTO_CHECK_KEY);
            EditorGUI.BeginChangeCheck();
            autoCheck = EditorGUI.ToggleLeft(pos, "Auto Check", autoCheck);
            if (EditorGUI.EndChangeCheck())
            {
                EditorPrefs.SetBool(AUTO_CHECK_KEY, autoCheck);
            }
        }

        #region [Static Methods]

        private static List<Entry> Entries = new List<Entry>();

        [MenuItem("Tools/Pancake/Apex Validator", false, 15)]
        private static void Validate()
        {
            Entries.Clear();
            TypeCache.TypeCollection types = TypeCache.GetTypesWithAttribute<CustomEditor>();
            for (int i = 0; i < types.Count; i++)
            {
                Type type = types[i];

                if (!type.IsSubclassOf(typeof(Editor)) || type == typeof(AEditor))
                {
                    continue;
                }
                
                CustomEditor customEditor;
                try
                {
                    customEditor = type.GetCustomAttribute<CustomEditor>();
                }
                catch (AmbiguousMatchException)
                {
                    continue;
                }

                Type inspectedType = null;
                bool editorForChildClasses = false;
                foreach (FieldInfo fieldInfo in customEditor.GetType().AllFields())
                {
                    if (fieldInfo.Name == "m_InspectedType")
                    {
                        inspectedType = fieldInfo.GetValue(customEditor) as Type;
                    }
                    else if (fieldInfo.Name == "m_EditorForChildClasses" && inspectedType != null)
                    {
                        editorForChildClasses = (bool) fieldInfo.GetValue(customEditor);
                        break;
                    }
                }

                if (inspectedType != null && (inspectedType == typeof(UnityEngine.Object) || string.IsNullOrEmpty(inspectedType.Namespace) ||
                                              (!string.IsNullOrEmpty(inspectedType.Namespace) &&
                                               !inspectedType.Namespace.StartsWith("Pancake", StringComparison.Ordinal) &&
                                               !inspectedType.Namespace.StartsWith("TMPro", StringComparison.Ordinal) &&
                                               !inspectedType.Namespace.StartsWith("Coffee", StringComparison.Ordinal) &&
                                               !inspectedType.Namespace.StartsWith("UnityEditor", StringComparison.Ordinal) &&
                                               !inspectedType.Namespace.StartsWith("UnityEngine", StringComparison.Ordinal))))
                {
                    ScriptableObject sc = CreateInstance(type);
                    MonoScript monoScript = MonoScript.FromScriptableObject(sc);
                    if (monoScript != null)
                    {
                        Entries.Add(new Entry(inspectedType, monoScript, editorForChildClasses));
                    }

                    DestroyImmediate(sc);
                }
            }

            if (Entries.Count > 0)
            {
                Entries.Sort();
                if (HasOpenInstances<ApexValidatorWindow>())
                {
                    FocusWindowIfItsOpen<ApexValidatorWindow>();
                }
                else
                {
                    ApexValidatorWindow window = GetWindow<ApexValidatorWindow>(true);
                    window.titleContent = new GUIContent("Apex Validator");
                    window.minSize = new Vector2(500, 236);
                    window.maxSize = window.minSize;
                    window.Show();
                }
            }
            else
            {
                Debug.Log("<b>Apex Validator</b> successfully performed the validation, no warning were detected.");
                if (HasOpenInstances<ApexValidatorWindow>())
                {
                    GetWindow<ApexValidatorWindow>().Close();
                }
            }
        }

        // [UnityEditor.Callbacks.DidReloadScripts]
        // private static void OnReloadScripts()
        // {
        //     const string GUID = "Apex On Startup Validation";
        //     if (!SessionState.GetBool(GUID, false))
        //     {
        //         ApexSettings settings = ApexSettings.instance;
        //         if (settings.ValidateOnStartup())
        //         {
        //             if (!HasOpenInstances<ApexValidatorWindow>())
        //             {
        //                 Validate();
        //             }
        //         }
        //
        //         SessionState.SetBool(GUID, true);
        //     }
        // }

        private static void FixEntry(Entry entry)
        {
            string code = string.Empty;
            string path = AssetDatabase.GetAssetPath(entry.editorScript);
            using (StreamReader reader = new StreamReader(path))
            {
                while (!reader.EndOfStream)
                {
                    string line = reader.ReadLine();
                    if (line.Contains("[CustomEditor(typeof("))
                    {
                        line = $"#region [Apex Auto Fix]\n// {line}\n #endregion";
                    }

                    code += line + "\n";
                }

                code = code.Remove(code.Length - 1, 1);
            }

            using (StreamWriter writer = new StreamWriter(path))
            {
                writer.Write(code);
            }
        }

        #endregion
    }
}