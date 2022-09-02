#if POWER_INSPECTOR
using System;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Pancake.Editor
{
	/// <summary>
	/// Class responsible for injecting <see cref="RenameableHeaderWrapper"/>
	/// into the component header elements of open Power Inspector windows.
	/// </summary>
	[InitializeOnLoad]
	internal static class PowerInspectorComponentHeaderCallbackDelegator
	{
		private const string PowerInspectorAssemblyName = "PowerInspector";
		private const string EditorGUIDrawerFullTypeName = "Sisus.EditorGUIDrawer";
		private const BindingFlags AnyStatic = BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly;

		static PowerInspectorComponentHeaderCallbackDelegator()
		{
			Assembly powerInspectorAssembly;
			try
			{
				powerInspectorAssembly = Assembly.Load(PowerInspectorAssemblyName);
			}
			catch(FileNotFoundException)
			{
				#if DEV_MODE
				Debug.LogError($"Assembly \"{PowerInspectorAssemblyName}\" not found.");
				#endif
				return;
			}

			if(powerInspectorAssembly is null)
			{
				#if DEV_MODE
				Debug.LogError($"Assembly \"{PowerInspectorAssemblyName}\" not found.");
				#endif
				return;
			}
			
			var editorGUIDrawer = powerInspectorAssembly.GetType(EditorGUIDrawerFullTypeName);
			if(editorGUIDrawer is null)
			{
				#if DEV_MODE
				Debug.LogError($"Type \"{EditorGUIDrawerFullTypeName}\" not found in assembly {powerInspectorAssembly.GetName()}.");
				#endif
				return;
			}

			AddListener(editorGUIDrawer, "BeforeComponentHeaderGUI", nameof(OnBeforeComponentHeaderGUI));
			AddListener(editorGUIDrawer, "AfterComponentHeaderGUI", nameof(OnAfterComponentHeaderGUI));
		}
		
		private static void AddListener(Type editorGUIDrawer, string eventName, string listenerName)
		{
			var @event = editorGUIDrawer.GetEvent(eventName, AnyStatic);
			if(@event is null)
			{
				#if DEV_MODE
				Debug.LogError($"Event \"{eventName}\" not found in class {editorGUIDrawer}.");
				#endif
				return;
			}

			var method = typeof(PowerInspectorComponentHeaderCallbackDelegator).GetMethod(listenerName, AnyStatic);
			if(method is null)
			{
				#if DEV_MODE
				Debug.LogError($"Method \"{listenerName}\" not found in class {nameof(PowerInspectorComponentHeaderCallbackDelegator)}.");
				#endif
				return;
			}

			Type delegateType = @event.EventHandlerType;
			Delegate @delegate = Delegate.CreateDelegate(delegateType, method, false);
			if(@delegate is null)
			{
				#if DEV_MODE
				Debug.LogError($"Failed to create delegate of type {delegateType.Name} from {method.DeclaringType.Name}.{method.Name}.");
				#endif
				return;
			}

			@event.RemoveEventHandler(null, @delegate);
			@event.AddEventHandler(null, @delegate);
		}

		private static float OnBeforeComponentHeaderGUI(Object[] targets, Rect headerRect, bool headerIsSelected)
		{
			var component = targets.Length == 0 ? null : targets[0] as Component;
			return ComponentHeader.InvokeBeforeHeaderGUI(component, headerRect, headerIsSelected, true);
		}

		private static float OnAfterComponentHeaderGUI(Object[] targets, Rect headerRect, bool headerIsSelected)
		{
			var component = targets.Length == 0 ? null : targets[0] as Component;
			return ComponentHeader.InvokeAfterHeaderGUI(component, headerRect, headerIsSelected, true);
		}
	}
}
#endif
