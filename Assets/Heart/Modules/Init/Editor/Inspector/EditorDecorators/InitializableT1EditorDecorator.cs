using System;
using UnityEditor;

namespace Sisus.Init.EditorOnly
{
	internal sealed class InitializableT1EditorDecorator : InitializableEditorDecorator
	{
		protected override Type BaseTypeDefinition => typeof(IInitializable<>);
		public InitializableT1EditorDecorator(Editor decoratedEditor) : base(decoratedEditor) { }
	}
}