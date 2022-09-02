using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace Pancake.Editor
{
    /// <summary>
    /// Class that can wrap the <see cref="IMGUIContainer.onGUIHandler"/>
    /// of a component header <see cref="IMGUIContainer"/> and allow injecting
    /// drawing logic to occure before and after its drawing.
    /// </summary>
    internal class ComponentHeaderWrapper
    {
        private readonly IMGUIContainer headerElement;
        private readonly Component component;
        private readonly Action wrappedOnGUIHandler;
        private readonly bool supportsRichText;

        public Component Component => component;

        public ComponentHeaderWrapper(IMGUIContainer headerElement, Component component, bool supportsRichText)
		{
            this.headerElement = headerElement;
            this.component = component;
            this.supportsRichText = supportsRichText;
            wrappedOnGUIHandler = headerElement.onGUIHandler;
        }

		public void DrawWrappedHeaderGUI()
        {
            if(headerElement is null || component == null)
            {
                return;
            }

            Rect headerRect = headerElement.contentRect;
            bool HeaderIsSelected = headerElement.focusController.focusedElement == headerElement;

            ComponentHeader.InvokeBeforeHeaderGUI(component, headerRect, HeaderIsSelected, supportsRichText);
            wrappedOnGUIHandler?.Invoke();
            ComponentHeader.InvokeAfterHeaderGUI(component, headerRect, HeaderIsSelected, supportsRichText);
        }
    }
}