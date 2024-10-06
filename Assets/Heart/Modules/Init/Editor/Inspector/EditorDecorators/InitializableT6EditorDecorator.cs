using System;
using UnityEditor;

namespace Sisus.Init.EditorOnly
{
	internal sealed class InitializableT6EditorDecorator : InitializableEditorDecorator
	{
		protected override Type BaseTypeDefinition => typeof(IInitializable<,,,,,>);
		public InitializableT6EditorDecorator(Editor decoratedEditor) : base(decoratedEditor) { }
	}
}