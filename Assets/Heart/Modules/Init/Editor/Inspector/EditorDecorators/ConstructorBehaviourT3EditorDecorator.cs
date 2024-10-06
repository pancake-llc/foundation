	using System;
	using UnityEditor;

	namespace Sisus.Init.EditorOnly
	{
		internal sealed class ConstructorBehaviourT3EditorDecorator : BaseConstructorBehaviourEditorDecorator
		{
			protected override Type BaseTypeDefinition => typeof(ConstructorBehaviour<,,>);
			public ConstructorBehaviourT3EditorDecorator(Editor decoratedEditor) : base(decoratedEditor) { }
		}
	}