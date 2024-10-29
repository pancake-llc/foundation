using System;
using UnityEngine;
using System.Diagnostics.CodeAnalysis;
using Sisus.Init.Serialization;
using Object = UnityEngine.Object;

namespace Sisus.Init.Internal
{
	/// <summary>
	/// Defines a single service that derives from <see cref="Object"/> as well
	/// as the defining type of the services which its clients can use to retrieving the service instance.
	/// <para>
	/// Used by both the <see cref="Services"/> components
	/// </para>
	/// </summary>
	[Serializable]
	public sealed class ServiceDefinition
	{
		#pragma warning disable CS0649
		public Object service;

		public _Type definingType = new();
		#pragma warning restore CS0649

		public ServiceDefinition(Object service, _Type definingType)
		{
			this.service = service;	
			this.definingType = definingType;	
		}

		public ServiceDefinition(Object service, Type definingType)
		{
			this.service = service;
			this.definingType = new _Type(definingType);
		}

		internal int GetStateBasedHashCode() => service ? service.GetHashCode() ^ definingType.GetHashCode() : 0;
		
		#if UNITY_EDITOR
		public static void OnValidate(Object obj, ref ServiceDefinition definition)
		{
			if(!definition.service)
			{
				return;
			}

			if(definition.definingType.Value == null)
			{
				UnityEditor.Undo.RecordObject(obj, "Set Service Defining Type");
				definition.definingType = new _Type(definition.service.GetType());
			}
			else if(!IsAssignableFrom(definition.definingType.Value, definition.service))
			{
				#if DEV_MODE
				Debug.Log($"Service {TypeUtility.ToString(definition.service.GetType())} can not be cast to defining type {TypeUtility.ToString(definition.definingType.Value)}. Setting to null.", obj);
				#endif

				definition.definingType.Value = null;
			}
		}

		private static bool IsAssignableFrom([DisallowNull] Type definingType, [DisallowNull] Object service)
		{
			var serviceType = service.GetType();
			if(definingType.IsAssignableFrom(serviceType))
			{
				return true;
			}

			foreach(var @interface in serviceType.GetInterfaces())
			{
				if(@interface.IsGenericType && !@interface.IsGenericTypeDefinition && @interface.GetGenericTypeDefinition() == typeof(IValueProvider<>))
				{
					var providedValueType = @interface.GetGenericArguments()[0];
					if(definingType.IsAssignableFrom(providedValueType))
					{
						return true;
					}
				}
			}

			return false;
		}
		#endif
	}
}