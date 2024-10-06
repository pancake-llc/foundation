using System;
using UnityEditor;

namespace Sisus.Init.EditorOnly
{
	internal sealed class InitializableT4EditorDecorator : InitializableEditorDecorator
	{
		protected override Type BaseTypeDefinition => typeof(IInitializable<,,,>);
		public InitializableT4EditorDecorator(Editor decoratedEditor) : base(decoratedEditor) { }
	}
}