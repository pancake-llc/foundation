using System.Collections.Generic;
using UnityEngine;
using System.Reflection;

namespace Sisus.Init
{
	/// <summary>
	/// Specifies the different possible states that <see cref="Any{T}"/> or an initializer have
	/// in terms of having one or more missing initialization arguments for its clients.
	/// </summary>
	public enum NullGuardResult
	{
		/// <summary>
		/// No arguments are null.
		/// </summary>
		Passed,

		/// <summary>
		/// One or more arguments are null.
		/// </summary>
		ValueMissing,

		/// <summary>
		/// One or more value providers have invalid state that needs to be fixed, or they will not be able to provide a value at runtime.
		/// </summary>
		[Tooltip("This value provider has invalid state that needs to be fixed, or it will not be able to provide a value at runtime.")]
		InvalidValueProviderState,

		/// <summary>
		/// No arguments are null, but one or more arguments are a value provider which will
		/// not be able to provide a value at runtime.
		/// </summary>
		[Tooltip("This value provider will not be able to provide a value at runtime.")]
		ValueProviderValueMissing,

		/// <summary>
		/// An exception was encountered while trying to retrieve one or more arguments.
		/// </summary>
		[Tooltip("An exception was thrown while trying to retrieve the value of this value provider.")]
		ValueProviderException,

		/// <summary>
		/// An exception was encountered while trying to retrieve one or more arguments.
		/// </summary>
		[Tooltip("An exception was thrown by the client when receiving the arguments.")]
		ClientException,

		/// <summary>
		/// No arguments are null, but one or more arguments are a value provider which has
		/// a null return value at this time. They might still be able to provide a value
		/// at runtime.
		/// </summary>
		[Tooltip("This value provider was not able to provide a value in edit mode. It might still be able to provide a value at runtime.")]
		ValueProviderValueNullInEditMode,

		/// <summary>
		/// The initializer is not a valid client for receiving services from the value provider.
		/// <para>
		/// The value provider could for example only offer services to <see cref="Clients"/>
		/// within a particular scene or transform hierarchy.
		/// </para>
		/// </summary>
		[Tooltip("The initializer is not a valid client for receiving services from this value provider.\n\n" +
		"The value provider might for example only offer services to clients within a particular scene or transform hierarchy.")]
		ClientNotSupported,

		/// <summary>
		/// The value provider does not support providing a service matching the argument type.
		/// </summary>
		[Tooltip("The value provider does not support providing a service matching the argument type.")]
		TypeNotSupported
	}

	public static class NullGuardResultExtensions
	{
		private static readonly Dictionary<NullGuardResult, string> tooltips;

		static NullGuardResultExtensions()
		{
			var enumMembers = typeof(NullGuardResult).GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly);
			int count = enumMembers.Length;
			tooltips = new(count);
			for(int i = 0; i < count; i++)
			{
				var member = enumMembers[i];
				if(member.FieldType == typeof(NullGuardResult))
				{
					tooltips[(NullGuardResult)member.GetValue(null)] = member.GetCustomAttribute<TooltipAttribute>() is TooltipAttribute attribute
						? attribute.tooltip
						: "";
				}
			}
		}

		public static NullGuardResult Join(this NullGuardResult previous, NullGuardResult next) => previous switch
		{
			NullGuardResult.Passed => next,
			NullGuardResult.ValueProviderValueNullInEditMode => next is NullGuardResult.Passed ? previous : next,
			NullGuardResult.ValueProviderValueMissing => next is NullGuardResult.Passed or NullGuardResult.ValueProviderValueNullInEditMode ? previous : next,
			_ => previous
		};

		public static string GetTooltip(this NullGuardResult result) => tooltips[result];
	}
}