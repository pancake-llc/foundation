using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace Pancake.Editor
{
    public abstract class SerializedMember : VisualEntity, ISerializedMember, IMemberInfo, IMemberTarget, IMemberType, IMemberLabel
    {
        private static readonly List<MemberManipulator> ManipulatorsNone;

        private Type type;
        private object target;
        private MemberInfo memberInfo;
        private SerializedObject serializedObject;
        private GUIContent label;
        private List<MemberManipulator> manipulators;

        /// <summary>
        /// Static constructor of serialized member.
        /// </summary>
        static SerializedMember() { ManipulatorsNone = new List<MemberManipulator>(); }

        /// <summary>
        /// Implement this constructor to make initializations.
        /// </summary>
        /// <param name="serializedObject">Serialized object reference of this serialized member.</param>
        /// <param name="memberPath">Member path of this serialized member.</param>
        public SerializedMember(SerializedObject serializedObject, string memberPath)
        {
            this.serializedObject = serializedObject;

            FindMemberInfo(serializedObject,
                memberPath,
                out memberInfo,
                out type,
                out target);

            label = new GUIContent(memberInfo?.Name ?? memberPath);

            manipulators = ManipulatorsNone;
            AddManipulators(GetAttributes<ManipulatorAttribute>());
        }

        #region [Abstract Methods]

        /// <summary>
        /// Implement this method to handle GUI of member.
        /// </summary>
        /// <param name="position">Rectangle position to draw member GUI.</param>
        protected abstract void OnMemberGUI(Rect position);

        /// <summary>
        /// Implement this method to calculate height of member.
        /// </summary>
        /// <param name="position">Rectangle position to draw member GUI.</param>
        protected abstract float GetMemberHeight();

        #endregion

        #region [ISerializedMember Implementation]

        /// <summary>
        /// Target serialized object reference of serialized member.
        /// </summary>
        public SerializedObject GetSerializedObject() { return serializedObject; }

        #endregion

        #region [IMemberInfo Implementation]

        /// <summary>
        /// Member info of serialized member.
        /// </summary>
        public MemberInfo GetMemberInfo() { return memberInfo; }

        #endregion

        #region [IMemberTarget Implementation]

        /// <summary>
        /// Target object of serialized member.
        /// </summary>
        public object GetMemberTarget() { return target; }

        #endregion

        #region [IMemberType Implementation]

        /// <summary>
        /// Type of serialized member.
        /// </summary>
        public Type GetMemberType() { return type; }

        #endregion

        /// <summary>
        /// Called for rendering and handling visual entity.
        /// </summary>
        /// <param name="position">Rectangle position.</param>
        public sealed override void OnGUI(Rect position)
        {
            ExecuteAllManipulatorsBeforeCallback();
            OnMemberGUI(position);
            ExecuteAllManipulatorsAfterCallback();
        }

        /// <summary>
        /// Height of serialized member.
        /// </summary>
        public sealed override float GetHeight()
        {
            if (IsVisible())
            {
                return GetMemberHeight();
            }

            return 0;
        }

        /// <summary>
        /// Add Manipulators to serialized member.
        /// </summary>
        /// <param name="attributes">Manipulator attributes.</param>
        public void AddManipulators(ManipulatorAttribute[] attributes)
        {
            if (attributes != null)
            {
                for (int i = 0; i < attributes.Length; i++)
                {
                    ManipulatorAttribute attribute = attributes[i];
                    if (FieldHelper.Manipulators.TryGetValue(attribute.GetType(), out MemberManipulator manipulator))
                    {
                        manipulator = Activator.CreateInstance(manipulator.GetType()) as MemberManipulator;
                        manipulator.Initialize(this, attribute);

                        if (manipulators == ManipulatorsNone)
                            manipulators = new List<MemberManipulator>();
                        manipulators.Add(manipulator);
                    }
                }
            }
            else
            {
                ClearManipulators();
            }
        }

        /// <summary>
        /// Add manipulator to serialized member.
        /// </summary>
        /// <param name="attribute">Manipulator attribute.</param>
        public void AddManipulator(ManipulatorAttribute attribute)
        {
            if (FieldHelper.Manipulators.TryGetValue(attribute.GetType(), out MemberManipulator manipulator))
            {
                manipulator = Activator.CreateInstance(manipulator.GetType()) as MemberManipulator;
                manipulator.Initialize(this, attribute);

                if (manipulators == ManipulatorsNone)
                    manipulators = new List<MemberManipulator>();
                manipulators.Add(manipulator);
            }
        }

        /// <summary>
        /// Remove all manipulator with same attribute type.
        /// </summary>
        /// <param name="attributes">Manipulator attribute type.</param>
        public void RemoveManipulators(ManipulatorAttribute attribute)
        {
            if (FieldHelper.Manipulators.TryGetValue(attribute.GetType(), out MemberManipulator manipulator))
            {
                List<int> indexes = new List<int>();

                for (int i = 0; i < manipulators.Count; i++)
                {
                    if (manipulators[i].GetType() == manipulator.GetType())
                    {
                        indexes.Add(i);
                    }
                }

                for (int i = 0; i < indexes.Count; i++)
                {
                    manipulators.RemoveAt(indexes[i]);
                }

                if (manipulators.Count == 0)
                {
                    manipulators = ManipulatorsNone;
                }
            }
        }

        /// <summary>
        /// Remove manipulator at index.
        /// </summary>
        /// <param name="index">Manipulator index.</param>
        public void RemoveManipulator(int index)
        {
            manipulators.RemoveAt(index);

            if (manipulators.Count == 0)
            {
                manipulators = ManipulatorsNone;
            }
        }

        /// <summary>
        /// Clear all Manipulators.
        /// </summary>
        public void ClearManipulators() { manipulators = ManipulatorsNone; }

        /// <summary>
        /// visual entity visibility state.
        /// </summary>
        public override bool IsVisible() { return VisibilityCallback?.Invoke() ?? true; }

        /// <summary>
        /// Get attached specified attribute.
        /// </summary>
        public T GetAttribute<T>() where T : PancakeAttribute
        {
            if (memberInfo != null)
            {
                T attribute = (T) Attribute.GetCustomAttribute(memberInfo, typeof(T));
                return attribute;
            }

            return null;
        }

        /// <summary>
        /// Get all attached specified attributes.
        /// </summary>
        public T[] GetAttributes<T>() where T : PancakeAttribute
        {
            if (memberInfo != null)
            {
                T[] attributes = (T[]) Attribute.GetCustomAttributes(memberInfo, typeof(T));
                return attributes;
            }

            return new T[0];
        }

        /// <summary>
        /// Execute all before gui callback which attached to this element.
        /// </summary>
        private void ExecuteAllManipulatorsBeforeCallback()
        {
            int count = manipulators.Count;
            if (count > 0)
            {
                for (int i = 0; i < count; i++)
                {
                    manipulators[i].OnBeforeGUI();
                }
            }
        }

        /// <summary>
        /// Execute all after gui callback which attached to this element.
        /// </summary>
        private void ExecuteAllManipulatorsAfterCallback()
        {
            int count = manipulators.Count;
            if (count > 0)
            {
                for (int i = 0; i < count; i++)
                {
                    manipulators[i].OnAfterGUI();
                }
            }
        }

        #region [Static Methods]

        /// <summary>
        /// Find member info of serialized member. 
        /// </summary>
        /// <param name="serializedObject">SerializedObject reference of this serialized member.</param>
        /// <param name="memberPath">Member path of this serialized member.</param>
        /// <param name="memberInfo">Output reference of member info.</param>
        /// <param name="type">Output reference of member type.</param>
        /// <param name="target">Output reference of member target object.</param>
        internal static void FindMemberInfo(SerializedObject serializedObject, string memberPath, out MemberInfo memberInfo, out Type type, out object target)
        {
            object localTarget = serializedObject.targetObject;
            Type localType = localTarget.GetType();
            MemberInfo localMemberInfo = null;

            string[] pathSplit = memberPath.Split('.');
            Queue<string> paths = new Queue<string>(pathSplit);

            void Search()
            {
                string memberName = paths.Dequeue();
                if (memberName.Contains("data["))
                {
                    string result = Regex.Match(memberName, @"\d+").Value;
                    int index = Convert.ToInt32(result);

                    int count = 0;
                    if (localMemberInfo is FieldInfo fieldInfo)
                    {
                        IEnumerable enumerable = fieldInfo.GetValue(localTarget) as IEnumerable;
                        foreach (object element in enumerable)
                        {
                            if (index == count)
                            {
                                localType = element.GetType();
                                localTarget = element;
                                if (paths.Count > 0)
                                {
                                    Search();
                                    break;
                                }
                            }

                            count++;
                        }
                    }
                }


                MemberInfo member = null;
                foreach (MemberInfo item in localType.AllMembers())
                {
                    if (item.Name == memberName)
                    {
                        member = item;
                        break;
                    }
                }

                if (member != null)
                {
                    localMemberInfo = member;

                    if (localMemberInfo is FieldInfo fieldInfo)
                    {
                        localType = fieldInfo.FieldType;
                        if (!localType.IsArray && paths.Count > 0)
                        {
                            localTarget = fieldInfo.GetValue(localTarget);
                        }
                    }
                    else if (localMemberInfo is MethodInfo methodInfo)
                    {
                        localType = methodInfo.ReturnType;
                    }
                }

                if (paths.Count > 0)
                {
                    Search();
                }
            }

            Search();

            target = localTarget;
            type = localType;
            memberInfo = localMemberInfo;
        }

        #endregion

        #region [Event Callback Functions]

        /// <summary>
        /// Use this callback to custom visibility condition.
        /// </summary>
        public event Func<bool> VisibilityCallback;

        #endregion

        #region [Getter / Setter]

        public GUIContent GetLabel() { return label; }

        public void SetLabel(GUIContent value) { label = value; }

        #endregion
    }
}