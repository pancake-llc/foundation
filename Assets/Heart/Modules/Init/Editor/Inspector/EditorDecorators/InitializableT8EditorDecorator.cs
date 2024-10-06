using System;
using UnityEditor;

namespace Sisus.Init.EditorOnly
{
	internal sealed class InitializableT8EditorDecorator : InitializableEditorDecorator
	{
		protected override Type BaseTypeDefinition => typeof(IInitializable<,,,,,,,>);
		public InitializableT8EditorDecorator(Editor decoratedEditor) : base(decoratedEditor) { }
	}
}