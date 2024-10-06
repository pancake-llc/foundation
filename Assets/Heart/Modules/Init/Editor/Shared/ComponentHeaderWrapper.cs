using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Sisus.Shared.EditorOnly
{
    /// <summary>
    /// Class that can wrap the <see cref="IMGUIContainer.onGUIHandler"/>
    /// of a component header <see cref="IMGUIContainer"/> and allow injecting
    /// drawing logic to occur before and after its drawing.
    /// </summary>
    internal class ComponentHeaderWrapper
    {
        private readonly IMGUIContainer headerElement;
        private readonly Component component;
        private readonly Action wrappedOnGUIHandler;
        private readonly bool supportsRichText;

        private ComponentHeaderWrapper(IMGUIContainer headerElement, Component component, bool supportsRichText)
		{
            this.headerElement = headerElement;
            this.component = component;
            this.supportsRichText = supportsRichText;
            wrappedOnGUIHandler = headerElement.onGUIHandler;
        }

        public static void WrapIfNotAlreadyWrapped((Editor editor, IMGUIContainer header) editorAndHeader, bool supportsRichText)
        {
            var header = editorAndHeader.header;
            var onGUIHandler = header.onGUIHandler;
		    if(string.Equals(onGUIHandler.Method.Name, nameof(DrawWrappedHeaderGUI)))
		    {
			    return;
		    }

		    var targetComponent = editorAndHeader.editor.target as Component;
		    var wrapper = new ComponentHeaderWrapper(header, targetComponent, supportsRichText);
		    header.onGUIHandler = wrapper.DrawWrappedHeaderGUI;
        }

		private void DrawWrappedHeaderGUI()
        {
            if(!component)
            {
                Unwrap();
                return;
            }

            var headerRect = headerElement.contentRect;
            bool headerIsSelected = headerElement.focusController.focusedElement == headerElement;

            ComponentHeader.InvokeBeforeHeaderGUI(component, headerRect, headerIsSelected, supportsRichText);
            wrappedOnGUIHandler?.Invoke();
            ComponentHeader.InvokeAfterHeaderGUI(component, headerRect, headerIsSelected, supportsRichText);
        }

        private void Unwrap()
        {
            if(headerElement is not null)
            {
                headerElement.onGUIHandler = wrappedOnGUIHandler;
            }
        }
    }
}