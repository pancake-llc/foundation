using UnityEditor;
using UnityEngine;

namespace Pancake.Editor
{
    public class BoxGroupInspectorElement : HeaderGroupBaseInspectorElement
    {
        private readonly GUIContent _headerLabel;
        private readonly bool _foldout;
        private readonly string _path;

        public BoxGroupInspectorElement(DeclareBoxGroupAttribute attribute)
        {
            _headerLabel = attribute.Title == null ? GUIContent.none : new GUIContent(attribute.Title);
            _foldout = attribute.Foldout;
            _path = attribute.Path;
        }

        protected override float GetHeaderHeight(float width)
        {
            if (string.IsNullOrEmpty(_headerLabel.text)) return 0f;

            return base.GetHeaderHeight(width);
        }

        protected override void DrawHeader(Rect position)
        {
            Uniform.DrawBox(position, Uniform.TabOnlyOne);

            var headerLabelRect = new Rect(position)
            {
                xMin = position.xMin + 6, xMax = position.xMax - 6, yMin = position.yMin + 2, yMax = position.yMax - 2,
            };

            if (_foldout)
            {
                bool state = Uniform.GetFoldoutState(_path);
                EditorGUILayout.BeginHorizontal(state ? Uniform.GroupHeader : Uniform.GroupHeaderCollapse);

                if (GUI.Button(headerLabelRect, _headerLabel, Uniform.FoldoutButton))
                {
                    Uniform.SetFoldoutState(_path, !state);
                }

                var iconRect = new Rect(headerLabelRect.x + headerLabelRect.width - 10 - 5, headerLabelRect.y, 10, headerLabelRect.height);
                GUI.Label(iconRect, Uniform.GetChevronIcon(state), Uniform.Cheveron);
                EditorGUILayout.EndHorizontal();
            }
            else
            {
                EditorGUI.LabelField(headerLabelRect, _headerLabel);
            }
        }

        protected override bool IsFoldout() { return _foldout; }

        protected override bool FoldoutState() { return Uniform.GetFoldoutState(_path); }
    }
}