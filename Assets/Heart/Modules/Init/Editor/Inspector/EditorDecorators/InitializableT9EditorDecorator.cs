using System;
using UnityEditor;

namespace Sisus.Init.EditorOnly
{
	internal sealed class InitializableT9EditorDecorator : InitializableEditorDecorator
	{
		protected override Type BaseTypeDefinition => typeof(IInitializable<,,,,,,,,>);
		public InitializableT9EditorDecorator(Editor decoratedEditor) : base(decoratedEditor) { }
	}
}