using System;
using UnityEditor;

namespace Sisus.Init.EditorOnly
{
	internal sealed class InitializableT10EditorDecorator : InitializableEditorDecorator
	{
		protected override Type BaseTypeDefinition => typeof(IInitializable<,,,,,,,,,>);
		public InitializableT10EditorDecorator(Editor decoratedEditor) : base(decoratedEditor) { }
	}
}