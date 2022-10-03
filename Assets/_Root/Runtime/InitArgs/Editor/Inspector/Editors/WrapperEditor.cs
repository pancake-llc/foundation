#define DEBUG_WRAPPED_SCRIPT

using System;
using JetBrains.Annotations;
using UnityEditor;

namespace Pancake.Init.EditorOnly
{
    [CustomEditor(typeof(Wrapper<>), true, isFallback=true), CanEditMultipleObjects]
    public sealed class WrapperEditor : InitializableEditor
    {
		private bool hasCustomEditor;

        protected override Type BaseTypeDefinition => typeof(Wrapper<>);
        private object WrappedObject => (target as IWrapper).WrappedObject;

		protected override void Setup()
		{
			hasCustomEditor = InitializableEditorInjector.HasCustomEditor(target.GetType());
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