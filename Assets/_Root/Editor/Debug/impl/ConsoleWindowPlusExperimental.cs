using UnityEngine;

namespace Pancake.Debugging.Console
{
    using JetBrains.Annotations;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Text;
    using System.Text.RegularExpressions;
    using UnityEditor;
    using UnityEditor.Compilation;
    using Object = Object;

    [InitializeOnLoad]
	internal sealed class ConsoleWindowPlusExperimental : ConsoleWindowPlusBase, IHasCustomMenu, ISerializationCallbackReceiver
	{
		[Flags]
		private enum ConsoleFlags
		{
			Collapse = 1 << 0,
			ClearOnPlay = 1 << 1,
			ErrorPause = 1 << 2,
			Verbose = 1 << 3,
			StopForAssert = 1 << 4,
			StopForError = 1 << 5,
			Autoscroll = 1 << 6,
			LogLevelLog = 1 << 7,
			LogLevelWarning = 1 << 8,
			LogLevelError = 1 << 9,
			ShowTimestamp = 1 << 10,
			ClearOnBuild = 1 << 11,
		}

		[Flags]
		internal enum Mode
		{
			Error = 1 << 0,
			Assert = 1 << 1,
			Log = 1 << 2,
			Fatal = 1 << 4,
			DontPreprocessCondition = 1 << 5,
			AssetImportError = 1 << 6,
			AssetImportWarning = 1 << 7,
			ScriptingError = 1 << 8,
			ScriptingWarning = 1 << 9,
			ScriptingLog = 1 << 10,
			ScriptCompileError = 1 << 11,
			ScriptCompileWarning = 1 << 12,
			StickyError = 1 << 13,
			MayIgnoreLineNumber = 1 << 14,
			ReportBug = 1 << 15,
			DisplayPreviousErrorInStatusBar = 1 << 16,
			ScriptingException = 1 << 17,
			DontExtractStacktrace = 1 << 18,
			ShouldClearOnPlay = 1 << 19,
			GraphCompileError = 1 << 20,
			ScriptingAssertion = 1 << 21,
			VisualScriptingError = 1 << 22
		}

		public const double newEntryHighlightDuration = 0.25d;

		private const double MaxEditorStartupTime = 120d;
		private const float DoubleClickThreshold = 0.5f;
		private const string FilterControlName = "SearchFilter";
		
		private static readonly Vector2 iconSizeCompact = new Vector2(16f, 16f);
		private static readonly Vector2 iconSizeNormal = new Vector2(32f, 32f);
		private static readonly char[] spaceChar = new char[] { ' ' };
		private static ConsoleWindowPlusExperimental instance;

		private const float logEntryHeightCompact = 22f;
		private const float logEntryHeightNormal = 38f;

		private const int detailAreaPadding = 5;
		private const float scrollToBottomMinSpeed = 1f;
		private const float resizerHeight = 5f;

		private static readonly GUIContent ClearLabel = new GUIContent("Clear");
		private static readonly GUIContent errorPauseLabel = new GUIContent("Error Pause");
		private static readonly GUIContent editorLabel = new GUIContent("Editor");

		private static bool wasOpenedViaMenuItem;
		private static GUIStyle boxStyle;
		private static GUIStyle textAreaStyle;
		private static Texture2D boxBgOdd;
		private static Texture2D boxBgEven;
		private static Texture2D boxBgSelected;
		private static Texture2D errorIcon;
		private static Texture2D errorIconSmall;
		private static Texture2D warningIcon;
		private static Texture2D warningIconSmall;
		private static Texture2D infoIcon;
		private static Texture2D infoIconSmall;

		[SerializeField]
		private GUIContent infoLabel = new GUIContent("0");
		[SerializeField]
		private GUIContent warningLabel = new GUIContent("0");
		[SerializeField]
		private GUIContent errorLabel = new GUIContent("0");

		[SerializeField]
		private float bottomHeight = 175f;
		[SerializeField]
		private bool collapse = false;
		[SerializeField]
		private bool clearOnPlay = false;
		[SerializeField]
		private bool clearOnBuild = false;
		[SerializeField]
		private bool errorPause = false;
		[SerializeField]
		private bool showLog = true;
		[SerializeField]
		private bool showWarnings = true;
		[SerializeField]
		private bool showErrors = true;
		[SerializeField]
		private bool showTimestamps;
		[SerializeField]
		private Vector2 iconSize = iconSizeNormal;
		[SerializeField]
		private float logEntryHeight = logEntryHeightNormal;

		[SerializeField]
		private Vector2 scrollPosition;

		// These must be serialized because they can be used to detect IsScrolledToBottom during OnEnable.
		[SerializeField]
		private Rect mainPanelWindowRect;
		[SerializeField]
		private Rect mainPanelContentRect;

		[SerializeField]
		private List<Log> logs = new List<Log>(32768);

		[SerializeField]
		private int logsInfo;
		[SerializeField]
		private int logsWarning;
		[SerializeField]
		private int logsError;

		[SerializeField]
		private int selectedVisibleLogIndex = -1;
		[SerializeField]
		private List<Log> visibleLogs = new List<Log>(2500);

		[SerializeField]
		private string filter = "";
		[NonSerialized]
		private string filterTrimmed = "";
		[NonSerialized]
		private bool useRegex = true;
		[NonSerialized]
		private Regex filterRegex = new Regex("");
		[NonSerialized]
		private List<Regex> filterRegexParts = new List<Regex>();
		[NonSerialized]
		private string[] filterParts = new string[0];

		[SerializeField]
		private bool isScrolledToBottom = false;
		[SerializeField]
		private bool scrollToBottomInstantlyNextTick = true;

		[SerializeField]
		private GUIStyle toolbarSearchFieldStyle;
		[SerializeField]
		private GUIStyle toolbarSearchFieldCancelButtonStyle;
		[SerializeField]
		private GUIStyle toolbarSearchFieldCancelButtonEmptyStyle;
		[SerializeField]
		private GUIContent searchIcon;

		[SerializeField]
		private float scrollToBottomSpeed = 1f;
		[SerializeField]
		private float calculatedScrollDistanceToBottom = 0f;
		[SerializeField]
		private bool hasConsoleFlagsToRestore = false;
		[SerializeField]
		private ConsoleFlags consoleFlagsToRestore = default;
		[SerializeField]
		private bool restoreLogEntriesPreviousFilterText = false;
		[SerializeField]
		private string logEntriesPreviousFilterText = "";
		[SerializeField]
		private StackTraceDrawer stackTraceDrawer = new StackTraceDrawer();
		[SerializeField]
		private float heightLastFrame;
		[SerializeField]
		private UseMonospaceFont useMonospaceFont = UseMonospaceFont.Never;

		[NonSerialized]
		private readonly Dictionary<int, int> collapsedLogCountByHash = new Dictionary<int, int>();
		[NonSerialized]
		private int[] collapsedLogCountByHashKeys = new int[0];
		[NonSerialized]
		private int[] collapsedLogCountByHashValues = new int[0];
		[NonSerialized]
		private bool wasBuildingPlayerLastFrame = false;
		[NonSerialized]
		private bool wasCompilingLastFrame = false;

		[NonSerialized]
		private readonly List<Log> compileErrors = new List<Log>();
		[NonSerialized]
		private readonly List<Log> compileWarnings = new List<Log>();

		[NonSerialized]
		private readonly StringBuilder stringBuilder = new StringBuilder();
		[NonSerialized]
		private int clickCount = 0;
		[NonSerialized]
		private double lastClickTime = -1d;
		[NonSerialized]
		private double lastScrollTime = -1d;
		[NonSerialized]
		private bool mouseMovedAfterMouseDown = false;
		[NonSerialized]
		private bool mouseIsDown = false;
		[NonSerialized]
		private Vector2 mouseDownPosition = new Vector2(-100000f, 100000f);
		[NonSerialized]
		private bool resizing = false;

		[NonSerialized]
		private Color lineColor;
		[NonSerialized]
		private KeyValuePair<Log, bool> mouseoveredLogStackTraceContainsUnityObjectType = new KeyValuePair<Log, bool>();
		[NonSerialized]
		private bool initialMessagesFetched = false;
		[NonSerialized]
		float timeUntilShouldFetchInitialMessages = 1f;
		[NonSerialized]
		private bool firstMessageReceived = false;
		[NonSerialized]
		private int focusFilterField = 3;

		protected override string Title
		{
			get
			{
				return "Console +";
			}
		}

		private int MaxLineCount => compactMode ? 1 : 2;

		private Log SelectedLog
		{
			get
			{
				if(selectedVisibleLogIndex == -1)
				{
					return null;
				}

				if(visibleLogs.Count <= selectedVisibleLogIndex)
				{
					return null;
				}

				return visibleLogs[selectedVisibleLogIndex];
			}

			set
			{
				SetSelectedLogIndex(value == null ? -1 : visibleLogs.IndexOf(value));
			}
		}

		private float EffectiveBottomHeight
		{
			get
			{
				if(selectedVisibleLogIndex == -1 && DebugLogExtensionsPreferences.HideDetailAreaWhenNoLogEntrySelected)
				{
					return 0f;
				}

				float result = bottomHeight;
				if(result <= 30f)
				{
					return 30f;
				}

				float minSpaceForEverythingElse = Mathf.Min(position.height * 0.5f, 100f);
				float totalHeight = position.height;
				float spaceLeftForEverythingElse = totalHeight - bottomHeight;
				if(spaceLeftForEverythingElse < minSpaceForEverythingElse)
				{
					return totalHeight - minSpaceForEverythingElse;
				}

				return bottomHeight;
			}
		}

		public static void Open()
		{
			wasOpenedViaMenuItem = true;
			GetWindow<ConsoleWindowPlusExperimental>();
		}

		[UsedImplicitly]
		public static void ClearMessages()
		{
			if(instance != null)
			{
				instance.Clear(true);
			}
		}

		private static bool JustLaunchedEditor()
		{
			if(wasOpenedViaMenuItem)
			{
				return false;
			}

			return !wasOpenedViaMenuItem && EditorApplication.timeSinceStartup < MaxEditorStartupTime;
		}

		private void FetchInitialLogEntries()
		{
			compileErrors.Clear();

			var logEntriesType = typeof(EditorApplication).Assembly.GetType("UnityEditor.LogEntries");
			if(logEntriesType == null)
			{
				Debug.LogWarning("Type UnityEditor.LogEntries not found.");
				return;
			}

			var logEntryType = typeof(EditorApplication).Assembly.GetType("UnityEditor.LogEntry");
			if(logEntryType == null)
			{
				Debug.LogWarning("Type UnityEditor.LogEntry not found.");
				return;
			}

			var startGettingEntries = logEntriesType.GetMethod("StartGettingEntries", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
			if(startGettingEntries == null)
			{
				Debug.LogWarning("Method UnityEditor.LogEntries.StartGettingEntries not found.");
				return;
			}

			var endGettingEntries = logEntriesType.GetMethod("EndGettingEntries", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
			if(endGettingEntries == null)
			{
				Debug.LogWarning("Method UnityEditor.LogEntries.EndGettingEntries not found.");
				return;
			}

			var messageField = logEntryType.GetField("message", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			if(messageField == null)
			{
				Debug.LogWarning("Field UnityEditor.LogEntry.message not found.");
				return;
			}

			var fileField = logEntryType.GetField("file", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			if(fileField == null)
			{
				Debug.LogWarning("Field UnityEditor.LogEntry.file not found.");
				return;
			}

			var modeField = logEntryType.GetField("mode", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			if(modeField == null)
			{
				Debug.LogWarning("Field UnityEditor.LogEntry.mode not found.");
				return;
			}

			var instanceIDField = logEntryType.GetField("instanceID", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			if(instanceIDField == null)
			{
				Debug.LogWarning("Field UnityEditor.LogEntry.instanceID not found.");
				return;
			}

			var getCount = logEntriesType.GetMethod("GetCount", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
			int count = (int)getCount.Invoke(null, null);

			for(int i = logs.Count - 1; i >= 0; i--)
			{
				if(logs[i].isCompileErrorOrWarning)
				{
					logs.RemoveAt(i);
				}
			}

			startGettingEntries.Invoke(null, null);

			bool scriptCompilationFailed = EditorUtility.scriptCompilationFailed;

			var getEntry = logEntriesType.GetMethod("GetEntryInternal", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
			object[] args = new object[] { 0, Activator.CreateInstance(logEntryType) };
			for(int i = 0; i < count; i++)
			{
				args[0] = i;
				getEntry.Invoke(null, args);
				object logEntry = args[1];
				string message = messageField.GetValue(logEntry) as string;
				string file = (string)fileField.GetValue(logEntry);
				Mode mode = (Mode)(int)modeField.GetValue(logEntry);
				int instanceID = (int)instanceIDField.GetValue(logEntry);
				Object instance = instanceID > 0 ? EditorUtility.InstanceIDToObject(instanceID) : null;

				var type = GetLogTypeFromMode(mode);

				if(mode.HasFlag(Mode.ScriptCompileError))
				{
					compileErrors.Add(new Log(message.Trim(), "", LogType.Error, instance, true));
					continue;
				}

				string stackTrace;
				int messageEnd = message.IndexOf("UnityEngine.Debug:", StringComparison.Ordinal);
				if(messageEnd == -1)
				{
					int at = message.IndexOf(" (at ");
					stackTrace = "";
					if(at != -1)
					{
						int stackTraceStart = message.LastIndexOf('\n', at);
						if(stackTraceStart != -1)
						{
							stackTrace = message.Substring(stackTraceStart + 1);
							message = message.Substring(0, stackTraceStart).TrimEnd();
						}
					}
				}
				else if(message.Length > messageEnd + 18 + 4)
				{
					stackTrace = message.Substring(messageEnd);
					message = message.Substring(0, messageEnd).TrimEnd();
				}
				else
				{
					stackTrace = "";
				}

				switch(type)
				{
					case LogType.Error:
					case LogType.Assert:
					case LogType.Exception:
						logsError++;
						break;
					case LogType.Warning:
						logsWarning++;
						break;
					default:
						logsInfo++;
						break;
				}

				var log = new Log(message, stackTrace, type, instance, true);
				logs.Add(log);
			}

			endGettingEntries.Invoke(null, null);

			if(restoreLogEntriesPreviousFilterText)
			{
				var setFilteringText = logEntriesType.GetMethod("SetFilteringText", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
				if(setFilteringText != null)
				{
					setFilteringText.Invoke(null, new object[] { logEntriesPreviousFilterText });
					restoreLogEntriesPreviousFilterText = false;
					logEntriesPreviousFilterText = "";
				}
				else
				{
					Debug.LogWarning("Method UnityEditor.LogEntries.SetFilteringText not found.");
					restoreLogEntriesPreviousFilterText = false;
					logEntriesPreviousFilterText = "";
				}
			}

			if(hasConsoleFlagsToRestore)
			{
				var consoleFlagsProperty = logEntriesType.GetProperty("consoleFlags", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
				if(consoleFlagsProperty != null)
				{
					consoleFlagsProperty.SetValue(null, consoleFlagsToRestore);
				}
				else
				{
					Debug.LogWarning("Property UnityEditor.LogEntries.consoleFlags not found.");
				}
				hasConsoleFlagsToRestore = false;
			}

			int errorCount = logsError + compileErrors.Count;
			errorLabel.text = Mathf.Min(errorCount, 9999).ToString();
			warningLabel.text = Mathf.Min(logsWarning, 9999).ToString();
			infoLabel.text = Mathf.Min(logsInfo, 9999).ToString();

			RebuildVisibleLogs(true);

			SetSelectedLogIndex(-1);

			if(errorPause && logsError > 0 && Application.isPlaying)
			{
				Debug.Break();
			}
		}

		[UsedImplicitly]
		protected override void OnEnable()
		{
			instance = this;

			if(JustLaunchedEditor())
			{
				OnApplicationStart();
			}

			ApplyFilter(false);

			minSize = new Vector2(210f, 160f);

			infoIcon = EditorGUIUtility.Load("icons/console.infoicon.png") as Texture2D;
			warningIcon = EditorGUIUtility.Load("icons/console.warnicon.png") as Texture2D;
			errorIcon = EditorGUIUtility.Load("icons/console.erroricon.png") as Texture2D;

			infoIconSmall = EditorGUIUtility.Load("icons/console.infoicon.sml.png") as Texture2D;
			warningIconSmall = EditorGUIUtility.Load("icons/console.warnicon.sml.png") as Texture2D;
			errorIconSmall = EditorGUIUtility.Load("icons/console.erroricon.sml.png") as Texture2D;

			if(EditorGUIUtility.isProSkin)
			{
				titleContent = new GUIContent(Title, EditorGUIUtility.Load("d_UnityEditor.ConsoleWindow") as Texture);
			}
			else
			{
				titleContent = new GUIContent(Title, EditorGUIUtility.Load("UnityEditor.ConsoleWindow") as Texture);
			}

			boxStyle = new GUIStyle();
			boxStyle.stretchHeight = false;
			boxStyle.stretchWidth = false;
			boxStyle.alignment = TextAnchor.MiddleLeft;
			boxStyle.contentOffset = new Vector2(5f, 0f);
			boxStyle.clipping = TextClipping.Clip;

			if(EditorGUIUtility.isProSkin)
			{
				boxStyle.normal.textColor = new Color(0.8f, 0.8f, 0.8f);
				boxBgOdd = EditorGUIUtility.Load("builtin skins/darkskin/images/cn entrybackodd.png") as Texture2D;
				boxBgEven = EditorGUIUtility.Load("builtin skins/darkskin/images/cnentrybackeven.png") as Texture2D;
				boxBgSelected = EditorGUIUtility.Load("builtin skins/darkskin/images/menuitemhover.png") as Texture2D;
			}
			else
			{
				boxStyle.normal.textColor = new Color(0f, 0f, 0f);
				boxBgOdd = EditorGUIUtility.Load("builtin skins/lightskin/images/cn entrybackodd.png") as Texture2D;
				boxBgEven = EditorGUIUtility.Load("builtin skins/lightskin/images/cnentrybackeven.png") as Texture2D;
				boxBgSelected = EditorGUIUtility.Load("builtin skins/lightskin/images/menuitemhover.png") as Texture2D;
			}

			textAreaStyle = new GUIStyle();
			if(EditorGUIUtility.isProSkin)
			{
				textAreaStyle.normal.background = EditorGUIUtility.Load("builtin skins/darkskin/images/projectbrowsericonareabg.png") as Texture2D;
				textAreaStyle.normal.textColor = new Color(0.9f, 0.9f, 0.9f);
			}
			else
			{
				textAreaStyle.normal.background = EditorGUIUtility.Load("builtin skins/lightskin/images/projectbrowsericonareabg.png") as Texture2D;
				textAreaStyle.normal.textColor = new Color(0f, 0f, 0f);
			}
			textAreaStyle.richText = true;
			textAreaStyle.clipping = TextClipping.Clip;
			textAreaStyle.padding = new RectOffset(detailAreaPadding, detailAreaPadding, detailAreaPadding, detailAreaPadding);
			textAreaStyle.wordWrap = true;

			lineColor = EditorGUIUtility.isProSkin ? Color.black : Color.grey;

			compileWarnings.Clear();

			isScrolledToBottom = visibleLogs.Count == 0 || IsScrolledToBottom();

			stackTraceDrawer.OnEnable(textAreaStyle, SelectedLog, lineColor);

			infoLabel = new GUIContent(Mathf.Min(logsInfo, 9999).ToString(), infoIconSmall);
			warningLabel = new GUIContent(Mathf.Min(logsWarning + compileWarnings.Count, 9999).ToString(), warningIconSmall);
			errorLabel = new GUIContent(Mathf.Min(logsError + compileErrors.Count, 9999).ToString(), errorIconSmall);

			compactMode = DebugLogExtensionsPreferences.CompactModeEnabled;
			UpdateCompactModeRelatedValues();

            useMonospaceFont = DebugLogExtensionsPreferences.UseMonospaceFont;
            ApplyFontSettings();

			base.OnEnable();
		}

		private void OnApplicationStart()
		{
			logs.Clear();
			logsInfo = 0;
			logsWarning = 0;
			logsError = 0;
			selectedVisibleLogIndex = -1;
			visibleLogs.Clear();
			isScrolledToBottom = true;
			scrollToBottomInstantlyNextTick = true;			
			infoLabel.text = "0";
			warningLabel.text = "0";
			errorLabel.text = "0";
			stackTraceDrawer.Clear();
		}

		protected override void OnEnabledChannelsChanged(Channels channels)
		{
			base.OnEnabledChannelsChanged(channels);

			RebuildVisibleLogs(true);
		}

		private void OnPlayModeStateChanged(PlayModeStateChange state)
		{
			if(state == PlayModeStateChange.ExitingEditMode)
			{
				EnableAllLogTypesInBuildInConsole();

				if(clearOnPlay)
				{
					Clear(true);
					initialMessagesFetched = false;
					firstMessageReceived = false;
				}
			}

			// fixes issue of losing delegate subscriptions when leaving play mode
			subscribedToEvents = false;
			ResubscribeToEvents();
		}

		private void EnableAllLogTypesInBuildInConsole()
		{
			var logEntriesType = typeof(EditorApplication).Assembly.GetType("UnityEditor.LogEntries");
			var consoleFlagsProperty = logEntriesType.GetProperty("consoleFlags", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
			ConsoleFlags consoleFlagsWas = (ConsoleFlags)consoleFlagsProperty.GetValue(null, null);
			ConsoleFlags setConsoleFlags = consoleFlagsWas;
			if(!setConsoleFlags.HasFlag(ConsoleFlags.LogLevelLog))
			{
				setConsoleFlags |= ConsoleFlags.LogLevelLog;
			}
			if(!setConsoleFlags.HasFlag(ConsoleFlags.LogLevelWarning))
			{
				setConsoleFlags |= ConsoleFlags.LogLevelWarning;
			}
			if(!setConsoleFlags.HasFlag(ConsoleFlags.LogLevelError))
			{
				setConsoleFlags |= ConsoleFlags.LogLevelError;
			}
			if(setConsoleFlags != consoleFlagsWas)
			{
				hasConsoleFlagsToRestore = true;
				consoleFlagsToRestore = consoleFlagsWas;
				consoleFlagsProperty.SetValue(null, setConsoleFlags);
			}
			else
			{
				hasConsoleFlagsToRestore = false;
			}

			// Remove log filter temporarily to get all entries
			var getFilteringText = logEntriesType.GetMethod("GetFilteringText", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
			if(getFilteringText != null)
			{
				logEntriesPreviousFilterText = (string)getFilteringText.Invoke(null, null);
				restoreLogEntriesPreviousFilterText = !string.IsNullOrEmpty(logEntriesPreviousFilterText);
			}
			else
			{
				#if UNITY_2020_1_OR_NEWER
				Debug.LogWarning("Method UnityEditor.LogEntries.GetFilteringText not found.");
				#endif
				logEntriesPreviousFilterText = "";
				restoreLogEntriesPreviousFilterText = false;
			}

			if(restoreLogEntriesPreviousFilterText)
			{
				var setFilteringText = logEntriesType.GetMethod("SetFilteringText", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
				if(setFilteringText != null)
				{
					setFilteringText.Invoke(null, new object[] { "" });
				}
				else
				{
					#if UNITY_2020_1_OR_NEWER
					Debug.LogWarning("Method UnityEditor.LogEntries.SetFilteringText not found.");
					#endif
					restoreLogEntriesPreviousFilterText = false;
				}
			}
		}

		protected override void UnsubscribeFromEvents()
		{
			base.UnsubscribeFromEvents();

			Debug.LogMessageSuppressed -= OnLogMessageSuppressed;
			CompilationPipeline.compilationStarted -= OnCompilationProcessStarted;
			CompilationPipeline.assemblyCompilationFinished -= OnSingleAssemblyCompilationFinished;
			CompilationPipeline.compilationFinished -= OnCompilationProcessFinished;
			EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
		}

		protected override void SubscribeToEvents()
		{
			base.SubscribeToEvents();

			Debug.LogMessageSuppressed += OnLogMessageSuppressed;
			CompilationPipeline.compilationStarted += OnCompilationProcessStarted;
			CompilationPipeline.assemblyCompilationFinished += OnSingleAssemblyCompilationFinished;
			CompilationPipeline.compilationFinished += OnCompilationProcessFinished;
			EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
		}

		private void OnCompilationProcessStarted(object context)
		{
			visibleLogs.RemoveAll((log) => log.isCompileErrorOrWarning);
			compileErrors.Clear();
			compileWarnings.Clear();

			firstMessageReceived = false;
			initialMessagesFetched = false;
			timeUntilShouldFetchInitialMessages = 1f;

			EnableAllLogTypesInBuildInConsole();

			ResubscribeToEvents();

			// Intentionally not clearing visible logs, so the Console visible contents don't get cleared for the user when compilation starts.
			// Visible logs should still get rebuilt later on as initial log entries are retrieved.
			logs.Clear();
			logsInfo = 0;
			logsWarning = 0;
			logsError = 0;

			wasCompilingLastFrame = true;
		}

		private void OnSingleAssemblyCompilationFinished(string assemblyName, CompilerMessage[] compilerMessages)
		{
			wasCompilingLastFrame = true;
			ResubscribeToEvents();
		}

		private void OnCompilationProcessFinished(object context)
		{
			wasCompilingLastFrame = true;
		}

		[UsedImplicitly]
		protected override void OnDisable()
		{
			base.OnDisable();

			Debug.LogMessageSuppressed -= OnLogMessageSuppressed;
			EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
			Debug.channels.OnEnabledChannelsChanged -= OnEnabledChannelsChanged;
		}

		private void ScrollTowardsBottomStep()
		{
			if(!isScrolledToBottom)
			{
				return;
			}

			float distance = GetScrollDistanceToBottom();
			if(isScrolledToBottom && distance > 0f)
			{
				if(scrollToBottomInstantlyNextTick)
				{
					scrollPosition.y = GetMaxScroll() + 10f;
					scrollToBottomInstantlyNextTick = false;
					calculatedScrollDistanceToBottom = 0f;
				}
				else if(DebugLogExtensionsPreferences.ScrollAnimations)
				{
					scrollPosition.y += scrollToBottomSpeed;
					calculatedScrollDistanceToBottom = Mathf.Max(0f, calculatedScrollDistanceToBottom - scrollToBottomSpeed);
				}
				else
				{
					scrollPosition.y = GetMaxScroll() + 10f;
					calculatedScrollDistanceToBottom = 0f;
				}
				Repaint();
			}
		}

		[UsedImplicitly]
		protected override void OnGUI()
		{
            float height = position.height;
            if(heightLastFrame != height)
            {
                heightLastFrame = height;
                if(isScrolledToBottom || (mainPanelContentRect.height - mainPanelWindowRect.height - scrollPosition.y <= logEntryHeight))
                {
                    isScrolledToBottom = true;
                    ScrollToBottomInstantly();
                    scrollToBottomInstantlyNextTick = true;
                    calculatedScrollDistanceToBottom = 0f;
                }
            }

            if(!initialized)
			{
				toolbarSearchFieldStyle = EditorStyles.toolbarSearchField;
				toolbarSearchFieldCancelButtonStyle = GUI.skin.GetStyle("ToolbarSeachCancelButton");
				toolbarSearchFieldCancelButtonEmptyStyle = GUI.skin.GetStyle("SearchCancelButtonEmpty");

				searchIcon = new GUIContent(EditorGUIUtility.FindTexture("Search Icon"), "Click to find all objects of type in scene");

				stackTraceDrawer.Initialize(searchIcon);

				base.OnGUI();

				if(wasOpenedViaMenuItem || JustLaunchedEditor())
				{
					timeUntilShouldFetchInitialMessages = 1f;
					EnableAllLogTypesInBuildInConsole();
				}
			}
			else if(!subscribedToEvents)
			{
				ResubscribeToEvents();
			}
			else if(EditorApplication.isCompiling)
			{
				wasCompilingLastFrame = true;
			}
			else if(wasCompilingLastFrame)
			{
				wasCompilingLastFrame = false;
			}
			else if(!initialMessagesFetched)
			{
				Repaint();

				if(firstMessageReceived || timeUntilShouldFetchInitialMessages <= 0f)
				{
					timeUntilShouldFetchInitialMessages = 0f;
					initialMessagesFetched = true;
					logsInfo = 0;
					logsWarning = 0;
					logsError = 0;
					FetchInitialLogEntries();
				}
				else
				{
					timeUntilShouldFetchInitialMessages -= 0.01f;
				}
			}

			switch(Event.current.rawType)
			{
				case EventType.ValidateCommand:
					if(string.Equals(Event.current.commandName, "Copy") && SelectedLog != null)
					{
						if(!EditorGUIUtility.textFieldHasSelection)
						{
							stackTraceDrawer.CopyToClipboard();
						}
					}
					break;
				case EventType.KeyDown:
					switch(Event.current.keyCode)
					{
						case KeyCode.Home:
							if(!IsEditingSearchFilter() && visibleLogs.Count > 0)
							{
								SetSelectedLogIndex(0);
								isScrolledToBottom = false;
							}
							break;
						case KeyCode.PageUp:
							if(visibleLogs.Count > 0)
							{
								SetSelectedLogIndex(Mathf.Max(0, selectedVisibleLogIndex - Mathf.FloorToInt(mainPanelWindowRect.height / logEntryHeight) + 1));
								isScrolledToBottom = false;
								UnfocusFilterField();
							}
							break;
						case KeyCode.PageDown:
							if(visibleLogs.Count > 0)
							{
								SetSelectedLogIndex(Mathf.Min(visibleLogs.Count - 1, selectedVisibleLogIndex + Mathf.FloorToInt(mainPanelWindowRect.height / logEntryHeight) - 1));
								UnfocusFilterField();
								GUIUtility.ExitGUI();
							}
							break;
						case KeyCode.Escape:
							if(!IsEditingSearchFilter() && visibleLogs.Count > 0 && selectedVisibleLogIndex != -1)
							{
								SetSelectedLogIndex(-1);
								GUIUtility.ExitGUI();
							}
							break;
						case KeyCode.End:
							if(!IsEditingSearchFilter() && visibleLogs.Count > 0)
							{
								SetSelectedLogIndex(visibleLogs.Count - 1);
								GUIUtility.ExitGUI();
							}
							break;
						case KeyCode.UpArrow:
							if(visibleLogs.Count > 0)
							{
								SetSelectedLogIndex(Mathf.Max(0, selectedVisibleLogIndex - 1));
								isScrolledToBottom = false;
								UnfocusFilterField();
								GUIUtility.ExitGUI();
							}
							break;
						case KeyCode.DownArrow:
							if(visibleLogs.Count > 0)
							{
								SetSelectedLogIndex(Mathf.Min(visibleLogs.Count - 1, selectedVisibleLogIndex + 1));
								UnfocusFilterField();
								GUIUtility.ExitGUI();
							}
							break;
						case KeyCode.Return:
						case KeyCode.KeypadEnter:
							if(IsEditingSearchFilter())
							{
								if(filter.Length > 0)
								{
									if(selectedVisibleLogIndex == -1 && visibleLogs.Count > 0)
									{
										SetSelectedLogIndex(0);
									}
									filter = "";
									ApplyFilter(true);
								}
								UnfocusFilterField();
								GUIUtility.ExitGUI();
							}
							else if(selectedVisibleLogIndex != -1)
							{
								stackTraceDrawer.OpenScript();
							}
							break;
					}
					break;
				case EventType.Repaint:
					if(isScrolledToBottom && GetScrollDistanceToBottom() > 0f)
					{
						ScrollTowardsBottomStep();
						Repaint();
					}
					break;
				case EventType.MouseDown:
					
					mouseMovedAfterMouseDown = false;
					if(EditorApplication.timeSinceStartup - lastClickTime > DoubleClickThreshold || Vector2.Distance(mouseDownPosition, Event.current.mousePosition) > 3f)
					{
						clickCount = 0;
						lastClickTime = 0d;
					}
					mouseDownPosition = Event.current.mousePosition;
					mouseIsDown = true;					
					break;
				case EventType.MouseDrag:
				case EventType.MouseMove:
					if(Vector2.Distance(mouseDownPosition, Event.current.mousePosition) > 3f)
					{
						mouseMovedAfterMouseDown = true;
						clickCount = 0;
						lastClickTime = 0d;
					}
					break;
				case EventType.MouseUp:
					mouseIsDown = false;
					if(Vector2.Distance(mouseDownPosition, Event.current.mousePosition) > 3f)
					{
						mouseMovedAfterMouseDown = true;
					}
					if(mouseMovedAfterMouseDown)
					{
						clickCount = 0;
						mouseMovedAfterMouseDown = false;
					}
					else if(EditorApplication.timeSinceStartup - lastClickTime < DoubleClickThreshold)
					{
						clickCount++;
					}
					else
					{
						clickCount = 1;
					}
					lastClickTime = clickCount > 0 ? EditorApplication.timeSinceStartup : 0d;
					break;
				case EventType.ScrollWheel:
					lastScrollTime = EditorApplication.timeSinceStartup;
					break;
			}

			var resizerRect = new Rect(0, position.height - EffectiveBottomHeight - resizerHeight, position.width, resizerHeight * 2f);
			HandleResizing(resizerRect);

			var iconSizeWas = EditorGUIUtility.GetIconSize();

			if(BuildPipeline.isBuildingPlayer)
			{
				if(!wasBuildingPlayerLastFrame)
				{
					wasBuildingPlayerLastFrame = true;
					if(clearOnBuild)
					{
						Clear();
					}
				}
			}
			else
			{
				wasBuildingPlayerLastFrame = false;
			}

			DrawMenuBar();
			DrawMainList();
			DrawDetailArea();
			DrawResizer(resizerRect);

			if(GUI.changed)
			{
				Repaint();
			}

			EditorGUIUtility.SetIconSize(iconSizeWas);
		}

		private bool IsEditingSearchFilter()
		{
			return string.Equals(GUI.GetNameOfFocusedControl(), FilterControlName);
		}

		private void UnfocusFilterField()
		{
			if(IsEditingSearchFilter() && GUIUtility.hotControl == 0)
			{
				GUIUtility.keyboardControl = 0;
			}
			EditorGUIUtility.editingTextField = false;
			focusFilterField = 0;
		}

		private void Clear()
		{
			// only clear compile warnings if clear button is pressed twice?
			Clear(logs.Count == 0 && compileWarnings.Count > 0);
		}

		private void Clear(bool clearCompileWarnings)
		{
			// Clear internal editor as well
			var logEntriesType = typeof(EditorApplication).Assembly.GetType("UnityEditor.LogEntries");
			if(logEntriesType != null)
			{
				var clear = logEntriesType.GetMethod("Clear", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
				if(clear != null)
				{
					clear.Invoke(null, null);
				}
			}

			if(clearCompileWarnings)
			{
				compileWarnings.Clear();
			}

			logs.Clear();
			logsInfo = 0;
			logsWarning = 0;
			logsError = compileErrors.Count;
			visibleLogs.Clear();
			SetSelectedLogIndex(-1);
			collapsedLogCountByHash.Clear();

			infoLabel.text = "0";
			warningLabel.text = "0";
			errorLabel.text = "0";

			isScrolledToBottom = true;

			// needed if has compile errors or warnings
			RebuildVisibleLogs(true);
		}

		private void DrawMenuBar()
		{
			EditorGUIUtility.SetIconSize(new Vector2(16f, 16f));

			GUILayout.BeginHorizontal(EditorStyles.toolbar, GUILayout.ExpandHeight(true));
			{
				const float clearButtonWidth = 42f;
				const float clearSettingsDropdownWidth = 17f;
				const float errorPauseWidth = 75f;
				const float collapseButtonWidth = 62f;

				if(GUILayout.Button(ClearLabel, EditorStyles.toolbarButton, GUILayout.Width(clearButtonWidth)))
				{
					Clear();
					GUIUtility.ExitGUI();
				}

				if(GUILayout.Button(GUIContent.none, EditorStyles.toolbarDropDown, GUILayout.Width(clearSettingsDropdownWidth)))
				{
					var menu = new GenericMenu();
					menu.AddItem(new GUIContent("Clear on Play"), clearOnPlay, ToggleClearOnPlay);
					menu.AddItem(new GUIContent("Clear on Build"), clearOnBuild, ToggleClearOnBuild);

					if(position.width <= 500f)
					{
						menu.AddSeparator("");
						menu.AddItem(new GUIContent("Collapse"), collapse, ToggleCollapse);
						menu.AddItem(errorPauseLabel, errorPause, ToggleErrorPause);
					}

					menu.DropDown(new Rect(0f, 0f, 59f, 20f));
					GUIUtility.ExitGUI();
				}

				bool drawFilterField = position.width > 520f;
				bool drawErrorPause = position.width > 464f;
				bool drawCollapseButton = position.width > 389f;

				if(drawErrorPause)
				{
					errorPause = GUILayout.Toggle(errorPause, errorPauseLabel, EditorStyles.toolbarButton, GUILayout.Width(errorPauseWidth));
				}

				if(GUILayout.Button(editorLabel, EditorStyles.toolbarPopup))
				{
					var menu = new GenericMenu();
					menu.AddItem(new GUIContent("Player Logging"), PlayerLoggingEnabled(), TogglePlayerLogging);
					menu.AddItem(new GUIContent("Full Log (Developer Mode Only)"), PlayerSettings.GetStackTraceLogType(LogType.Log) == StackTraceLogType.Full, ToggleFullLogging);
					
					float x = clearButtonWidth + clearSettingsDropdownWidth + (drawErrorPause ? errorPauseWidth : 0f);
					menu.DropDown(new Rect(x, 0f, 75f, 20f));
					GUIUtility.ExitGUI();
				}

				GUILayout.FlexibleSpace();

				if(drawFilterField)
				{
					var searchFieldRect = EditorGUILayout.GetControlRect(GUILayout.MaxWidth(300f));

					Rect clearButtonRect = searchFieldRect;
					clearButtonRect.x += clearButtonRect.width - 14f;
					clearButtonRect.width = 14f;

					bool hasFilter = filter.Length > 0;
					var cancelButtonStyle = hasFilter ? toolbarSearchFieldCancelButtonStyle : toolbarSearchFieldCancelButtonEmptyStyle;

					if(hasFilter)
					{
						EditorGUIUtility.AddCursorRect(clearButtonRect, MouseCursor.Link);
					}

					if(GUI.Button(clearButtonRect, GUIContent.none, cancelButtonStyle))
					{
						EditorGUIUtility.editingTextField = false;
						filter = "";
						ApplyFilter(true);
						GUIUtility.ExitGUI();
					}

					if(Event.current.type == EventType.ExecuteCommand && string.Equals(Event.current.commandName, "Find"))
					{
						EditorGUI.FocusTextInControl(FilterControlName);
						if(Event.current.type != EventType.Layout)
						{
							Event.current.Use();
							GUIUtility.ExitGUI();
						}
					}
					else if(focusFilterField > 0)
					{
						focusFilterField--;
						Repaint();
						EditorGUI.FocusTextInControl(FilterControlName);
						EditorGUIUtility.editingTextField = true;
					}

					GUI.SetNextControlName(FilterControlName);

					string setFilter = EditorGUI.TextField(searchFieldRect, GUIContent.none, filter, toolbarSearchFieldStyle);

					if(!string.Equals(setFilter, filter))
					{
						filter = setFilter;
						ApplyFilter(true);
						focusFilterField = 3;
					}

					GUI.Label(clearButtonRect, GUIContent.none, cancelButtonStyle);
				}

				if(GUILayout.Button(fullChannelsButtonLabel, EditorStyles.toolbarPopup))
				{
					var menu = BuildChannelPopupMenu();
					float x = position.width - EditorStyles.toolbarButton.CalcSize(infoLabel).x - EditorStyles.toolbarButton.CalcSize(warningLabel).x - EditorStyles.toolbarButton.CalcSize(errorLabel).x - EditorStyles.toolbarPopup.CalcSize(fullChannelsButtonLabel).x - 1f;
					if(drawCollapseButton)
					{
						x -= collapseButtonWidth;
					}
					menu.DropDown(new Rect(x, 0f, 75f, 20f));
					GUIUtility.ExitGUI();
				}

				if(drawCollapseButton)
				{
					bool setCollapse = GUILayout.Toggle(collapse, new GUIContent("Collapse"), EditorStyles.toolbarButton, GUILayout.Width(collapseButtonWidth));
					if(collapse != setCollapse)
					{
						collapse = setCollapse;
						RebuildVisibleLogs(true);
						GUIUtility.ExitGUI();
					}
				}

				bool set = GUILayout.Toggle(showLog, infoLabel, EditorStyles.toolbarButton);
				if(set != showLog)
				{
					showLog = set;
					RebuildVisibleLogs(true);
					GUIUtility.ExitGUI();
				}

				set = GUILayout.Toggle(showWarnings, warningLabel, EditorStyles.toolbarButton);
				if(set != showWarnings)
				{
					showWarnings = set;
					RebuildVisibleLogs(true);
					GUIUtility.ExitGUI();
				}

				set = GUILayout.Toggle(showErrors, errorLabel, EditorStyles.toolbarButton);
				if(set != showErrors)
				{
					showErrors = set;
					RebuildVisibleLogs(true);
					GUIUtility.ExitGUI();
				}
			}
			GUILayout.EndHorizontal();
		}

		private void ApplyFilter(bool rebuildVisibleLogs)
		{
			filterTrimmed = filter.Trim();
			filterParts = filterTrimmed.Split(spaceChar, StringSplitOptions.RemoveEmptyEntries);

			filterRegexParts.Clear();

			try
			{
				filterRegex = new Regex(filterTrimmed, RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);
				useRegex = true;
			}
			catch(ArgumentException)
			{
				useRegex = false;
				filterRegex = new Regex("");
			}

			if(useRegex)
			{
				try
				{
					foreach(var filterPart in filterParts)
					{
						filterRegexParts.Add(new Regex(filterPart, RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled));
					}
				}
				catch(ArgumentException)
				{
					filterRegexParts.Clear();
				}
			}

			if(rebuildVisibleLogs)
			{
				RebuildVisibleLogs(true);
			}

			if(isScrolledToBottom)
			{
				scrollToBottomInstantlyNextTick = true;
			}
		}

		private void ToggleCollapse()
		{
			collapse = !collapse;
			RebuildVisibleLogs(true);
		}

		private void ToggleErrorPause()
		{
			errorPause = !errorPause;
		}

		private bool PlayerLoggingEnabled()
		{
			return PlayerConnectionLogReceiver.PlayerLoggingEnabled();
		}

		private void TogglePlayerLogging()
		{
			PlayerConnectionLogReceiver.State = PlayerLoggingEnabled() ? PlayerConnectionLogReceiver.ConnectionState.Disconnected : PlayerConnectionLogReceiver.ConnectionState.CleanLog;
		}

		private bool FullLoggingEnabled()
		{
			return PlayerConnectionLogReceiver.FullLoggingEnabled();
		}

		private void ToggleFullLogging()
		{
			PlayerConnectionLogReceiver.State = FullLoggingEnabled() ? PlayerConnectionLogReceiver.ConnectionState.CleanLog : PlayerConnectionLogReceiver.ConnectionState.FullLog;
		}

		private void ToggleClearOnPlay()
		{
			clearOnPlay = !clearOnPlay;
		}

		private void ToggleClearOnBuild()
		{
			clearOnBuild = !clearOnBuild;
		}

		private void DrawMainList()
		{
            EditorGUIUtility.SetIconSize(iconSize);

			int count = visibleLogs.Count;
			if(count == 0)
			{
				return;
			}

			UpdateUpperPanelRects();

			var setUpperPanelScroll = GUI.BeginScrollView(mainPanelWindowRect, scrollPosition, mainPanelContentRect);
			if(setUpperPanelScroll != scrollPosition)
			{
				scrollPosition = setUpperPanelScroll;

				if(GetScrollDistanceToBottom() <= 0f)
				{
					if(!isScrolledToBottom)
					{
						isScrolledToBottom = true;
					}
				}
				else if(mouseIsDown || (EditorApplication.timeSinceStartup - lastScrollTime) < 0.01d)
				{
					isScrolledToBottom = false;
				}
			}
			
			int firstVisible = scrollPosition.y < logEntryHeight ? 0 : Mathf.FloorToInt(scrollPosition.y / logEntryHeight);
			int lastVisible = Mathf.Min(count - 1, firstVisible + Mathf.CeilToInt(mainPanelWindowRect.height / logEntryHeight));

			var rect = new Rect(0f, firstVisible * logEntryHeight, position.width, logEntryHeight);
			var mousePosition = Event.current.mousePosition;

			bool highlightNewEntires = DebugLogExtensionsPreferences.LogUpdatedEffects;
			bool shouldRepaint = false;
			bool isOdd = firstVisible % 2 == 0;
			int i;
			if(!collapse)
			{
				for(i = firstVisible; i <= lastVisible; i++)
				{
					#if DEV_MODE
					Debug.Assert(i >= 0, i);
					Debug.Assert(i < visibleLogs.Count, $"i ({i}) >= visibleLogs.Count ({visibleLogs.Count})");
					#endif

					var log = visibleLogs[i];
					bool isSelected = i == selectedVisibleLogIndex;
					var boxStyle = GetMessageBoxStyle(isOdd, isSelected);

					bool mouseovered = rect.Contains(mousePosition);
					if(mouseovered)
					{
						shouldRepaint = true;
					}

					var iconRect = rect;
					iconRect.x = rect.width - 40f;
					iconRect.y += compactMode ? 1f : 9f;
					iconRect.width = 100f;
					iconRect.height = 20f;

					if(!log.appearAnimationDone && highlightNewEntires)
					{
						GUI.color = GetHighlightedEntryColor(log);
						shouldRepaint = true;
					}

					if(DrawBox(rect, log.listViewLabel is null ? BuildListViewLabel(log) : log.listViewLabel, boxStyle))
					{
						SetSelectedLogIndex(i);

						if(iconRect.Contains(mousePosition))
						{
							SelectContextObject(log);
						}
						else if(log.context != null)
						{
							PingContextObject(log);
						}

						GUIUtility.ExitGUI();
					}

					DrawIconRect(log, iconRect, boxStyle, mouseovered);

					GUI.color = Color.white;

					rect.y += logEntryHeight;
					isOdd = !isOdd;
				}
			}
			else
			{
				for(i = firstVisible; i <= lastVisible; i++)
				{
					#if DEV_MODE
					Debug.Assert(i >= 0, i);
					Debug.Assert(i < visibleLogs.Count, $"i ({i}) >= visibleLogs.Count ({visibleLogs.Count})");
					#endif

					var log = visibleLogs[i];
					var icon = GetMessageIcon(log.type);
					bool isSelected = i == selectedVisibleLogIndex;
					var boxStyle = GetMessageBoxStyle(isOdd, isSelected);

					bool mouseovered = rect.Contains(mousePosition);
					if(mouseovered)
					{
						shouldRepaint = true;
					}

					var countRect = rect;
					GUIStyle countStyle = "CN CountBadge";
					int collapsedCount;
					if(!collapsedLogCountByHash.TryGetValue(log.hash, out collapsedCount))
					{
						collapsedCount = 1;
					}
					var countLabel = new GUIContent(collapsedCount.ToString());
					var size = countStyle.CalcSize(countLabel);
					countRect.width = size.x;
					countRect.height = size.y;
					countRect.x = rect.xMax - size.x - 17f;
					countRect.y += 10f;

					var iconRect = countRect;
					iconRect.x -= countRect.width + 14f;
					iconRect.y = rect.y + 9f;
					iconRect.width = 100f;
					iconRect.height = 20f;

					if(!log.appearAnimationDone && highlightNewEntires)
					{
						GUI.color = GetHighlightedEntryColor(log);
						shouldRepaint = true;
					}

					if(DrawBox(rect, log.listViewLabel is null ? BuildListViewLabel(log) : log.listViewLabel, boxStyle))
					{
						SetSelectedLogIndex(i);
						
						if(iconRect.Contains(mousePosition))
						{
							SelectContextObject(log);
						}
						else if(log.context != null)
						{
							PingContextObject(log);
						}

						GUIUtility.ExitGUI();
					}

					DrawIconRect(log, iconRect, boxStyle, mouseovered);

					GUI.color = Color.white;

					GUI.Label(countRect, countLabel, countStyle);

					rect.y += logEntryHeight;
					isOdd = !isOdd;
				}
			}

			// Handle background color and clicking empty space to deselect current log.
			float remainingSpace = mainPanelWindowRect.height - rect.y;
			if(remainingSpace > 0f)
			{
				rect.height = remainingSpace;

				if(i % 2 == 0)
				{
					boxStyle.normal.background = boxBgOdd;
				}
				else
				{
					boxStyle.normal.background = boxBgEven;
				}

				GUI.Label(rect, GUIContent.none, boxStyle);

				if(clickCount == 1 && Event.current.type == EventType.MouseUp && rect.Contains(Event.current.mousePosition))
				{
					SetSelectedLogIndex(-1);
					GUIUtility.ExitGUI();
				}
			}

			if(shouldRepaint)
			{
				Repaint();
			}

			GUI.EndScrollView();
		}

		private void PingContextObject(Log log)
		{
			Object[] foundInstances;
			if(!TryFindInstancesOfUnityObjectType(log, out foundInstances) || foundInstances.Length == 0)
			{
				return;
			}

			switch(foundInstances.Length)
			{
				case 1:
					EditorGUIUtility.PingObject(foundInstances[0]);
					return;
				default:
					Selection.objects = foundInstances;
					return;
			}
		}

		private void SelectContextObject(Log log)
		{
			Object[] foundInstances;

			int hash = log.hash;
			int collapsedCount;
			if(collapse && collapsedLogCountByHash.TryGetValue(hash, out collapsedCount) && collapsedCount > 1)
			{
				HashSet<Object> contexts = new HashSet<Object>();
				foreach(var test in logs)
				{
					if(test.hash == hash && TryFindInstancesOfUnityObjectType(test, out foundInstances))
					{
						foreach(var instance in foundInstances)
						{
							contexts.Add(instance);
						}
					}
				}
				foundInstances = contexts.ToArray();
			}
			else if(!TryFindInstancesOfUnityObjectType(log, out foundInstances))
			{
				return;
			}

			if(foundInstances.Length > 0)
			{
				Selection.objects = foundInstances;
			}
		}

		private bool StackTraceContainsUnityObjectType(Log log)
		{
			Type classType;
			return stackTraceDrawer.TryExtractUnityObjectTypeFromStackTrace(log, out classType);
		}

		private bool TryFindInstancesOfUnityObjectType(Log log, out Object[] foundInstances)
		{
			if(log.context != null)
			{
				foundInstances = new Object[] { log.context is Component ? (log.context as Component).gameObject : log.context };
				return true;
			}

			Type classType;
			if(!stackTraceDrawer.TryExtractUnityObjectTypeFromStackTrace(log, out classType))
			{
				foundInstances = new Object[0];				
				return false;
			}

			if(typeof(ScriptableObject).IsAssignableFrom(classType))
			{
				foundInstances = AssetDatabase.FindAssets("t:" + classType.Name).Select((guid) => AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(guid), classType)).ToArray();
				return foundInstances.Length > 0;
			}
			else if(typeof(Component).IsAssignableFrom(classType))
			{
				#if UNITY_2020_1_OR_NEWER
				foundInstances = FindObjectsOfType(classType, true).Select((i) => (i as Component).gameObject).ToArray();
				#else
				foundInstances = FindObjectsOfType(classType).Select((i) => (i as Component).gameObject).ToArray();
				#endif
				return foundInstances.Length > 0;
			}
			foundInstances = new Object[0];
			return false;
		}

		private void DrawIconRect(Log log, Rect iconRect, GUIStyle boxStyle, bool rowMouseovered)
		{
			if(log.ContextIcon.image == null)
			{
				if(!rowMouseovered)
				{
					return;
				}
				if(mouseoveredLogStackTraceContainsUnityObjectType.Key != log)
				{
					mouseoveredLogStackTraceContainsUnityObjectType = new KeyValuePair<Log, bool>(log, StackTraceContainsUnityObjectType(log));
				}
				if(!mouseoveredLogStackTraceContainsUnityObjectType.Value)
				{
					return;
				}
			}

			var iconSizeWas = EditorGUIUtility.GetIconSize();
			EditorGUIUtility.SetIconSize(new Vector2(16f, 16f));

			if(log.ContextIcon.image != null)
			{
				GUI.Label(iconRect, log.ContextIcon, boxStyle);
			}
			else
			{
				var color = Color.white;
				float width = position.width;
				float visibleSpectrumMin = width * 0.5f;
				float visibleSpectrumMax = width - 100f;
				var mousePosition = Event.current.mousePosition;
				if(visibleSpectrumMax <= visibleSpectrumMin)
				{
					color.a = 1f;
				}
				else if(mousePosition.x < visibleSpectrumMin)
				{
					color.a = 0f;
				}
				else
				{
					color.a = Mathf.Clamp((mousePosition.x - visibleSpectrumMin) / (visibleSpectrumMax - visibleSpectrumMin), 0f, 1f);
				}
				
				GUI.color = color;
				GUI.Label(iconRect, searchIcon, boxStyle);
				GUI.color = Color.white;
			}

			EditorGUIUtility.SetIconSize(iconSizeWas);
		}

		private void UpdateUpperPanelRects()
		{
			const float adHocFix = 18f;
			mainPanelWindowRect = new Rect(0f, 21f, position.width, position.height - EffectiveBottomHeight - adHocFix);

			mainPanelContentRect = mainPanelWindowRect;
			mainPanelContentRect.y = 0f;
			mainPanelContentRect.height = visibleLogs.Count * logEntryHeight;

			bool hasScrollbar = mainPanelContentRect.height > mainPanelWindowRect.height;
			if(hasScrollbar)
			{
				const float ScrollBarWidth = 13f;
				mainPanelContentRect.width -= ScrollBarWidth;
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private float GetMaxScroll()
		{
			return mainPanelContentRect.height - mainPanelWindowRect.height;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private bool IsScrolledToBottom()
		{
			return mainPanelContentRect.height - mainPanelWindowRect.height - scrollPosition.y <= 3f;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private float GetScrollDistanceToBottom()
		{
			return mainPanelContentRect.height - mainPanelWindowRect.height - scrollPosition.y;
		}

		private void SetSelectedLogIndex(int value)
		{
			if(value >= visibleLogs.Count)
			{
				value = visibleLogs.Count - 1;
			}

			bool detailAreaIsBecomingVisible = selectedVisibleLogIndex == -1 && value != -1 && DebugLogExtensionsPreferences.HideDetailAreaWhenNoLogEntrySelected;

			selectedVisibleLogIndex = value;

			stackTraceDrawer.Log = SelectedLog;

			Repaint();

			if(value < 0)
			{
				return;
			}

			float logStart = selectedVisibleLogIndex * logEntryHeight;

			if(detailAreaIsBecomingVisible)
			{
				UpdateUpperPanelRects();

				float logEnd = logStart + logEntryHeight;
				float viewEnd = scrollPosition.y + mainPanelWindowRect.height;
				if(logEnd <= viewEnd && logStart >= scrollPosition.y)
				{
					isScrolledToBottom = IsScrolledToBottom();
					return;
				}

				if(isScrolledToBottom)
				{
					scrollToBottomInstantlyNextTick = true;
					ScrollToBottomInstantly();
					return;
				}
			}

			if(logStart < scrollPosition.y)
			{
				scrollPosition.y = logStart;
				isScrolledToBottom = false;
			}
			else
			{
				float logEnd = logStart + logEntryHeight;
				float viewEnd = scrollPosition.y + mainPanelWindowRect.height;
				if(logEnd > viewEnd)
				{
					scrollPosition.y = logStart - mainPanelWindowRect.height + logEntryHeight;
					isScrolledToBottom = selectedVisibleLogIndex == visibleLogs.Count - 1;
				}
			}
		}

		private GUIContent BuildListViewLabel(Log log)
		{
			string text = log.text;

			int textLength = text.Length;
			if(textLength > 1)
			{
				int maxLineCount = MaxLineCount;
				int lineBreak = -1;
				for(int i = 1; i <= maxLineCount; i++)
				{
					lineBreak = text.IndexOf('\n', lineBreak + 1);
					if(lineBreak == -1)
					{
						break;
					}

					if(lineBreak == textLength - 1)
					{
						text = text.Substring(0, textLength - 1);
						break;
					}

					if(i == maxLineCount)
					{
						text = text.Substring(0, lineBreak);
						break;
					}
				}
			}

			string content;
			if(!showTimestamps)
			{
				content = text;
			}
			else
			{
				stringBuilder.Append("<color=grey>[");
				stringBuilder.Append(log.timeHour);
				stringBuilder.Append(':');
				stringBuilder.Append(log.timeMinute);
				stringBuilder.Append(':');
				stringBuilder.Append(log.timeSecond);
				stringBuilder.Append("]</color>  ");

				stringBuilder.Append(text);

				content = stringBuilder.ToString();
				stringBuilder.Length = 0;
			}

			log.listViewLabel = new GUIContent(content, GetMessageIcon(log.type));

			return log.listViewLabel;
		}

		private void DrawDetailArea()
		{
			if(selectedVisibleLogIndex == -1)
			{
				return;
			}

			var lowerPanelRect = new Rect(0, position.height - EffectiveBottomHeight + resizerHeight, position.width, EffectiveBottomHeight - resizerHeight);
			GUILayout.BeginArea(lowerPanelRect);
			stackTraceDrawer.OnGUI(lowerPanelRect, clickCount);
			GUILayout.EndArea();
		}

		private void HandleResizing(Rect resizerRect)
		{
			switch(Event.current.rawType)
			{
				case EventType.MouseDown:
					if(Event.current.button == 0 && resizerRect.Contains(Event.current.mousePosition))
					{
						resizing = true;
						Event.current.Use();
					}
					break;
				case EventType.MouseUp:
					resizing = false;
					break;
				case EventType.MouseDrag:
				case EventType.MouseMove:
					if(resizing)
					{
						float scrollDistanceToBottomWas = GetScrollDistanceToBottom();

						bottomHeight = Mathf.Max(30f, position.height - Event.current.mousePosition.y);

						UpdateUpperPanelRects();

						// Ensure that if was scrolled to bottom, scroll position remains pinned to floor
						if(isScrolledToBottom && GetScrollDistanceToBottom() != scrollDistanceToBottomWas && scrollDistanceToBottomWas < 3f)
						{
							ScrollToBottomInstantly();
						}

						Repaint();
					}
					break;
			}
		}

		private void DrawResizer(Rect resizerRect)
		{
			var lineRect = resizerRect;
			lineRect.y += resizerHeight + 2f;
			lineRect.height = 1f;
			EditorGUI.DrawRect(lineRect, lineColor);

			EditorGUIUtility.AddCursorRect(resizerRect, MouseCursor.ResizeVertical);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static GUIStyle GetMessageBoxStyle(bool isOdd, bool isSelected)
		{
			boxStyle.normal.background = isSelected ? boxBgSelected : isOdd ? boxBgOdd : boxBgEven;
			return boxStyle;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static Texture2D GetMessageIcon(LogType logType)
		{
			switch(logType)
			{
				case LogType.Error:
				case LogType.Exception:
				case LogType.Assert:
					return errorIcon;
				case LogType.Warning:
					return warningIcon;
				default:
					return infoIcon;
			}
		}

		private Color GetHighlightedEntryColor(Log log)
		{
			double secondsShown = log.AgeInSeconds;
			if(secondsShown >= newEntryHighlightDuration)
			{
				log.appearAnimationDone = true;
				return Color.white;
			}

			// Lerping formula:
			// x * (1 - t) + end * t;
			// x * (1f - t) + 1f * t;
			// x * (1f - t) + t;
			float t = (float)(secondsShown / newEntryHighlightDuration);
			float progress = 1f - t;

			switch(log.type)
			{
				case LogType.Error:
				case LogType.Exception:
				case LogType.Assert:
					return new Color
					(
							   progress + t,
						0.5f * progress + t,
						0.5f * progress + t
					);
				case LogType.Warning:
					return new Color
					(
							   progress + t,
						0.92f * progress + t,
						0.016f * progress + t
					);
				default:
					return new Color
					(
						0.8f * progress + t,
							   progress + t,
							   progress + t
					);
			}
		}

		private bool DrawBox(Rect rect, GUIContent label, GUIStyle boxStyle)
		{
			GUI.Label(rect, label, boxStyle);

			if(Event.current.type == EventType.MouseDown && rect.Contains(Event.current.mousePosition) && clickCount <= 1)
			{
				return true;
			}

			if(clickCount == 2 && Event.current.type == EventType.MouseUp)
			{
				Event.current.Use();
				stackTraceDrawer.OpenScript();
				return false;
			}

			return false;
		}

		protected override void OnLogMessageReceived(string message, string stackTrace, LogType type)
		{
			if(!initialMessagesFetched)
			{
				firstMessageReceived = true;
				return;
			}

			Object context = Debug.LastMessageContext;
			Debug.LastMessageContext = null;

			string unformatted = Debug.LastMessageUnformatted;
			Debug.LastMessageUnformatted = null;
			if(string.IsNullOrEmpty(unformatted))
			{
				unformatted = message;
			}

			Repaint();

			if(type == LogType.Exception && stackTrace.StartsWith("Rethrow as ", StringComparison.Ordinal))
			{
				int outerExceptionEnd = stackTrace.IndexOf('\n', 12);
				if(outerExceptionEnd != -1)
				{
					message += "\n" + stackTrace.Substring(0, outerExceptionEnd);
					stackTrace = stackTrace.Substring(outerExceptionEnd + 1).Trim();
				}
			}

			bool isCompileWarningOrError = wasCompilingLastFrame && (type == LogType.Error || type == LogType.Warning);
			var log = new Log(unformatted, message, stackTrace, type, context, isCompileWarningOrError);

			if(isCompileWarningOrError)
			{
				if(type == LogType.Error)
				{
					if(ShouldShow(log))
					{
						int index = compileErrors.Count;
						if(visibleLogs.Count >= index)
						{
							visibleLogs.Insert(index, log);
						}
						else
						{
							visibleLogs.Insert(0, log);
						}
						UpdateScrollToBottomSpeed(1);
					}
					compileErrors.Add(log);

					int errorCount = logsError + compileErrors.Count;
					if(errorCount <= 9999)
					{
						errorLabel.text = errorCount.ToString();
					}
					return;
				}
				if(ShouldShow(log))
				{
					int index = compileErrors.Count + compileWarnings.Count;
					if(visibleLogs.Count >= index)
					{
						visibleLogs.Insert(index, log);
					}
					else
					{
						visibleLogs.Insert(0, log);
					}
					UpdateScrollToBottomSpeed(1);
				}
				compileWarnings.Add(log);
				return;
			}

			logs.Add(log);

			if(ShouldShow(log))
			{
				if(!collapse)
				{
					visibleLogs.Add(log);
					UpdateScrollToBottomSpeed(1);
				}
				else
				{
					int hash = log.hash;
					int collapsedCount;
					if(collapsedLogCountByHash.TryGetValue(hash, out collapsedCount))
					{
						collapsedLogCountByHash[hash] = collapsedCount + 1;

						// Remove previously visible and replace with the new log.
						// This way the time stamp will be updated.
						int index;
						for(index = visibleLogs.Count - 1; index >= 0; index--)
						{
							if(visibleLogs[index].hash == hash)
							{
								break;
							}
						}

						if(index != -1)
						{
							visibleLogs.RemoveAt(index);

							// Two ways to do this. If new messages are pushed to the bottom of the list it is easier to notice when updates occur to them.
							// On the other hand, if one message is spammed dozens of times per second and it is pushed to the bottom with automatic scroll to bottom
							// enabled, then it can be very difficult to see anything else.
							// One option could be to only push to bottom if number of new visible log entries received within the last second is low enough.
							if(DebugLogExtensionsPreferences.PushToBottomInCollapsedMode)
							{
								visibleLogs.Add(log);
							}
							else
							{
								visibleLogs.Insert(index, log);
							}
						}
					}
					else
					{
						collapsedLogCountByHash[hash] = 1;
						visibleLogs.Add(log);

						UpdateScrollToBottomSpeed(1);
					}
				}
			}

			switch(type)
			{
				case LogType.Log:
					if(logsInfo < 9999)
					{
						infoLabel.text = (logsInfo + 1).ToString();
					}
					logsInfo++;
					break;
				case LogType.Warning:
					int countWas = logsWarning + compileWarnings.Count;
					if(countWas < 9999)
					{
						warningLabel.text = (countWas + 1).ToString();
					}
					logsWarning++;
					break;
				default:
					countWas = logsError + compileErrors.Count;
					if(countWas < 9999)
					{
						errorLabel.text = (countWas + 1).ToString();
					}
					logsError++;
					if(errorPause && Application.isPlaying)
					{
						Debug.Break();
					}
					break;
			}
		}

		private void UpdateScrollToBottomSpeed(int newLogEntries)
		{
			if(!isScrolledToBottom)
			{
				scrollToBottomSpeed = 0f;
				return;
			}

			calculatedScrollDistanceToBottom += newLogEntries * logEntryHeight;

			if(calculatedScrollDistanceToBottom <= 0f)
			{
				scrollToBottomSpeed = 0f;
				return;
			}

			if(calculatedScrollDistanceToBottom < 1f)
			{
				calculatedScrollDistanceToBottom = 1f;
			}

			const float speedMultiplier = 0.01f;

			scrollToBottomSpeed = Mathf.Max(calculatedScrollDistanceToBottom * speedMultiplier, scrollToBottomMinSpeed);
		}

		private void OnLogMessageSuppressed(string messageUnformatted, string message, string stackTrace, LogType type, Object context)
		{
			Repaint();

			var log = new Log(messageUnformatted, message, stackTrace, type, context, false);

			logs.Add(log);

			// Counts are still incremented even for suppressed messages
			switch(type)
			{
				case LogType.Log:
					if(logsInfo < 9999)
					{
						infoLabel.text = (logsInfo + 1).ToString();
					}
					logsInfo++;
					break;
				case LogType.Warning:
					int countWas = logsWarning + compileWarnings.Count;
					if(countWas < 9999)
					{
						warningLabel.text = (countWas + 1).ToString();
					}
					logsWarning++;
					break;
				default:
					countWas = logsError + compileErrors.Count;
					if(countWas < 9999)
					{
						errorLabel.text = (countWas + 1).ToString();
					}
					logsError++;
					//if(errorPause && Application.isPlaying) // UPDATE: Won't do error pause for suppressed messages.
					//{
					//	Debug.Break();
					//}
					break;
			}
		}

		private void RebuildVisibleLogs(bool skipScrollAnimations)
		{
			var selectedLogWas = SelectedLog;

			int visibleCountWas = visibleLogs.Count;

			visibleLogs.Clear();
			collapsedLogCountByHash.Clear();

			for(int n = 0, count = compileErrors.Count; n < count; n++)
			{
				var log = compileErrors[n];
				if(ShouldShow(log))
				{
					visibleLogs.Add(log);
				}
			}
			for(int n = 0, count = compileWarnings.Count; n < count; n++)
			{
				var log = compileWarnings[n];
				if(ShouldShow(log))
				{
					visibleLogs.Add(log);
				}
			}

			if(!collapse)
			{
				for(int n = 0, count = logs.Count; n < count; n++)
				{
					var log = logs[n];
					if(ShouldShow(log))
					{
						visibleLogs.Add(log);
					}
				}
			}
			else
			{
				for(int n = 0, count = logs.Count; n < count; n++)
				{
					var log = logs[n];
					if(ShouldShow(log))
					{
						int hash = log.hash;
						int collapsedCount;
						if(collapsedLogCountByHash.TryGetValue(hash, out collapsedCount))
						{
							collapsedLogCountByHash[hash] = collapsedCount + 1;
						}
						else
						{
							collapsedLogCountByHash[hash] = 1;
							visibleLogs.Add(log);
						}
					}
				}
			}

			int newCount = visibleLogs.Count;

			Repaint();

			if(isScrolledToBottom && skipScrollAnimations)
			{
				ScrollToBottomInstantly();
				scrollToBottomInstantlyNextTick = true;
			}
			else
			{
				UpdateScrollToBottomSpeed(newCount - visibleCountWas);
			}

			if(selectedLogWas != null)
			{
				SelectedLog = selectedLogWas;
			}
		}

		private void ScrollToBottomInstantly()
		{
			scrollPosition.y = GetMaxScroll() + 10f;
		}

		private bool ShouldShow(Log log)
		{
			switch(log.type)
			{
				case LogType.Log:
					if(!showLog)
					{
						return false;
					}
					break;
				case LogType.Warning:
					if(!showWarnings)
					{
						return false;
					}
					break;
				default:
					if(!showErrors)
					{
						return false;
					}
					break;
			}

			if(!Debug.channels.ShouldShowMessageWithChannels(log.channels))
			{
				return false;
			}

			if(filterTrimmed.Length == 0)
			{
				return true;
			}

			if(log.textUnformatted.IndexOf(filterTrimmed, StringComparison.OrdinalIgnoreCase) != -1)
			{
				return true;
			}

			int filterPartCount = filterParts.Length;
			if(filterPartCount > 1)
			{
				bool passedAll = true;
				for(int n = filterPartCount - 1; n >= 0; n--)
				{
					if(log.textUnformatted.IndexOf(filterParts[n], StringComparison.OrdinalIgnoreCase) == -1)
					{
						passedAll = false;
						break;
					}
				}
				if(passedAll)
				{
					return true;
				}
			}

			if(!useRegex)
			{
				return false;
			}

			if(filterRegex.IsMatch(log.textUnformatted))
			{
				return true;
			}

			filterPartCount = filterRegexParts.Count;

			if(filterPartCount <= 1)
			{
				return false;
			}

			for(int n = filterPartCount - 1; n >= 0; n--)
			{
				if(!filterRegexParts[n].IsMatch(log.textUnformatted))
				{
					return false;
				}
			}
			return true;
		}

		public void AddItemsToMenu(GenericMenu menu)
		{
			menu.AddItem(new GUIContent("Open Player Log"), false, OpenPlayerLog);
			menu.AddItem(new GUIContent("Open Editor Log"), false, OpenEditorLog);
			menu.AddItem(new GUIContent("Show Timestamp"), showTimestamps, ToggleShowTimestamp);
			menu.AddItem(new GUIContent("Stack Trace Logging/All/None"), IsStackTraceLogTypeEnabledForAll(StackTraceLogType.None), SetStackTraceLogTypeAll, StackTraceLogType.None);
			menu.AddItem(new GUIContent("Stack Trace Logging/All/ScriptOnly"), IsStackTraceLogTypeEnabledForAll(StackTraceLogType.ScriptOnly), SetStackTraceLogTypeAll, StackTraceLogType.ScriptOnly);
			menu.AddItem(new GUIContent("Stack Trace Logging/All/Full"), IsStackTraceLogTypeEnabledForAll(StackTraceLogType.Full), SetStackTraceLogTypeAll, StackTraceLogType.Full);
			menu.AddItem(new GUIContent("Smooth Scrolling"), DebugLogExtensionsPreferences.ScrollAnimations, ()=> DebugLogExtensionsPreferences.ScrollAnimations = !DebugLogExtensionsPreferences.ScrollAnimations);
			menu.AddItem(new GUIContent("Compact Mode"), compactMode, ToggleCompactMode);
			#if UNITY_2021_2_OR_NEWER
			menu.AddItem(new GUIContent("Use Monospace Font/Never"), useMonospaceFont == UseMonospaceFont.Never, SetUseMonospaceFont, UseMonospaceFont.Never);
			menu.AddItem(new GUIContent("Use Monospace Font/Details View Only"), useMonospaceFont == UseMonospaceFont.DetailsViewOnly, SetUseMonospaceFont, UseMonospaceFont.DetailsViewOnly);
			menu.AddItem(new GUIContent("Use Monospace Font/Always"), useMonospaceFont == UseMonospaceFont.Always, SetUseMonospaceFont, UseMonospaceFont.Always);
			#endif
		}

		#if UNITY_2021_2_OR_NEWER
		private void SetUseMonospaceFont(object value)
        {
            useMonospaceFont = (UseMonospaceFont)value;
            ApplyFontSettings();
        }
		#endif

        private void ApplyFontSettings()
        {
			#if UNITY_2021_2_OR_NEWER
			Font monospaceFont = EditorGUIUtility.Load("Fonts/RobotoMono/RobotoMono-Regular.ttf") as Font;
			boxStyle.font = useMonospaceFont == UseMonospaceFont.Always ? monospaceFont : null;
			textAreaStyle.font = useMonospaceFont != UseMonospaceFont.Never ? monospaceFont : null;
			#endif

			const int listEntryFontSize = 12;
			boxStyle.fontSize = listEntryFontSize;

			const int detailsViewFontSize = 14;
			textAreaStyle.fontSize = detailsViewFontSize;
		}

        private void ToggleCompactMode()
        {
            compactMode = !compactMode;
			DebugLogExtensionsPreferences.CompactModeEnabled = compactMode;
			UpdateCompactModeRelatedValues();
			ScrollToBottomInstantly();
			isScrolledToBottom = true;
			scrollToBottomInstantlyNextTick = true;
		}

        private void UpdateCompactModeRelatedValues()
        {
            if(compactMode)
            {
                logEntryHeight = logEntryHeightCompact;
                iconSize = iconSizeCompact;
            }
            else
            {
                logEntryHeight = logEntryHeightNormal;
                iconSize = iconSizeNormal;
            }

			// Clear cached labels because displayed line count has changed
            for(int i = logs.Count - 1; i >= 0; i--)
            {
				logs[i].listViewLabel = null;
            }
        }

        [SerializeField]
		private bool compactMode;

		private void OpenPlayerLog()
		{
			Application.OpenURL(GetPlayerLogPath());
		}

		private string GetPlayerLogPath()
		{
			switch(Application.platform)
			{
				case RuntimePlatform.OSXEditor:
					return "~/Library/Logs/Unity/Player.log";
				case RuntimePlatform.LinuxEditor:
					return Path.Combine("~/.config/unity3d", Application.companyName, Application.productName, "Player.log");
				default:
					return Path.Combine(Environment.GetEnvironmentVariable("AppData"), "..", "LocalLow", Application.companyName, Application.productName, "Player.log");
			}
		}

		private void OpenEditorLog()
		{
			Application.OpenURL(GetEditorLogPath());
		}

		private string GetEditorLogPath()
		{
			switch(Application.platform)
			{
				case RuntimePlatform.OSXEditor:
					return "~/Library/Logs/Unity/Editor.log";
				case RuntimePlatform.LinuxEditor:
					return Path.Combine("~/.config/unity3d", "Editor.log");
				default:
					return Path.Combine(Environment.GetEnvironmentVariable("LocalAppData"), "Unity", "Editor", "Editor.log");
			}
		}

		private void ToggleShowTimestamp()
		{
			showTimestamps = !showTimestamps;
			ClearAllCachedLogEntryLabels();
			Repaint();
		}

		private void ClearAllCachedLogEntryLabels()
        {
			for(int i = logs.Count - 1; i >= 0; i--)
            {
				logs[i].listViewLabel = null;
			}
        }

		private void SetStackTraceLogTypeAll(object parameter)
		{
			var type = (StackTraceLogType)parameter;
			PlayerSettings.SetStackTraceLogType(LogType.Log, type);
			PlayerSettings.SetStackTraceLogType(LogType.Warning, type);
			PlayerSettings.SetStackTraceLogType(LogType.Error, type);
			PlayerSettings.SetStackTraceLogType(LogType.Assert, type);
			PlayerSettings.SetStackTraceLogType(LogType.Exception, type);
		}

		private bool IsStackTraceLogTypeEnabledForAll(StackTraceLogType type)
		{
			return PlayerSettings.GetStackTraceLogType(LogType.Log) == type && PlayerSettings.GetStackTraceLogType(LogType.Error) == type && PlayerSettings.GetStackTraceLogType(LogType.Error) == type && PlayerSettings.GetStackTraceLogType(LogType.Assert) == type && PlayerSettings.GetStackTraceLogType(LogType.Exception) == type;
		}

		[UsedImplicitly]
		public override void OnBeforeSerialize()
		{
			base.OnBeforeSerialize();

			int count = collapsedLogCountByHash.Count;
			collapsedLogCountByHashKeys = new int[count];
			collapsedLogCountByHash.Keys.CopyTo(collapsedLogCountByHashKeys, 0);
			collapsedLogCountByHashValues = new int[count];
			collapsedLogCountByHash.Values.CopyTo(collapsedLogCountByHashValues, 0);
		}

		[UsedImplicitly]
		public override void OnAfterDeserialize()
		{
			base.OnAfterDeserialize();

			if(collapsedLogCountByHashKeys != null)
			{
				int count = collapsedLogCountByHashKeys.Length;
				collapsedLogCountByHash.Clear();
				for(int n = 0; n < count; n++)
				{
					collapsedLogCountByHash.Add(collapsedLogCountByHashKeys[n], collapsedLogCountByHashValues[n]);
				}
			}
		}

		private static LogType GetLogTypeFromMode(Mode mode)
		{
			if(mode.HasFlag(Mode.Log) || mode.HasFlag(Mode.ScriptingLog))
			{
				return LogType.Log;
			}
			if(mode.HasFlag(Mode.ScriptCompileWarning) || mode.HasFlag(Mode.ScriptingWarning) || mode.HasFlag(Mode.AssetImportWarning))
			{
				return LogType.Warning;
			}
			if(mode.HasFlag(Mode.Assert) || mode.HasFlag(Mode.ScriptingAssertion))
			{
				return LogType.Assert;
			}
			return LogType.Error;
		}
	}
}