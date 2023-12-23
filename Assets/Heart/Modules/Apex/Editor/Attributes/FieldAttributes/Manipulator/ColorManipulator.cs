using Pancake.Apex;
using UnityEngine;

namespace Pancake.ApexEditor
{
    [ManipulatorTarget(typeof(ColorAttribute))]
    public sealed class ColorManipulator : MemberManipulator
    {
        private ColorAttribute attribute;
        private Color color;
        private Color previousColor;

        /// <summary>
        /// Called once when initializing member manipulator.
        /// </summary>
        /// <param name="member">Serialized member with ManipulatorAttribute.</param>
        /// <param name="ManipulatorAttribute">ManipulatorAttribute of serialized member.</param>
        public override void Initialize(SerializedMember member, ManipulatorAttribute ManipulatorAttribute)
        {
            attribute = ManipulatorAttribute as ColorAttribute;
            color = new Color(attribute.r, attribute.g, attribute.b, attribute.a);
        }

        /// <summary>
        /// Called before rendering member GUI.
        /// </summary>
        public override void OnBeforeGUI()
        {
            switch (attribute.Target)
            {
                case ColorTarget.Global:
                    previousColor = GUI.color;
                    GUI.color = color;
                    break;
                case ColorTarget.Content:
                    previousColor = GUI.contentColor;
                    GUI.contentColor = color;
                    break;
                case ColorTarget.Background:
                    previousColor = GUI.backgroundColor;
                    GUI.backgroundColor = color;
                    break;
            }
        }

        /// <summary>
        /// Called after rendering member GUI.
        /// </summary>
        public override void OnAfterGUI()
        {
            switch (attribute.Target)
            {
                case ColorTarget.Global:
                    GUI.color = previousColor;
                    break;
                case ColorTarget.Content:
                    GUI.contentColor = previousColor;
                    break;
                case ColorTarget.Background:
                    GUI.backgroundColor = previousColor;
                    break;
            }
        }
    }
}