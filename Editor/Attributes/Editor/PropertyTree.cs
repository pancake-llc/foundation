using System;
using UnityEditor;
using UnityEngine;
using Pancake.Attribute;

namespace PancakeEditor.Attribute
{
    public abstract class PropertyTree
    {
        private PropertyInspectorElement _rootPropertyElement;
        private Rect _cachedOuterRect = new Rect(0, 0, 0, 0);

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
            if (_rootPropertyElement != null && _rootPropertyElement.IsAttached)
            {
                _rootPropertyElement.DetachInternal();
            }
        }

        public virtual void Update(bool forceUpdate = false) { RepaintFrame++; }

        public virtual bool ApplyChanges() { return false; }

        public void RunValidationIfRequired()
        {
            if (!ValidationRequired)
            {
                return;
            }

            RunValidation();
        }
        
        public void RunValidation()
        {
            ValidationRequired = false;

            RootProperty.RunValidation();

            RequestRepaint();
        }

        public virtual void Draw()
        {
            RepaintRequired = false;

            if (_rootPropertyElement == null)
            {
                _rootPropertyElement =
                    new PropertyInspectorElement(RootProperty, new PropertyInspectorElement.Props {forceInline = !RootProperty.TryGetMemberInfo(out _),});
                _rootPropertyElement.AttachInternal();
            }

            _rootPropertyElement.Update();
            var rectOuter = GUILayoutUtility.GetRect(0, 9999, 0, 0);
            _cachedOuterRect = Event.current.type == EventType.Layout ? _cachedOuterRect : rectOuter;

            var rect = new Rect(_cachedOuterRect);
            rect.height = _rootPropertyElement.GetHeight(rect.width);

            rect = EditorGUI.IndentedRect(rect);
            var oldIndent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;

            GUILayoutUtility.GetRect(_cachedOuterRect.width, rect.height);
            
            _rootPropertyElement.OnGUI(rect);

            EditorGUI.indentLevel = oldIndent;
        }

        public void EnumerateValidationResults(Action<Property, ValidationResult> call) { RootProperty.EnumerateValidationResults(call); }

        public void RequestRepaint() { RepaintRequired = true; }

        public void RequestValidation()
        {
            ValidationRequired = true;

            RequestRepaint();
        }

        public abstract void ForceCreateUndoGroup();
    }
}