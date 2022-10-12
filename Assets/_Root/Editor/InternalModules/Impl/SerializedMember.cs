using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Pancake.Editor
{
    public abstract class SerializedMember : VisualEntity, IMemberLabel
    {
        private static readonly List<MemberManipulator> ManipulatorsNone;
        static SerializedMember() { ManipulatorsNone = new List<MemberManipulator>(); }

        public readonly MemberInfo memberInfo;
        public readonly SerializedObject serializedObject;

        private GUIContent label;
        private List<MemberManipulator> Manipulators = ManipulatorsNone;

        /// <summary>
        /// Implement this constructor to make initializations.
        /// </summary>
        /// <param name="serializedObject">Serialized object reference of this serialized member.</param>
        /// <param name="memberName">Member name of this serialized member.</param>
        public SerializedMember(SerializedObject serializedObject, string memberName)
        {
            this.serializedObject = serializedObject;
            memberInfo = FindMemberInfo(serializedObject, memberName);
            label = new GUIContent(memberName);
            AddManipulators(GetAttributes<ManipulatorAttribute>());
        }

        #region [Abstract Methods]

        /// <summary>
        /// Find member info of serialized member. 
        /// </summary>
        /// <param name="serializedObject">Serialized object reference of this serialized member.</param>
        /// <param name="memberName">Member name of this serialized member.</param>
        protected abstract MemberInfo FindMemberInfo(SerializedObject serializedObject, string memberName);

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
                    if (InspectorReflection.Manipulators.TryGetValue(attribute.GetType(), out MemberManipulator manipulator))
                    {
                        manipulator = Activator.CreateInstance(manipulator.GetType()) as MemberManipulator;
                        manipulator.Initialize(this, attribute);

                        if (Manipulators == ManipulatorsNone)
                            Manipulators = new List<MemberManipulator>();
                        Manipulators.Add(manipulator);
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
            if (InspectorReflection.Manipulators.TryGetValue(attribute.GetType(), out MemberManipulator manipulator))
            {
                manipulator = Activator.CreateInstance(manipulator.GetType()) as MemberManipulator;
                manipulator.Initialize(this, attribute);

                if (Manipulators == ManipulatorsNone)
                    Manipulators = new List<MemberManipulator>();
                Manipulators.Add(manipulator);
            }
        }

        /// <summary>
        /// Remove all manipulator with same attribute type.
        /// </summary>
        /// <param name="attributes">Manipulator attribute type.</param>
        public void RemoveManipulators(ManipulatorAttribute attribute)
        {
            if (InspectorReflection.Manipulators.TryGetValue(attribute.GetType(), out MemberManipulator manipulator))
            {
                List<int> indexes = new List<int>();

                for (int i = 0; i < Manipulators.Count; i++)
                {
                    if (Manipulators[i].GetType() == manipulator.GetType())
                    {
                        indexes.Add(i);
                    }
                }

                for (int i = 0; i < indexes.Count; i++)
                {
                    Manipulators.RemoveAt(indexes[i]);
                }

                if (Manipulators.Count == 0)
                {
                    Manipulators = ManipulatorsNone;
                }
            }
        }

        /// <summary>
        /// Remove manipulator at index.
        /// </summary>
        /// <param name="index">Manipulator index.</param>
        public void RemoveManipulator(int index)
        {
            Manipulators.RemoveAt(index);

            if (Manipulators.Count == 0)
            {
                Manipulators = ManipulatorsNone;
            }
        }

        /// <summary>
        /// Clear all Manipulators.
        /// </summary>
        public void ClearManipulators() { Manipulators = ManipulatorsNone; }

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
            int count = Manipulators.Count;
            if (count > 0)
            {
                for (int i = 0; i < count; i++)
                {
                    Manipulators[i].OnBeforeGUI();
                }
            }
        }

        /// <summary>
        /// Execute all after gui callback which attached to this element.
        /// </summary>
        private void ExecuteAllManipulatorsAfterCallback()
        {
            int count = Manipulators.Count;
            if (count > 0)
            {
                for (int i = 0; i < count; i++)
                {
                    Manipulators[i].OnAfterGUI();
                }
            }
        }

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