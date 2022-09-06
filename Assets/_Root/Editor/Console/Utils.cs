using System;
using System.Collections.Generic;
using System.Linq;

namespace Needle.Console
{
	internal static class Utils
	{
		public static int IndexOf<T>(this IEnumerable<T> list, Predicate<T> condition)
		{
			var index = -1;
			var found = list.Any(x =>
			{
				index++;
				return condition(x);
			});
			if (found) return index;
			return -1;
		}

		public static string SanitizeMenuItemText(this string txt)
		{
			return txt.Replace("/", "_"); //.Replace("\n", "").Replace("\t", "");
		}

		public static bool TryFindIndex<T>(this List<T> list, T el, out int index)
		{
			for (var i = 0; i < list.Count; i++)
			{
				if (list[i].Equals(el))
				{
					index = i;
					return true;
				}
			}

			index = -1;
			return false;
		}
	}
}