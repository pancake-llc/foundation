using System;
using System.Runtime.CompilerServices;
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsWrapped(IMGUIContainer header) => string.Equals(header.onGUIHandler.Method.Name, nameof(DrawWrappedHeaderGUI));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Wrap(Editor editor, IMGUIContainer header, bool supportsRichText)
        {
            var targetComponent = editor.target as Component;
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