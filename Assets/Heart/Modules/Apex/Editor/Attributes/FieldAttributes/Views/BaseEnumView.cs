using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Pancake.Apex;
using Pancake.ExLib.Reflection;
using UnityEditor;
using UnityEngine;
using Vexe.Runtime.Extensions;
using Object = UnityEngine.Object;

namespace Pancake.ApexEditor
{
/// <summary>
    /// Base view for enum field type, contains miscellaneous helper stuff for operation with enum field.
    /// </summary>
    public abstract class BaseEnumView : FieldView, ITypeValidationCallback
    {
        private struct EnumInfo
        {
            public readonly long bit;
            public readonly string rawName;
            public readonly GUIContent content;

            public EnumInfo(long bit, string rawName, GUIContent content)
            {
                this.bit = bit;
                this.rawName = rawName;
                this.content = content;
            }
        }

        protected struct MenuItem
        {
            public readonly GUIContent content;
            public readonly long bit;
            public readonly bool isOn;

            public MenuItem(GUIContent content, long bit, bool isOn)
            {
                this.content = content;
                this.bit = bit;
                this.isOn = isOn;
            }
        }

        private const char SEPARATOR = ',';
        private const string EMPTY_NAME = "Empty";
        private const string NOTHING_NAME = "Nothing";
        private const string EVERYTHING_NAME = "Everything";

        private static readonly Dictionary<Type, List<EnumInfo>> Enums;

        private bool flagsMode;
        private bool emptyEnum;
        private Type enumType;
        private BaseEnumAttribute attribute;

        private object target;
        private SerializedObject serializedObject;
        private MemberGetter<object, object> getter;
        private MemberSetter<object, object> setter;
        private MethodCaller<object, IEnumerable> methodCallback;
        private MemberGetter<object, IEnumerable> memberCallback;

        /// <summary>
        /// Static constructor.
        /// </summary>
        static BaseEnumView()
        {
            Enums = new Dictionary<Type, List<EnumInfo>>();
        }

        /// <summary>
        /// Called once when initializing PropertyView.
        /// </summary>
        /// <param name="field">Serialized element with ViewAttribute.</param>
        /// <param name="viewAttribute">ViewAttribute of serialized element.</param>
        /// <param name="label">Label of serialized element.</param>
        public override void Initialize(SerializedField field, ViewAttribute viewAttribute, GUIContent label)
        {
            attribute = viewAttribute as BaseEnumAttribute;
            if(attribute == null)
            {
                throw new Exception("Base Enum View must be used with [BaseEnum] attribute implementation!");
            }

            serializedObject = field.GetSerializedObject();

            target = field.GetDeclaringObject();

            FieldInfo fieldInfo = (FieldInfo)field.GetMemberInfo();
            enumType = fieldInfo.FieldType;
            getter = fieldInfo.DelegateForGet();
            setter = fieldInfo.DelegateForSet();
            flagsMode = attribute.UseAsFlags || fieldInfo.FieldType.GetCustomAttribute<FlagsAttribute>() != null;

            if (!string.IsNullOrEmpty(attribute.HideValues))
            {
                FindCallback(attribute.HideValues, field.GetDeclaringObject().GetType());
            }

            RegisterEnum(enumType);
            if (Enums.TryGetValue(enumType, out List<EnumInfo> enums))
            {
                emptyEnum = enums.Count == 0;
            }
        }

        /// <summary>
        /// Return true if this property valid the using with this attribute.
        /// If return false, this property attribute will be ignored.
        /// </summary>
        /// <param name="property">Reference of serialized property.</param>
        public bool IsValidProperty(SerializedProperty property)
        {
            return property.propertyType == SerializedPropertyType.Enum;
        }

        /// <summary>
        /// Current enum value name.
        /// </summary>
        /// <param name="fieldWidth">Set current width of field to automatically shorten the name if it does not fit into the size of the field.</param>
        /// <returns>Return current enum value name in string representation.</returns>
        public string GetValueName(float? fieldWidth = null)
        {
            if (emptyEnum)
            {
                return flagsMode ? EVERYTHING_NAME : EMPTY_NAME;
            }

            const string MIXED = "Mixed...";

            object enumValue = getter.Invoke(target);
            if (Enums.TryGetValue(enumType, out List<EnumInfo> enums))
            {
                long currentBit = Convert.ToInt64(enumValue);
                if (flagsMode)
                {
                    if (currentBit == 0)
                    {
                        EnumInfo enumInfo = enums[0];
                        if (enumInfo.bit == 0)
                        {
                            return enumInfo.content.text;
                        }
                        return NOTHING_NAME;
                    }
                    else if (currentBit == -1)
                    {
                        EnumInfo enumInfo = enums[enums.Count - 1];
                        if (enumInfo.bit == -1)
                        {
                            return enumInfo.content.text;
                        }
                        return EVERYTHING_NAME;
                    }

                    StringBuilder stringBuilder = new StringBuilder();

                    if (attribute.ShortNaming)
                    {
                        int startIndex = 0;
                        ReadOnlySpan<char> enumSpan = enumValue.ToString().AsSpan();

                        for (int i = 0; i < enumSpan.Length; i++)
                        {
                            if (enumSpan[i] == SEPARATOR)
                            {
                                if (i > startIndex)
                                {
                                    string slice = enumSpan.Slice(startIndex, i - startIndex).ToString();
                                    if (TryGetContent(slice, out GUIContent content))
                                    {
                                        stringBuilder.Append(content.text);
                                        stringBuilder.Append(attribute.FlagSeparator);
                                        startIndex = i + 2;
                                    }
                                }
                            }
                        }

                        if (startIndex < enumSpan.Length)
                        {
                            string slice = enumSpan.Slice(startIndex).ToString();
                            if (TryGetContent(slice, out GUIContent content))
                            {
                                stringBuilder.Append(content.text);
                            }
                        }
                    }
                    else
                    {
                        long maxBit = GetMaxBit();
                        long maxAvailableBit = GetMaxAvailable(maxBit);
                        for (int i = 0; i < enums.Count; i++)
                        {
                            EnumInfo enumInfo = enums[i];

                            bool isSelected = currentBit == enumInfo.bit;
                            if (!isSelected && flagsMode && currentBit != 0 && enumInfo.bit != 0)
                            {
                                if (currentBit == -1 || (maxAvailableBit == maxBit && currentBit == maxBit))
                                {
                                    isSelected = true;
                                }
                                else
                                {
                                    isSelected = IsNestedFlag(currentBit, enumInfo.bit) && enumInfo.bit <= currentBit;
                                }
                            }

                            if (isSelected)
                            {
                                if (!attribute.ShowPathAlways)
                                {
                                    stringBuilder.Append(Path.GetFileName(enumInfo.content.text));
                                }
                                else
                                {
                                    stringBuilder.Append(enumInfo.content.text);
                                }
                                stringBuilder.Append(attribute.FlagSeparator);
                            }
                        }

                        if (stringBuilder.Length > 0)
                        {
                            stringBuilder.Length -= attribute.FlagSeparator.Length;
                        }
                    }

                    string sbText = stringBuilder.ToString();
                    if (fieldWidth.HasValue && EditorStyles.popup.CalcSize(new GUIContent(sbText)).x > fieldWidth.Value)
                    {
                        return MIXED;
                    }

                    return sbText;
                }
                else
                {
                    for (int i = 0; i < enums.Count; i++)
                    {
                        EnumInfo enumInfo = enums[i];
                        if (enumInfo.bit == currentBit)
                        {
                            if (fieldWidth.HasValue && enumInfo.content.text.Length > fieldWidth.Value)
                            {
                                return MIXED;
                            }

                            return enumInfo.content.text;
                        }
                    }
                }
            }

            string valueName = enumValue.ToString();
            if (fieldWidth.HasValue && EditorStyles.popup.CalcSize(new GUIContent(valueName)).x > fieldWidth.Value)
            {
                return MIXED;
            }

            return valueName;
        }

        /// <summary>
        /// Try get content of specified enum by bit value.
        /// </summary>
        /// <param name="bit">Enum bit value.</param>
        /// <param name="content">Output reference of enum content.</param>
        /// <returns>True if content found, otherwise false.</returns>
        public bool TryGetContent(long bit, out GUIContent content)
        {
            if (Enums.TryGetValue(enumType, out List<EnumInfo> enums))
            {
                for (int i = 0; i < enums.Count; i++)
                {
                    EnumInfo enumInfo = enums[i];
                    if (bit == enumInfo.bit)
                    {
                        content = enumInfo.content;
                        if (!attribute.ShowPathAlways)
                        {
                            content.text = Path.GetFileName(content.text);
                        }

                        return true;
                    }
                }
            }
            content = GUIContent.none;
            return false;
        }

        /// <summary>
        /// Try get content of specified enum by enum raw name.
        /// </summary>
        /// <param name="bit">Raw enum name.</param>
        /// <param name="content">Output reference of enum content.</param>
        /// <returns>True if content found, otherwise false.</returns>
        public bool TryGetContent(string enumName, out GUIContent content)
        {
            if (Enums.TryGetValue(enumType, out List<EnumInfo> enums))
            {
                for (int i = 0; i < enums.Count; i++)
                {
                    EnumInfo enumInfo = enums[i];
                    if (enumName == enumInfo.rawName)
                    {
                        content = enumInfo.content;
                        if (!attribute.ShowPathAlways)
                        {
                            content.text = Path.GetFileName(content.text);
                        }

                        return true;
                    }
                }
            }
            content = GUIContent.none;
            return false;
        }

        /// <summary>
        /// Current enum bit value.
        /// </summary>
        /// <returns>Enum bit value in long representation.</returns>
        public long GetCurrentBit()
        {
            return Convert.ToInt64(getter.Invoke(target));
        }

        /// <summary>
        /// Max bit of current enum.
        /// </summary>
        /// <returns>Enum bit value in long representation.</returns>
        public long GetMaxBit()
        {
            long maxBit = 0;
            Array array = Enum.GetValues(enumType);
            foreach (object value in array)
            {
                long bit = Convert.ToInt64(value);
                if(bit == 0 || bit == -1)
                {
                    continue;
                }

                maxBit |= bit;
            }
            
            return maxBit;
        }

        /// <summary>
        /// Get max available bit, by excepting hide bit values from max bit.
        /// </summary>
        /// <param name="maxBit">Max bit of enum in long representation.</param>
        public long GetMaxAvailable(long maxBit)
        {
            IEnumerable hideValues = GetHideValues();
            if (hideValues != null)
            {
                foreach (object hideValue in hideValues)
                {
                    long hideBit = Convert.ToInt64(hideValue);

                    if (hideBit == 0 || hideBit == -1)
                    {
                        continue;
                    }

                    maxBit &= ~hideBit;
                }
            }
            return maxBit;
        }

        /// <summary>
        /// Loop through all hide values.
        /// </summary>
        public IEnumerable GetHideValues()
        {
            if(methodCallback != null)
            {
                return methodCallback.Invoke(target, null);
            }
            else if(memberCallback != null)
            {
                return memberCallback.Invoke(target);
            }
            return null;
        }

        /// <summary>
        /// Check if has hide value, which can be iterated.
        /// </summary>
        public bool HasHideValues()
        {
            return !string.IsNullOrEmpty(attribute.HideValues) && (memberCallback != null || methodCallback != null);
        }

        /// <summary>
        /// Change current enum value to the new value.
        /// </summary>
        /// <param name="bit">Enum bit value.</param>
        public void SetEnum(long bit)
        {
            if (flagsMode)
            {
                long maxBit = GetMaxBit();
                long maxAvailableBit = GetMaxAvailable(maxBit);
                if (bit == 0)
                {
                    setter.Invoke(ref target, Enum.ToObject(enumType, bit));
                }
                else if (bit == -1)
                {
                    if (HasHideValues())
                    {
                        bit = maxBit;
                    }

                    setter.Invoke(ref target, Enum.ToObject(enumType, bit));
                }
                else
                {
                    long newValue = 0;
                    long currentBit = GetCurrentBit();
                    if (IsNestedFlag(currentBit, bit))
                    {
                        if (currentBit == -1)
                        {
                            newValue = maxBit & ~bit;
                        }
                        else
                        {
                            newValue = currentBit & ~bit;
                        }

                        if (HasHideValues())
                        {
                            IEnumerable hideValues = GetHideValues();
                            foreach (object hideValue in hideValues)
                            {
                                if(newValue == Convert.ToInt64(hideValue))
                                {
                                    return;
                                }
                            }
                        }
                    }
                    else
                    {
                        newValue = currentBit == -1 ? bit : currentBit | bit;
                    }

                    if (newValue == maxBit && maxBit == maxAvailableBit)
                    {
                        newValue = -1;
                    }
                    else if (newValue < 0)
                    {
                        newValue = 0;
                    }

                    setter.Invoke(ref target, Enum.ToObject(enumType, newValue));
                }
            }
            else
            {
                long numericValue = Convert.ToInt64(getter.Invoke(target));
                setter.Invoke(ref target, Enum.ToObject(enumType, bit));
            }
        }

        /// <summary>
        /// Check that specified enum nested.
        /// </summary>
        /// <param name="target">Enum where need check.</param>
        /// <param name="check">Enum what need check.</param>
        public bool IsNestedFlag(long target, long check)
        {
            return (target & check) == check;
        }

        /// <summary>
        /// Check that the enumeration is empty (does not define any values).
        /// </summary>
        public bool IsEmpty()
        {
            return emptyEnum;
        }

        /// <summary>
        /// Check that current enum, used as flag.
        /// </summary>
        /// <returns></returns>
        public bool FlagMode()
        {
            return flagsMode;
        }

        /// <summary>
        /// System type of Enum.
        /// </summary>
        public Type GetEnumType()
        {
            return enumType;
        }

        /// <summary>
        /// SetEnum(long bit) shortcut, to use it in menu contexts.
        /// </summary>
        /// <param name="data">Enum bit value in long representation.</param>
        protected void SetEnumFunction(object data)
        {
            SetEnum(Convert.ToInt64(data));
            serializedObject.Update();
        }

        /// <summary>
        /// Register current enum in Apex enum storage.
        /// </summary>
        /// <param name="enumType">System type of enum.</param>
        private void RegisterEnum(Type enumType)
        {
            if (!Enums.TryGetValue(enumType, out List<EnumInfo> enums))
            {
                List<FieldInfo> enumFields = enumType.GetFields().ToList();
                for (int i = 0; i < enumFields.Count; i++)
                {
                    if (!enumFields[i].FieldType.IsEnum)
                    {
                        enumFields.RemoveAt(i);
                        i--;
                    }
                }

                enums = new List<EnumInfo>(enumFields.Count);

                for (int i = 0; i < enumFields.Count; i++)
                {
                    FieldInfo fieldInfo = enumFields[i];
                    GUIContent content = new GUIContent();
                    object value = fieldInfo.GetValue(target);
                    object[] attributes = fieldInfo.GetCustomAttributes(typeof(SearchContent), false);
                    if (attributes.Length > 0)
                    {
                        SearchContent attribute = attributes[0] as SearchContent;
                        if (attribute != null)
                        {
                            content = new GUIContent(attribute.name, attribute.Tooltip);
                            if (SearchContentUtility.TryLoadContentImage(attribute.Image, out Texture2D icon))
                            {
                                content.image = icon;
                            }
                        }
                    }
                    else
                    {
                        content.text = ObjectNames.NicifyVariableName(value.ToString());
                    }
                    enums.Add(new EnumInfo(Convert.ToInt64(value), value.ToString(), content));
                }

                enums.TrimExcess();
                Enums.Add(enumType, enums);
            }
        }

        /// <summary>
        /// Try find used callback method to dynamically hide specified values.
        /// </summary>
        /// <param name="name">Name of callback.</param>
        /// <param name="type">Type where search callback.</param>
        /// <param name="callback">Output reference of callback.</param>
        /// <returns>True if callback found, otherwise false.</returns>
        private void FindCallback(string name, Type type)
        {
            Type limitDescendant = target is MonoBehaviour ? typeof(MonoBehaviour) : typeof(Object);
            foreach (MemberInfo memberInfo in type.AllMembers(limitDescendant))
            {
                if(memberInfo.Name == name)
                {
                    if (memberInfo is FieldInfo fieldInfo && typeof(IEnumerable).IsAssignableFrom(fieldInfo.FieldType))
                    {
                        memberCallback = fieldInfo.DelegateForGet<object, IEnumerable>();
                        break;
                    }
                    else if (memberInfo is PropertyInfo propertyInfo && propertyInfo.CanRead && typeof(IEnumerable).IsAssignableFrom(propertyInfo.PropertyType))
                    {
                        memberCallback = propertyInfo.DelegateForGet<object, IEnumerable>();
                        break;
                    }
                    else if (memberInfo is MethodInfo methodInfo && typeof(IEnumerable).IsAssignableFrom(methodInfo.ReturnType))
                    {
                        methodCallback = methodInfo.DelegateForCall<object, IEnumerable>();
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// List of enum items, ready to use it in menu contexts
        /// </summary>
        /// <returns></returns>
        protected List<MenuItem> GetMenuItems()
        {
            bool hasNone = false;
            bool hasAll = false;
            long currentBit = GetCurrentBit();
            long maxBit = GetMaxBit();
            long maxAvailableBit = GetMaxAvailable(maxBit);
            List<MenuItem> items = new List<MenuItem>();
            IEnumerable hideValues = GetHideValues();
            if (Enums.TryGetValue(enumType, out List<EnumInfo> enums))
            {
                items.Capacity = enums.Count;
                for (int i = 0; i < enums.Count; i++)
                {
                    EnumInfo enumInfo = enums[i];

                    if (enumInfo.bit == 0)
                    {
                        hasNone = true;
                    }
                    else if (enumInfo.bit == -1)
                    {
                        hasAll = true;
                    }

                    bool skip = false;
                    if(hideValues != null)
                    {
                        foreach (object hideValue in hideValues)
                        {
                            long hideBit = Convert.ToInt64(hideValue);
                            if (enumInfo.bit == hideBit)
                            {
                                skip = true;
                                break;
                            }
                        }
                    }

                    if (skip)
                    {
                        continue;
                    }

                    bool on = currentBit == enumInfo.bit;
                    if (!on && flagsMode && currentBit != 0 && enumInfo.bit != 0)
                    {
                        if (currentBit == -1 || (maxBit == maxAvailableBit && currentBit == maxBit))
                        {
                            on = true;
                        }
                        else
                        {
                            on = IsNestedFlag(currentBit, enumInfo.bit) && enumInfo.bit <= currentBit;
                        }
                    }

                    GUIContent content = new GUIContent(enumInfo.content);
                    if (!attribute.AllowPaths)
                    {
                        content.text = Path.GetFileName(content.text);
                    }

                    if(enumInfo.bit == 0 && i != 0)
                    {
                        items.Insert(0, new MenuItem(content, enumInfo.bit, on));
                    }
                    else if(enumInfo.bit == -1)
                    {
                        int index = attribute.UnityOrder ? 1 : items.Count - 1;
                        items.Insert(index, new MenuItem(content, enumInfo.bit, on));
                    }
                    else
                    {
                        items.Add(new MenuItem(content, enumInfo.bit, on));
                    }

                }
            }

            if (flagsMode && attribute.ShortcutBits)
            {
                if (!hasNone)
                {
                    items.Insert(0, new MenuItem(new GUIContent(NOTHING_NAME), 0, currentBit == 0));
                }
                
                if (!hasAll)
                {
                    int index = attribute.UnityOrder ? 1 : items.Count - 1;
                    items.Insert(index, new MenuItem(new GUIContent(EVERYTHING_NAME), -1, currentBit == -1 || currentBit == maxBit));
                }
            }

            items.TrimExcess();
            return items;
        }
    }
}