using System;
using Sisus.Init.Internal;
using UnityEditor;
using UnityEngine;

namespace Sisus.Init.EditorOnly.Internal
{
	internal static class ComponentMenuItems
	{
		public const string ShowInitSection = "CONTEXT/Component/Show Init Section";
		private const string AddServiceTag = "CONTEXT/Component/Make Service Of Type...";
		private const string GenerateInitializer = "CONTEXT/Component/Generate Initializer...";

		[MenuItem(AddServiceTag, priority = 1500)]
		private static void AddServiceTagMenuItem(MenuCommand command)
			=> EditorServiceTagUtility.openSelectTagsMenuFor = command.context as Component;

		[MenuItem(AddServiceTag, priority = 1500, validate = true)]
		private static bool IsAddServiceTagMenuItemClickable(MenuCommand command)
			// If component has the ServiceAttribute disable opening of the service tags selection menu.
			=> command.context && command.context is Component component && EditorServiceTagUtility.CanAddServiceTag(component);

		[MenuItem(GenerateInitializer, priority = 1501)]
		private static void GenerateInitializerMenuItem(MenuCommand command) => ScriptGenerator.CreateInitializer(command.context);

		[MenuItem(GenerateInitializer, priority = 1501, validate = true)]
		private static bool IsGenerateInitializerMenuItemClickable(MenuCommand command)
		{
			return command.context && (command.context is Component
				 || (command.context is ScriptableObject scriptableObject && TypeUtility.DerivesFromGenericBaseType(scriptableObject.GetType())))
				 && !InitializerEditorUtility.HasAnyInitializerTypes(command.context.GetType());
		}

		#if !INIT_ARGS_DISABLE_SHOW_INIT_SECTION_CONTEXT_MENU_ITEM
		[MenuItem(ShowInitSection, priority = 1502)]
		#endif
		private static void ToggleShowInitSection(MenuCommand command)
		{
			if(TryGetTypeAndScript(command, out Type initializableType, out MonoScript script))
			{
				InitializerGUI.ToggleHideInitSection(script, initializableType);
			}
		}

		private static bool TryGetTypeAndScript(MenuCommand command, out Type initializableType, out MonoScript script)
		{
			if(command.context == null)
			{
				script = null;
				initializableType = null;
				return false;
			}

			if(command.context is MonoBehaviour monoBehaviour)
			{
				script = MonoScript.FromMonoBehaviour(monoBehaviour);
				initializableType = monoBehaviour.GetType();
			}
			else if(script = command.context as MonoScript)
			{
				initializableType = script.GetClass();
			}
			else
			{
				script = null;
				initializableType = command.context.GetType();
			}

			return true;
		}

		#if !INIT_ARGS_DISABLE_SHOW_INIT_SECTION_CONTEXT_MENU_ITEM
		[MenuItem(ShowInitSection, priority = 1502, validate = true)]
		#endif
		private static bool IsToggleShowShowInitSectionClickable(MenuCommand command)
		{
			var target = command.context;
			if(!target)
			{
				return false;
			}

			
			if(InitializerUtility.HasInitializer(target))
			{
				return false;
			}
			
			var targetType = target.GetType();
			return InitializerEditorUtility.IsInitializable(targetType) || InitializerEditorUtility.HasAnyInitializerTypes(targetType);
		}
	}
}