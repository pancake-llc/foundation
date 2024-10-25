using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using UnityEngine;

namespace Sisus.ComponentNames
{
	/// <summary>
	/// Extension methods for <see cref="Component"/> that can be used to
	/// <see cref="GetName">get</see> or <see cref="SetName">set</see> the
	/// name of the <see cref="Component"/>.
	/// </summary>
	public static class ComponentExtensions
	{
		/// <summary>
		/// Gets the name of the <paramref name="component"/>.
		/// <para>
		/// In the editor this corresponds with the name of the component as shown in the Inspector.
		/// </para>
		/// <para>
		/// In builds this always returns the <see cref="System.Type.Name">type name</see> of the component class.
		/// </para>
		/// </summary>
		/// <param name="component"> The <see cref="Component"/> whose name to get. </param>
		/// <returns></returns>
		[return: NotNull]
		public static string GetName([DisallowNull] this Component component)
		{
			#if UNITY_EDITOR
			return Editor.ComponentName.Get(component).name;
			#else
			return component.GetType().Name;
			#endif
		}

		/// <summary>
		/// Sets the name of the <paramref name="component"/>.
		/// <para>
		/// In builds calls to this method are ignored and <see cref="GetName"/> will always return
		/// the <see cref="System.Type.Name">name</see> of the component class.
		/// </para>
		/// </summary>
		/// <param name="component"> The <see cref="Component"/> whose name to set. </param>
		/// <param name="name"> The new name for the <paramref name="component"/>. </param>
		[Conditional("UNITY_EDITOR")]
		public static void SetName([DisallowNull] this Component component, string name)
		{
			#if UNITY_EDITOR
			Editor.ComponentName.Set(component, name, Editor.ModifyOptions.Defaults);
			#endif
		}

		/// <summary>
		/// Sets the name of the <paramref name="component"/>.
		/// <para>
		/// In builds calls to this method are ignored and <see cref="GetName"/> will always return
		/// the <see cref="System.Type.Name">name</see> of the component class.
		/// </para>
		/// </summary>
		/// <param name="component"> The <see cref="Component"/> whose name to set. </param>
		/// <param name="name"> The new name for the <paramref name="component"/>. </param>
		/// <param name="classNameSuffix">
		/// If <see langword="true"/> then the default name of the component (which is usually the name
		/// of the component class) will be shown in the Inspector as a suffix after the <paramref name="name"/>.
		/// </param>
		[Conditional("UNITY_EDITOR")]
		public static void SetName([DisallowNull] this Component component, string name, bool classNameSuffix)
		{
			#if UNITY_EDITOR
			Editor.ComponentName.Set(component, name, classNameSuffix, Editor.ModifyOptions.Defaults);
			#endif
		}

		/// <summary>
		/// Sets the name of the <paramref name="component"/>.
		/// <para>
		/// In builds calls to this method are ignored and <see cref="GetName"/> will always return
		/// the <see cref="System.Type.Name">name</see> of the component class.
		/// </para>
		/// </summary>
		/// <param name="component"> The <see cref="Component"/> whose name to set. </param>
		/// <param name="name"> The new name for the <paramref name="component"/>. </param>
		/// <param name="suffix">
		/// Auxiliary suffix text to show in the Inspector after the <paramref name="name"/>.
		/// <para>
		/// If this is <see langword="null"/> or <see cref="string.Empty"/> then no suffix
		/// will be shown after the name in the Inspector.
		/// </para>
		/// </param>
		[Conditional("UNITY_EDITOR")]
		public static void SetName([DisallowNull] this Component component, string name, [MaybeNull] string suffix)
		{
			#if UNITY_EDITOR
			Editor.ComponentName.Set(component, name, suffix, Editor.ModifyOptions.Defaults);
			#endif
		}

		/// <summary>
		/// Sets the name of the <paramref name="component"/> and its tooltip in the Inspector.
		/// <para>
		/// In builds calls to this method are ignored and <see cref="GetName"/> will always return
		/// the <see cref="System.Type.Name">name</see> of the component class.
		/// </para>
		/// </summary>
		/// <param name="component"> The <see cref="Component"/> whose name to set. </param>
		/// <param name="name"> The new name for the <paramref name="component"/>. </param>
		/// <param name="classNameSuffix">
		/// If <see langword="true"/> then the default name of the component (which is usually the name
		/// of the component class) will be shown in the Inspector as a suffix after the <paramref name="name"/>.
		/// </param>
		/// <param name="tooltip"> The new Inspector tooltip for the <paramref name="component"/>. </param>
		[Conditional("UNITY_EDITOR")]
		public static void SetName([DisallowNull] this Component component, string name, bool classNameSuffix, string tooltip)
		{
			#if UNITY_EDITOR
			UnityEditor.EditorApplication.delayCall += ()=>
			{
				Editor.ComponentTooltip.Set(component, tooltip, Editor.ModifyOptions.Immediate | Editor.ModifyOptions.DisallowRemoveNameContainer);
				Editor.ComponentName.Set(component, name, classNameSuffix, Editor.ModifyOptions.Immediate);
			};
			#endif
		}

		/// <summary>
		/// Sets the name of the <paramref name="component"/> and its tooltip in the Inspector.
		/// <para>
		/// In builds calls to this method are ignored and <see cref="GetName"/> will always return
		/// the <see cref="System.Type.Name">name</see> of the component class.
		/// </para>
		/// </summary>
		/// <param name="component"> The <see cref="Component"/> whose name to set. </param>
		/// <param name="headerContent">
		/// The Inspector title to give the <paramref name="component"/>, consisting of a name, suffix and tooltip.
		/// </param>
		[Conditional("UNITY_EDITOR")]
		public static void SetName([DisallowNull] this Component component, HeaderContent headerContent)
		{
			#if UNITY_EDITOR
			UnityEditor.EditorApplication.delayCall += ()=>
			{
				Editor.ComponentTooltip.Set(component, headerContent.tooltip, Editor.ModifyOptions.Immediate | Editor.ModifyOptions.DisallowRemoveNameContainer);
				Editor.ComponentName.Set(component, new Editor.NameWithSuffix(headerContent, component), Editor.ModifyOptions.Immediate);
			};
			#endif
		}

		/// <summary>
		/// Sets the name of the <paramref name="component"/>.
		/// <para>
		/// In builds calls to this method are ignored and <see cref="GetName"/> will always return
		/// the <see cref="System.Type.Name">name</see> of the component class.
		/// </para>
		/// </summary>
		/// <param name="component"> The <see cref="Component"/> whose name to set. </param>
		/// <param name="nameAndTooltip"> The new name and Inspector tooltip for the <paramref name="component"/>. </param>
		/// <param name="classNameSuffix">
		/// If <see langword="true"/> then the default name of the component (which is usually the name
		/// of the component class) will be shown in the Inspector as a suffix after the name.
		/// </param>
		[Conditional("UNITY_EDITOR")]
		public static void SetName([DisallowNull] this Component component, GUIContent nameAndTooltip, bool classNameSuffix)
		{
			#if UNITY_EDITOR
			UnityEditor.EditorApplication.delayCall += ()=>
			{
				Editor.ComponentTooltip.Set(component, nameAndTooltip.tooltip, Editor.ModifyOptions.Immediate | Editor.ModifyOptions.DisallowRemoveNameContainer);
				Editor.ComponentName.Set(component, nameAndTooltip.text, classNameSuffix, Editor.ModifyOptions.Immediate);
			};
			#endif
		}

		/// <summary>
		/// Sets the name of the <paramref name="component"/>.
		/// <para>
		/// In builds calls to this method are ignored and <see cref="GetName"/> will always return
		/// the <see cref="System.Type.Name">name</see> of the component class.
		/// </para>
		/// </summary>
		/// <param name="component"> The <see cref="Component"/> whose name to set. </param>
		/// <param name="name"> The new name for the <paramref name="component"/>. </param>
		/// <param name="suffix">
		/// Auxiliary suffix text to show in the Inspector after the <paramref name="name"/>.
		/// <para>
		/// If this is <see langword="null"/> or <see cref="string.Empty"/> then no suffix
		/// will be shown after the name in the Inspector.
		/// </para>
		/// </param>
		/// <param name="tooltip"> The new Inspector tooltip for the <paramref name="component"/>. </param>
		[Conditional("UNITY_EDITOR")]
		public static void SetName([DisallowNull] this Component component, string name, [MaybeNull] string suffix, string tooltip)
		{
			#if UNITY_EDITOR
			UnityEditor.EditorApplication.delayCall += ()=>
			{
				Editor.ComponentTooltip.Set(component, tooltip, Editor.ModifyOptions.Immediate | Editor.ModifyOptions.DisallowRemoveNameContainer);
				Editor.ComponentName.Set(component, name, suffix, Editor.ModifyOptions.Immediate);
			};
			#endif
		}
    }
}