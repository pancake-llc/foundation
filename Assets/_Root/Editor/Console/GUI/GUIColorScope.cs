using System;
using UnityEngine;

namespace Needle.Console
{
	internal readonly struct GUIColorScope : IDisposable
	{
		private readonly Color prev;
		
		public GUIColorScope(Color col)
		{
			prev = GUI.color;
			GUI.color = col;
		}
		
		public void Dispose()
		{
			GUI.color = prev;
		}
	}
}