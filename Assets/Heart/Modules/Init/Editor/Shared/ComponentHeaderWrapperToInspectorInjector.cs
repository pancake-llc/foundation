using System.Diagnostics.CodeAnalysis;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using static Sisus.Init.EditorOnly.InspectorContents;

namespace Sisus.Init.EditorOnly
{
	[InitializeOnLoad]
	internal static class ComponentHeaderWrapperToInspectorInjector
	{
		static ComponentHeaderWrapperToInspectorInjector()
		{
			Editor.finishedDefaultHeaderGUI -= AfterInspectorRootEditorHeaderGUI;
			Editor.finishedDefaultHeaderGUI += AfterInspectorRootEditorHeaderGUI;
		}

		private static void AfterInspectorRootEditorHeaderGUI(Editor editor)
		{
			if(editor.target is GameObject)
			{
				// Handle InspectorWindow
				AfterGameObjectHeaderGUI(editor);
				return;
			}
			
			if(editor.target is Component)
			{
				// Handle PropertyEditor window opened via "Properties..." context menu item
				AfterComponentPropertiesHeaderGUI(editor);
			}
		}

		private static void AfterGameObjectHeaderGUI([DisallowNull] Editor gameObjectEditor)
		{
			foreach(var editorAndHeader in GetComponentHeaderElementsFromEditorWindowOf(gameObjectEditor))
			{
				ComponentHeaderWrapper.WrapIfNotAlreadyWrapped(editorAndHeader, true);
			}
		}

		private static void AfterComponentPropertiesHeaderGUI([DisallowNull] Editor componentEditor)
		{
			if(GetComponentHeaderElementFromPropertyEditorOf(componentEditor) is (Editor, IMGUIContainer) editorAndHeader)
			{
				ComponentHeaderWrapper.WrapIfNotAlreadyWrapped(editorAndHeader, false);
			}
		}
	}
}