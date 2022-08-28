using UnityEngine;

namespace Pancake.Core
{
    public class InputExtension
    {
        /// <summary>
        /// All possible states for a button. Can be used in a state machine.
        /// </summary>
        public enum ButtonStates
        {
            Off,
            ButtonDown,
            ButtonPressed,
            ButtonUp
        }

        public enum AxisTypes
        {
            Positive,
            Negative
        }

        /// <summary>
        /// Takes an axis and returns a ButtonState depending on whether the axis is pressed or not (useful for xbox triggers for example), and when you need to use an axis/trigger as a binary thing
        /// </summary>
        /// <returns>The axis as button.</returns>
        /// <param name="axisName">Axis name.</param>
        /// <param name="threshold">Threshold value below which the button is off or released.</param>
        /// <param name="currentState">Current state of the axis.</param>
        /// <param name="axisType"></param>
        public static ButtonStates ProcessAxisAsButton(string axisName, float threshold, ButtonStates currentState, AxisTypes axisType = AxisTypes.Positive)
        {
            float axisValue = Input.GetAxis(axisName);
            ButtonStates returnState;

            bool comparison = (axisType == AxisTypes.Positive) ? (axisValue < threshold) : (axisValue > threshold);

            if (comparison)
            {
                if (currentState == ButtonStates.ButtonPressed)
                {
                    returnState = ButtonStates.ButtonUp;
                }
                else
                {
                    returnState = ButtonStates.Off;
                }
            }
            else
            {
                if (currentState == ButtonStates.Off)
                {
                    returnState = ButtonStates.ButtonDown;
                }
                else
                {
                    returnState = ButtonStates.ButtonPressed;
                }
            }

            return returnState;
        }

        /// <summary>
        /// IM button, short for 
        /// </summary>
        public class InputButton
        {
            public StateMachine<ButtonStates> State { get; protected set; }
            public string buttonID;

            public delegate void ButtonDownMethodDelegate();

            public delegate void ButtonPressedMethodDelegate();

            public delegate void ButtonUpMethodDelegate();

            public ButtonDownMethodDelegate ButtonDownMethod;
            public ButtonPressedMethodDelegate ButtonPressedMethod;
            public ButtonUpMethodDelegate ButtonUpMethod;

            public float TimeSinceLastButtonDown { get { return Time.unscaledTime - lastButtonDownAt; } }
            public float TimeSinceLastButtonUp { get { return Time.unscaledTime - lastButtonUpAt; } }

            protected float lastButtonDownAt;
            protected float lastButtonUpAt;

            public InputButton(
                string playerID,
                string buttonID,
                ButtonDownMethodDelegate btnDown = null,
                ButtonPressedMethodDelegate btnPressed = null,
                ButtonUpMethodDelegate btnUp = null)
            {
                this.buttonID = playerID + "_" + buttonID;
                ButtonDownMethod = btnDown;
                ButtonUpMethod = btnUp;
                ButtonPressedMethod = btnPressed;
                State = new StateMachine<ButtonStates>(null, false);
                State.ChangeState(ButtonStates.Off);
            }

            public virtual void TriggerButtonDown()
            {
                lastButtonDownAt = Time.unscaledTime;
                if (ButtonDownMethod == null)
                {
                    State.ChangeState(ButtonStates.ButtonDown);
                }
                else
                {
                    ButtonDownMethod();
                }
            }

            public virtual void TriggerButtonPressed()
            {
                if (ButtonPressedMethod == null)
                {
                    State.ChangeState(ButtonStates.ButtonPressed);
                }
                else
                {
                    ButtonPressedMethod();
                }
            }

            public virtual void TriggerButtonUp()
            {
                lastButtonUpAt = Time.unscaledTime;
                if (ButtonUpMethod == null)
                {
                    State.ChangeState(ButtonStates.ButtonUp);
                }
                else
                {
                    ButtonUpMethod();
                }
            }
        }
    }
}