using System;
using UnityEditor;

namespace Sisus.Init.EditorOnly
{
	internal sealed class InitializableT12EditorDecorator : InitializableEditorDecorator
	{
		protected override Type BaseTypeDefinition => typeof(IInitializable<,,,,,,,,,,,>);
		public InitializableT12EditorDecorator(Editor decoratedEditor) : base(decoratedEditor) { }
	}
}