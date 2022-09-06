namespace Needle.Console
{
	/// <summary>
	/// Add a marker to stacktraces for highlighting in console ignoring everything before
	/// </summary>
	internal static class StacktraceMarkerUtil
	{
		public const string Marker = "";

		public static string RemoveDemystifyStacktraceMarker(this string str)
		{
			return str.Replace(Marker, "");
		}
		
		public static void AddMarker(ref string stacktrace)
		{
			stacktrace = Marker + stacktrace;
		}
		
		public static void RemoveMarkers(ref string stacktrace)
		{
			stacktrace = stacktrace.Replace(Marker, "");
		}
		
		
		public static bool IsPrefix(string line)
		{
			return line.StartsWith(Marker);
		}
	}
}