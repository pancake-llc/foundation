using System;
using UnityEditor;

namespace Sisus.Init.EditorOnly
{
	internal sealed class InitializableT3EditorDecorator : InitializableEditorDecorator
	{
		protected override Type BaseTypeDefinition => typeof(IInitializable<,,>);
		public InitializableT3EditorDecorator(Editor decoratedEditor) : base(decoratedEditor) { }
	}
}