using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Pancake.Editor
{
    public class SerializedField : SerializedMember, IEnumerable<SerializedField>
    {
        sealed class EmptyDrawer : FieldDrawer
        {
            public override void OnGUI(Rect position, SerializedField element, GUIContent label) { }
        }

        sealed class EmptyView : FieldView
        {
            public override void OnGUI(Rect position, SerializedField element, GUIContent label) { }
        }

        private static readonly List<SerializedField> ChildrenNone;
        private static readonly FieldDrawer DrawerNone;
        private static readonly FieldView ViewNone;
        private static readonly List<FieldValidator> ValidatorsNone;
        private static readonly List<FieldDecorator> DecoratorsNone;
        private static readonly List<FieldInlineDecorator> InlineDecoratorsNone;

        public readonly SerializedProperty serializedProperty;

        private FieldDrawer drawer = DrawerNone;
        private FieldView view = ViewNone;
        private List<SerializedField> children = ChildrenNone;
        private List<FieldValidator> validators = ValidatorsNone;
        private List<FieldDecorator> decorators = DecoratorsNone;
        private List<FieldInlineDecorator> inlineDecorators = InlineDecoratorsNone;
        private MethodInfo onChangedCallback;
        private object[] callbackParameter;
        private bool toggleOnLabelClick;
        private bool defaultEditor;
        private float elementHeight;

        /// <summary>
        /// Static constructor of serialized field.
        /// </summary>
        static SerializedField()
        {
            DrawerNone = new EmptyDrawer();
            ViewNone = new EmptyView();
            ChildrenNone = new List<SerializedField>();
            ValidatorsNone = new List<FieldValidator>();
            DecoratorsNone = new List<FieldDecorator>();
            InlineDecoratorsNone = new List<FieldInlineDecorator>();
        }

        /// <summary>
        /// Implement this constructor to make initializations.
        /// </summary>
        /// <param name="serializedObject">Serialized object reference of this serialized member.</param>
        /// <param name="memberName">Member name of this serialized member.</param>
        public SerializedField(SerializedObject serializedObject, string memberName)
            : base(serializedObject, memberName)
        {
            SerializedProperty serializedProperty = serializedObject.FindProperty(memberName);
            if (serializedProperty != null)
            {
                this.serializedProperty = serializedProperty.Copy();

                ApplyNestedProperties();
                ApplyLabel();
                RegisterCallbacks();
                LoadDrawer();

                AddView(GetAttribute<ViewAttribute>());
                AddValidators(GetAttributes<ValidatorAttribute>());
                AddDecorators(GetAttributes<DecoratorAttribute>());
                AddInlineDecorators(GetAttributes<InlineDecoratorAttribute>());

                toggleOnLabelClick = true;
                defaultEditor = GetAttribute<UseDefaultEditor>() != null;
                if (!defaultEditor && memberInfo != null)
                {
                    FieldInfo fieldInfo = memberInfo as FieldInfo;
                    Func<EditorSettings.ExceptType, bool> predicate = (exceptType) =>
                    {
                        Type type = fieldInfo.FieldType;
                        do
                        {
                            if (type.Name == exceptType.GetName()) return true;
                            type = type.BaseType;
                        } while (type != null && exceptType.SubClasses());

                        return false;
                    };
                    defaultEditor = EditorSettings.Current.GetAttributeExceptTypes() == null || EditorSettings.Current.GetAttributeExceptTypes().Any(predicate);
                }
            }
        }

        /// <summary>
        /// Find member info of serialized member. 
        /// </summary>
        /// <param name="serializedObject">Serialized object reference of this serialized member.</param>
        /// <param name="memberName">Member name of this serialized member.</param>
        protected override MemberInfo FindMemberInfo(SerializedObject serializedObject, string memberName)
        {
            Type type = serializedObject.targetObject.GetType();
            BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
            FieldInfo fieldInfo = type.GetAllMembers(memberName, flags).Where(m => m is FieldInfo).FirstOrDefault() as FieldInfo;
            return fieldInfo;
        }

        /// <summary>
        /// Called for rendering and handling serialized field.
        /// <br>Implement this method to override drawing of serialized field.</br>
        /// </summary>
        /// <param name="position">Rectangle position to draw serialized field GUI.</param>
        protected override void OnMemberGUI(Rect position)
        {
            ExecuteAllValidators();

            position = DrawAllTopDecorators(position);

            if (position.height > 0)
            {
                Rect elementPosition = DrawAllInlineDecorators(position);

                EditorGUI.BeginChangeCheck();
                if (defaultEditor)
                    OnUnityGUI(position);
                else if (view != ViewNone)
                    view.OnGUI(elementPosition, this, GetLabel());
                else
                    OnDefaultGUI(elementPosition);
                if (EditorGUI.EndChangeCheck())
                {
                    onChangedCallback?.Invoke(serializedObject.targetObject, callbackParameter);
                }

                position.height = Mathf.Max(0, position.height - elementHeight);
                DrawAllBottomDecorators(position);
            }
        }

        /// <summary>
        /// Implement this method to override default drawing of serialized field.
        /// </summary>
        /// <param name="position">Rectangle position to draw default GUI.</param>
        protected virtual void OnDefaultGUI(Rect position)
        {
            if (drawer != DrawerNone)
            {
                drawer.OnGUI(position, this, GetLabel());
            }
            else if (children.Count > 0)
            {
                if (IsGenericArray())
                {
                    OnArrayGUI(position);
                }
                else if (serializedProperty.propertyType == SerializedPropertyType.Generic || serializedProperty.propertyType == SerializedPropertyType.ManagedReference)
                {
                    OnReferenceGUI(position);
                }
                else
                {
                    position.height = Mathf.Max(0, position.height - EditorGUIUtility.singleLineHeight);
                    if (position.height >= 0)
                    {
                        Rect foldoutPosition = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
                        serializedProperty.isExpanded = EditorGUI.Foldout(foldoutPosition, serializedProperty.isExpanded, GetLabel(), toggleOnLabelClick);
                        if (serializedProperty.isExpanded && position.height > 0)
                        {
                            EditorGUI.indentLevel++;
                            foldoutPosition.yMax += EditorGUIUtility.standardVerticalSpacing;
                            DrawChildren(foldoutPosition);
                            EditorGUI.indentLevel--;
                        }
                    }
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
            position.height = Mathf.Max(0, position.height - EditorGUIUtility.singleLineHeight);
            if (position.height >= 0)
            {
                Rect foldoutPosition = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
                serializedProperty.isExpanded = EditorGUI.Foldout(foldoutPosition, serializedProperty.isExpanded, GetLabel(), toggleOnLabelClick);

                if (serializedProperty.isExpanded && position.height > 0)
                {
                    EditorGUI.indentLevel++;
                    foldoutPosition.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;


                    SerializedField sizeField = children[0];
                    if (sizeField.IsVisible())
                    {
                        position.height = Mathf.Max(0, position.height - EditorGUIUtility.singleLineHeight);
                        if (position.height > 0)
                        {
                            Rect sizePosition = new Rect(foldoutPosition.x, foldoutPosition.y, foldoutPosition.width, EditorGUIUtility.singleLineHeight);
                            EditorGUI.BeginChangeCheck();
                            sizeField.OnMemberGUI(sizePosition);
                            if (EditorGUI.EndChangeCheck())
                            {
                                serializedObject.ApplyModifiedProperties();
                                ApplyNestedProperties();
                                return;
                            }

                            foldoutPosition.y = sizePosition.yMax + EditorGUIUtility.standardVerticalSpacing;
                            if (children.Count == 1)
                            {
                                foldoutPosition.y += EditorGUIUtility.standardVerticalSpacing;
                            }
                        }
                    }

                    if (position.height > 0)
                    {
                        for (int i = 1; i < children.Count; i++)
                        {
                            if (position.height == 0)
                            {
                                break;
                            }

                            SerializedField child = children[i];
                            if (child.IsVisible())
                            {
                                float height = child.GetHeight();
                                if (position.height < height)
                                {
                                    height = position.height;
                                    position.height = 0;
                                }
                                else
                                {
                                    position.height -= height;
                                }

                                if (height > 0)
                                {
                                    Rect childPosition = new Rect(foldoutPosition.x, foldoutPosition.y, foldoutPosition.width, height);
                                    child.OnMemberGUI(childPosition);
                                    foldoutPosition.y += height;
                                    if (i < children.Count - 1)
                                    {
                                        foldoutPosition.y += EditorGUIUtility.standardVerticalSpacing;
                                    }
                                }
                            }
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
            position.height = Mathf.Max(0, position.height - EditorGUIUtility.singleLineHeight);
            if (position.height >= 0)
            {
                Rect foldoutPosition = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
                serializedProperty.isExpanded = EditorGUI.Foldout(foldoutPosition, serializedProperty.isExpanded, GetLabel(), toggleOnLabelClick);
                if (serializedProperty.isExpanded && position.height > 0)
                {
                    EditorGUI.indentLevel++;
                    foldoutPosition.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
                    for (int i = 0; i < children.Count; i++)
                    {
                        if (position.height == 0)
                        {
                            break;
                        }

                        SerializedField child = children[i];
                        if (child.IsVisible())
                        {
                            float height = child.GetHeight();
                            if (position.height < height)
                            {
                                height = position.height;
                                position.height = 0;
                            }
                            else
                            {
                                position.height -= height;
                            }

                            if (height > 0)
                            {
                                Rect childPosition = new Rect(foldoutPosition.x, foldoutPosition.y, foldoutPosition.width, height);
                                child.OnMemberGUI(childPosition);
                                foldoutPosition.y += height;
                                if (i < children.Count - 1)
                                {
                                    foldoutPosition.y += EditorGUIUtility.standardVerticalSpacing;
                                }
                            }
                        }
                    }

                    EditorGUI.indentLevel--;
                }
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
            for (int i = 0; i < children.Count; i++)
            {
                if (position.height == 0)
                {
                    break;
                }

                SerializedField child = children[i];
                if (child.IsVisible())
                {
                    float height = child.GetHeight();
                    if (position.height < height)
                    {
                        height = position.height;
                        position.height = 0;
                    }
                    else
                    {
                        position.height -= height;
                    }

                    if (height > 0)
                    {
                        Rect childPosition = new Rect(position.x, position.y, position.width, height);
                        EditorGUI.BeginChangeCheck();
                        child.OnMemberGUI(childPosition);
                        if (EditorGUI.EndChangeCheck() && child.serializedProperty.propertyType == SerializedPropertyType.ArraySize)
                        {
                            serializedObject.ApplyModifiedProperties();
                            ApplyNestedProperties();
                            return;
                        }

                        position.y += height;
                        if (i < children.Count - 1)
                        {
                            position.y += EditorGUIUtility.standardVerticalSpacing;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Total height required to draw serialized field.
        /// </summary>
        protected override float GetMemberHeight()
        {
            elementHeight = GetElementHeight();
            return elementHeight + GetDecoratorsHeight();
        }

        /// <summary>
        /// Height required to draw serialized field, without decorators.
        /// </summary>
        public virtual float GetElementHeight()
        {
            if (!defaultEditor)
            {
                if (view != ViewNone)
                {
                    return view.GetHeight(this, GetLabel());
                }
                else if (drawer != DrawerNone)
                {
                    return drawer.GetHeight(this, GetLabel());
                }
                else
                {
                    if (children.Count > 0)
                    {
                        float height = EditorGUIUtility.singleLineHeight;
                        if (serializedProperty.isExpanded)
                        {
                            height += GetChildrenHeight() + EditorGUIUtility.standardVerticalSpacing;
                        }

                        return height;
                    }
                }
            }

            return EditorGUI.GetPropertyHeight(serializedProperty, GetLabel(), true);
        }

        /// <summary>
        /// Height required to draw serialized field children.
        /// </summary>
        public virtual float GetChildrenHeight()
        {
            float height = 0;
            for (int i = 0; i < children.Count; i++)
            {
                SerializedField child = children[i];
                if (child.IsVisible())
                {
                    height += child.GetHeight();
                    if (i < children.Count - 1)
                    {
                        height += EditorGUIUtility.standardVerticalSpacing;
                    }
                }
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
                    float decoratorHeight = decorators[i].GetHeight();
                    height += decoratorHeight;
                    if (decoratorHeight > 0)
                    {
                        if (i < decorators.Count)
                        {
                            height += EditorGUIUtility.standardVerticalSpacing;
                        }
                    }
                }
            }

            return height;
        }

        /// <summary>
        /// Recursive move through all nested properties and initialize them.
        /// </summary>
        public void ApplyNestedProperties()
        {
            switch (serializedProperty.propertyType)
            {
                default:
                    children = ChildrenNone;
                    break;
                case SerializedPropertyType.Generic:
                case SerializedPropertyType.ManagedReference:
                    HideChildrenAttribute hideChildrenAttribute = GetAttribute<HideChildrenAttribute>();
                    children = new List<SerializedField>();
                    foreach (SerializedProperty child in GetVisibleChildren())
                    {
                        SerializedField field = new SerializedField(serializedObject, child.propertyPath);

                        if (IsGenericArray())
                        {
                            field.AddView(GetAttribute<ViewAttribute>());
                            field.AddValidators(GetAttributes<ValidatorAttribute>());
                        }

                        if (hideChildrenAttribute != null && hideChildrenAttribute.names.Any(n => n == child.propertyPath))
                        {
                            field.VisibilityCallback += () => false;
                        }

                        children.Add(field);
                    }

                    break;
            }
        }

        /// <summary>
        /// Load drawer for this serialized field.
        /// </summary>
        public void LoadDrawer()
        {
            if (memberInfo is FieldInfo fieldInfo)
            {
                if (InspectorReflection.Drawers.TryGetValue(fieldInfo.FieldType, out FieldDrawer drawer))
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
            if (attribute != null)
            {
                if (InspectorReflection.Views.TryGetValue(attribute.GetType(), out FieldView view))
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
            else
            {
                view = ViewNone;
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
                    if (InspectorReflection.Validators.TryGetValue(attribute.GetType(), out FieldValidator validator))
                    {
                        validator = Activator.CreateInstance(validator.GetType()) as FieldValidator;
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
            if (InspectorReflection.Validators.TryGetValue(attribute.GetType(), out FieldValidator validator))
            {
                validator = Activator.CreateInstance(validator.GetType()) as FieldValidator;
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
            if (InspectorReflection.Validators.TryGetValue(attribute.GetType(), out FieldValidator validator))
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
                    if (InspectorReflection.Decorators.TryGetValue(attribute.GetType(), out FieldDecorator decorator))
                    {
                        decorator = Activator.CreateInstance(decorator.GetType()) as FieldDecorator;
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
            if (InspectorReflection.Decorators.TryGetValue(attribute.GetType(), out FieldDecorator decorator))
            {
                decorator = Activator.CreateInstance(decorator.GetType()) as FieldDecorator;
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
            if (InspectorReflection.Decorators.TryGetValue(attribute.GetType(), out FieldDecorator decorator))
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
                    if (InspectorReflection.InlineDecorators.TryGetValue(attribute.GetType(), out FieldInlineDecorator inlineDecorator))
                    {
                        inlineDecorator = Activator.CreateInstance(inlineDecorator.GetType()) as FieldInlineDecorator;
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
            if (InspectorReflection.InlineDecorators.TryGetValue(attribute.GetType(), out FieldInlineDecorator inlineDecorator))
            {
                inlineDecorator = Activator.CreateInstance(inlineDecorator.GetType()) as FieldInlineDecorator;
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
            if (InspectorReflection.InlineDecorators.TryGetValue(attribute.GetType(), out FieldInlineDecorator inlineDecorator))
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
        /// Find serialized field by property path.
        /// </summary>
        public SerializedField FindElement(string propertyPath)
        {
            SerializedField element = null;
            for (int i = 0; i < children.Count; i++)
            {
                SerializedField child = children[i];
                if (child.serializedProperty.propertyPath == propertyPath)
                {
                    element = child;
                    break;
                }
            }

            return element;
        }

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
        protected Rect DrawAllTopDecorators(Rect position)
        {
            int count = decorators.Count;
            if (count > 0)
            {
                for (int i = 0; i < count; i++)
                {
                    FieldDecorator decorator = decorators[i];
                    if (decorator.GetSide() == DecoratorSide.Top)
                    {
                        float height = decorator.GetHeight();
                        if (position.height < height)
                        {
                            height = position.height;
                            position.height = 0;
                        }
                        else
                        {
                            position.height -= height;
                        }

                        if (height > 0)
                        {
                            Rect rect = new Rect(position.x, position.y, position.width, height);
                            decorator.OnGUI(EditorGUI.IndentedRect(rect));
                            if (rect.height > 0)
                            {
                                position.y += rect.height + EditorGUIUtility.standardVerticalSpacing;
                            }
                        }
                    }
                }
            }

            return position;
        }

        /// <summary>
        /// Draw all decorators which attached to this element.
        /// </summary>
        protected void DrawAllBottomDecorators(Rect position)
        {
            int count = decorators.Count;
            if (count > 0)
            {
                position.y += elementHeight + EditorGUIUtility.standardVerticalSpacing;
                for (int i = 0; i < count; i++)
                {
                    FieldDecorator decorator = decorators[i];
                    if (decorator.GetSide() == DecoratorSide.Bottom)
                    {
                        float height = decorator.GetHeight();
                        if (position.height < height)
                        {
                            height = position.height;
                            position.height = 0;
                        }
                        else
                        {
                            position.height -= height;
                        }

                        if (height > 0)
                        {
                            Rect rect = new Rect(position.x, position.y, position.width, height);
                            decorator.OnGUI(EditorGUI.IndentedRect(rect));
                            if (rect.height > 0)
                            {
                                position.y += height + EditorGUIUtility.standardVerticalSpacing;
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Draw all decorators which attached to this element.
        /// </summary>
        protected Rect DrawAllInlineDecorators(Rect totalPosition)
        {
            int count = inlineDecorators.Count;
            if (count > 0)
            {
                for (int i = 0; i < count; i++)
                {
                    FieldInlineDecorator inlineDecorator = inlineDecorators[i];
                    float width = inlineDecorator.GetWidth();
                    if (inlineDecorator.GetSide() == InlineDecoratorSide.Right)
                    {
                        totalPosition.width -= width;
                        Rect rect = new Rect(totalPosition.xMax, totalPosition.y, width, elementHeight);
                        inlineDecorator.OnGUI(rect);
                    }
                    else if (inlineDecorator.GetSide() == InlineDecoratorSide.Left)
                    {
                        Rect rect = new Rect(totalPosition.x + EditorGUIUtility.labelWidth, totalPosition.y, width, elementHeight);
                        EditorGUIUtility.labelWidth += width;
                        inlineDecorator.OnGUI(rect);
                    }
                }
            }

            return totalPosition;
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
        /// Check element field attributes and apply specified callbacks.
        /// </summary>
        private void RegisterCallbacks()
        {
            Type type = serializedObject.targetObject.GetType();
            BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static;


            OnChangedCallbackAttribute onChangedCallbackAttribute = GetAttribute<OnChangedCallbackAttribute>();
            if (onChangedCallbackAttribute != null)
            {
                onChangedCallback = type.GetAllMembers(onChangedCallbackAttribute.method, flags)
                    .Where(m => m is MethodInfo method &&
                                ((method.GetParameters().Length == 1 && method.GetParameters()[0].ParameterType == typeof(SerializedProperty)) ||
                                 method.GetParameters().Length == 0))
                    .FirstOrDefault() as MethodInfo;

                if (onChangedCallback.GetParameters().Length > 0)
                {
                    callbackParameter = new object[1] {serializedProperty.Copy()};
                }
            }
        }

        /// <summary>
        /// serialized field is generic array.
        /// </summary>
        public bool IsGenericArray() { return serializedProperty.isArray && serializedProperty.propertyType == SerializedPropertyType.Generic; }

        private IEnumerable<SerializedProperty> GetVisibleChildren()
        {
            SerializedProperty currentProperty = serializedProperty.Copy();
            SerializedProperty nextSiblingProperty = serializedProperty.Copy();
            {
                nextSiblingProperty.NextVisible(false);
            }

            if (currentProperty.NextVisible(true))
            {
                do
                {
                    if (SerializedProperty.EqualContents(currentProperty, nextSiblingProperty))
                        break;

                    yield return currentProperty;
                } while (currentProperty.NextVisible(false));
            }
        }

        #region [Overloading Type Conversion Operations]

        public static implicit operator SerializedField(SerializedProperty property) { return new SerializedField(property.serializedObject, property.propertyPath); }

        public static explicit operator SerializedProperty(SerializedField element) { return element.serializedProperty; }

        #endregion

        #region [Serialized Property Method Wrapper]

        public void SetFloat(float value)
        {
            serializedProperty.floatValue = value;
            serializedObject.ApplyModifiedProperties();
        }

        public void SetInteger(int value)
        {
            serializedProperty.intValue = value;
            serializedObject.ApplyModifiedProperties();
        }

        public int GetInteger() { return serializedProperty.intValue; }

        public void SetString(string value)
        {
            serializedProperty.stringValue = value;
            serializedObject.ApplyModifiedProperties();
        }

        public string GetString() { return serializedProperty.stringValue; }

        public void SetBool(bool value)
        {
            serializedProperty.boolValue = value;
            serializedObject.ApplyModifiedProperties();
        }

        public void SetObject(Object value)
        {
            serializedProperty.objectReferenceValue = value;
            serializedObject.ApplyModifiedProperties();
        }

        public Object GetObject() { return serializedProperty.objectReferenceValue; }

        public void SetObjectInstanceID(int value)
        {
            serializedProperty.objectReferenceInstanceIDValue = value;
            serializedObject.ApplyModifiedProperties();
        }

        public void SetManagedReference(object value)
        {
            serializedProperty.managedReferenceValue = value;
            serializedObject.ApplyModifiedProperties();
            ApplyNestedProperties();
        }

        public void CreateManagedReference(Type type) { SetManagedReference(Activator.CreateInstance(type)); }

        public void CreateManagedReference<T>() { CreateManagedReference(typeof(T)); }

        public Type GetManagedReferenceType()
        {
            string[] baseTypeAndAssemblyName = serializedProperty.managedReferenceFieldTypename.Split(' ');
            string baseTypeString = string.Format("{0}, {1}", baseTypeAndAssemblyName[1], baseTypeAndAssemblyName[0]);
            return Type.GetType(baseTypeString);
        }

        public void SetColor(Color value)
        {
            serializedProperty.colorValue = value;
            serializedObject.ApplyModifiedProperties();
        }

        public void SetAnimationCurve(AnimationCurve value)
        {
            serializedProperty.animationCurveValue = value;
            serializedObject.ApplyModifiedProperties();
        }

        public void SetEnum<T>(T value) where T : Enum
        {
            for (int i = 0; i < serializedProperty.enumNames.Length; i++)
            {
                if (serializedProperty.enumNames[i] == value.ToString())
                {
                    serializedProperty.enumValueIndex = i;
                    serializedObject.ApplyModifiedProperties();
                    break;
                }
            }
        }

        public void SetEnum(int valueIndex)
        {
            serializedProperty.enumValueIndex = valueIndex;
            serializedObject.ApplyModifiedProperties();
        }

        public string GetEnumValue() { return serializedProperty.enumDisplayNames[serializedProperty.enumValueIndex]; }

        public string GetEnumValue(int valueIndex) { return serializedProperty.enumDisplayNames[valueIndex]; }

        public int GetEnumValueCount() { return serializedProperty.enumDisplayNames.Length; }

        public void SetQuaternion(Quaternion value)
        {
            serializedProperty.quaternionValue = value;
            serializedObject.ApplyModifiedProperties();
        }

        public void SetLayerMask(string value)
        {
            serializedProperty.intValue = LayerMask.NameToLayer(value);
            serializedObject.ApplyModifiedProperties();
        }

        public void SetLayerMask(int value)
        {
            serializedProperty.intValue = 1 << value;
            serializedObject.ApplyModifiedProperties();
        }

        public void SetVector2(Vector2 value)
        {
            serializedProperty.vector2Value = value;
            serializedObject.ApplyModifiedProperties();
        }

        public Vector2 GetVector2() { return serializedProperty.vector2Value; }

        public void SetVector2Int(Vector2Int value)
        {
            serializedProperty.vector2IntValue = value;
            serializedObject.ApplyModifiedProperties();
        }

        public Vector2Int GetVector2Int() { return serializedProperty.vector2IntValue; }

        public void SetVector3(Vector3 value)
        {
            serializedProperty.vector3Value = value;
            serializedObject.ApplyModifiedProperties();
        }

        public void SetVector3Int(Vector3Int value)
        {
            serializedProperty.vector3IntValue = value;
            serializedObject.ApplyModifiedProperties();
        }

        public void SetRect(Rect value)
        {
            serializedProperty.rectValue = value;
            serializedObject.ApplyModifiedProperties();
        }

        public void SetRectInt(RectInt value)
        {
            serializedProperty.rectIntValue = value;
            serializedObject.ApplyModifiedProperties();
        }

        public void SetBounds(Bounds value)
        {
            serializedProperty.boundsValue = value;
            serializedObject.ApplyModifiedProperties();
        }

        public void SetBoundsInt(BoundsInt value)
        {
            serializedProperty.boundsIntValue = value;
            serializedObject.ApplyModifiedProperties();
        }

        public void ResizeArray(int value)
        {
            serializedProperty.arraySize = value;
            serializedObject.ApplyModifiedProperties();
            ApplyNestedProperties();
        }

        public void IncreaseArraySize()
        {
            serializedProperty.arraySize++;
            serializedObject.ApplyModifiedProperties();
            ApplyNestedProperties();
        }

        public void InsertArrayElement(int index)
        {
            serializedProperty.InsertArrayElementAtIndex(index);
            serializedObject.ApplyModifiedProperties();
            ApplyNestedProperties();
        }

        public int GetArrayLength() { return serializedProperty.arraySize; }

        public void RemoveArrayElement(int index)
        {
            serializedProperty.DeleteArrayElementAtIndex(index);
            serializedObject.ApplyModifiedProperties();
            ApplyNestedProperties();
        }

        public SerializedField GetArrayElement(int index) { return children[++index]; }

        public bool IsExpanded() { return serializedProperty.isExpanded; }

        public void IsExpanded(bool value) { serializedProperty.isExpanded = value; }

        #endregion

        #region [IEnumerable Implementation]

        public IEnumerator<SerializedField> GetEnumerator() { return children.GetEnumerator(); }

        IEnumerator IEnumerable.GetEnumerator() { return GetEnumerator(); }

        #endregion

        #region [Getter / Setter]

        public SerializedField GetChild(int index) { return children[index]; }

        public int GetChildrenCount() { return children.Count; }

        #endregion
    }
}