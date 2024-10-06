using UnityEditor;
using UnityEngine;
using static Sisus.Shared.EditorOnly.InspectorContents;

namespace Sisus.Shared.EditorOnly
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
            if(Event.current.type != EventType.Repaint)
            {
                editor.Repaint();
                return;
            }

            foreach(var editorAndHeader in GetAllHeaderElements(editor))
            {
                ComponentHeaderWrapper.WrapIfNotAlreadyWrapped(editorAndHeader, true);
            }
        }
    }
}