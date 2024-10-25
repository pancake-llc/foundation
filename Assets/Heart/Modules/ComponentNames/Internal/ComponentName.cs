#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.CompilerServices;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

[assembly: InternalsVisibleTo("ComponentNames.Tests")]

namespace Sisus.ComponentNames.Editor
{
	/// <summary>
	/// Utility class for getting and setting component names.
	/// </summary>
	internal static class ComponentName
	{
		internal static event Action<Component, NameWithSuffix> InspectorTitleChanged;

		private static readonly Dictionary<Type, NameWithSuffix> originalInspectorTitles = new();
		private static readonly Dictionary<Object, NameWithSuffix> inspectorTitleOverrides = new();

		/// <summary>
		/// Should class name be added as suffix in the inspector by default?
		/// </summary>
		internal static bool AddClassNameAsInspectorSuffixByDefault
		{
			get => EditorPrefs.GetBool("ComponentNames.AddClassNameInspectorSuffix", true);
			set => EditorPrefs.SetBool("ComponentNames.AddClassNameInspectorSuffix", value);
		}

		/// <summary>
		/// The format for inspector suffixes.
		/// </summary>
		internal static string InspectorSuffixFormat
		{
			get => EditorPrefs.GetString("ComponentNames.InspectorSuffixFormat", " <color=grey>({0})</color>");
			set => EditorPrefs.SetString("ComponentNames.InspectorSuffixFormat", value);
		}

		/// <summary>
		/// Remove the default "(Script)" suffix from MonoBehaviour inspector titles?
		/// </summary>
		internal static bool RemoveDefaultScriptSuffix
		{
			get => EditorPrefs.GetBool("ComponentNames.RemoveDefaultScriptSuffix", true);
			set => EditorPrefs.GetBool("ComponentNames.RemoveDefaultScriptSuffix", value);
		}

		private static bool TryGetOverride([DisallowNull] Component component, out NameWithSuffix nameOverride)
			=> inspectorTitleOverrides.TryGetValue(component, out nameOverride);

		internal static NameWithSuffix Get([DisallowNull] Component component)
			=> TryGetOverride(component, out var @override) ? @override : GetDefault(component, withoutScriptSuffix:RemoveDefaultScriptSuffix);

		internal static void Set([DisallowNull] Component component, [AllowNull] string name, ModifyOptions modifyOptions)
		{
			#if DEV_MODE
			Debug.Assert(component, name);
			#endif

			if(IsNullEmptyOrDefault(component, name))
			{
				ResetToDefault(component, modifyOptions);
				return;
			}

			if(name.EndsWith(')'))
			{
				int i = name.IndexOf('(');
				if(i != -1)
				{
					string suffix = name.Substring(i + 1, name.Length - i - 2);
					name = name.Substring(0, i).Trim();
					if(name.Length == 0)
					{
						name = GetDefault(component, true);
					}

					Set(component, name, suffix, modifyOptions);
					return;
				}
			}

			Set(component, name, AddClassNameAsInspectorSuffixByDefault, modifyOptions);
		}

		internal static void Set([DisallowNull] Component component, [AllowNull] string name, bool addTypeNameSuffix, ModifyOptions modifyOptions)
		{
			#if DEV_MODE
			Debug.Assert(component, name);
			#endif

			if(IsNullEmptyOrDefault(component, name))
			{
				ResetToDefault(component, modifyOptions);
				return;
			}

			if(!addTypeNameSuffix)
			{
				Set(component, name, "", modifyOptions);
				return;
			}

			var suffix = GetDefault(component, withoutScriptSuffix:true).name;

			// Avoid situations like "Cube" => "Cube (Cube (Mesh Filter))"
			if(suffix.EndsWith(')'))
			{
				int i = suffix.IndexOf('(') + 1;
				if(i > 0)
				{
					suffix = suffix.Substring(i, suffix.Length - i - 1);
				}
			}

			if(string.Equals(name, suffix))
			{
				Set(component, name, null, modifyOptions);
				return;
			}

			Set(component, name, suffix, modifyOptions);
		}

		internal static void Set([DisallowNull] Component component, [AllowNull] string name, [AllowNull] string suffix, ModifyOptions modifyOptions)
		{
			#if DEV_MODE
			Debug.Assert(component, name);
			#endif

			if(IsNullEmptyOrDefault(component, name, suffix))
			{
				ResetToDefault(component, modifyOptions);
				return;
			}

			var titleAndSuffix = new NameWithSuffix(component, name, suffix);
			inspectorTitleOverrides[component] = titleAndSuffix;

			var tileAndSuffixJoined = titleAndSuffix.ToString(false);
			NameContainer.TryGetOrCreate(component, modifyOptions, nameContainer =>
			{
				nameContainer.NameOverride = titleAndSuffix.ToString(false);
			}, tileAndSuffixJoined, null);

			InspectorTitleChanged?.Invoke(component, titleAndSuffix);
		}

		private static void Set([DisallowNull] Component component, NameWithSuffix nameWithSuffix, ModifyOptions modifyOptions)
		{
			#if DEV_MODE
			Debug.Assert(component, nameWithSuffix);
			#endif

			if(IsNullEmptyOrDefault(component, nameWithSuffix))
			{
				ResetToDefault(component, modifyOptions);
				return;
			}

			var tileAndSuffixJoined = nameWithSuffix.ToString(false);
			NameContainer.TryGetOrCreate(component, modifyOptions, nameContainer =>
			{
				nameContainer.NameOverride = tileAndSuffixJoined;
			}, tileAndSuffixJoined, null);

			InspectorTitleChanged?.Invoke(component, nameWithSuffix);
		}

		internal static NameWithSuffix GetInspectorTitle([DisallowNull] Component component)
		{
			if(inspectorTitleOverrides.TryGetValue(component, out var inspectorTitle))
			{
				return inspectorTitle;
			}

			return GetDefault(component, RemoveDefaultScriptSuffix);
		}

		internal static NameWithSuffix GetDefault([DisallowNull] Component component, bool withoutScriptSuffix)
		{
			var componentType = component.GetType();
			// TODO: Is it bad that the tooltip gets discarded here? do we need to then call this twice to also get the tooltip?
			// Maybe not, if it's only updated on mouse enter for example. Investigate!
			var (title, suffix, _) = HeaderContentUtility.GetCustomHeaderContent(component, componentType);

			if(title.IsDefault)
			{
				var @default = GetDefaultBuiltIn(component, componentType, withoutScriptSuffix:false);
				title = @default.name;

				if(suffix.IsDefault)
				{
					suffix = withoutScriptSuffix && string.Equals(@default.suffix, "Script") ? "" : @default.suffix;
				}
			}
			else if(suffix.IsDefault)
			{
				suffix = GetDefaultBuiltIn(component, componentType, withoutScriptSuffix:false).name;
			}

			return new(title, suffix);
		}

		internal static NameWithSuffix GetDefaultBuiltIn(Component component, Type componentType, bool withoutScriptSuffix)
		{
			if(originalInspectorTitles.TryGetValue(componentType, out var result))
			{
				return result;
			}

			if(componentType.GetCustomAttribute<AddComponentMenu>(false) is AddComponentMenu addComponentMenu
			&& addComponentMenu.componentMenu is {Length: > 0} menuPath)
			{
				int lastCategoryEnd = menuPath.LastIndexOf('/');
				result = new(lastCategoryEnd != -1 ? menuPath.Substring(lastCategoryEnd + 1) : menuPath, "");
				return CacheAndReturn(result);
			}

			var title = ObjectNames.GetInspectorTitle(component);
			if(title.EndsWith(')'))
			{
				int suffixStart = title.LastIndexOf('(');
				if(suffixStart != -1)
				{
					var name = title.Substring(0, suffixStart).TrimEnd(' ');
					var suffix = title.Substring(suffixStart  + 1, title.Length - suffixStart - 2);
					if(withoutScriptSuffix && string.Equals(suffix, "Script"))
					{
						return CacheAndReturn(new(name, ""));
					}

					return CacheAndReturn(new(name, suffix));
				}
			}

			return CacheAndReturn(new(title, ""));

			NameWithSuffix CacheAndReturn(in NameWithSuffix result)
			{
				originalInspectorTitles.Add(componentType, result);
				return result;
			}
		}

		internal static void EnsureOriginalInspectorTitleIsCached([DisallowNull] Component component) => _ = GetDefaultBuiltIn(component, component.GetType(), RemoveDefaultScriptSuffix);
		internal static void EnsureOriginalInspectorTitleIsCached([DisallowNull] Component component, [DisallowNull] Type componentType) => _ = GetDefaultBuiltIn(component, componentType, RemoveDefaultScriptSuffix);

		/// <summary>
		/// Resets the title of the component to its default value.
		/// </summary>
		/// <param name="component"> The component whose title to reset. </param>
		/// <param name="modifyOptions">
		/// <see cref="ModifyOptions.Immediate"/> can be used when calling from the main thread, outside of deserialization context.
		/// <para>
		/// <see cref="ModifyOptions.NonUndoable"/> can be used when to make the action non-undoable.
		/// </para>
		/// <para>
		/// Both <see cref="ModifyOptions.Immediate"/> and <see cref="ModifyOptions.NonUndoable"/> flags are disabled by default.
		/// </para>
		/// </param>
		internal static void ResetToDefault([DisallowNull] Component component, ModifyOptions modifyOptions)
		{
			bool invokeInspectorTitleChanged = inspectorTitleOverrides.Remove(component);

			if(modifyOptions.IsUpdatingContainerAllowed())
			{
				RemoveOrResetNameContainer(component, modifyOptions, ref invokeInspectorTitleChanged);
			}

			if(invokeInspectorTitleChanged)
			{
				InspectorTitleChanged?.Invoke(component, new NameWithSuffix(component));
			}

			static void RemoveOrResetNameContainer(Component component, ModifyOptions modifyOptions, ref bool invokeInspectorTitleChanged)
			{
				if(modifyOptions.IsDelayed())
				{
					bool invokeInspectorTitleChangedDelayed = invokeInspectorTitleChanged;
					EditorApplication.delayCall += ()=>
					{
						RemoveOrResetNameContainer(component, modifyOptions.Immediately(), ref invokeInspectorTitleChangedDelayed);

						if(invokeInspectorTitleChangedDelayed)
						{
							InspectorTitleChanged?.Invoke(component, new NameWithSuffix(component));
						}
					};

					invokeInspectorTitleChanged = false;
					return;
				}

				if(!NameContainer.TryGet(component, out var nameContainer))
				{
					return;
				}

				if(string.IsNullOrEmpty(nameContainer.TooltipOverride) && modifyOptions.IsRemovingNameContainerAllowed())
				{
					nameContainer.Remove(modifyOptions);
					invokeInspectorTitleChanged = true;
					return;
				}

				if(!string.IsNullOrEmpty(nameContainer.NameOverride))
				{
					nameContainer.NameOverride = "";
					invokeInspectorTitleChanged = true;
				}
			}
		}

		internal static bool IsNullEmptyOrDefault(Component component, string name)
		{
			if(string.IsNullOrEmpty(name))
			{
				return true;
			}

			if(string.Equals(WithoutScriptSuffix(name), GetDefault(component, withoutScriptSuffix:true).ToString(richText:false)))
			{
				return true;
			}

			return false;
		}

		internal static bool IsNullEmptyOrDefault(Component component, NameWithSuffix nameWithSuffix)
		{
			if(string.IsNullOrEmpty(nameWithSuffix.name))
			{
				return true;
			}

			if(string.Equals(WithoutScriptSuffix(nameWithSuffix.name), GetDefault(component, withoutScriptSuffix:true).ToString(richText:false)))
			{
				return true;
			}

			return false;
		}

		private static bool IsNullEmptyOrDefault(Component component, string name, string suffix)
		{
			if(string.IsNullOrEmpty(name))
			{
				return suffix is null || string.Equals(suffix, "Script") || string.Equals(suffix, GetDefault(component, withoutScriptSuffix:true).name);
			}

			var defaultName = GetDefault(component, withoutScriptSuffix:true);
			if(string.Equals(name, defaultName.name))
			{
				return suffix is null || string.Equals(suffix, "Script");
			}

			return false;
		}

		internal static string WithoutRichTextTags([NotNull] string text)
		{
			const int MaxIterations = 10;
			for(int iteration = 1; iteration < MaxIterations; iteration++)
			{
				int index = text.IndexOf("<color=", StringComparison.OrdinalIgnoreCase);
				if(index == -1)
				{
					return text;
				}

				int endIndex = text.IndexOf('>', index + "<color=".Length);
				if(endIndex > 0)
				{
					text = text.Remove(index, endIndex + 1 - index);
				}

				index = text.IndexOf("</color>", StringComparison.OrdinalIgnoreCase);
				if(index == -1)
				{
					return text;
				}

				text = text.Remove(index, "</color>".Length);
			}

			return text;
		}

		private static string WithoutScriptSuffix(string title) => title.EndsWith(" (Script)") ? title.Substring(0, title.Length - " (Script)".Length) : title;
	}
}
#endif