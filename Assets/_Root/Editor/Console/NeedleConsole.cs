using System.IO;
using System.Text;
using System.Threading.Tasks;
using Unity.Profiling;
using UnityEditor;
using UnityEngine;

namespace Needle.Console
{
	public static class NeedleConsole
	{

		[HyperlinkCallback(Href = "OpenNeedleConsoleSettings")]
		private static void OpenNeedleConsoleUserPreferences()
		{
			SettingsService.OpenUserPreferences("Preferences/Needle/Console");
		}
		
		[InitializeOnLoadMethod]
		private static void Init()
		{
			var projectSettings = NeedleConsoleProjectSettings.instance;
			var settings = NeedleConsoleSettings.instance;
			if (projectSettings.FirstInstall)
			{
				async void InstalledLog()
				{
					await Task.Delay(100);
					Enable();
					projectSettings.FirstInstall = false;
					projectSettings.Save();
					Debug.Log(
						$"Thanks for installing Needle Console. You can find Settings under <a href=\"OpenNeedleConsoleSettings\">Edit/Preferences/Needle/Console</a>\n" +
						$"If you discover issues please report them <a href=\"https://github.com/needle-tools/needle-console/issues\">on github</a>\n" +
						$"Also feel free to join <a href=\"https://discord.gg/CFZDp4b\">our discord</a>");
				}

				InstalledLog();
				InstallDefaultTheme();
				async void InstallDefaultTheme()
				{
					while (true)
					{
						try
						{
							if (settings.SetDefaultTheme()) break;
						}
						catch
						{
							// ignore
						}
						await Task.Delay(1_000);
					}
				}
			}

			if (settings.CurrentTheme != null)
			{
				settings.CurrentTheme.EnsureEntries();
				settings.CurrentTheme.SetActive();
			}
		}

		public static void Enable()
		{
			NeedleConsoleSettings.instance.Enabled = true;
			NeedleConsoleSettings.instance.Save();
			Patcher.ApplyPatches();
		}

		public static void Disable()
		{
			NeedleConsoleSettings.instance.Enabled = false;
			NeedleConsoleSettings.instance.Save();
			Patcher.RemovePatches();
		}

		private static readonly StringBuilder builder = new StringBuilder();
		internal static string DemystifyEndMarker = "�";

		public static void Apply(ref string stacktrace)
		{
			try
			{
				using (new ProfilerMarker("Demystify.Apply").Auto())
				{
					string[] lines = null;
					using (new ProfilerMarker("Split Lines").Auto())
						lines = stacktrace.Split('\n');
					var settings = NeedleConsoleSettings.instance;
					var foundPrefix = false;
					var foundEnd = false;
					foreach (var t in lines)
					{
						var line = t;
						if (line == DemystifyEndMarker)
						{
							foundEnd = true;
							builder.AppendLine();
							continue;
						}

						if (foundEnd)
						{
							builder.AppendLine(line);
							continue;
						}

						using (new ProfilerMarker("Remove Markers").Auto())
						{
							if (StacktraceMarkerUtil.IsPrefix(line))
							{
								StacktraceMarkerUtil.RemoveMarkers(ref line);
								if (!string.IsNullOrEmpty(settings.Separator))
									builder.AppendLine(settings.Separator);
								foundPrefix = true;
							}
						}

						if (foundPrefix && settings.UseSyntaxHighlighting)
							SyntaxHighlighting.AddSyntaxHighlighting(ref line);

						var l = line.Trim();
						if (!string.IsNullOrEmpty(l))
						{
							if (!l.EndsWith("\n"))
								builder.AppendLine(l);
							else
								builder.Append(l);
						}
					}

					var res = builder.ToString();
					if (!string.IsNullOrWhiteSpace(res))
					{
						stacktrace = res;
					}

					builder.Clear();
				}
			}
			catch
				// (Exception e)
			{
				// ignore
			}
		}
	}
}
