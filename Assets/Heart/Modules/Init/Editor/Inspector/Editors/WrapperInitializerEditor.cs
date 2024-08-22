using System;
using System.Linq;
using UnityEditor;

namespace Sisus.Init.EditorOnly
{
	[CanEditMultipleObjects]
	public class WrapperInitializerEditor : InitializerEditor
	{
		protected override Type[] GetInitArgumentTypes(Type[] genericArguments) => genericArguments.Skip(2).ToArray();
	}
}