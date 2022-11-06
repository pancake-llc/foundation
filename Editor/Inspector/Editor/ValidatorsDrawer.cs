using System.Collections.Generic;
using UnityEditor;

namespace Pancake.Editor
{
    internal class ValidatorsDrawer : CustomDrawer
    {
        public override InspectorElement CreateElementInternal(Property property, InspectorElement next)
        {
            if (!property.HasValidators)
            {
                return next;
            }

            var element = new InspectorElement();
            element.AddChild(new PropertyValidationResultInspectorElement(property));
            element.AddChild(next);
            return element;
        }

        public class PropertyValidationResultInspectorElement : InspectorElement
        {
            private readonly Property _property;
            private IReadOnlyList<ValidationResult> _validationResults;

            public PropertyValidationResultInspectorElement(Property property) { _property = property; }

            public override float GetHeight(float width)
            {
                if (ChildrenCount == 0)
                {
                    return -EditorGUIUtility.standardVerticalSpacing;
                }

                return base.GetHeight(width);
            }

            public override bool Update()
            {
                var dirty = base.Update();

                dirty |= GenerateValidationResults();

                return dirty;
            }

            private bool GenerateValidationResults()
            {
                if (ReferenceEquals(_property.ValidationResults, _validationResults))
                {
                    return false;
                }

                _validationResults = _property.ValidationResults;

                RemoveAllChildren();

                foreach (var result in _validationResults)
                {
                    AddChild(new InfoBoxInspectorElement(result.Message, result.MessageType));
                }

                return true;
            }
        }
    }
}