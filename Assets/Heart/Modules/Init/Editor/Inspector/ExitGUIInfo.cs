#define DEBUG_ENABLED

using System.Diagnostics.CodeAnalysis;
using UnityEditor;

namespace Sisus.Init.EditorOnly.Internal
{
	public readonly struct ExitGUIInfo
	{
		public static readonly ExitGUIInfo None = new();

		public bool WasThrownThisFrame { get; }
		[MaybeNull] public Editor Thrower { get; }

		public ExitGUIInfo(Editor thrower)
		{
			WasThrownThisFrame = true;
			Thrower = thrower;
		}

		public static implicit operator bool(ExitGUIInfo info) => info.WasThrownThisFrame;
		public static implicit operator Editor(ExitGUIInfo info) => info.Thrower;
	}
}