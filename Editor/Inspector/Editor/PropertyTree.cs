using System;
using UnityEditor;
using UnityEngine;

namespace Pancake.Editor
{
    public abstract class PropertyTree
    {
        private PropertyInspectorElement _rootPropertyInspectorElement;

        public PropertyDefinition RootPropertyDefinition { get; protected set; }
        public Property RootProperty { get; protected set; }

        public Type TargetObjectType { get; protected set; }
        public int TargetsCount { get; protected set; }
        public bool TargetIsPersistent { get; protected set; }

        public bool ValidationRequired { get; private set; } = true;
        public bool RepaintRequired { get; private set; } = true;

        public int RepaintFrame { get; private set; } = 0;

        public virtual void Dispose()
        {
            if (_rootPropertyInspectorElement != null && _rootPropertyInspectorElement.IsAttached)
            {
                _rootPropertyInspectorElement.DetachInternal();
            }
        }

        public virtual void Update(bool forceUpdate = false) { RepaintFrame++; }

        public virtual bool ApplyChanges() { return false; }

        public void RunValidation()
        {
            ValidationRequired = false;

            RootProperty.RunValidation();

            RequestRepaint();
        }

        public void Draw(float? viewWidth = null)
        {
            RepaintRequired = false;

            if (_rootPropertyInspectorElement == null)
            {
                _rootPropertyInspectorElement = new PropertyInspectorElement(RootProperty, new PropertyInspectorElement.Props {forceInline = !RootProperty.TryGetMemberInfo(out _),});
                _rootPropertyInspectorElement.AttachInternal();
            }

            _rootPropertyInspectorElement.Update();
            var width = viewWidth ?? GUILayoutUtility.GetRect(0, 9999, 0, 0).width;
            var height = _rootPropertyInspectorElement.GetHeight(width);
            var rect = GUILayoutUtility.GetRect(width, height);

            if (viewWidth == null)
            {
                rect.xMin += 3;
            }

            rect = EditorGUI.IndentedRect(rect);
            var oldIndent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;

            _rootPropertyInspectorElement.OnGUI(rect);

            EditorGUI.indentLevel = oldIndent;
        }

        public void EnumerateValidationResults(Action<Property, ValidationResult> call) { RootProperty.EnumerateValidationResults(call); }

        public virtual void RequestRepaint() { RepaintRequired = true; }

        public void RequestValidation()
        {
            ValidationRequired = true;

            RequestRepaint();
        }

        public abstract void ForceCreateUndoGroup();
    }
}