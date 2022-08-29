using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Pancake.Editor
{
    public enum ToolbarType
    {
        None = 0,
        Place = 1,
    }
    
    public class Toolbar : EditorWindow
    {
        private const int MIN_SIZE = 32;
        private const int WIDTH = 556;
        private const int HEIGHT = 520;

        private static Toolbar instance;
        private static int controlId;
        private static ToolbarType tool;
        public static Toolbar Instance => instance;
        public static int ControlId { set => controlId = value; }

        public static ToolbarType Tool
        {
            get => tool;
            set
            {
                if (tool == value) return;

                var previousTool = tool;
                tool = value;
                
                RepaintWindow();
            }
        }

        private bool _wasDocked;
        private bool _toolChanged;
        private Vector2 _scollPosition = Vector2.zero;
        private GUIStyle _foldoutButtonStyle;

        private bool IsDocked
        {
            get
            {
                var isDockedMethod = typeof(EditorWindow)
                    .GetProperty("docked", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static)
                    ?.GetGetMethod(true);
                return isDockedMethod != null && (bool) isDockedMethod.Invoke(this, null);
            }
        }

        //[MenuItem("Tools/Pancake/Level Editor/Toolbar &_4", false, 1100)]
        public static void ShowWindow()
        {
            bool status = instance == null;
            instance = GetWindow<Toolbar>("Tools");
            if (status) instance.position = new Rect(instance.position.x, instance.position.y, WIDTH, MIN_SIZE);
        }

        public static void RepaintWindow()
        {
            if (instance != null) instance.Repaint();
        }
        
        private void OnEnable()
        {
            minSize = new Vector2(MIN_SIZE, MIN_SIZE);
            _wasDocked = !IsDocked;
            ControlId = GUIUtility.GetControlID(GetHashCode(), FocusType.Passive);
            _foldoutButtonStyle = new GUIStyle(Uniform.ToggleButtonToolbar);
        }

        private void OnGUI()
        {
            bool widthGreaterThanHeight = position.width > position.height;
            UpdateFoldoutButtonStyle();
            
            using (var scrollView = new EditorGUILayout.ScrollViewScope(_scollPosition, false, false,
                       widthGreaterThanHeight ? GUI.skin.horizontalScrollbar : GUIStyle.none,
                       widthGreaterThanHeight ? GUIStyle.none : GUI.skin.verticalScrollbar, GUIStyle.none))
            {
                _scollPosition = scrollView.scrollPosition;
                using (position.width > position.height
                           ? new GUILayout.HorizontalScope(Uniform.BoxArea)
                           : (GUI.Scope)new GUILayout.VerticalScope(Uniform.BoxArea))
                {
                    
                    Draw();
                    GUILayout.Space(5);
                    GUILayout.FlexibleSpace();
                }
            }
        }
        
        private void UpdateFoldoutButtonStyle()
        {
            if (position.width >= position.height)
            {
                _foldoutButtonStyle.fixedWidth = 16;
                _foldoutButtonStyle.fixedHeight = 24;
            }
            else
            {
                _foldoutButtonStyle.fixedWidth = 24;
                _foldoutButtonStyle.fixedHeight = 16;
            }
        }

        private void Draw()
        {
            using (var check = new EditorGUI.ChangeCheckScope())
            {
                var t = Tool;
                var placeSelected = t == ToolbarType.Place;
                t = GUILayout.Toggle(placeSelected, Uniform.PinIcon, Uniform.ToggleButtonToolbar)
                    ? ToolbarType.Place : placeSelected ? ToolbarType.None : t;

                if (check.changed || _toolChanged)
                {
                    _toolChanged = false;
                    Tool = t;
                }
            }
        }
        
    }
}