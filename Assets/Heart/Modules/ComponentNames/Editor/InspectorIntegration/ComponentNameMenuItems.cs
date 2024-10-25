using UnityEditor;
using UnityEngine;

namespace Sisus.ComponentNames.Editor
{
    internal static class ComponentNameMenuItems
    {
        [MenuItem("CONTEXT/Component/Rename")]
        private static void Rename(MenuCommand command)
        {
            var target = command.context as Component;
            if(target && target is not NameContainer)
            {
                NameContainer.StartRenaming(target);
            }
        }
    }
}