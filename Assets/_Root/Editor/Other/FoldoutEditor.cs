using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using Object = System.Object;

namespace Pancake.Editor
{
    [CustomEditor(typeof(object), true, isFallback = true)]
    [CanEditMultipleObjects]
    public class FoldoutEditor : UnityEditor.Editor
    {
        //===============================//
        // Members
        //===============================//

        private Dictionary<string, CacheFoldProp> cacheFolds = new Dictionary<string, CacheFoldProp>();
        private List<SerializedProperty> props = new List<SerializedProperty>();
        private List<MethodInfo> methods = new List<MethodInfo>();
        private bool initialized;

        //===============================//
        // Logic
        //===============================//

        private void OnEnable() { initialized = false; }

        private void OnDisable()
        {
            //if (Application.wantsToQuit)
            //if (applicationIsQuitting) return;
            //	if (Toolbox.isQuittingOrChangingScene()) return;
            if (target != null)
                foreach (var c in cacheFolds)
                {
                    EditorPrefs.SetBool(string.Format($"{c.Value.atr.Name}{c.Value.props[0].name}{target.GetInstanceID()}"), c.Value.expanded);
                    c.Value.Dispose();
                }
        }

        public override bool RequiresConstantRepaint() { return EditorFramework.needToRepaint; }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            Setup();

            if (props.Count == 0)
            {
                DrawDefaultInspector();
                return;
            }

            Body();

            //Header();

            serializedObject.ApplyModifiedProperties();

            //void Header()
            //{
            //    using (new EditorGUI.DisabledScope("m_Script" == props[0].propertyPath))
            //    {
            //        EditorGUILayout.Space();
            //        EditorGUILayout.PropertyField(props[0], true);
            //        EditorGUILayout.Space();
            //    }
            //}

            void Body()
            {
                //EditorGUILayout.Space();

                for (var i = 1; i < props.Count; i++)
                {
                    // if (props[i].isArray)
                    // {
                    // 	DrawPropertySortableArray(props[i]);
                    // }
                    // else
                    // {
                    EditorGUILayout.PropertyField(props[i], true);
                    //}
                }

                //EditorGUILayout.Space();

                if (methods == null) return;
                foreach (MethodInfo memberInfo in methods)
                {
                    this.UseButton(memberInfo);
                }

                foreach (var pair in cacheFolds)
                {
                    this.UseVerticalLayout(() => Foldout(pair.Value), StyleFramework.box, pair.Value.atr.Styled);
                    EditorGUI.indentLevel = 0;
                }
            }

            void Foldout(CacheFoldProp cache)
            {
                cache.expanded = EditorGUILayout.Foldout(cache.expanded, cache.atr.Name, true, StyleFramework.foldout);

                if (cache.expanded)
                {
                    EditorGUI.indentLevel = cache.atr.Styled ? 1 : 0;

                    for (int i = 0; i < cache.props.Count; i++)
                    {
                        int i1 = i;
                        this.UseVerticalLayout(() => Child(i1), StyleFramework.boxChild, cache.atr.Styled);
                    }
                }

                void Child(int i)
                {
                    // if (cache.props[i].isArray)
                    // {
                    // 	DrawPropertySortableArray(cache.props[i]);
                    // }
                    // else
                    // {
                    EditorGUI.BeginDisabledGroup(cache.atr.ReadOnly);
                    EditorGUILayout.PropertyField(cache.props[i], new GUIContent(ObjectNames.NicifyVariableName(cache.props[i].name)), true);
                    EditorGUI.EndDisabledGroup();
                    //}
                }
            }

            void Setup()
            {
                EditorFramework.currentEvent = Event.current;
                if (!initialized)
                {
                    //	SetupButtons();

                    List<FieldInfo> objectFields;
                    FoldoutAttribute prevFold = default;

                    var length = EditorTypes.Get(target, out objectFields);

                    for (var i = 0; i < length; i++)
                    {
                        #region FOLDERS

                        var fold = Attribute.GetCustomAttribute(objectFields[i], typeof(FoldoutAttribute)) as FoldoutAttribute;
                        CacheFoldProp c;
                        if (fold == null)
                        {
                            if (prevFold != null && prevFold.FoldEverything)
                            {
                                if (!cacheFolds.TryGetValue(prevFold.Name, out c))
                                {
                                    cacheFolds.Add(prevFold.Name, new CacheFoldProp {atr = prevFold, types = new HashSet<string> {objectFields[i].Name}});
                                }
                                else
                                {
                                    c.types.Add(objectFields[i].Name);
                                }
                            }

                            continue;
                        }

                        prevFold = fold;

                        if (!cacheFolds.TryGetValue(fold.Name, out c))
                        {
                            var expanded = EditorPrefs.GetBool(string.Format($"{fold.Name}{objectFields[i].Name}{target.GetInstanceID()}"), false);
                            cacheFolds.Add(fold.Name, new CacheFoldProp {atr = fold, types = new HashSet<string> {objectFields[i].Name}, expanded = expanded});
                        }
                        else c.types.Add(objectFields[i].Name);

                        #endregion
                    }

                    var property = serializedObject.GetIterator();
                    var next = property.NextVisible(true);
                    if (next)
                    {
                        do
                        {
                            HandleFoldProp(property);
                        } while (property.NextVisible(false));
                    }

                    initialized = true;
                }
            }

            // void SetupButtons()
            // {
            // 	var members = GetButtonMembers(target);
            //
            // 	foreach (var memberInfo in members)
            // 	{
            // 		var method = memberInfo as MethodInfo;
            // 		if (method == null)
            // 		{
            // 			continue;
            // 		}
            //
            // 		if (method.GetParameters().Length > 0)
            // 		{
            // 			continue;
            // 		}
            //
            // 		if (methods == null) methods = new List<MethodInfo>();
            // 		methods.Add(method);
            // 	}
            // }
        }

        public void HandleFoldProp(SerializedProperty prop)
        {
            bool shouldBeFolded = false;

            foreach (var pair in cacheFolds)
            {
                if (pair.Value.types.Contains(prop.name))
                {
                    var pr = prop.Copy();
                    shouldBeFolded = true;
                    pair.Value.props.Add(pr);

                    break;
                }
            }

            if (shouldBeFolded == false)
            {
                var pr = prop.Copy();
                props.Add(pr);
            }
        }

        // IEnumerable<MemberInfo> GetButtonMembers(object target)
        // {
        // 	return target.GetType()
        // 			.GetMembers(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.NonPublic)
        // 			.Where(CheckButtonAttribute);
        // }

        // bool CheckButtonAttribute(MemberInfo memberInfo)
        // {
        // 	return Attribute.IsDefined(memberInfo, typeof(ButtonAttribute));
        // }

        private class CacheFoldProp
        {
            public HashSet<string> types = new HashSet<string>();
            public List<SerializedProperty> props = new List<SerializedProperty>();
            public FoldoutAttribute atr;
            public bool expanded;

            public void Dispose()
            {
                props.Clear();
                types.Clear();
                atr = null;
            }
        }
    }

    internal static class EditorUIHelper
    {
        public static void UseVerticalLayout(this UnityEditor.Editor e, Action action, GUIStyle style, bool styled)
        {
            if (styled) EditorGUILayout.BeginVertical(style);
            else EditorGUILayout.BeginVertical();
            action();
            EditorGUILayout.EndVertical();
        }

        public static void UseButton(this UnityEditor.Editor e, MethodInfo m)
        {
            if (GUILayout.Button(m.Name))
            {
                m.Invoke(e.target, null);
            }
        }
    }

    internal static class StyleFramework
    {
        public static GUIStyle box;
        public static GUIStyle boxChild;
        public static GUIStyle foldout;
        public static GUIStyle button;
        public static GUIStyle text;

        static StyleFramework()
        {
            bool pro = EditorGUIUtility.isProSkin;

            var uiTex_in = Resources.Load<Texture2D>("foldout_arrow_closed");
            var uiTex_in_on = Resources.Load<Texture2D>("foldout_arrow_open");

            var c_on = pro ? Color.white : new Color(51 / 255f, 102 / 255f, 204 / 255f, 1);

            button = new GUIStyle(EditorStyles.miniButton);
            button.font = Font.CreateDynamicFontFromOSFont(new[] {"Terminus (TTF) for Windows", "Calibri"}, 17);

            text = new GUIStyle(EditorStyles.label);
            text.richText = true;
            text.contentOffset = new Vector2(0, 5);
            text.font = Font.CreateDynamicFontFromOSFont(new[] {"Terminus (TTF) for Windows", "Calibri"}, 14);

            foldout = new GUIStyle(EditorStyles.foldout);

            foldout.overflow = new RectOffset(-10, 0, 3, 0);
            foldout.padding = new RectOffset(15, 0, 0, 0);

            foldout.active.textColor = c_on;
            foldout.active.background = uiTex_in;
            foldout.onActive.textColor = c_on;
            foldout.onActive.background = uiTex_in_on;

            foldout.focused.textColor = c_on;
            foldout.focused.background = uiTex_in;
            foldout.onFocused.textColor = c_on;
            foldout.onFocused.background = uiTex_in_on;

            foldout.hover.textColor = c_on;
            foldout.hover.background = uiTex_in;

            foldout.onHover.textColor = c_on;
            foldout.onHover.background = uiTex_in_on;

            box = new GUIStyle(GUI.skin.box);
            box.padding = new RectOffset(20, 0, 5, 5);

            boxChild = new GUIStyle(GUI.skin.box);
            boxChild.active.textColor = c_on;
            boxChild.active.background = uiTex_in;
            boxChild.onActive.textColor = c_on;
            boxChild.onActive.background = uiTex_in_on;

            boxChild.focused.textColor = c_on;
            boxChild.focused.background = uiTex_in;
            boxChild.onFocused.textColor = c_on;
            boxChild.onFocused.background = uiTex_in_on;

            EditorStyles.foldout.active.textColor = c_on;
            EditorStyles.foldout.active.background = uiTex_in;
            EditorStyles.foldout.onActive.textColor = c_on;
            EditorStyles.foldout.onActive.background = uiTex_in_on;

            EditorStyles.foldout.focused.textColor = c_on;
            EditorStyles.foldout.focused.background = uiTex_in;
            EditorStyles.foldout.onFocused.textColor = c_on;
            EditorStyles.foldout.onFocused.background = uiTex_in_on;

            EditorStyles.foldout.hover.textColor = c_on;
            EditorStyles.foldout.hover.background = uiTex_in;

            EditorStyles.foldout.onHover.textColor = c_on;
            EditorStyles.foldout.onHover.background = uiTex_in_on;
        }

        public static string FirstLetterToUpperCase(this string s)
        {
            if (string.IsNullOrEmpty(s))
                return string.Empty;

            var a = s.ToCharArray();
            a[0] = char.ToUpper(a[0]);
            return new string(a);
        }

        public static IList<Type> GetTypeTree(this Type t)
        {
            var types = new List<Type>();
            while (t.BaseType != null)
            {
                types.Add(t);
                t = t.BaseType;
            }

            return types;
        }
    }

    internal static class EditorTypes
    {
        public static Dictionary<int, List<FieldInfo>> fields = new Dictionary<int, List<FieldInfo>>(FastComparable.Default);

        public static int Get(Object target, out List<FieldInfo> objectFields)
        {
            var t = target.GetType();

            var bindingFlags = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
            objectFields = GetMembersInclPrivateBase(t, bindingFlags).ToList();

            return objectFields.Count;
        }

        public static FieldInfo[] GetMembersInclPrivateBase(Type t, BindingFlags flags)
        {
            var memberList = new List<FieldInfo>();
            memberList.AddRange(t.GetFields(flags));
            Type currentType = t;
            while ((currentType = currentType.BaseType) != null)
                memberList.AddRange(currentType.GetFields(flags));
            return memberList.ToArray();
        }
    }

    internal class FastComparable : IEqualityComparer<int>
    {
        public static FastComparable Default = new FastComparable();

        public bool Equals(int x, int y) { return x == y; }

        public int GetHashCode(int obj) { return obj.GetHashCode(); }
    }

    [InitializeOnLoad]
    public static class EditorFramework
    {
        internal static bool needToRepaint;

        internal static Event currentEvent;
        internal static float t;

        static EditorFramework() { EditorApplication.update += Updating; }

        private static void Updating()
        {
            CheckMouse();

            if (needToRepaint)
            {
                t += Time.deltaTime;

                if (t >= 0.3f)
                {
                    t -= 0.3f;
                    needToRepaint = false;
                }
            }
        }

        private static void CheckMouse()
        {
            var ev = currentEvent;
            if (ev == null) return;

            if (ev.type == EventType.MouseMove)
                needToRepaint = true;
        }
    }
}