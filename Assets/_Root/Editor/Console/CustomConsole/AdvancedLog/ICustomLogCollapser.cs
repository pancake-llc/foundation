using System.Collections.Generic;
using UnityEditor;

namespace Needle.Console
{
	internal interface ICustomLogCollapser
	{
		// int GetCount(int index);
		bool OnHandleLog(LogEntry entry, int row, string preview, List<CachedConsoleInfo> entries);
	}
}