using UnityEngine;

namespace Needle.Console
{
	internal static class GUIUtils
	{
		private static Material _simpleColored;
		internal static Material SimpleColored
		{
			get {
				if (!_simpleColored)
				{
					var shader = Shader.Find("Hidden/Internal-Colored");
					_simpleColored = new Material(shader);
					_simpleColored.hideFlags = HideFlags.HideAndDontSave;
					_simpleColored.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
					_simpleColored.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
					_simpleColored.SetInt("_Cull", (int)UnityEngine.Rendering.CullMode.Off);
					_simpleColored.SetInt("_ZWrite", 0);
				}
				return _simpleColored;
			}
		}
	}
}