using System;
using System.Linq;
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
	internal sealed class ComponentHeaderWrapper
	{
		private readonly IMGUIContainer headerElement;
		private readonly Component[] targets;
		private readonly Action wrappedOnGUIHandler;
		private readonly bool supportsRichText;

		private ComponentHeaderWrapper(IMGUIContainer headerElement, Component[] targets, bool supportsRichText)
		{
			this.headerElement = headerElement;
			this.targets = targets;
			this.supportsRichText = supportsRichText;
			wrappedOnGUIHandler = headerElement.onGUIHandler;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool IsWrapped(IMGUIContainer header) => string.Equals(header.onGUIHandler.Method.Name, nameof(DrawWrappedHeaderGUI));

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Wrap(Editor editor, IMGUIContainer header, bool supportsRichText)
		{
			var components = editor.targets.Cast<Component>().ToArray();
			var wrapper = new ComponentHeaderWrapper(header, components, supportsRichText);
			header.onGUIHandler = wrapper.DrawWrappedHeaderGUI;
		}

		private void DrawWrappedHeaderGUI()
		{
			for(int i = targets.Length - 1; i >= 0; i--)
			{
				if(!targets[i])
				{
					Unwrap();
					return;
				}
			}

			var headerRect = headerElement.contentRect;
			bool headerIsSelected = headerElement.focusController.focusedElement == headerElement;

			ComponentHeader.InvokeBeforeHeaderGUI(targets, headerRect, headerIsSelected, supportsRichText);
			wrappedOnGUIHandler?.Invoke();
			ComponentHeader.InvokeAfterHeaderGUI(targets, headerRect, headerIsSelected, supportsRichText);
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