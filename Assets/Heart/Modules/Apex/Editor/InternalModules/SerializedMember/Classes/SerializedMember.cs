using Pancake.Apex;
using Pancake.ExLib.Reflection;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
using Vexe.Runtime.Extensions;
using Object = UnityEngine.Object;

namespace Pancake.ApexEditor
{
    public abstract class SerializedMember : VisualEntity, ISerializedMember, IMemberInfo, IMemberDeclaringObject, IMemberType, IMemberLabel, IGUIChangedCallback
    {
        public struct Info
        {
            public readonly Type type;
            public readonly MemberInfo memberInfo;
            public readonly object declaringObject;

            public Info(Type type, MemberInfo memberInfo, object declaringObject)
            {
                this.type = type;
                this.memberInfo = memberInfo;
                this.declaringObject = declaringObject;
            }
        }

        private static readonly List<MemberManipulator> ManipulatorsNone;

        private Type type;
        private object declaringObject;
        private MemberInfo memberInfo;
        private SerializedObject serializedObject;
        private GUIContent label;
        private List<MemberManipulator> manipulators;
        private bool isGUIChanged;

        // Stored callback properties.
        private MethodCaller<object, object> onGUIChanged;

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
            : base(memberPath)
        {
            this.serializedObject = serializedObject;

            Info info = GetInfo(serializedObject, memberPath);
            type = info.type;
            memberInfo = info.memberInfo;
            declaringObject = info.declaringObject;

            label = new GUIContent(memberInfo?.Name ?? memberPath);

            manipulators = ManipulatorsNone;
            AddManipulators(GetAttributes<ManipulatorAttribute>());
            RegisterCallbacks();
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

        #region [IMemberDeclaringObject Implementation]

        /// <summary>
        /// Declaring object of serialized member.
        /// </summary>
        public object GetDeclaringObject() { return declaringObject; }

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
            OnBeforeGUI();
            ExecuteAllManipulatorsBeforeCallback();
            EditorGUI.BeginChangeCheck();
            OnMemberGUI(position);
            if (EditorGUI.EndChangeCheck())
            {
                isGUIChanged = true;
                onGUIChanged.SafeInvoke(GetDeclaringObject());
                GUIChanged?.Invoke();
            }

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
        /// visual entity visibility state.
        /// </summary>
        public override bool IsVisible()
        {
            return VisibilityCallback?.Invoke() ?? true;
        }

        /// <summary>
        /// Called every GUI call, before all member GUI methods.
        /// </summary>
        protected virtual void OnBeforeGUI()
        {
            isGUIChanged = false;
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
        /// Get attached specified attribute.
        /// </summary>
        public T GetAttribute<T>() where T : ApexAttribute
        {
            if (memberInfo != null)
            {
                return memberInfo.GetCustomAttribute<T>();
            }

            return null;
        }

        /// <summary>
        /// Get all attached specified attributes.
        /// </summary>
        public T[] GetAttributes<T>() where T : ApexAttribute
        {
            if (memberInfo != null)
            {
                T[] attributes = memberInfo.GetCustomAttributes<T>().ToArray();
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

        /// <summary>
        /// Search and register callbacks.
        /// </summary>
        private void RegisterCallbacks()
        {
            OnGUIChangedAttribute attribute = GetAttribute<OnGUIChangedAttribute>();
            if (attribute != null)
            {
                var t = GetDeclaringObject().GetType();
                var limitDescendant = GetDeclaringObject() is MonoBehaviour ? typeof(MonoBehaviour) : typeof(Object);
                foreach (MethodInfo methodInfo in t.AllMethods(limitDescendant))
                {
                    if (methodInfo.Name == attribute.name && methodInfo.GetParameters().Length == 0)
                    {
                        onGUIChanged = methodInfo.DelegateForCall();
                        break;
                    }
                }
            }
        }

        #region [Static Methods]

        public static Info GetInfo(SerializedObject serializedObject, string memberPath)
        {
            var target = serializedObject.targetObject;
            var type = target.GetType();
            var limitDescendant = target is MonoBehaviour ? typeof(MonoBehaviour) : typeof(Object);
            MemberInfo memberInfo = null;
            object declaringObject = target;

            string[] pathSplit = memberPath.Split('.');
            Queue<string> paths = new Queue<string>(pathSplit);
            RecursiveSearch(ref paths, ref type, ref limitDescendant, ref memberInfo, ref declaringObject);

            return new Info(type, memberInfo, declaringObject);
        }

        private static void RecursiveSearch(ref Queue<string> paths, ref Type type, ref Type limitDescendant, ref MemberInfo memberInfo, ref object declaringObject)
        {
            string memberName = paths.Dequeue();
            if (memberName.Contains("data["))
            {
                string result = Regex.Match(memberName, @"\d+").Value;
                int index = Convert.ToInt32(result);

                int count = 0;
                if (memberInfo is FieldInfo fieldInfo)
                {
                    object mainObject = fieldInfo.GetValue(declaringObject);
                    IEnumerable enumerable = mainObject as IEnumerable;
                    foreach (object element in enumerable)
                    {
                        if (index == count)
                        {
                            if (element != null)
                            {
                                type = element.GetType();
                            }
                            else if (mainObject.GetType().IsGenericType)
                            {
                                type = mainObject.GetType().GetGenericArguments()[0];
                            }
                            else
                            {
                                type = mainObject.GetType().GetElementType();
                            }


                            if (element != null && paths.Count > 0)
                            {
                                declaringObject = element;
                                RecursiveSearch(ref paths, ref type, ref limitDescendant, ref memberInfo, ref declaringObject);
                                break;
                            }
                        }

                        count++;
                    }
                }
            }


            MemberInfo member = null;
            foreach (MemberInfo item in type.AllMembers(limitDescendant))
            {
                if (item.Name == memberName)
                {
                    member = item;
                    break;
                }
            }

            if (member != null)
            {
                memberInfo = member;

                if (memberInfo is FieldInfo fieldInfo)
                {
                    type = fieldInfo.FieldType;
                    if (paths.Count > 0 && ((!type.IsGenericType && !type.IsArray) || (type.IsGenericType && type.GetInterface("IEnumerable`1") == null)))
                    {
                        declaringObject = fieldInfo.GetValue(declaringObject);
                        type = declaringObject.GetType();
                    }
                }
                else if (memberInfo is MethodInfo methodInfo)
                {
                    type = methodInfo.ReturnType;
                }
            }

            if (paths.Count > 0)
            {
                RecursiveSearch(ref paths, ref type, ref limitDescendant, ref memberInfo, ref declaringObject);
            }
        }

        #endregion

        #region [Events]

        /// <summary>
        /// Use this callback to custom visibility condition.
        /// </summary>
        public event Func<bool> VisibilityCallback;

        /// <summary>
        /// Called when GUI has been changed.
        /// </summary>
        public event Action GUIChanged;
        #endregion

        #region [Getter / Setter]

        public GUIContent GetLabel() { return label; }

        public void SetLabel(GUIContent value) { label = value; }
        public bool IsGUIChanged() { return isGUIChanged; }

        #endregion
    }
}