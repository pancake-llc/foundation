using UnityEditor;
using UnityEngine;

namespace Sisus.ComponentNames.EditorOnly
{
    internal static class ComponentNameMenuItems
    {
        [MenuItem("CONTEXT/Component/Rename")]
        private static void Rename(MenuCommand command) => NameContainer.StartRenaming(command.context as Component);
    }
}