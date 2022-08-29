#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Reflection;
using System;
using Object = UnityEngine.Object;
// ReSharper disable InconsistentNaming
// ReSharper disable FieldCanBeMadeReadOnly.Local

namespace Pancake.Toolbar 
{
    public static class MainToolbarsView
    {
        const BindingFlags AnyBind = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static;

        static Type mainViewT = typeof(UnityEditor.Editor).Assembly.GetType("UnityEditor.MainView");
        static Type viewT = mainViewT.BaseType;
        static Type appBarT = typeof(UnityEditor.Editor).Assembly.GetType("UnityEditor.AppStatusBar");
        static Type toolbarT = typeof(UnityEditor.Editor).Assembly.GetType("UnityEditor.Toolbar");
        static FieldInfo m_UseTopView = mainViewT.GetField("m_UseTopView", AnyBind);
        static FieldInfo m_TopViewHeight = mainViewT.GetField("m_TopViewHeight", AnyBind);
        static FieldInfo m_UseBottomView = mainViewT.GetField("m_UseBottomView", AnyBind);
        static MethodInfo addChild = mainViewT.GetMethod("AddChild", AnyBind, null, new Type[] { viewT }, null);
        static MethodInfo insertChild = mainViewT.GetMethod("AddChild", AnyBind, null, new Type[] { viewT, typeof(int) }, null);
        static MethodInfo removeChild = mainViewT.GetMethod("RemoveChild", AnyBind, null, new Type[] { viewT }, null);
        static MethodInfo SetPosition = mainViewT.GetMethod("SetPosition", AnyBind);
        static PropertyInfo windowPosition = mainViewT.GetProperty("windowPosition", AnyBind);
        static PropertyInfo position = viewT.GetProperty("position", AnyBind);

        const string ToggleMainToolbarPath = "Tools/Toolbars/Main Toolbar %&-";
        const string ToggleAppStatusBarPath = "Tools/Toolbars/Status Bar %&=";

        public static bool isMainToolbarEnabled => Resources.FindObjectsOfTypeAll(toolbarT).Length != 0;
        public static bool isAppStatusBarEnabled => Resources.FindObjectsOfTypeAll(appBarT).Length != 0;

        [MenuItem(ToggleMainToolbarPath, true)]
        static bool MainToolbarValidate() { Menu.SetChecked(ToggleMainToolbarPath, isMainToolbarEnabled); return true; }
        [MenuItem(ToggleAppStatusBarPath, true)]
        static bool AppStatusBarValidate() { Menu.SetChecked(ToggleAppStatusBarPath, isAppStatusBarEnabled); return true; }

        [MenuItem(ToggleMainToolbarPath)]
        public static void ToggleMainToolbar()
        {
            //EditorApplication.delayCall += EditorViewModule.RefreshAllModules;
            var exist = Resources.FindObjectsOfTypeAll(toolbarT).Length != 0;
            var mainView = Resources.FindObjectsOfTypeAll(mainViewT)[0];

            if (!exist)
            {
                var toolbar = ScriptableObject.CreateInstance(toolbarT);

                m_TopViewHeight.SetValue(mainView, 30);
                m_UseTopView.SetValue(mainView, true);

                insertChild.Invoke(mainView, new object[] { toolbar, 0 });
                UpdateMainViewPosition(mainView);
                return;
            }
            var bar = Resources.FindObjectsOfTypeAll(toolbarT)[0];
            m_TopViewHeight.SetValue(mainView, -1);
            m_UseTopView.SetValue(mainView, false);

            removeChild.Invoke(mainView, new object[] { bar });

            var pos = (Rect)windowPosition.GetValue(mainView);
            SetPosition.Invoke(mainView, new object[] { pos });

            Object.DestroyImmediate(bar);
        }

        [MenuItem(ToggleAppStatusBarPath)]
        public static void ToggleAppStatusBar()
        {
            //EditorApplication.delayCall += EditorViewModule.RefreshAllModules;
            var exist = Resources.FindObjectsOfTypeAll(appBarT).Length != 0;
            var mainView = Resources.FindObjectsOfTypeAll(mainViewT)[0];

            if (!exist)
            {
                var toolbar = ScriptableObject.CreateInstance(appBarT);

                m_UseBottomView.SetValue(mainView, true);

                addChild.Invoke(mainView, new object[] { toolbar });
                var pos = (Rect)position.GetValue(toolbar);
                pos.height = 20;
                position.SetValue(toolbar, pos);
                UpdateMainViewPosition(mainView);
                //EditorViewModule.RefreshAllModules();
                return;
            }
            var bar = Resources.FindObjectsOfTypeAll(appBarT)[0];
            m_UseBottomView.SetValue(mainView, false);

            removeChild.Invoke(mainView, new object[] { bar });

            UpdateMainViewPosition(mainView);

            Object.DestroyImmediate(bar);
        }

        static void UpdateMainViewPosition(object mainView)
        {
            var pos = (Rect)windowPosition.GetValue(mainView);
            SetPosition.Invoke(mainView, new object[] { pos });
        }
    }
}
#endif