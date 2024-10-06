using System;
using UnityEditor;

namespace Sisus.Init.EditorOnly
{
	internal sealed class InitializableT11EditorDecorator : InitializableEditorDecorator
	{
		protected override Type BaseTypeDefinition => typeof(IInitializable<,,,,,,,,,,>);
		public InitializableT11EditorDecorator(Editor decoratedEditor) : base(decoratedEditor) { }
	}
}