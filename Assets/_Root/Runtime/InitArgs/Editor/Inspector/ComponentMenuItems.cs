using UnityEditor;
using UnityEngine;

namespace Pancake.Editor.Init
{
	internal static class ComponentMenuItems
	{
		private const string AddServiceTagMenuItemName = "CONTEXT/Component/Make Service Of Type...";

		[MenuItem(AddServiceTagMenuItemName, priority = 1500)]
		private static void AddServiceTag(MenuCommand command)
			=> ServiceTagUtility.openSelectTagsMenuFor = command.context as Component;

		[MenuItem(AddServiceTagMenuItemName, validate = true)]
		private static bool ShowAddServiceTag(MenuCommand command)
			// If component has the ServiceAttribute disable opening of the service tags selection menu.
			=> command.context is Component component && ServiceTagUtility.CanAddServiceTag(component);
	}
}