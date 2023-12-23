using Pancake.Apex;
using Pancake.ExLib.Reflection;
using Pancake.ExLibEditor;
using Pancake.ExLibEditor.Windows;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEditor;
using UnityEngine;
using Vexe.Runtime.Extensions;
using Object = UnityEngine.Object;

namespace Pancake.ApexEditor
{
    public class SerializedField : SerializedMember, ISerializedField, IContainer
    {
        sealed class EmptyDrawer : FieldDrawer
        {
            public override void OnGUI(Rect position, SerializedField serializedField, GUIContent label) { }
        }

        sealed class EmptyView : FieldView
        {
            public override void OnGUI(Rect position, SerializedField serializedField, GUIContent label) { }
        }

        private class ErrorBlock
        {
            private GUIContent content;
            private GUIContent prefix;
            private float width;

            public ErrorBlock(string memberName, string message)
            {
                content = new GUIContent(message);
                if (string.IsNullOrEmpty(memberName))
                {
                    prefix = GUIContent.none;
                }
                else
                {
                    prefix = new GUIContent(memberName);
                }
            }

            public void OnGUI(Rect position)
            {
                int controlID = GUIUtility.GetControlID(FocusType.Passive, position);
                position = EditorGUI.PrefixLabel(position, controlID, prefix);
                EditorGUI.HelpBox(position, content.text, MessageType.Error);
                width = position.width;
            }

            public float GetHeight() { return EditorStyles.helpBox.CalcHeight(content, width); }

            public string GetMessage() { return content.text; }
        }

        private static readonly List<SerializedMember> ChildrenNone;
        private static readonly FieldDrawer DrawerNone;
        private static readonly FieldView ViewNone;
        private static readonly List<FieldValidator> ValidatorsNone;
        private static readonly List<FieldDecorator> DecoratorsNone;
        private static readonly List<FieldInlineDecorator> InlineDecoratorsNone;
        private static IDictionary UnityPropertyDrawers;

        /// <summary>
        /// Static constructor of serialized field.
        /// </summary>
        static SerializedField()
        {
            ChildrenNone = new List<SerializedMember>();
            DrawerNone = new EmptyDrawer();
            ViewNone = new EmptyView();
            ValidatorsNone = new List<FieldValidator>();
            DecoratorsNone = new List<FieldDecorator>();
            InlineDecoratorsNone = new List<FieldInlineDecorator>();
        }

        private SerializedProperty serializedProperty;
        private List<SerializedMember> children;
        private FieldDrawer drawer;
        private FieldView view;
        private bool hasPropertyDrawer;
        private List<FieldValidator> validators;
        private List<FieldDecorator> decorators;
        private List<FieldInlineDecorator> inlineDecorators;
        private object lastReference;
        private int lastArrayLength;
        private float elementHeight;
        private bool toggleOnLabelClick;
        private bool childrenLoaded;
        private bool defaultEditor;
        private bool isValueChanged;
        private ErrorBlock errorBlock;

        // Stored callback properties.
        private object lastValue;
        private MethodCaller<object, object> onValueChanged;
        private MemberGetter<object, object> valueGetter;
        private MemberGetter<object, object[]> valueArrayGetter;

        /// <summary>
        /// Implement this constructor to make initializations.
        /// </summary>
        /// <param name="serializedObject">Serialized object reference of this serialized member.</param>
        /// <param name="memberName">Member name of this serialized member.</param>
        public SerializedField(SerializedObject serializedObject, string memberName)
            : base(serializedObject, memberName)
        {
            try
            {
                serializedProperty = GetSerializedObject().FindProperty(memberName);
            }
            catch (Exception ex)
            {
                errorBlock = new ErrorBlock(memberName, ex.Message);
                return;
            }

            if (serializedProperty != null)
            {
                children = ChildrenNone;
                view = ViewNone;
                drawer = DrawerNone;
                validators = ValidatorsNone;
                decorators = DecoratorsNone;
                inlineDecorators = InlineDecoratorsNone;

                toggleOnLabelClick = true;

                if (IsGenericArray())
                {
                    lastArrayLength = serializedProperty.arraySize;
                }

                if (serializedProperty.propertyType == SerializedPropertyType.ManagedReference)
                {
                    lastReference = serializedProperty.managedReferenceValue;
                }

                if (IsExpanded())
                {
                    ApplyChildren();
                }

                ApplyLabel();
                if (memberName == "m_Script")
                {
                    AddManipulator(new ReadOnlyAttribute());
                    return;
                }
                
                RegisterCallbacks();
                LoadDrawer();
                SafeLoadScriptAttributeUtility();

                if (UnityPropertyDrawers != null)
                {
                    hasPropertyDrawer = UnityPropertyDrawers.Contains(GetMemberType());
                }

                AddViews(GetAttributes<ViewAttribute>());
                AddValidators(GetAttributes<ValidatorAttribute>());
                if (!IsArrayElement())
                {
                    AddDecorators(GetAttributes<DecoratorAttribute>());
                    AddInlineDecorators(GetAttributes<InlineDecoratorAttribute>());
                }

                Type type = GetMemberType();
                defaultEditor = GetAttribute<UseDefaultEditor>() != null || ApexUtility.IsExceptType(type);
            }
            else
            {
                if(GetMemberType() != null)
                {
                    errorBlock = new ErrorBlock(memberName ,"The property cannot be serialized, the [Serializable] attribute may be missing.");
                }
                else
                {
                    errorBlock = new ErrorBlock(memberName, "Undefined error. The property and type was not found.");
                }
            }
        }

        #region [SerializedMember Implementation]

        /// <summary>
        /// Called for rendering and handling serialized field.
        /// <br>Implement this method to override drawing of serialized field.</br>
        /// </summary>
        /// <param name="position">Rectangle position to draw serialized field GUI.</param>
        protected override void OnMemberGUI(Rect position)
        {
            if (errorBlock == null)
            {
                OnBeforeGUI();
                MonitorSerializedProperty();
                DrawAllTopDecorators(ref position);

                if (position.height > 0)
                {
                    float labelWidth = EditorGUIUtility.labelWidth;
                    float totalHeight = position.height;

                    position.height = elementHeight;
                    DrawAllInlineDecorators(ref position);

                    OnFieldGUI(position);
                    position.y = position.yMax + EditorGUIUtility.standardVerticalSpacing;

                    EditorGUIUtility.labelWidth = labelWidth;

                    position.height = totalHeight - elementHeight;
                    DrawAllBottomDecorators(ref position);
                }
                InvokeCallbacks();

                ExecuteAllValidators();
            }
            else
            {
                errorBlock.OnGUI(position);
            }
        }

        /// <summary>
        /// Total height required to draw serialized field.
        /// </summary>
        protected override float GetMemberHeight()
        {
            if(errorBlock == null)
            {
                MonitorSerializedProperty();
                elementHeight = GetFieldHeight();
                return elementHeight + GetDecoratorsHeight();
            }

            return errorBlock.GetHeight();
        }

        #endregion

        /// <summary>
        /// Called every GUI call, before all member GUI methods.
        /// </summary>
        protected override void OnBeforeGUI()
        {
            base.OnBeforeGUI();
            isValueChanged = false;
        }

        /// <summary>
        /// Implement this method to override default drawing of serialized field.
        /// </summary>
        /// <param name="position">Rectangle position to draw default GUI.</param>
        protected virtual void OnFieldGUI(Rect position)
        {
            if (defaultEditor)
            {
                OnUnityGUI(position);
            }
            else if (view != ViewNone)
            {
                view.OnGUI(position, this, GetLabel());
            }
            else if (drawer != DrawerNone)
            {
                drawer.OnGUI(position, this, GetLabel());
            }
            else if (hasPropertyDrawer)
            {
                OnUnityGUI(position);
            }
            else if (serializedProperty.propertyType == SerializedPropertyType.Generic || serializedProperty.propertyType == SerializedPropertyType.ManagedReference)
            {
                if (IsGenericArray())
                {
                    OnArrayGUI(position);
                }
                else
                {
                    OnReferenceGUI(position);
                }
            }
            else
            {
                OnUnityGUI(position);
            }
        }

        /// <summary>
        /// Implement this method to override default drawing <i>Array</i> type of serialized field.
        /// </summary>
        /// <param name="position">Rectangle position to draw array.</param>
        protected virtual void OnArrayGUI(Rect position)
        {
            float totalHeight = position.height - EditorGUIUtility.singleLineHeight;

            position.height = EditorGUIUtility.singleLineHeight;
            using(new EditorGUI.IndentLevelScope(1))
            {
                IsExpanded(EditorGUI.Foldout(position, IsExpanded(), GetLabel(), toggleOnLabelClick));
            }
            position.y = position.yMax + EditorGUIUtility.standardVerticalSpacing;
            
            if (IsExpanded() && totalHeight > 0)
            {
                EditorGUI.indentLevel++;
                SerializedMember sizeField = children[0];
                if (sizeField.IsVisible())
                {
                    totalHeight -= EditorGUIUtility.singleLineHeight;
                    EditorGUI.BeginChangeCheck();
                    sizeField.OnGUI(position);
                    if (EditorGUI.EndChangeCheck())
                    {
                        GetSerializedObject().ApplyModifiedProperties();
                        ApplyChildren();
                        lastArrayLength = serializedProperty.arraySize;
                        return;
                    }

                    position.y = position.yMax + EditorGUIUtility.standardVerticalSpacing;
                }


                if (totalHeight > 0)
                {
                    for (int i = 1; i < children.Count; i++)
                    {
                        if (totalHeight <= 0)
                        {
                            break;
                        }

                        SerializedMember child = children[i];
                        if (child.IsVisible())
                        {
                            position.height = child.GetHeight();
                            child.OnGUI(position);
                            position.y = position.yMax + EditorGUIUtility.standardVerticalSpacing;
                            totalHeight -= position.height;
                        }
                    }

                    EditorGUI.indentLevel--;
                }
            }
        }

        /// <summary>
        /// Implement this method to override default drawing <i>Generic</i> or <i>ManagedReference</i> type serialized field.
        /// </summary>
        /// <param name="position">Rectangle position to draw field.</param>
        protected virtual void OnReferenceGUI(Rect position)
        {
            if (serializedProperty.hasVisibleChildren)
            {
                bool isValid = true;
                if (serializedProperty.propertyType == SerializedPropertyType.ManagedReference)
                {
                    object value = serializedProperty.managedReferenceValue;
                    isValid = value != null && !value.GetType().IsAbstract;
                }

                if (isValid)
                {
                    float totalHeight = position.height - EditorGUIUtility.singleLineHeight;

                    position.height = EditorGUIUtility.singleLineHeight;
                    using (new EditorGUI.IndentLevelScope(1))
                    {
                        IsExpanded(EditorGUI.Foldout(position, IsExpanded(), GetLabel(), toggleOnLabelClick));
                    }
                    position.y = position.yMax + EditorGUIUtility.standardVerticalSpacing;
                    
                    if (IsExpanded() && totalHeight > 0)
                    {
                        EditorGUI.indentLevel++;
                        for (int i = 0; i < children.Count; i++)
                        {
                            if (totalHeight <= 0)
                            {
                                break;
                            }

                            SerializedMember child = children[i];
                            if (child.IsVisible())
                            {
                                position.height = child.GetHeight();
                                child.OnGUI(position);
                                position.y = position.yMax + EditorGUIUtility.standardVerticalSpacing;
                                totalHeight -= position.height;
                            }
                        }

                        EditorGUI.indentLevel--;
                    }
                }
                else if (serializedProperty.propertyType == SerializedPropertyType.ManagedReference)
                {
                    position = EditorGUI.PrefixLabel(position, GetLabel());
                    if (GUI.Button(position, "Select derived type...", EditorStyles.popup))
                    {
                        OpenDerivedTypesWindow(position);
                    }
                }
            }
            else
            {
                GUI.Label(position, GetLabel());
            }
        }

        /// <summary>
        /// Default unity editor gui implementation.
        /// </summary>
        /// <param name="position">Rectangle position to draw field.</param>
        protected void OnUnityGUI(Rect position) { EditorGUI.PropertyField(position, serializedProperty, GetLabel(), true); }

        /// <summary>
        /// Draw serialized field children.
        /// </summary>
        /// <param name="position">Rectangle position to draw children.</param>
        public void DrawChildren(Rect position)
        {
            float totalHeight = position.height;
            for (int i = 0; i < children.Count; i++)
            {
                if (totalHeight <= 0)
                {
                    break;
                }

                SerializedMember child = children[i];
                if (child.IsVisible())
                {
                    position.height = child.GetHeight();
                    EditorGUI.BeginChangeCheck();
                    child.OnGUI(position);
                    if (EditorGUI.EndChangeCheck() && child is SerializedField field && field.serializedProperty.propertyType == SerializedPropertyType.ArraySize)
                    {
                        GetSerializedObject().ApplyModifiedProperties();
                        ApplyChildren();
                        return;
                    }

                    position.y = position.yMax + EditorGUIUtility.standardVerticalSpacing;
                    totalHeight -= position.height;
                }
            }
        }

        /// <summary>
        /// Height required to draw serialized field, without decorators.
        /// </summary>
        public virtual float GetFieldHeight()
        {
            if (defaultEditor)
            {
                return EditorGUI.GetPropertyHeight(serializedProperty, GetLabel(), true);
            }
            else if (view != ViewNone)
            {
                return view.GetHeight(this, GetLabel());
            }
            else if (drawer != DrawerNone)
            {
                return drawer.GetHeight(this, GetLabel());
            }
            else if (hasPropertyDrawer)
            {
                return EditorGUI.GetPropertyHeight(serializedProperty, GetLabel(), true);
            }
            else if (children.Count > 0)
            {
                float height = EditorGUIUtility.singleLineHeight;
                if (IsExpanded())
                {
                    height += GetChildrenHeight() + EditorGUIUtility.standardVerticalSpacing;
                }

                return height;
            }

            return EditorGUI.GetPropertyHeight(serializedProperty, GetLabel(), true);
        }

        /// <summary>
        /// Height required to draw serialized field children.
        /// </summary>
        public virtual float GetChildrenHeight()
        {
            float height = 0;
            if (children.Count > 0)
            {
                for (int i = 0; i < children.Count; i++)
                {
                    SerializedMember child = children[i];
                    if (child.IsVisible())
                    {
                        height += child.GetHeight() + EditorGUIUtility.standardVerticalSpacing;
                    }
                }

                height -= EditorGUIUtility.standardVerticalSpacing;
            }

            return height;
        }

        /// <summary>
        /// Height required to draw painters of serialized field.
        /// </summary>
        public virtual float GetDecoratorsHeight()
        {
            float height = 0;
            if (decorators.Count > 0)
            {
                for (int i = 0; i < decorators.Count; i++)
                {
                    FieldDecorator decorator = decorators[i];
                    if (decorator.IsVisible())
                    {
                        height += decorator.GetHeight() + EditorGUIUtility.standardVerticalSpacing;
                    }
                }
            }

            return height;
        }

        /// <summary>
        /// Recursive move through all nested properties and initialize them.
        /// </summary>
        public void ApplyChildren()
        {
            if (serializedProperty.hasVisibleChildren)
            {
                HideChildrenAttribute hideChildrenAttribute = GetAttribute<HideChildrenAttribute>();

                int count = 0;
                children = new List<SerializedMember>();
                foreach (SerializedProperty child in serializedProperty.GetVisibleChildren())
                {
                    try
                    {
                        SerializedField field = new SerializedField(GetSerializedObject(), child.propertyPath);
                        field.UpdateParent = ApplyChildren;
                        field.ValueChanged += (value) => ValueChanged?.Invoke(lastValue);

                        string relativePath = child.propertyPath.Substring(serializedProperty.propertyPath.Length + 1);
                        if (hideChildrenAttribute != null && hideChildrenAttribute.names.Any(n => n == relativePath))
                        {
                            field.VisibilityCallback += () => false;
                        }

                        field.SetOrder(count++);
                        children.Add(field);
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError(
                            $"Failed to create a child of serialized field on the path: {child.propertyPath} (Field has been ignored)\nException: <i>{ex.Message}</i>");
                    }
                }

                // Collect all buttons with [Button] attribute.
                FieldInfo fieldInfo = GetMemberInfo() as FieldInfo;
                object value = fieldInfo.GetValue(GetDeclaringObject());
                if (value != null)
                {
                    var type = value.GetType();
                    var limitDescendant = value is MonoBehaviour ? typeof(MonoBehaviour) : typeof(Object);
                    foreach (MethodInfo methodInfo in type.AllMethods(limitDescendant))
                    {
                        MethodButtonAttribute methodButtonAttribute = methodInfo.GetCustomAttribute<MethodButtonAttribute>();
                        if (methodButtonAttribute != null)
                        {
                            if (methodButtonAttribute is ButtonAttribute)
                            {
                                try
                                {
                                    MethodButton button = new Button(GetSerializedObject(), $"{serializedProperty.propertyPath}.{methodInfo.Name}") {Repaint = Repaint};

                                    button.SetOrder(count++);
                                    children.Add(button);
                                }
                                catch (Exception ex)
                                {
                                    Debug.LogError(
                                        $"Failed to create a method button <b>{methodInfo.Name}</b> of the {GetSerializedObject().targetObject.GetType().Name} object. (Button has been ignored)\n<b><color=red>Exception: {ex.Message}</color></b>\n\nStacktrace:");
                                }
                            }
                            else if (methodButtonAttribute is RepeatButtonAttribute)
                            {
                                try
                                {
                                    MethodButton button =
                                        new RepeatButton(GetSerializedObject(), $"{serializedProperty.propertyPath}.{methodInfo.Name}") {Repaint = Repaint};

                                    button.SetOrder(count++);
                                    children.Add(button);
                                }
                                catch (Exception ex)
                                {
                                    Debug.LogError(
                                        $"Failed to create a method button <b>{methodInfo.Name}</b> of the {GetSerializedObject().targetObject.GetType().Name} object. (Button has been ignored)\n<b><color=red>Exception: {ex.Message}</color></b>\n\nStacktrace:");
                                }
                            }
                            else if (methodButtonAttribute is ToggleButtonAttribute)
                            {
                                try
                                {
                                    MethodButton button =
                                        new ToggleButton(GetSerializedObject(), $"{serializedProperty.propertyPath}.{methodInfo.Name}") {Repaint = Repaint};

                                    button.SetOrder(count++);
                                    children.Add(button);
                                }
                                catch (Exception ex)
                                {
                                    Debug.LogError(
                                        $"Failed to create a method button <b>{methodInfo.Name}</b> of the {GetSerializedObject().targetObject.GetType().Name} object. (Button has been ignored)\n<b><color=red>Exception: {ex.Message}</color></b>\n\nStacktrace:");
                                }
                            }
                        }
                    }
                }

                children.TrimExcess();
                children.Sort();

                childrenLoaded = true;
                GUI.changed = true;
            }
            else
            {
                children = ChildrenNone;
            }
        }

        /// <summary>
        /// Clear all nested properties.
        /// </summary>
        public void DisposeChildren()
        {
            children.Clear();
            children = ChildrenNone;
            childrenLoaded = false;
            GUI.changed = true;
        }

        /// <summary>
        /// Load drawer for this serialized field.
        /// </summary>
        public void LoadDrawer()
        {
            if (GetMemberType() != null)
            {
                if (FieldHelper.Drawers.TryGetValue(GetMemberType(), out FieldDrawer drawer))
                {
                    drawer = Activator.CreateInstance(drawer.GetType()) as FieldDrawer;
                    drawer.Initialize(this, GetLabel());
                    this.drawer = drawer;
                }
            }
        }

        /// <summary>
        /// Remove element drawer.
        /// </summary>
        public void RemoveDrawer() { drawer = DrawerNone; }

        /// <summary>
        /// Add view attribute to element.
        /// </summary>
        /// <param name="attribute">View attribute.</param>
        public void AddView(ViewAttribute attribute)
        {
            view = ViewNone;
            if (attribute != null)
            {
                if (FieldHelper.Views.TryGetValue(attribute.GetType(), out FieldView view))
                {
                    view = Activator.CreateInstance(view.GetType()) as FieldView;
                    if (view is ITypeValidationCallback verification && !verification.IsValidProperty(serializedProperty))
                    {
                        return;
                    }

                    view.Initialize(this, attribute, GetLabel());
                    this.view = view;
                }
            }
        }

        /// <summary>
        /// Add view attributes to element and array children.
        /// </summary>
        /// <param name="attributes">View attribute.</param>
        public void AddViews(ViewAttribute[] attributes)
        {
            view = ViewNone;
            if (attributes.Length > 0)
            {
                for (int i = 0; i < attributes.Length; i++)
                {
                    ViewAttribute attribute = attributes[i];
                    if (FieldHelper.Views.TryGetValue(attribute.GetType(), out FieldView view))
                    {
                        view = Activator.CreateInstance(view.GetType()) as FieldView;
                        if (view is ITypeValidationCallback verification && !verification.IsValidProperty(serializedProperty))
                        {
                            continue;
                        }

                        view.Initialize(this, attribute, GetLabel());
                        this.view = view;
                    }
                }
            }
        }

        /// <summary>
        /// Remove view from element.
        /// </summary>
        public void RemoveView() { view = ViewNone; }

        /// <summary>
        /// Add validators to serialized field.
        /// </summary>
        /// <param name="attributes">Validator attributes.</param>
        public void AddValidators(ValidatorAttribute[] attributes)
        {
            if (attributes != null)
            {
                for (int i = 0; i < attributes.Length; i++)
                {
                    ValidatorAttribute attribute = attributes[i];
                    if (FieldHelper.Validators.TryGetValue(attribute.GetType(), out FieldValidator validator))
                    {
                        validator = Activator.CreateInstance(validator.GetType()) as FieldValidator;
                        if (validator is ITypeValidationCallback verification && !verification.IsValidProperty(serializedProperty))
                        {
                            return;
                        }

                        validator.Initialize(this, attribute, GetLabel());

                        if (validators == ValidatorsNone)
                            validators = new List<FieldValidator>();
                        validators.Add(validator);
                    }
                }
            }
            else
            {
                ClearValidators();
            }
        }

        /// <summary>
        /// Add validator to serialized field.
        /// </summary>
        /// <param name="attribute">Validator attribute.</param>
        public void AddValidator(ValidatorAttribute attribute)
        {
            if (FieldHelper.Validators.TryGetValue(attribute.GetType(), out FieldValidator validator))
            {
                validator = Activator.CreateInstance(validator.GetType()) as FieldValidator;
                if (validator is ITypeValidationCallback verification && !verification.IsValidProperty(serializedProperty))
                {
                    return;
                }

                validator.Initialize(this, attribute, GetLabel());

                if (validators == ValidatorsNone)
                    validators = new List<FieldValidator>();
                validators.Add(validator);
            }
        }

        /// <summary>
        /// Remove all validators with same attribute type.
        /// </summary>
        /// <param name="attributes">Validator attribute type.</param>
        public void RemoveValidators(ValidatorAttribute attribute)
        {
            if (FieldHelper.Validators.TryGetValue(attribute.GetType(), out FieldValidator validator))
            {
                List<int> indexes = new List<int>();

                for (int i = 0; i < validators.Count; i++)
                {
                    if (validators[i].GetType() == validator.GetType())
                    {
                        indexes.Add(i);
                    }
                }

                for (int i = 0; i < indexes.Count; i++)
                {
                    validators.RemoveAt(indexes[i]);
                }

                if (validators.Count == 0)
                {
                    ClearValidators();
                }
            }
        }

        /// <summary>
        /// Remove validator at index.
        /// </summary>
        /// <param name="index">Validator index.</param>
        public void RemoveValidator(int index)
        {
            validators.RemoveAt(index);

            if (validators.Count == 0)
            {
                ClearValidators();
            }
        }

        /// <summary>
        /// Clear all validators.
        /// </summary>
        public void ClearValidators() { validators = ValidatorsNone; }

        /// <summary>
        /// Add decorators to serialized field.
        /// </summary>
        /// <param name="attributes">Decorator attributes.</param>
        public void AddDecorators(DecoratorAttribute[] attributes)
        {
            if (attributes != null)
            {
                for (int i = 0; i < attributes.Length; i++)
                {
                    DecoratorAttribute attribute = attributes[i];
                    if (FieldHelper.Decorators.TryGetValue(attribute.GetType(), out FieldDecorator decorator))
                    {
                        decorator = Activator.CreateInstance(decorator.GetType()) as FieldDecorator;
                        if (decorator is ITypeValidationCallback verification && !verification.IsValidProperty(serializedProperty))
                        {
                            return;
                        }

                        decorator.Initialize(this, attribute, GetLabel());

                        if (decorators == DecoratorsNone)
                            decorators = new List<FieldDecorator>();
                        decorators.Add(decorator);
                    }
                }
            }
            else
            {
                ClearDecorators();
            }
        }

        /// <summary>
        /// Add decorator to serialized field.
        /// </summary>
        /// <param name="attribute">Decorator attribute.</param>
        public void AddDecorator(DecoratorAttribute attribute)
        {
            if (FieldHelper.Decorators.TryGetValue(attribute.GetType(), out FieldDecorator decorator))
            {
                decorator = Activator.CreateInstance(decorator.GetType()) as FieldDecorator;
                if (decorator is ITypeValidationCallback verification && !verification.IsValidProperty(serializedProperty))
                {
                    return;
                }

                decorator.Initialize(this, attribute, GetLabel());

                if (decorators == DecoratorsNone)
                    decorators = new List<FieldDecorator>();
                decorators.Add(decorator);
            }
        }

        /// <summary>
        /// Remove all decorators with same attribute type.
        /// </summary>
        /// <param name="attributes">Decorator attribute type.</param>
        public void RemoveDecorators(DecoratorAttribute attribute)
        {
            if (FieldHelper.Decorators.TryGetValue(attribute.GetType(), out FieldDecorator decorator))
            {
                List<int> indexes = new List<int>();

                for (int i = 0; i < decorators.Count; i++)
                {
                    if (decorators[i].GetType() == decorator.GetType())
                    {
                        indexes.Add(i);
                    }
                }

                for (int i = 0; i < indexes.Count; i++)
                {
                    decorators.RemoveAt(indexes[i]);
                }

                if (decorators.Count == 0)
                {
                    ClearDecorators();
                }
            }
        }

        /// <summary>
        /// Remove decorator at index.
        /// </summary>
        /// <param name="index">Decorator index.</param>
        public void RemoveDecorator(int index)
        {
            decorators.RemoveAt(index);

            if (decorators.Count == 0)
            {
                ClearDecorators();
            }
        }

        /// <summary>
        /// Clear all decorators.
        /// </summary>
        public void ClearDecorators() { decorators = DecoratorsNone; }

        /// <summary>
        /// Add decorators to serialized field.
        /// </summary>
        /// <param name="attributes">Decorator attributes.</param>
        public void AddInlineDecorators(InlineDecoratorAttribute[] attributes)
        {
            if (attributes != null)
            {
                for (int i = 0; i < attributes.Length; i++)
                {
                    InlineDecoratorAttribute attribute = attributes[i];
                    if (FieldHelper.InlineDecorators.TryGetValue(attribute.GetType(), out FieldInlineDecorator inlineDecorator))
                    {
                        inlineDecorator = Activator.CreateInstance(inlineDecorator.GetType()) as FieldInlineDecorator;
                        if (inlineDecorator is ITypeValidationCallback verification && !verification.IsValidProperty(serializedProperty))
                        {
                            return;
                        }

                        inlineDecorator.Initialize(this, attribute, GetLabel());

                        if (inlineDecorators == InlineDecoratorsNone)
                            inlineDecorators = new List<FieldInlineDecorator>();
                        inlineDecorators.Add(inlineDecorator);
                    }
                }
            }
            else
            {
                ClearInlineDecorators();
            }
        }

        /// <summary>
        /// Add inline decorator to serialized field.
        /// </summary>
        /// <param name="attribute">Inline decorator attribute.</param>
        public void AddInlineDecorator(InlineDecoratorAttribute attribute)
        {
            if (FieldHelper.InlineDecorators.TryGetValue(attribute.GetType(), out FieldInlineDecorator inlineDecorator))
            {
                inlineDecorator = Activator.CreateInstance(inlineDecorator.GetType()) as FieldInlineDecorator;
                if (inlineDecorator is ITypeValidationCallback verification && !verification.IsValidProperty(serializedProperty))
                {
                    return;
                }

                inlineDecorator.Initialize(this, attribute, GetLabel());

                if (inlineDecorators == InlineDecoratorsNone)
                    inlineDecorators = new List<FieldInlineDecorator>();
                inlineDecorators.Add(inlineDecorator);
            }
        }

        /// <summary>
        /// Remove all inline decorator with same attribute type.
        /// </summary>
        /// <param name="attributes">Inline decorator attribute type.</param>
        public void RemoveInlineDecorators(InlineDecoratorAttribute attribute)
        {
            if (FieldHelper.InlineDecorators.TryGetValue(attribute.GetType(), out FieldInlineDecorator inlineDecorator))
            {
                List<int> indexes = new List<int>();

                for (int i = 0; i < inlineDecorators.Count; i++)
                {
                    if (inlineDecorators[i].GetType() == inlineDecorator.GetType())
                    {
                        indexes.Add(i);
                    }
                }

                for (int i = 0; i < indexes.Count; i++)
                {
                    inlineDecorators.RemoveAt(indexes[i]);
                }

                if (inlineDecorators.Count == 0)
                {
                    ClearInlineDecorators();
                }
            }
        }

        /// <summary>
        /// Remove inline decorator at index.
        /// </summary>
        /// <param name="index">Inline decorator index.</param>
        public void RemoveInlineDecorator(int index)
        {
            inlineDecorators.RemoveAt(index);

            if (inlineDecorators.Count == 0)
            {
                ClearInlineDecorators();
            }
        }

        /// <summary>
        /// Clear all inline decorators.
        /// </summary>
        public void ClearInlineDecorators() { inlineDecorators = InlineDecoratorsNone; }

        /// <summary>
        /// Should the label be a clickable part of the control?
        /// <br><i>Only for default foldout control drawing.</i></br>
        /// </summary>
        public void ToggleOnLabelClick(bool value) { toggleOnLabelClick = value; }

        /// <summary>
        /// Execute all validators which attached to this element.
        /// </summary>
        protected void ExecuteAllValidators()
        {
            if (validators.Count > 0)
            {
                for (int i = 0; i < validators.Count; i++)
                {
                    validators[i].Validate(this);
                }
            }
        }

        /// <summary>
        /// Draw all decorators which attached to this element.
        /// </summary>
        protected void DrawAllTopDecorators(ref Rect position)
        {
            float x = position.x;
            float width = position.width;
            float totalHeight = position.height;

            position = EditorGUI.IndentedRect(position);
            for (int i = 0; i < decorators.Count; i++)
            {
                if (totalHeight > 0)
                {
                    FieldDecorator decorator = decorators[i];
                    if (decorator.IsVisible())
                    {
                        if (decorator.GetSide() == DecoratorSide.Top)
                        {
                            position.height = decorator.GetHeight();
                            decorator.OnGUI(position);
                            position.y = position.yMax + EditorGUIUtility.standardVerticalSpacing;

                            totalHeight -= position.height;
                        }
                    }
                }
            }
            position.x = x;
            position.width = width;
        }

        /// <summary>
        /// Draw all decorators which attached to this element.
        /// </summary>
        protected void DrawAllBottomDecorators(ref Rect position)
        {
            float x = position.x;
            float width = position.width;
            float totalHeight = position.height;

            position = EditorGUI.IndentedRect(position);
            for (int i = 0; i < decorators.Count; i++)
            {
                if (totalHeight > 0)
                {
                    FieldDecorator decorator = decorators[i];
                    if (decorator.IsVisible())
                    {
                        if (decorator.GetSide() == DecoratorSide.Bottom)
                        {
                            position.height = decorator.GetHeight();
                            decorator.OnGUI(position);
                            position.y = position.yMax + EditorGUIUtility.standardVerticalSpacing;

                            totalHeight -= position.height;
                        }
                    }
                }
            }

            position.x = x;
            position.width = width;
        }

        /// <summary>
        /// Draw all decorators which attached to this element.
        /// </summary>
        protected void DrawAllInlineDecorators(ref Rect position)
        {
            float x = position.x;
            float totalWidth = position.width;

            for (int i = 0; i < inlineDecorators.Count; i++)
            {
                FieldInlineDecorator inlineDecorator = inlineDecorators[i];
                if (inlineDecorator.IsVisible())
                {
                    float width = inlineDecorator.GetWidth();
                    if (inlineDecorator.GetSide() == InlineDecoratorSide.Right)
                    {
                        position.width -= width;
                        totalWidth = position.width;

                        position.x = position.xMax;
                        position.width = width;
                        inlineDecorator.OnGUI(position);
                    }
                    else if (inlineDecorator.GetSide() == InlineDecoratorSide.Left)
                    {
                        position.x += EditorGUIUtility.labelWidth;
                        position.width = width;

                        EditorGUIUtility.labelWidth += width;
                        inlineDecorator.OnGUI(position);
                    }

                    position.x = x;
                    position.width = totalWidth;
                }
            }
        }

        /// <summary>
        /// Check element field attributes and apply specified label.
        /// </summary>
        protected void ApplyLabel()
        {
            HideLabelAttribute hideLabelAttribute = GetAttribute<HideLabelAttribute>();
            if (hideLabelAttribute == null)
            {
                LabelAttribute labelAttribute = GetAttribute<LabelAttribute>();
                if (labelAttribute != null)
                {
                    SetLabel(new GUIContent(labelAttribute.name, serializedProperty.tooltip));
                }
                else
                {
                    SetLabel(new GUIContent(serializedProperty.displayName, serializedProperty.tooltip));
                }
            }
            else
            {
                SetLabel(GUIContent.none);
            }
        }

        /// <summary>
        /// Serialized field is generic array?
        /// </summary>
        public bool IsGenericArray() { return serializedProperty.isArray && serializedProperty.propertyType == SerializedPropertyType.Generic; }

        /// <summary>
        /// Serialized field is array element?
        /// </summary>
        public bool IsArrayElement()
        {
            if (serializedProperty.propertyType == SerializedPropertyType.ArraySize)
            {
                return true;
            }

            const string ELEMENT_NAME = "data[";
            string[] paths = serializedProperty.propertyPath.Split('.');
            string lastPath = paths[paths.Length - 1];
            return lastPath.Contains(ELEMENT_NAME);
        }

        /// <summary>
        /// Children serialized fields is loaded?
        /// </summary>
        public bool ChildrenLoaded() { return childrenLoaded; }

        /// <summary>
        /// Request to update the parent serialized field to recreate all child elements.
        /// <br>If there is no parent serialized field, the request will be ignored.</br>
        /// </summary>
        protected void RequestToUpdateParent() { UpdateParent?.Invoke(); }

        /// <summary>
        /// Search and register callbacks.
        /// </summary>
        private void RegisterCallbacks()
        {
            FieldInfo fieldInfo = GetMemberInfo() as FieldInfo;
            if (fieldInfo == null)
            {
                return;
            }

            if (fieldInfo.FieldType.IsArray)
            {
                valueArrayGetter = fieldInfo.DelegateForGet<object, object[]>();
                lastValue = valueArrayGetter.Invoke(GetDeclaringObject());
            }
            else
            {
                valueGetter = fieldInfo.DelegateForGet();
                lastValue = valueGetter.Invoke(GetDeclaringObject());
            }

            OnValueChangedAttribute valueAttribute = GetAttribute<OnValueChangedAttribute>();
            if (valueAttribute != null)
            {
                Type type = GetDeclaringObject().GetType();
                foreach (MethodInfo methodInfo in type.AllMethods())
                {
                    if (methodInfo.Name == valueAttribute.name)
                    {
                        ParameterInfo[] parameters = methodInfo.GetParameters();
                        if (parameters.Length == 0 || (parameters.Length == 1 && parameters[0].ParameterType == typeof(SerializedProperty)))
                        {
                            onValueChanged = methodInfo.DelegateForCall();
                            break;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Invoke registered callbacks.
        /// </summary>
        private void InvokeCallbacks()
        {
            object value = null;
            if (valueGetter != null)
            {
                if(serializedProperty.propertyType == SerializedPropertyType.ObjectReference)
                {
                    value = serializedProperty.objectReferenceValue;
                }
                else
                {
                    value = valueGetter.Invoke(GetDeclaringObject());
                }
            }
            else if (valueArrayGetter != null)
            {
                value = valueArrayGetter.Invoke(GetDeclaringObject());
            }

            if ((value != null && !value.Equals(lastValue)) || (value != null && lastValue == null) || (value == null && lastValue != null))
            {
                if (onValueChanged != null)
                {
                    int parameterCount = onValueChanged.Method.GetParameters().Length;
                    if (parameterCount == 0)
                    {
                        onValueChanged.Invoke(GetDeclaringObject(), null);
                    }
                    else
                    {
                        onValueChanged.Invoke(GetDeclaringObject(), new object[1] {serializedProperty});
                    }
                }

                isValueChanged = true;
                ValueChanged?.Invoke(value);
                lastValue = value;
                ValueChanged?.Invoke(value);
            }
        }

        /// <summary>
        /// Monitoring serialized property changes outside the serialize field.
        /// </summary>
        private void MonitorSerializedProperty()
        {
            if (serializedProperty.propertyType == SerializedPropertyType.ManagedReference &&
                ((serializedProperty.managedReferenceValue != null && !serializedProperty.managedReferenceValue.Equals(lastReference)) ||
                 (serializedProperty.managedReferenceValue != null && lastReference == null) ||
                 (serializedProperty.managedReferenceValue == null && lastReference != null)))
            {
                LoadDrawer();

                AddView(GetAttribute<ViewAttribute>());
                AddValidators(GetAttributes<ValidatorAttribute>());
                if (!IsArrayElement())
                {
                    AddDecorators(GetAttributes<DecoratorAttribute>());
                    AddInlineDecorators(GetAttributes<InlineDecoratorAttribute>());
                }

                if (IsExpanded())
                {
                    ApplyChildren();
                }

                lastReference = serializedProperty.managedReferenceValue;
            }
            else if (IsGenericArray() && lastArrayLength != serializedProperty.arraySize)
            {
                if (IsExpanded())
                {
                    ApplyChildren();
                }

                lastArrayLength = serializedProperty.arraySize;
            }
        }

        /// <summary>
        /// Show ExSearch window with all derived types of this member type.
        /// </summary>
        /// <param name="position">Button rectangle position.</param>
        private void OpenDerivedTypesWindow(Rect position)
        {
            ExSearchWindow searchWindow = ExSearchWindow.Create("Derived Types");

            TypeCache.TypeCollection types = TypeCache.GetTypesDerivedFrom(GetMemberType());
            for (int i = 0; i < types.Count; i++)
            {
                Type type = types[i];
                if (type.IsAbstract)
                {
                    continue;
                }

                GUIContent content = new GUIContent(type.Name);
                SearchContent attribute = type.GetCustomAttribute<SearchContent>();
                if (attribute != null)
                {
                    if (attribute.Hidden)
                    {
                        continue;
                    }

                    content.text = attribute.name;
                    content.tooltip = attribute.Tooltip;
                    if (SearchContentUtility.TryLoadContentImage(attribute.Image, out Texture2D image))
                    {
                        content.image = image;
                    }
                }

                searchWindow.AddEntry(content,
                    () =>
                    {
                        serializedProperty.managedReferenceValue = Activator.CreateInstance(type);
                        serializedProperty.serializedObject.ApplyModifiedProperties();
                        RequestToUpdateParent();
                    });
            }

            searchWindow.Open(position);
        }

        #region [ISerializedField Implementation]
        /// <summary>
        /// Target serialized property of serialized field.
        /// </summary>
        public SerializedProperty GetSerializedProperty()
        {
            return serializedProperty;
        }
        #endregion

        #region [IContainer Implementation]
        /// <summary>
        /// Loop through all entities of the entity container.
        /// </summary>
        public IEnumerable<VisualEntity> Entities
        {
            get
            {
                return children;
            }
        }
        #endregion

        #region [Serialized Property Method Wrappers]

        public float GetFloat() { return serializedProperty.floatValue; }

        public void SetFloat(float value) { serializedProperty.floatValue = value; }

        public int GetInteger() { return serializedProperty.intValue; }

        public void SetInteger(int value) { serializedProperty.intValue = value; }

        public string GetString() { return serializedProperty.stringValue; }

        public void SetString(string value) { serializedProperty.stringValue = value; }

        public bool GetBool() { return serializedProperty.boolValue; }

        public void SetBool(bool value) { serializedProperty.boolValue = value; }

        public Object GetObject() { return serializedProperty.objectReferenceValue; }

        public void SetObject(Object value) { serializedProperty.objectReferenceValue = value; }

        public int GetObjectInstanceID() { return serializedProperty.objectReferenceInstanceIDValue; }

        public void SetObjectInstanceID(int value) { serializedProperty.objectReferenceInstanceIDValue = value; }

        public object GetManagedReference() { return serializedProperty.managedReferenceValue; }

        public void SetManagedReference(object value)
        {
            serializedProperty.managedReferenceValue = value;
            lastReference = serializedProperty.managedReferenceValue;
            GetSerializedObject().ApplyModifiedProperties();
            if (IsExpanded())
            {
                ApplyChildren();
            }
        }

        public void SetManagedReference(Type type) { SetManagedReference(Activator.CreateInstance(type)); }

        public void SetManagedReference<T>() { SetManagedReference(typeof(T)); }

        public Type GetManagedReferenceType()
        {
            string[] baseTypeAndAssemblyName = serializedProperty.managedReferenceFieldTypename.Split(' ');
            string baseTypeString = string.Format("{0}, {1}", baseTypeAndAssemblyName[1], baseTypeAndAssemblyName[0]);
            return Type.GetType(baseTypeString);
        }

        public Color GetColor() { return serializedProperty.colorValue; }

        public void SetColor(Color value) { serializedProperty.colorValue = value; }

        public void SetAnimationCurve(AnimationCurve value) { serializedProperty.animationCurveValue = value; }

        public void SetEnum<T>(T value) where T : Enum
        {
            for (int i = 0; i < serializedProperty.enumNames.Length; i++)
            {
                if (serializedProperty.enumNames[i] == value.ToString())
                {
                    serializedProperty.enumValueIndex = i;
                    break;
                }
            }
        }

        public void SetEnum(int valueIndex) { serializedProperty.enumValueIndex = valueIndex; }

        public string GetEnumDisplayValue() { return serializedProperty.enumDisplayNames[serializedProperty.enumValueIndex]; }

        public string GetEnumDisplayValue(int valueIndex) { return serializedProperty.enumDisplayNames[valueIndex]; }

        public string[] GetEnumDisplayValues() { return serializedProperty.enumDisplayNames; }

        public string GetEnumValue() { return serializedProperty.enumNames[serializedProperty.enumValueIndex]; }

        public string GetEnumValue(int valueIndex) { return serializedProperty.enumNames[valueIndex]; }

        public string[] GetEnumValues() { return serializedProperty.enumNames; }

        public int GetEnumValueCount() { return serializedProperty.enumDisplayNames.Length; }

        public int GetEnumValueIndex() { return serializedProperty.enumValueIndex; }

        public Quaternion GetQuaternion() { return serializedProperty.quaternionValue; }

        public void SetQuaternion(Quaternion value) { serializedProperty.quaternionValue = value; }

        public int GetLayerMask() { return serializedProperty.intValue; }

        public string GetLayerMaskName() { return LayerMask.LayerToName(serializedProperty.intValue); }

        public void SetLayerMask(string value) { serializedProperty.intValue = LayerMask.NameToLayer(value); }

        public void SetLayerMask(int value) { serializedProperty.intValue = 1 << value; }

        public Vector2 GetVector2() { return serializedProperty.vector2Value; }

        public void SetVector2(Vector2 value) { serializedProperty.vector2Value = value; }

        public Vector2Int GetVector2Int() { return serializedProperty.vector2IntValue; }

        public void SetVector2Int(Vector2Int value) { serializedProperty.vector2IntValue = value; }

        public Vector3 GetVector3() { return serializedProperty.vector3Value; }

        public void SetVector3(Vector3 value) { serializedProperty.vector3Value = value; }

        public Vector3 GetVector3Int() { return serializedProperty.vector3IntValue; }

        public void SetVector3Int(Vector3Int value) { serializedProperty.vector3IntValue = value; }

        public Rect GetRect() { return serializedProperty.rectValue; }

        public void SetRect(Rect value) { serializedProperty.rectValue = value; }

        public RectInt GetRectInt() { return serializedProperty.rectIntValue; }

        public void SetRectInt(RectInt value) { serializedProperty.rectIntValue = value; }

        public Bounds GetBounds() { return serializedProperty.boundsValue; }

        public void SetBounds(Bounds value) { serializedProperty.boundsValue = value; }

        public BoundsInt GetBoundsInt() { return serializedProperty.boundsIntValue; }

        public void SetBoundsInt(BoundsInt value) { serializedProperty.boundsIntValue = value; }

        public void ResizeArray(int value)
        {
            serializedProperty.arraySize = value;
            lastArrayLength = serializedProperty.arraySize;
            GetSerializedObject().ApplyModifiedProperties();
            if (IsExpanded())
            {
                ApplyChildren();
            }
        }

        public void IncreaseArraySize()
        {
            serializedProperty.arraySize++;
            lastArrayLength = serializedProperty.arraySize;
            GetSerializedObject().ApplyModifiedProperties();
            if (IsExpanded())
            {
                ApplyChildren();
            }
        }

        public void InsertArrayElement(int index)
        {
            serializedProperty.InsertArrayElementAtIndex(index);
            lastArrayLength = serializedProperty.arraySize;
            GetSerializedObject().ApplyModifiedProperties();
            if (IsExpanded())
            {
                ApplyChildren();
            }
        }

        public int GetArrayLength() { return serializedProperty.arraySize; }

        public void RemoveArrayElement(int index)
        {
            serializedProperty.DeleteArrayElementAtIndex(index);
            lastArrayLength = serializedProperty.arraySize;
            GetSerializedObject().ApplyModifiedProperties();
            if (IsExpanded())
            {
                ApplyChildren();
            }
        }

        public SerializedField GetArrayElement(int index)
        {
            if (!IsGenericArray())
            {
                throw new IndexOutOfRangeException($"An attempt to get an array element from a <b>{serializedProperty.name}</b> that is not an array!");
            }

            if (index >= serializedProperty.arraySize || index < 0)
            {
                throw new IndexOutOfRangeException($"Array (<b>{serializedProperty.propertyPath}</b>) index({index}) out of range!");
            }

            if (children.Count - 1 != serializedProperty.arraySize)
            {
                ApplyChildren();
                lastArrayLength = serializedProperty.arraySize;
            }

            return GetChild(++index) as SerializedField;
        }

        public bool IsExpanded() { return serializedProperty.isExpanded; }

        public void IsExpanded(bool value)
        {
            serializedProperty.isExpanded = value;
            if (serializedProperty.isExpanded && !childrenLoaded)
            {
                ApplyChildren();
            }
            else if (!serializedProperty.isExpanded && childrenLoaded)
            {
                DisposeChildren();
            }
        }

        #endregion

        #region [Static]
        private static void SafeLoadScriptAttributeUtility()
        {
            if (UnityPropertyDrawers == null)
            {
                try
                {
                    Assembly assembly = Assembly.GetAssembly(typeof(SerializedProperty));
                    Type scriptAttributeUtility = assembly.GetType("UnityEditor.ScriptAttributeUtility");
                    if (scriptAttributeUtility != null)
                    {
                        FieldInfo fieldInfo = scriptAttributeUtility.GetField("s_DrawerTypeForType", BindingFlags.NonPublic | BindingFlags.Static);
                        if (fieldInfo != null)
                        {
                            UnityPropertyDrawers = fieldInfo.GetValue(null) as IDictionary;
                        }
                    }
                }
                catch
                {
                    Debug.Log("Failed to load the Unity PropertyDrawer cache.");
                }
            }
        }
        #endregion
        
        
        #region [Event Callback Functions]

        /// <summary>
        /// Request to update parent of serialized field.
        /// </summary>
        private Action UpdateParent;

        /// <summary>
        /// Called when value changed.
        /// </summary>
        public event Action<object> ValueChanged;

        #endregion

        #region [Getter / Setter]

        public SerializedMember GetChild(int index)
        {
            if (!serializedProperty.hasVisibleChildren)
            {
                throw new IndexOutOfRangeException($"The <b>{serializedProperty.name}</b> has no visible children.");
            }

            if (children.Count == 0 && serializedProperty.hasVisibleChildren)
            {
                ApplyChildren();
            }

            if (index >= children.Count)
            {
                throw new IndexOutOfRangeException();
            }

            return children[index];
        }

        public T GetChild<T>(int index) where T : SerializedMember { return GetChild(index) as T; }

        public SerializedMember FindRelative(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                throw new ArgumentException($"The path cannot be null or empty!");
            }

            if (!serializedProperty.hasVisibleChildren)
            {
                throw new Exception($"The <b>{serializedProperty.name}</b> has no visible children.");
            }

            if (children.Count == 0 && serializedProperty.hasVisibleChildren)
            {
                ApplyChildren();
            }

            string[] paths = path.Split('.');
            string name = paths[0];
            for (int i = 0; i < children.Count; i++)
            {
                SerializedMember member = children[i];
                if (member.GetMemberInfo().Name == name)
                {
                    if (paths.Length > 1)
                    {
                        if (member is SerializedField field)
                        {
                            return field.FindRelative(string.Join('.', paths.Skip(1)));
                        }
                        else
                        {
                            throw new Exception("Member not found!");
                        }
                    }

                    return member;
                }
            }

            throw new Exception("Member not found!");
        }

        public T FindRelative<T>(string path) where T : SerializedMember { return FindRelative(path) as T; }


        public int GetChildrenCount() { return children.Count; }
        public bool IsValueChanged() { return isValueChanged; }

        #endregion
    }
}