using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using PackageInfo = UnityEditor.PackageManager.PackageInfo;

namespace Needle.Console
{
	[Serializable]
	public class PackageFilter : FilterBase<string>
	{
		private static PackageInfo[] allPackages;
		private static Dictionary<string, int> filePackageDict = new Dictionary<string, int>();

		public PackageFilter(ref List<FilterEntry> packages) : base(ref packages)
		{
		}

		protected override void OnChanged()
		{
			base.OnChanged();
			filePackageDict.Clear();
		}

		public override string GetLabel(int index)
		{
			return this[index];
		}

		protected override (FilterResult result, int index) OnFilter(string message, int mask, int row, LogEntryInfo info)
		{
			if (Count <= 0) return (FilterResult.Keep, -1);
			
			var index = -1;
			if (Count > 0 && !filePackageDict.TryGetValue(info.file, out index))
			{
				index = -1;
				if (TryGetPackage(info.file, out var package))
				{
					for (var i = 0; i < Count; i++)
					{
						var filtered = this[i];
						if (filtered == package.name)
						{
							index = i;
							break;
						}
					}
				}

				filePackageDict.Add(info.file, index);
			}

			if (index == -1) return (FilterResult.Keep, -1);
			if (IsSoloAtIndex(index)) return (FilterResult.Solo, index);
			if (IsActiveAtIndex(index)) return (FilterResult.Exclude, index);
			return (FilterResult.Keep, -1);
		}

		protected override bool MatchFilter(string entry, int index, string message, int mask, int row, LogEntryInfo info)
		{
			// custom implementation of Filter, should never be called
			return false;
		}

		internal static bool TryGetPackage(string path, out PackageInfo package)
		{
			if (allPackages == null)
			{
#if UNITY_2022_1_OR_NEWER
				allPackages = PackageInfo.GetAllRegisteredPackages();
#else
				allPackages = PackageInfo.GetAll();
#endif
				if (allPackages == null)
				{
					package = null;
					return false;
				}
			}

			if (string.IsNullOrWhiteSpace(path))
			{
				package = null;
				return false;
			}

			try
			{
				path = Path.GetFullPath(path);
			}
			catch (ArgumentException)
			{
				package = null;
				return false;
			}

			foreach (var pack in allPackages)
			{
				var pp = pack.resolvedPath;
				if (path.StartsWith(pp))
				{
					package = pack;
					return true;
				}
			}

			package = null;
			return false;
		}


		public override void AddLogEntryContextMenuItems(GenericMenu menu, LogEntryInfo clickedLog, string preview)
		{
			if (TryGetPackage(clickedLog.file, out var pack))
			{
				var str = pack.name;
				var text = "Package " + str;
				AddContextMenuItem_Hide(menu, HideMenuItemPrefix + text, str);
				AddContextMenuItem_Solo(menu, SoloMenuItemPrefix + text, str);
			}
		}
	}
}