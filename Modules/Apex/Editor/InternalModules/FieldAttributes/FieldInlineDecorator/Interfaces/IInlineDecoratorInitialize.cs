using Pancake.Apex;
using UnityEngine;

namespace Pancake.ApexEditor
{
    public interface IInlineDecoratorInitialize
    {
        /// <summary>
        /// Called once when initializing element inline decorator.
        /// </summary>
        /// <param name="element">Serialized element with InlineDecoratorAttribute.</param>
        /// <param name="inlineDecoratorAttribute">InlineDecoratorAttribute of serialized element.</param>
        /// <param name="label">Label of serialized property.</param>
        void Initialize(SerializedField element, InlineDecoratorAttribute inlineDecoratorAttribute, GUIContent label);
    }
}