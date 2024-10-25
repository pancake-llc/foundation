#define DEBUG_WRAPPED_SCRIPT

using System;
using System.Diagnostics.Contracts;
using UnityEditor;
using Object = UnityEngine.Object;

namespace Sisus.Init.EditorOnly.Internal
{
	internal sealed class WrapperEditorDecorator : InitializableEditorDecorator
	{
		// if false, then could (subtly) inform user about:
		// - target is non-serializable
		// - is target null or not
		// - MAYBE visualize target state?
		// Easiest way would be to draw state via runtime fields drawer. This way:
		// - can see if null or not
		// - fields being readonly indicates that it's non-serializable
		// => just need a way to override DrawRuntimeFields + header used
		private bool isWrappedObjectSerializable;

		protected override Type BaseTypeDefinition => typeof(Wrapper<>);
		private object WrappedObject => GetInitializable(target);

		public WrapperEditorDecorator(Editor decoratedEditor) : base(decoratedEditor)
			=> isWrappedObjectSerializable = WrapperUtility.TryGetGetWrappedObjectType(target, out Type wrappedObjectType) && wrappedObjectType.IsSerializable;

		protected override object GetInitializable(Object inspectedTarget) => ((IWrapper)inspectedTarget).WrappedObject;

		[Pure]
		protected override RuntimeFieldsDrawer CreateRuntimeFieldsDrawer()
			=> WrappedObject != null ? new RuntimeFieldsDrawer(WrappedObject, typeof(object)) : base.CreateRuntimeFieldsDrawer();

		public override void OnAfterInspectorGUI()
		{
			base.OnAfterInspectorGUI();

			if(!DecoratingDefaultOrOdinEditor)
			{
				return;
			}

			if(SerializedObject?.FindProperty("wrapped") is not { } wrappedObjectProperty
			   || !wrappedObjectProperty.NextVisible(true))
			{
				return;
			}

			SerializedObject.Update();

			do
			{
				EditorGUILayout.PropertyField(wrappedObjectProperty, true);
			}
			while(wrappedObjectProperty.NextVisible(false));

			SerializedObject.ApplyModifiedProperties();
		}
	}
}