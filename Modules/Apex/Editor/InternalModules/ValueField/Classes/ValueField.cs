using System.Reflection;
using System;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Pancake.ApexEditor
{
    public sealed class ValueField<T> : VisualEntity, IValueField<T>
    {
        internal enum ValueType
        {
            Undefined = -1,

            //
            // Summary:
            //     Integer property, for example int, byte, short, uint and long.
            Int = 0,

            //
            // Summary:
            //     Boolean property.
            Bool = 1,

            //
            // Summary:
            //     Float or Double property.
            Float = 2,

            //
            // Summary:
            //     String property.
            String = 3,

            //
            // Summary:
            //     Color property.
            Color = 4,

            //
            // Summary:
            //     Reference to an object that derives from UnityEngine.Object.
            Object = 5,

            //
            // Summary:
            //     LayerMask property.
            LayerMask = 6,

            //
            // Summary:
            //     Enumeration property.
            Enum = 7,

            //
            // Summary:
            //     2D vector property.
            Vector2 = 8,

            //
            // Summary:
            //     3D vector property.
            Vector3 = 9,

            //
            // Summary:
            //     4D vector property.
            Vector4 = 10,

            //
            // Summary:
            //     Rectangle property.
            Rect = 11,

            //
            // Summary:
            //     Bounds property.
            Bounds = 12,

            //
            // Summary:
            //     Quaternion property.
            Quaternion = 13,

            //
            // Summary:
            //     2D integer vector property.
            Vector2Int = 14,

            //
            // Summary:
            //     3D integer vector property.
            Vector3Int = 15,

            //
            // Summary:
            //     Rectangle with Integer values property.
            RectInt = 16,

            //
            // Summary:
            //     Bounds with Integer values property.
            BoundsInt = 17,
        }

        private T value;
        private ValueType valueType;
        private bool isEnumFlags;

        // Stored required properties.
        private GUIContent helpBoxContent;
        private float width;

        public ValueField(string name)
            : base(name)
        {
            valueType = GetValueType(typeof(T));

            if (valueType != ValueType.Undefined)
            {
                isEnumFlags = valueType == ValueType.Enum && typeof(T).GetCustomAttributes<FlagsAttribute>() != null;
                helpBoxContent = GUIContent.none;
            }
            else
            {
                helpBoxContent = new GUIContent(
                    $"ValueField does not support drawing custom types like {typeof(T).Name}. Implement new visual entity to draw your custom types.");
            }
        }

        public ValueField(string name, T value)
            : this(name)
        {
            this.value = value;
        }

        /// <summary>
        /// Called to draw field for editing the value suitable for the specified value of type.
        /// </summary>
        /// <param name="position">Rectangle position to draw field.</param>
        /// <param name="objValue">Output object value passed by reference.</param>
        private void OnFieldGUI(Rect position, out object objValue)
        {
            objValue = value;
            switch (valueType)
            {
                case ValueType.Undefined:
                    EditorGUI.HelpBox(position, helpBoxContent.text, MessageType.Error);
                    break;
                case ValueType.Int:
                    objValue = EditorGUI.IntField(position, GetName(), (int) objValue);
                    break;
                case ValueType.Bool:
                    objValue = EditorGUI.Toggle(position, GetName(), (bool) objValue);
                    break;
                case ValueType.Float:
                    objValue = EditorGUI.FloatField(position, GetName(), (float) objValue);
                    break;
                case ValueType.String:
                    objValue = EditorGUI.TextField(position, GetName(), (string) objValue);
                    break;
                case ValueType.Color:
                    objValue = EditorGUI.ColorField(position, GetName(), (Color) objValue);
                    break;
                case ValueType.Object:
                    objValue = EditorGUI.ObjectField(position,
                        GetName(),
                        (Object) objValue,
                        typeof(T),
                        true);
                    break;
                case ValueType.LayerMask:
                    objValue = EditorGUI.LayerField(position, GetName(), (LayerMask) objValue);
                    break;
                case ValueType.Enum:
                    if (isEnumFlags)
                        objValue = EditorGUI.EnumFlagsField(position, GetName(), (Enum) objValue);
                    else
                        objValue = EditorGUI.EnumPopup(position, GetName(), (Enum) objValue);
                    break;
                case ValueType.Vector2:
                    objValue = EditorGUI.Vector2Field(position, GetName(), (Vector2) objValue);
                    break;
                case ValueType.Vector3:
                    objValue = EditorGUI.Vector3Field(position, GetName(), (Vector3) objValue);
                    break;
                case ValueType.Vector4:
                    objValue = EditorGUI.Vector4Field(position, GetName(), (Vector4) objValue);
                    break;
                case ValueType.Rect:
                    objValue = EditorGUI.RectField(position, GetName(), (Rect) objValue);
                    break;
                case ValueType.Bounds:
                    objValue = EditorGUI.BoundsField(position, GetName(), (Bounds) objValue);
                    break;
                case ValueType.Quaternion:
                    Quaternion rot = (Quaternion) objValue;
                    Vector4 vector4 = new Vector4(rot.x, rot.y, rot.z, rot.w);
                    vector4 = EditorGUI.Vector4Field(position, GetName(), vector4);
                    objValue = vector4;
                    break;
                case ValueType.Vector2Int:
                    objValue = EditorGUI.Vector2IntField(position, GetName(), (Vector2Int) objValue);
                    break;
                case ValueType.Vector3Int:
                    objValue = EditorGUI.Vector3IntField(position, GetName(), (Vector3Int) objValue);
                    break;
                case ValueType.RectInt:
                    objValue = EditorGUI.RectIntField(position, GetName(), (RectInt) objValue);
                    break;
                case ValueType.BoundsInt:
                    objValue = EditorGUI.BoundsIntField(position, GetName(), (BoundsInt) objValue);
                    break;
            }
        }

        #region [VisualEntity Implementation]

        /// <summary>
        /// Called for rendering and handling visual entity.
        /// </summary>
        /// <param name="position">Rectangle position.</param>
        public override void OnGUI(Rect position)
        {
            width = position.width;

            EditorGUI.BeginChangeCheck();
            OnFieldGUI(position, out object objValue);
            if (EditorGUI.EndChangeCheck())
            {
                value = (T) objValue;
            }
        }

        /// <summary>
        /// Total height required to drawing float field.
        /// </summary>
        public override float GetHeight()
        {
            switch (valueType)
            {
                default:
                case ValueType.Int:
                case ValueType.Bool:
                case ValueType.Float:
                case ValueType.String:
                case ValueType.Color:
                case ValueType.Object:
                case ValueType.LayerMask:
                case ValueType.Enum:
                    return EditorGUIUtility.singleLineHeight;
                case ValueType.Vector2:
                case ValueType.Vector2Int:
                    return width <= 294 ? 38 : EditorGUIUtility.singleLineHeight;
                case ValueType.Vector3:
                case ValueType.Vector3Int:
                    return width <= 290 ? 38 : EditorGUIUtility.singleLineHeight;
                case ValueType.Vector4:
                case ValueType.Quaternion:
                    return width <= 292 ? 38 : EditorGUIUtility.singleLineHeight;
                case ValueType.Rect:
                case ValueType.RectInt:
                    return width <= 292 ? 58 : 38;
                case ValueType.Bounds:
                case ValueType.BoundsInt:
                    return 58;
                case ValueType.Undefined:
                    return EditorStyles.helpBox.CalcHeight(helpBoxContent, width);
            }
        }

        #endregion

        #region [Static Members]

        internal static ValueType GetValueType(Type value)
        {
            if (value == typeof(int))
                return ValueType.Int;
            else if (value == typeof(bool))
                return ValueType.Bool;
            else if (value == typeof(float))
                return ValueType.Float;
            else if (value == typeof(string))
                return ValueType.String;
            else if (value == typeof(Color))
                return ValueType.Color;
            else if (value == typeof(Object) || value.IsSubclassOf(typeof(Object)))
                return ValueType.Object;
            else if (value == typeof(LayerMask))
                return ValueType.LayerMask;
            else if (value.IsEnum)
                return ValueType.Enum;
            else if (value == typeof(Vector2))
                return ValueType.Vector2;
            else if (value == typeof(Vector3))
                return ValueType.Vector3;
            else if (value == typeof(Vector4))
                return ValueType.Vector4;
            else if (value == typeof(Rect))
                return ValueType.Rect;
            else if (value == typeof(Bounds))
                return ValueType.Bounds;
            else if (value == typeof(Quaternion))
                return ValueType.Quaternion;
            else if (value == typeof(Vector2Int))
                return ValueType.Vector2Int;
            else if (value == typeof(Vector3Int))
                return ValueType.Vector3Int;
            else if (value == typeof(RectInt))
                return ValueType.RectInt;
            else if (value == typeof(BoundsInt))
                return ValueType.BoundsInt;
            else
                return ValueType.Undefined;
        }

        #endregion

        #region [Getter / Setter]

        public T GetValue() { return value; }

        public void SetValue(T value) { this.value = value; }

        #endregion
    }
}