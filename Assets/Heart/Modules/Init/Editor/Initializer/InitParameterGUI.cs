using System;
using System.Reflection;
using Sisus.Shared.EditorOnly;
using UnityEditor;
using UnityEngine;

namespace Sisus.Init.EditorOnly.Internal
{
	internal sealed class InitParameterGUI : IDisposable
	{
		public static InitParameterGUI NowDrawing { get; private set; }

		public readonly SerializedProperty anyProperty;
		private readonly GUIContent label;
		private readonly bool isService;

		public Attribute[] Attributes { get; private set; }
		public bool NullArgumentGuardActive { get; private set; }

		public InitParameterGUI(GUIContent label, SerializedProperty anyProperty, Type valueType, Attribute[] attributes = null)
		{
			NowDrawing = this;

			this.label = label;
			this.anyProperty = anyProperty;
			Attributes = attributes;
			isService = ServiceUtility.TryGetFor(anyProperty.serializedObject.targetObject, valueType, out _);

			NowDrawing = null;
		}

		/// <summary>
		/// Draws Init argument field of an Initializer.
		/// </summary>
		/// <param name="anyProperty"> The <see cref="Any{T}"/> type field that holds the argument. </param>
		/// <param name="label"> Label for the Init argument field. </param>
		/// <param name="customDrawer">
		/// Custom property drawer that was defined for the field via a PropertyAttribute added to a field
		/// on an Init class nested inside the Initializer.
		/// <para>
		/// <see langword="null"/> if no custom drawer has been defined, in which case <see cref="AnyPropertyDrawer"/>
		/// is used to draw the field instead.
		/// </para>
		/// </param>
		/// <param name="isService">
		/// Is the argument a service?
		/// <para>
		/// If <see langword="true"/> then the field is drawn as a service tag.
		/// </para>
		/// </param>
		/// <param name="canBeNull">
		/// Is the argument allowed to be null or not?
		/// <para>
		/// If <see langword="false"/>, then the field will be tinted red if it has a null value.
		/// </para>
		/// </param>
		public void DrawArgumentField(bool canBeNull, bool servicesShown)
		{
			NowDrawing = this;
			NullArgumentGuardActive = !canBeNull;

			bool hasSerializedValue;
			if(anyProperty != null)
			{
				anyProperty.serializedObject.Update();
				var any = anyProperty.GetValue();
				var anyType = any.GetType();
				hasSerializedValue = (bool)anyType.GetMethod("HasSerializedValue", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).Invoke(any, null);
			}
			else
			{
				hasSerializedValue = false;
			}

			if(isService && !servicesShown && !hasSerializedValue)
			{
				NowDrawing = null;
				return;
			}

			EditorGUILayout.PropertyField(anyProperty, label);

			NowDrawing = null;
		}

		public void Dispose()
		{
			GC.SuppressFinalize(this);
			AnyPropertyDrawer.Dispose(anyProperty);
		}

		~InitParameterGUI() => AnyPropertyDrawer.Dispose(anyProperty);
	}
}