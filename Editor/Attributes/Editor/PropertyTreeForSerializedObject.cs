using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Pancake.Attribute;
using UnityEditor;

namespace PancakeEditor.Attribute
{
    public sealed class PropertyTreeForSerializedObject : PropertyTree
    {
        private readonly SerializedObject _serializedObject;
        private readonly SerializedProperty _scriptProperty;

        public PropertyTreeForSerializedObject([NotNull] SerializedObject serializedObject)
        {
            _serializedObject = serializedObject ?? throw new ArgumentNullException(nameof(serializedObject));
            _scriptProperty = serializedObject.FindProperty("m_Script");

            TargetObjectType = _serializedObject.targetObject.GetType();
            TargetsCount = _serializedObject.targetObjects.Length;
            TargetIsPersistent = _serializedObject.targetObject is var targetObject && targetObject != null && EditorUtility.IsPersistent(targetObject);

            RootPropertyDefinition = new PropertyDefinition(memberInfo: null,
                ownerType: null,
                order: -1,
                fieldName: "ROOT",
                fieldType: TargetObjectType,
                valueGetter: (self, targetIndex) => _serializedObject.targetObjects[targetIndex],
                valueSetter: (self, targetIndex, value) => _serializedObject.targetObjects[targetIndex],
                attributes: new List<System.Attribute>(),
                isArrayElement: false);

            RootProperty = new Property(this, null, RootPropertyDefinition, serializedObject);

            RootProperty.ValueChanged += OnPropertyChanged;
            RootProperty.ChildValueChanged += OnPropertyChanged;
        }

        public override void Dispose()
        {
            RootProperty.ChildValueChanged -= OnPropertyChanged;
            RootProperty.ValueChanged -= OnPropertyChanged;

            base.Dispose();
        }

        public override void Update(bool forceUpdate = false)
        {
            if (forceUpdate)
            {
                _serializedObject.SetIsDifferentCacheDirty();
                _serializedObject.Update();
            }

            base.Update(forceUpdate);
        }
        
        public override void Draw()
        {
            DrawMonoScriptProperty();

            base.Draw();
        }

        public override bool ApplyChanges()
        {
            var changed = base.ApplyChanges();
            changed |= _serializedObject.ApplyModifiedProperties();
            return changed;
        }

        public override void ForceCreateUndoGroup()
        {
            Undo.RegisterCompleteObjectUndo(_serializedObject.targetObjects, "Inspector");
            Undo.FlushUndoRecordObjects();
        }

        private void OnPropertyChanged(Property changedProperty)
        {
            foreach (var targetObject in _serializedObject.targetObjects)
            {
                EditorUtility.SetDirty(targetObject);
            }

            RequestValidation();
            RequestRepaint();
        }
        
        private void DrawMonoScriptProperty()
        {
            if (RootProperty.TryGetAttribute(out HideMonoAttribute _))
            {
                return;
            }

            EditorGUI.BeginDisabledGroup(true);
            var scriptRect = EditorGUILayout.GetControlRect(true);
            scriptRect.xMin += 3;
            EditorGUI.PropertyField(scriptRect, _scriptProperty);
            EditorGUI.EndDisabledGroup();
        }
    }
}