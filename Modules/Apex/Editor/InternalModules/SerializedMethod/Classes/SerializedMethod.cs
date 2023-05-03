using Pancake.Apex;
using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using Vexe.Runtime.Extensions;

namespace Pancake.ApexEditor
{
    public class SerializedMethod : SerializedMember
    {
        #region [Const Values]

        private const string ARG_ERROR_MESSAGE = "You cannot use an attribute with a method that has parameters.";

        #endregion

        #region [Static Initialization]

        private static Texture PlayButtonTexture;
        private static Texture PauseButtonTexture;
        private static GUIStyle IconButtonStyle;

        static SerializedMethod()
        {
            PlayButtonTexture = EditorGUIUtility.IconContent("PlayButton").image;
            PauseButtonTexture = EditorGUIUtility.IconContent("PauseButton").image;
        }

        #endregion

        private Type returnType;
        private object result;
        private int argCount;
        private bool keepCall;
        private bool isPressed;
        private float width;
        private GUIStyle style;

        // Stored callback properties.
        private MethodCaller<object, object> onClick;

        /// <summary>
        /// Implement this constructor to make initializations.
        /// </summary>
        /// <param name="serializedObject">Serialized object reference of this serialized member.</param>
        /// <param name="memberName">Member name of this serialized member.</param>
        public SerializedMethod(SerializedObject serializedObject, string memberName)
            : base(serializedObject, memberName)
        {
            MethodInfo methodInfo = GetMemberInfo() as MethodInfo;
            returnType = methodInfo.ReturnType;
            argCount = methodInfo.GetParameters().Length;
            onClick = methodInfo.DelegateForCall();
        }

        #region [SerializedMember Implementation]

        /// <summary>
        /// Called for rendering and handling serialized element.
        /// </summary>
        /// <param name="position">Rectangle position to draw member GUI.</param>
        protected override void OnMemberGUI(Rect position)
        {
            if (IconButtonStyle == null)
            {
                IconButtonStyle = new GUIStyle("IconButton");
            }

            if (style == null)
            {
                style = FindStyle();
            }

            width = position.width;
            if (argCount == 0)
            {
                if (returnType == typeof(void))
                {
                    if (GUI.Button(position, GetLabel(), style))
                    {
                        Invoke();
                    }
                }
                else
                {
                    const float WIDTH = 20;
                    position.width -= WIDTH;

                    position = EditorGUI.PrefixLabel(position, GetLabel());
                    EditorGUI.SelectableLabel(position, result?.ToString() ?? "null", EditorStyles.textField);

                    position.x = position.xMax + ApexGUIUtility.VerticalSpacing;
                    position.y += 0.5f;
                    position.width = WIDTH;

                    if (keepCall)
                    {
                        result = Invoke();
                    }

                    Event current = Event.current;
                    bool hover = position.Contains(current.mousePosition);

                    if (current.type == EventType.MouseDown && hover)
                    {
                        isPressed = true;
                        keepCall = current.shift;
                    }
                    else if (current.type == EventType.MouseUp && hover)
                    {
                        isPressed = false;
                        result = Invoke();
                        GUI.FocusControl(string.Empty);
                    }
                    else if (current.type == EventType.Repaint)
                    {
                        IconButtonStyle.Draw(position,
                            keepCall ? PauseButtonTexture : PlayButtonTexture,
                            hover && !isPressed,
                            true,
                            false,
                            false);
                    }
                }
            }
            else
            {
                EditorGUI.HelpBox(position, ARG_ERROR_MESSAGE, MessageType.Error);
            }
        }

        /// <summary>
        /// Serialized method height.
        /// </summary>
        protected override float GetMemberHeight()
        {
            if (style == null)
            {
                style = FindStyle();
            }

            if (argCount == 0)
            {
                return style.CalcHeight(GetLabel(), width);
            }
            else
            {
                GUIStyle helpBoxStyle = new GUIStyle(EditorStyles.helpBox);
                GUIContent content = new GUIContent(ARG_ERROR_MESSAGE);
                return helpBoxStyle.CalcHeight(content, width);
            }
        }

        #endregion

        /// <summary>
        /// Invoke method.
        /// </summary>
        public virtual object Invoke() { return onClick.Invoke(GetDeclaringObject(), null); }

        /// <summary>
        /// Implement this method to override method GUI style.
        /// </summary>
        /// <param name="serializedObject">Serialized object reference of this serialized member.</param>
        private GUIStyle FindStyle()
        {
            GUIStyleAttribute attribute = GetAttribute<GUIStyleAttribute>();
            if (attribute != null)
            {
                if (attribute.name[0] == '@')
                {
                    return new GUIStyle(attribute.name.Remove(0, 1));
                }

                return ApexCallbackUtility.GetCallbackResult<GUIStyle>(GetDeclaringObject(), attribute.name);
            }

            return new GUIStyle(GUI.skin.button);
        }

        #region [Getter / Setter]

        public GUIStyle GetStyle() { return style; }

        public void SetStyle(GUIStyle value) { style = value; }

        #endregion
    }
}