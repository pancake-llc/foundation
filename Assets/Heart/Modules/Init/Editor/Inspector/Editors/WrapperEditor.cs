#define DEBUG_WRAPPED_SCRIPT

using System;
using System.Diagnostics.Contracts;
using UnityEditor;
using Object = UnityEngine.Object;

namespace Sisus.Init.EditorOnly.Internal
{
	[CanEditMultipleObjects]
	public sealed class WrapperEditor : InitializableEditor
	{
		private bool hasCustomEditor;

		protected override Type BaseTypeDefinition => typeof(Wrapper<>);
		private object WrappedObject => (target as IWrapper).WrappedObject;

		protected override object GetInitializable(Object inspectedTarget) => (inspectedTarget as IWrapper).WrappedObject;

		protected override void Setup()
		{
			hasCustomEditor = CustomEditorType.Exists(target.GetType());
			base.Setup();
		}

		[Pure]
		protected override RuntimeFieldsDrawer CreateRuntimeFieldsDrawer()
		{
			if(WrappedObject != null)
			{
				return new RuntimeFieldsDrawer(WrappedObject, typeof(object));
			}

			return base.CreateRuntimeFieldsDrawer();
		}

		protected override void BaseGUI()
		{
			if(hasCustomEditor)
			{
				base.BaseGUI();
				return;
			}

			var wrappedObjectProperty = serializedObject.FindProperty("wrapped");
			if(wrappedObjectProperty != null && wrappedObjectProperty.NextVisible(true))
			{
				serializedObject.Update();

				do
				{
					EditorGUILayout.PropertyField(wrappedObjectProperty, true);
				}
				while(wrappedObjectProperty.NextVisible(false));

				serializedObject.ApplyModifiedProperties();
			}
		}
	}
}