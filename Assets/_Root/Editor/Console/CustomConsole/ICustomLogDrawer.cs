using System;
using System.Collections.Generic;
using UnityEngine;

namespace Needle.Console
{
	public interface ICustomLogDrawer
	{
		float GetContentHeight(float defaultRowHeight, int totalRows, out uint linesHandled);
		bool OnDrawStacktrace(int index, string rawText, Rect rect);
		bool OnDrawEntry(int index, bool isSelected, Rect rect, bool visible, out float height);
	}
}