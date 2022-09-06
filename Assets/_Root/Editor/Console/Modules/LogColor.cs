
using System;
using UnityEngine;

namespace Needle.Console
{
	internal static class LogColor
	{
		public static void CalcLogColor(string key, ref string str)
		{
			var col = GetColor(key, out var t);
			// str += " " + t;
			var hex = ColorUtility.ToHtmlStringRGB(col);
			str = "<color=#" + hex + ">" + str + "</color>";
		}
		
		public static Color GetColor(string str, out float hue)
		{
			hue = CalculateHash(str, 333) % 1f;// + .05f;
			var b = CalculateHash(str, 100) % 3;
			b = 0.5f + Mathf.Max(.7f, b);
			// var distToGreen = hue - .4f;
			// hue += distToGreen;// * 2f;// .3f;
			// hue = Mathf.Abs(hue);
			// hue %= 1f;
			var col = Color.HSVToRGB(hue, .8f, b);
			return col;
		}

		private static float CalculateHash(string read, float factor)
		{
			var hashedValue = 0f;
			foreach (var t in read)
			{
				hashedValue += factor * t * t / (float)char.MaxValue;
			}
			return (hashedValue * read.Length);
		}
	}
}