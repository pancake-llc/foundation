using System;
using System.IO;
using UnityEditor;
using JetBrains.Annotations;

namespace Pancake.Debugging
{
	public static class AssetUtility
	{
		/// <summary>
		/// Returns asset path to asset by exact name and extension if one exists or any empty string if it doesn't exist.
		/// </summary>
		/// <param name="byName"></param>
		/// <returns></returns>
		[NotNull]
		public static string FindByNameAndExtension([NotNull]string byName, [NotNull]string byExtension)
		{
			var guids = AssetDatabase.FindAssets(byName);

			int count = guids.Length;
			if(count == 0)
			{
				return null;
			}

			for(int n = count - 1; n >= 0; n--)
			{
				var guid = guids[n];
				var path = AssetDatabase.GUIDToAssetPath(guid);
				if(string.Equals(Path.GetFileNameWithoutExtension(path), byName, StringComparison.OrdinalIgnoreCase) && string.Equals(Path.GetExtension(path), byExtension, StringComparison.OrdinalIgnoreCase))
				{
					return path;
				}
			}

			return "";
		}
	}
}