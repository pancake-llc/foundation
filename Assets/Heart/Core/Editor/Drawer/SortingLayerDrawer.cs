using System.Linq;
using Pancake;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;

namespace PancakeEditor
{
    public class SortingLayerDrawer : OdinAttributeDrawer<SortingLayerAttribute, string>
    {
        private readonly GUIContent _buttonContent = new();

        protected override void Initialize() => UpdateButtonContent();

        private void UpdateButtonContent() { _buttonContent.text = ValueEntry.SmartValue; }
        
        protected override void DrawPropertyLayout(GUIContent label)
        {
            var rect = EditorGUILayout.GetControlRect(label != null);

            rect = label == null ? EditorGUI.IndentedRect(rect) : EditorGUI.PrefixLabel(rect, label);

            if (!EditorGUI.DropdownButton(rect, _buttonContent, FocusType.Passive)) return;

            var selector = new GenericSelector<string>(SortingLayer.layers.Select(layer => layer.name).ToArray());
            selector.SetSelection(ValueEntry.SmartValue);
            selector.ShowInPopup(rect.position);

            selector.SelectionChanged += x =>
            {
                ValueEntry.Property.Tree.DelayAction(() =>
                {
                    ValueEntry.SmartValue = x.FirstOrDefault();
                    UpdateButtonContent();
                });
            };
        }
    }
}