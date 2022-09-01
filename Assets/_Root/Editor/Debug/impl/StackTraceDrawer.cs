using UnityEngine;

namespace Pancake.Debugging.Console
{
	using System;
	using System.IO;
	using System.Linq;
	using System.Collections.Generic;
	using System.Text;
	using UnityEditor;
	using JetBrains.Annotations;
	using Object = UnityEngine.Object;

	[Serializable]
	public class StackTraceDrawer
	{
		private static readonly char[] methodArgsStartChars = new char[] { ' ', '(' };

		private static readonly string DebugStackTracePrefix;
		private static readonly string DevStackTracePrefix;
		private static readonly string CriticalStackTracePrefix;

		[SerializeField]
		private List<StackTraceData> stackTrace = new List<StackTraceData>();

		[SerializeField]
		private Vector2 scrollPosition;

		[SerializeField]
		private ColumnDrawer columnDrawer = new ColumnDrawer();

		[SerializeField]
		private CopyPreferences copyPreferences = new CopyPreferences();

		[NonSerialized]
		private GUIStyle textAreaStyle;
		[NonSerialized]
		private GUIStyle stackTraceEntryStyle;
		[NonSerialized]
		private GUIStyle stackTraceColumnHeaderStyle;

		[NonSerialized]
		private readonly StringBuilder stringBuilder = new StringBuilder();

		[NonSerialized]
		private Log log;

		[NonSerialized]
		private GUIContent copyIcon;

		[SerializeField]
		private GUIContent searchIcon;

		private int VisibleColumnCount
		{
			get
			{
				return columnDrawer.VisibleColumnCount;
			}
		}

		public Log Log
		{
			get
			{
				return log;
			}

			set
			{
				// Without this drawn texts might not update properly I think.
				EditorGUIUtility.editingTextField = false;

				log = value;
				UpdateStackTrace();
			}
		}

		static StackTraceDrawer()
		{
			DebugStackTracePrefix = typeof(Debug).FullName + ":";
			DevStackTracePrefix = typeof(Dev).FullName + ":";
			CriticalStackTracePrefix = typeof(Critical).FullName + ":";
		}

		public void OnEnable([NotNull]GUIStyle textAreaStyle, Log selectedLog, Color lineColor)
		{
			this.textAreaStyle = textAreaStyle;

			stackTraceEntryStyle = new GUIStyle(textAreaStyle);
			stackTraceEntryStyle.wordWrap = false;
			stackTraceEntryStyle.alignment = TextAnchor.MiddleLeft;

			stackTraceColumnHeaderStyle = new GUIStyle();
			stackTraceColumnHeaderStyle.fontStyle = FontStyle.Bold;
			stackTraceColumnHeaderStyle.alignment = TextAnchor.MiddleCenter;
			stackTraceColumnHeaderStyle.wordWrap = false;
			stackTraceColumnHeaderStyle.clipping = TextClipping.Clip;

			if(columnDrawer.ColumnCount == 0)
			{
				columnDrawer.AddColumn((int)Column.FindReferences, "Find", "", 0.2f, false, EditorPrefs.GetBool(Key.ShowFindReferences, true), "Find Objects Of Type");
				columnDrawer.AddColumn((int)Column.StackTrace, "Stack Trace", "", 0.2f, false, EditorPrefs.GetBool(Key.ShowStackTrace, false), "Full Stack Trace");
				columnDrawer.AddColumn((int)Column.Namespace, "Namespace", "", 0.2f, false, EditorPrefs.GetBool(Key.ShowNamespace, false), "Namespace");
				columnDrawer.AddColumn((int)Column.ScriptReference, "Script", "", 0.2f, false, EditorPrefs.GetBool(Key.ShowScriptReference, true), "Script Asset");
				columnDrawer.AddColumn((int)Column.Class, "Class", "", 0.2f, false, EditorPrefs.GetBool(Key.ShowClass, false), "Class");
				columnDrawer.AddColumn((int)Column.MethodShort, "Method", "", 0.2f, false, EditorPrefs.GetBool(Key.ShowMethodShort, true), "Method");
				columnDrawer.AddColumn((int)Column.Method, "Method", "", 0.2f, false, EditorPrefs.GetBool(Key.ShowMethod, false), "Method + Arguments");
				columnDrawer.AddColumn((int)Column.ClassAndMethodShort, "Method", "", 0.2f, false, EditorPrefs.GetBool(Key.ShowClassAndMethodShort, false), "Class + Method");
				columnDrawer.AddColumn((int)Column.ClassAndMethod, "Method", "", 0.2f, false, EditorPrefs.GetBool(Key.ShowClassAndMethod, false), "Class + Method + Arguments");
				columnDrawer.AddColumn((int)Column.Path, "Path", "", 0.2f, false, EditorPrefs.GetBool(Key.ShowPath, false), "Full Path");
				columnDrawer.AddColumn((int)Column.PathShort, "Path", "", 0.2f, false, EditorPrefs.GetBool(Key.ShowPathShort, false), "Truncated Path");
				columnDrawer.AddColumn((int)Column.LineNumber, "Line", "", 0.2f, false, EditorPrefs.GetBool(Key.ShowLineNumber, true), "Line");
			}
			columnDrawer.OnEnable(textAreaStyle, lineColor, OnItemClicked);
			columnDrawer.OnFinishedBuildingColumns();

			copyIcon = new GUIContent(EditorGUIUtility.IconContent("Clipboard"));
			copyIcon.tooltip = "Copy to clipboard";

			// Line number use wrong color formatting without this delay, probably because DebugFormatter hasn't had time to be initialized properly yet.
			// I'm not even sure if this is strictly necessary though?
			EditorApplication.delayCall += ()=> Log = selectedLog;
			log = selectedLog;
		}

		private void OnItemClicked(int columnId, int rowIndex, int clickCount, Rect drawRect)
		{
			if(rowIndex < 0 || rowIndex >= stackTrace.Count)
			{
				#if DEV_MODE
				Debug.LogWarning("StackTraceDrawer.OnItemClicked called with invalid rowIndex "+rowIndex);
				#endif
				return;
			}

			int button = Event.current.button;
			var stackTraceLine = stackTrace[rowIndex];
			string path = stackTraceLine.Path;
			var target = stackTraceLine.ScriptReference;
			var column = (Column)columnId;

			// Middle mouse button always pings target, no matter the column.
			if(button == 2 && column != Column.FindReferences && column != Column.ScriptReference)
			{
				if(path.Length == 0)
				{
					return;
				}

				if(Event.current.type != EventType.Used)
				{
					Event.current.Use();
				}
				
				if(target != null)
				{
					EditorGUIUtility.PingObject(target);
					return;
				}

				// For DLLs outside Assets folder open their containing folder in the explorer.
				EditorUtility.RevealInFinder(path);
				return;
			}

			Type ignored;
			bool isUnityObjectType = stackTraceLine.TryExtractUnityObjectTypeFromStackTrace(out ignored);
			bool hasScriptAsset = stackTraceLine.ScriptReference as MonoScript != null;

			Object[] foundInstances;
			switch(column)
			{
				case Column.Namespace:
					if(button == 1 && !EditorGUIUtility.textFieldHasSelection && stackTraceLine.Namespace.Length > 0)
					{
						var menu = new GenericMenu();
						menu.AddItem(new GUIContent("Copy Namespace"), false, () => EditorGUIUtility.systemCopyBuffer = stackTraceLine.Namespace);
						if(Event.current.type != EventType.Used)
						{
							Event.current.Use();
						}
						menu.DropDown(drawRect);
						return;
					}
					return;
				case Column.Class:
					if(button == 0 || button == 2)
					{
						if(Event.current.type != EventType.Used)
						{
							Event.current.Use();
						}
						if(target != null)
						{
							if(clickCount == 2)
							{
								Selection.objects = new Object[] { target };
								return;
							}
							EditorGUIUtility.PingObject(target);
						}
						return;
					}
					if(button == 1 && !EditorGUIUtility.textFieldHasSelection && stackTraceLine.ClassName.Length > 0)
					{
						var menu = new GenericMenu();

						if(hasScriptAsset)
						{
							menu.AddItem(new GUIContent("Edit Script"), false, ()=>OpenScriptFileAtStackTraceRow(rowIndex));
						}
						menu.AddItem(new GUIContent("Copy Name"), false, ()=> EditorGUIUtility.systemCopyBuffer = stackTraceLine.ClassName);
						menu.AddItem(new GUIContent("Copy Full Name"), false, () => EditorGUIUtility.systemCopyBuffer = stackTraceLine.Namespace + "."+ stackTraceLine.ClassName);
						if(isUnityObjectType)
						{
							menu.AddItem(new GUIContent("Find Instances Of Type"), false, () =>
							{
								if(TryFindUnityObjectInstances(stackTraceLine, out foundInstances))
								{
									Selection.objects = foundInstances;
								};
							});
						}
						if(Event.current.type != EventType.Used)
						{
							Event.current.Use();
						}
						menu.DropDown(drawRect);
						return;
					}
					return;
				case Column.ClassAndMethod:
				case Column.ClassAndMethodShort:
				case Column.Method:
				case Column.MethodShort:
					if(button == 0 || button == 2)
					{
						if(Event.current.type != EventType.Used)
						{
							Event.current.Use();
						}
						OpenScriptFileAtStackTraceRow(rowIndex);
						return;
					}
					if(button == 1 && !EditorGUIUtility.textFieldHasSelection && stackTraceLine.MethodName.Length > 0)
					{
						var menu = new GenericMenu();
						if(hasScriptAsset)
						{
							menu.AddItem(new GUIContent("Go To Line"), false, () => OpenScriptFileAtStackTraceRow(rowIndex));
						}

						if(column == Column.ClassAndMethod || column == Column.ClassAndMethodShort)
                        {
							menu.AddItem(new GUIContent("Copy Class Name"), false, () => EditorGUIUtility.systemCopyBuffer = stackTraceLine.ClassName);
							menu.AddItem(new GUIContent("Copy Method Name"), false, () => EditorGUIUtility.systemCopyBuffer = stackTraceLine.MethodName);
						}
						else
						{
							menu.AddItem(new GUIContent("Copy Name"), false, ()=> EditorGUIUtility.systemCopyBuffer = stackTraceLine.MethodName);
						}
						menu.AddItem(new GUIContent("Copy Full Name"), false, () => EditorGUIUtility.systemCopyBuffer = stackTraceLine.Namespace + "."+ stackTraceLine.ClassName + "." + stackTraceLine.MethodName);
						if(Event.current.type != EventType.Used)
						{
							Event.current.Use();
						}
						menu.DropDown(drawRect);
						return;
					}
					return;
				case Column.ScriptReference:
					// Let the object reference field handle click events if there's a reference.
					if(target != null)
					{
						return;
					}

					if(button == 0)
					{
						if(Event.current.type != EventType.Used)
						{
							Event.current.Use();
						}

						// Double click to show folder in explorer.
						if(clickCount == 2)
						{
							EditorUtility.RevealInFinder(path);
							return;
						}
						return;
					}
					if(button == 1 && !EditorGUIUtility.textFieldHasSelection)
					{
						if(stackTraceLine.Path.Length > 0)
						{
							var menu = new GenericMenu();
							menu.AddItem(new GUIContent("Copy Path"), false, ()=> EditorGUIUtility.systemCopyBuffer = stackTraceLine.Path);
							menu.AddItem(new GUIContent("Show In Explorer"), false, () => EditorUtility.RevealInFinder(path));
							if(isUnityObjectType)
							{
								menu.AddItem(new GUIContent("Find Instances Of Type"), false, () =>
								{
									if(TryFindUnityObjectInstances(stackTraceLine, out foundInstances))
									{
										Selection.objects = foundInstances;
									};
								});
							}
							if(Event.current.type != EventType.Used)
							{
								Event.current.Use();
							}
							menu.DropDown(drawRect);
						}
						return;
					}
					if(button == 2)
					{
						if(target != null)
						{
							#if POWER_INSPECTOR
							var selectionWas = Selection.objects;
							Selection.activeObject = target;
							EditorApplication.ExecuteMenuItem("Edit/Peek\tMMB");
							Selection.objects = selectionWas;
							#else
							EditorGUIUtility.PingObject(target);
							#endif
							return;
						}
					}
					return;
				case Column.Path:
				case Column.PathShort:
					if(button == 0)
					{
						if(Event.current.type != EventType.Used)
						{
							Event.current.Use();
						}

						// Double click to show folder in explorer.
						if(clickCount == 2)
						{
							EditorUtility.RevealInFinder(path);
							return;
						}
						else if(target != null)
						{
							EditorGUIUtility.PingObject(target);
						}
						return;
					}
					if(button == 1 && !EditorGUIUtility.textFieldHasSelection)
					{
						if(stackTraceLine.Path.Length > 0)
						{
							var menu = new GenericMenu();
							if(hasScriptAsset)
							{
								menu.AddItem(new GUIContent("Edit Script"), false, () => OpenScriptFileAtStackTraceRow(rowIndex));
							}
							menu.AddItem(new GUIContent("Copy Path"), false, ()=> EditorGUIUtility.systemCopyBuffer = stackTraceLine.Path);
							if(target != null)
							{
								menu.AddItem(new GUIContent("Ping"), false, () => EditorGUIUtility.PingObject(target));
							}
							menu.AddItem(new GUIContent("Show In Explorer"), false, () => EditorUtility.RevealInFinder(path));
							if(isUnityObjectType)
							{
								menu.AddItem(new GUIContent("Find Instances Of Type"), false, () =>
								{
									if(TryFindUnityObjectInstances(stackTraceLine, out foundInstances))
									{
										Selection.objects = foundInstances;
									};
								});
							}

							if(Event.current.type != EventType.Used)
							{
								Event.current.Use();
							}
							menu.DropDown(drawRect);
						}
					}
					return;
				case Column.LineNumber:
					if(button == 0 || button == 2)
					{
						if(Event.current.type != EventType.Used)
						{
							Event.current.Use();
						}
						OpenScriptFileAtStackTraceRow(rowIndex);
						return;
					}
					if(button == 1)
					{
						if(Event.current.type != EventType.Used)
						{
							Event.current.Use();
						}

						var menu = new GenericMenu();
						if(hasScriptAsset)
						{
							menu.AddItem(new GUIContent("Go To Line"), false, () => OpenScriptFileAtStackTraceRow(rowIndex));
						}
						menu.AddItem(new GUIContent("Copy Line Number"), false, ()=> EditorGUIUtility.systemCopyBuffer = stackTraceLine.LineNumber.ToString());
						if(stackTraceLine.Path.Length > 0)
						{
							menu.AddItem(new GUIContent("Copy Path"), false, ()=> EditorGUIUtility.systemCopyBuffer = stackTraceLine.Path);
							menu.AddItem(new GUIContent("Copy Path And Line Number"), false, () => EditorGUIUtility.systemCopyBuffer = stackTraceLine.Path + ":" + stackTraceLine.LineNumber);
						}
						menu.DropDown(drawRect);
					}
					return;
				case Column.FindReferences:
					if(TryFindUnityObjectInstances(stackTraceLine, out foundInstances))
					{
						if(button == 0)
                        {
							Selection.objects = foundInstances;
						}
						else if(button == 2)
                        {
							EditorGUIUtility.PingObject(foundInstances[0]);
							return;
						}
						else if(button == 1)
                        {
							var menu = new GenericMenu();
							foreach(var instance in foundInstances)
                            {
								menu.AddItem(new GUIContent(GetHierarchyOrAssetPath(instance)), false, () => Selection.activeObject = instance);
							}
							menu.DropDown(drawRect);
						}
					}
					return;
				case Column.StackTrace:
					if(button == 1 && !EditorGUIUtility.textFieldHasSelection && stackTraceLine.Path.Length > 0)
					{
						var menu = new GenericMenu();
						menu.AddItem(new GUIContent("Copy Row"), false, () => EditorGUIUtility.systemCopyBuffer = stackTraceLine.FullStackTrace);
						menu.AddItem(new GUIContent("Copy Path"), false, () => EditorGUIUtility.systemCopyBuffer = stackTraceLine.Path);
						if(target != null)
						{
							menu.AddItem(new GUIContent("Ping"), false, () => EditorGUIUtility.PingObject(target));
						}
						menu.AddItem(new GUIContent("Show In Explorer"), false, () => EditorUtility.RevealInFinder(path));
						if(isUnityObjectType)
						{
							menu.AddItem(new GUIContent("Find Instances Of Type"), false, () =>
							{
								if(TryFindUnityObjectInstances(stackTraceLine, out foundInstances))
								{
									Selection.objects = foundInstances;
								};
							});
						}

						if(Event.current.type != EventType.Used)
						{
							Event.current.Use();
						}
						menu.DropDown(drawRect);
					}
					return;
			}
		}

		private bool TryFindUnityObjectInstances(StackTraceData stackTraceLine, out Object[] foundInstances)
        {
			Type classType;
			if(!stackTraceLine.TryExtractUnityObjectTypeFromStackTrace(out classType))
			{
				foundInstances = new Object[0];
				return false;
			}
			if(typeof(ScriptableObject).IsAssignableFrom(classType))
			{
				foundInstances = AssetDatabase.FindAssets("t:" + classType.Name).Select((guid) => AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(guid), classType)).ToArray();
			}
			else if(typeof(Component).IsAssignableFrom(classType))
			{
				#if UNITY_2020_1_OR_NEWER
				foundInstances = Object.FindObjectsOfType(classType, true).Select((i) => (i as Component).gameObject).ToArray();
				#else
				foundInstances = Object.FindObjectsOfType(classType).Select((i) => (i as Component).gameObject).ToArray();
				#endif
			}
			else
            {
				foundInstances = new Object[0];
				return false;
            }
			return foundInstances.Length > 0;
        }

		private string GetHierarchyOrAssetPath([NotNull] Object obj)
        {
			var gameObject = obj as GameObject;
			if(gameObject != null)
            {
				return GetHierarchyPath(gameObject.transform);
            }
			var component = obj as Component;
			if(component != null)
			{
				return GetHierarchyPath(component.transform);
			}
			string assetPath = AssetDatabase.GetAssetPath(obj);
			if(assetPath.Length > 0)
            {
				return assetPath;
			}
			return obj.name;
        }

		private string GetHierarchyPath([NotNull]Transform transform)
		{
			stringBuilder.Append(transform.name);
			while(transform.parent != null)
			{
				transform = transform.parent;
				stringBuilder.Insert(0, '/');
				stringBuilder.Insert(0, transform.name);
			}
			string result = stringBuilder.ToString();
			stringBuilder.Length = 0;
			return result;
		}

		public void Clear()
		{
			stackTrace.Clear();
		}

		public void Initialize(GUIContent searchIcon)
        {
			this.searchIcon = searchIcon;
		}

		public void OnGUI(Rect drawRect, int clickCount)
		{
			if(drawRect.width <= 1f || drawRect.height <= 1f)
			{
				return;
			}

			scrollPosition = GUILayout.BeginScrollView(scrollPosition);

			if(log != null)
			{
				string message = log.text.Trim();
				var guiContent = new GUIContent(message);
				var messageRect = GUILayoutUtility.GetRect(guiContent, textAreaStyle);

				var rowRect = messageRect;
				rowRect.height = ColumnDrawer.RowHeight;
				rowRect.y += messageRect.height + 5f;

				var copyRect = rowRect;
				copyRect.x += rowRect.width - 25f;
				copyRect.y -= ColumnDrawer.RowHeight + 2f;
				copyRect.width = 20f;
				copyRect.height = 20f;

				if(clickCount == 1 && Event.current.button == 0 && Event.current.rawType == EventType.MouseUp && copyRect.Contains(Event.current.mousePosition))
				{
					Event.current.Use();
					CopyToClipboard();
					GUIUtility.ExitGUI();
				}

				if(clickCount == 1 && Event.current.button == 1 && Event.current.rawType == EventType.MouseUp && copyRect.Contains(Event.current.mousePosition))
				{
					Event.current.Use();

					var menu = new GenericMenu();
					menu.AddItem(new GUIContent("Include Message"), copyPreferences.includeMessage, () => copyPreferences.includeMessage = !copyPreferences.includeMessage);
					menu.AddSeparator("");
					menu.AddItem(new GUIContent("Add Separator"), copyPreferences.addSeparators, () => copyPreferences.addSeparators = !copyPreferences.addSeparators);
					menu.AddSeparator("");
					menu.AddItem(new GUIContent("Include Full Stack Trace"), copyPreferences.IncludeFullStackTrace, () => copyPreferences.IncludeFullStackTrace = !copyPreferences.IncludeFullStackTrace);
					menu.AddItem(new GUIContent("   Include Namespace"), copyPreferences.IncludeNamespace, () => copyPreferences.IncludeNamespace = !copyPreferences.IncludeNamespace);
					menu.AddItem(new GUIContent("   Include Class Name"), copyPreferences.IncludeClassName, () => copyPreferences.IncludeClassName = !copyPreferences.IncludeClassName);
					menu.AddItem(new GUIContent("   Include Method Name"), copyPreferences.includeMethodName, () => copyPreferences.includeMethodName = !copyPreferences.includeMethodName);
					menu.AddItem(new GUIContent("   Include Path"), copyPreferences.includePath, () => copyPreferences.includePath = !copyPreferences.includePath);
					menu.AddItem(new GUIContent("   Include Line Number"), copyPreferences.includeLineNumber, () => copyPreferences.includeLineNumber = !copyPreferences.includeLineNumber);
					menu.DropDown(copyRect);
					GUIUtility.ExitGUI();
				}
				
				if(clickCount == 2 && Event.current.button == 0 && Event.current.rawType == EventType.MouseUp && messageRect.Contains(Event.current.mousePosition))
				{
					Event.current.Use();
					clickCount = 0;					

					OpenScript();
					GUIUtility.ExitGUI();
				}

				EditorGUIUtility.AddCursorRect(copyRect, MouseCursor.Link);

				EditorGUI.SelectableLabel(messageRect, message, textAreaStyle);
				GUILayout.Space(messageRect.height);

				if(columnDrawer.RowCount > 0)
				{
					columnDrawer.OnGUI(rowRect, clickCount);
					GUILayout.Space(columnDrawer.Height);
				}

				var sizeWas = EditorGUIUtility.GetIconSize();
				EditorGUIUtility.SetIconSize(new Vector2(16f, 16f));
				GUI.Label(copyRect, copyIcon);
				EditorGUIUtility.SetIconSize(sizeWas);
			}

			GUILayout.EndScrollView();
		}

		public void CopyToClipboard()
		{
			var sb = new StringBuilder();

			if(copyPreferences.includeMessage)
			{
				sb.Append(log.textUnformatted);
				if(!log.textUnformatted.EndsWith("\n"))
				{
					sb.Append("\r\n");
				}

				if(copyPreferences.addSeparators)
				{
					sb.Append("-------------------------------------------------\r\n");
				}
			}

			if(copyPreferences.IncludeFullStackTrace)
			{
				sb.Append(log.stackTrace);
			}
			else
			{
				foreach(var stackTraceLine in stackTrace)
				{
					bool rowHasContent = false;

					if(copyPreferences.IncludeNamespace && stackTraceLine.Namespace.Length > 0)
					{
						sb.Append(stackTraceLine.Namespace);
						rowHasContent = true;
					}

					if(copyPreferences.IncludeClassName && stackTraceLine.ClassName.Length > 0)
					{
						if(rowHasContent)
						{
							sb.Append(".");
						}
						sb.Append(stackTraceLine.ClassName);
						rowHasContent = true;
					}

					if(copyPreferences.includeMethodName && stackTraceLine.MethodName.Length > 0)
					{
						if(rowHasContent)
						{
							sb.Append(".");
						}
						sb.Append(stackTraceLine.MethodName);
						rowHasContent = true;
					}

					if(copyPreferences.includePath && stackTraceLine.Path.Length > 0)
					{
						if(rowHasContent)
						{
							sb.Append(" (at ");
						}

						sb.Append(stackTraceLine.Path);

						if(copyPreferences.includeLineNumber && stackTraceLine.LineNumber != -1)
						{
							sb.Append(":");
							sb.Append(stackTraceLine.LineNumber);
						}

						if(rowHasContent)
						{
							sb.Append(")");
						}

						rowHasContent = true;
					}
					else if(copyPreferences.includeLineNumber && stackTraceLine.LineNumber != -1)
					{
						if(rowHasContent)
						{
							sb.Append(":");
							sb.Append(stackTraceLine.LineNumber);
						}
						else
						{
							sb.Append(stackTraceLine.LineNumber);
						}
						rowHasContent = true;
					}

					if(rowHasContent)
					{
						sb.Append("\r\n");
					}
				}
			}

			EditorGUIUtility.systemCopyBuffer = sb.ToString().Trim();
		}

		private Column GetNextVisibleColumn(Column origin, bool loopBack)
		{
			int index = columnDrawer.GetNextVisibleColumnId((int)origin, loopBack);
			return index <= 0 ? Column.None : (Column)index;
		}

		private Column GetPreviousVisibleColumn(Column origin, bool loopBack)
		{
			int index = columnDrawer.GetPreviousVisibleColumnId((int)origin, loopBack);
			return index <= 0 ? Column.None : (Column)index;
		}

		private float GetColumnWidth(Column column)
		{
			return columnDrawer.GetColumnWith((int)column);
		}

		private void SetColumnWidth(Column column, float width, bool adjustAdjascent = true)
		{
			const float min = 0.02f;
			float max = 0.90f;
			width = Mathf.Clamp(width, min, max);

			float currentWidth = GetColumnWidth(column);
			float increasedAmount = width - currentWidth;
			if(increasedAmount == 0f)
			{
				return;
			}

			if(adjustAdjascent)
			{
				var adjascentColumn = GetNextVisibleColumn(column, false);
				if(adjascentColumn == Column.None)
				{
					adjascentColumn = GetPreviousVisibleColumn(column, false);
				}
				if(adjascentColumn != Column.None)
				{
					float adjascentColumnWidth = GetColumnWidth(adjascentColumn);
					float setAdjascentColumnWidth = adjascentColumnWidth - increasedAmount;
					if(setAdjascentColumnWidth <= 0.02f && increasedAmount > 0f)
					{
						return;
					}
					SetColumnWidth(adjascentColumn, setAdjascentColumnWidth, false);
				}
			}

			columnDrawer.SetColumnWith((int)column, width);

			#if DEV_MODE
			AssertVisibleColumnWidthsAddUpToOne();
			#endif
		}

		private Column GetVisibleColumnAtIndex(int visibleColumnIndex)
		{
			int id = columnDrawer.GetVisibleColumnAtIndex(visibleColumnIndex);
			return id <= 0 ? Column.None : (Column)id;
		}

		#if DEV_MODE
		private void AssertVisibleColumnWidthsAddUpToOne()
		{
			float width = 0f;
			for(int n = VisibleColumnCount - 1; n >= 0; n--)
			{
				width += GetColumnWidth(GetVisibleColumnAtIndex(n));
			}
			Debug.Assert(width == 1f, width);
		}
		#endif

		private void UpdateStackTrace()
		{
			stackTrace.Clear();
			columnDrawer.ResetContents();

			if(log == null)
			{
				return;
			}

			var logType = log.type;
			string stackTraceTrimmed = log.stackTrace.Trim();
			string[] stackTraceLines;
			int count;
			if(string.IsNullOrEmpty(stackTraceTrimmed))
			{
				if(logType != LogType.Warning && logType != LogType.Error)
                {
					return;
                }

				// Handle compile error messages without stack trace like for example:
				// Assets\Sisus\Debug.Log Extensions\Source\DevCritical.cs(465,4): error CS0246: The type or namespace name 'Conditional' could not be found (are you missing a using directive or an assembly reference?)
				StackTraceData stackTraceData;
				if(StackTraceData.TryGet(log.textUnformatted, logType, stringBuilder, true, out stackTraceData))
				{
					stackTrace.Add(stackTraceData);
					stackTraceLines = new string[] { log.textUnformatted };
					count = 1;
				}
				else
                {
					stackTraceLines = new string[0];
					count = 0;
				}
			}
			else
			{
				stackTraceLines = stackTraceTrimmed.Split('\n');
				count = stackTraceLines.Length;
				if(count == 0)
				{
					return;
				}

				string stackTraceLine = stackTraceLines[0].Trim();

				if(!stackTraceLine.StartsWith("UnityEngine.Debug:", StringComparison.Ordinal))
				{
					StackTraceData stackTraceData;
					if(StackTraceData.TryGet(stackTraceLine, logType, stringBuilder, false, out stackTraceData))
					{
						stackTrace.Add(stackTraceData);
					}
				}

				if(count > 1)
				{
					bool checkToHideRow = true;

					var hideStackTraceRows = DebugLogExtensionsProjectSettings.Get().hideStackTraceRows;

					for(int n = 1; n < count; n++)
					{
						string text = stackTraceLines[n].Trim();
						if(text.Length == 0)
						{
							continue;
						}

						if(n == 1 && text.StartsWith(DebugStackTracePrefix, StringComparison.Ordinal) || text.StartsWith(DevStackTracePrefix, StringComparison.Ordinal) || text.StartsWith(CriticalStackTracePrefix, StringComparison.Ordinal))
						{
							continue;
						}

						StackTraceData stackTraceData;
						if(StackTraceData.TryGet(text, logType, stringBuilder, false, out stackTraceData))
						{
							if(checkToHideRow)
							{
								bool hideThisRow = false;
								
								foreach(var ignore in hideStackTraceRows)
								{
									bool checkNamespace = !string.IsNullOrEmpty(ignore.namespaceName);
									bool checkClassName = !string.IsNullOrEmpty(ignore.className);
									bool checkMethodName = !string.IsNullOrEmpty(ignore.methodName);

									if(!checkNamespace && !checkClassName && !checkMethodName)
                                    {
										continue;
                                    }

									if(checkNamespace)
                                    {
										int charCount = ignore.namespaceName.Length;
										if(ignore.namespaceName[charCount - 1] == '*')
										{
											if(charCount > 1 && !stackTraceData.Namespace.StartsWith(ignore.namespaceName.Substring(0, charCount - 1), StringComparison.Ordinal))
                                            {
												continue;
                                            }
										}
										else if(!string.Equals(ignore.namespaceName, stackTraceData.Namespace))
										{
											continue;
										}
									}

									if(checkClassName)
									{
										int charCount = ignore.className.Length;
										if(ignore.className[charCount - 1] == '*')
										{
											if(charCount > 1 && !stackTraceData.ClassName.StartsWith(ignore.className.Substring(0, charCount - 1), StringComparison.Ordinal))
											{
												continue;
											}
										}
										else if(!string.Equals(ignore.className, stackTraceData.ClassName))
										{
											continue;
										}
									}

									if(checkMethodName)
									{
										int charCount = ignore.methodName.Length;
										if(ignore.methodName[charCount - 1] == '*')
										{
											if(charCount > 1 && !stackTraceData.MethodName.StartsWith(ignore.methodName.Substring(0, charCount - 1), StringComparison.Ordinal))
											{
												continue;
											}
										}
										else if(!string.Equals(ignore.methodName, stackTraceData.MethodNameShort))
										{
											continue;
										}
									}
									hideThisRow = true;
									break;
								}
								if(hideThisRow)
								{
									continue;
								}
								checkToHideRow = false;
							}

							stackTrace.Add(stackTraceData);
						}
					}
				}
			}

			count = stackTrace.Count;
			for(int n = 0; n < count; n++)
			{
				var s = stackTrace[n];

				columnDrawer.AddContent((int)Column.FindReferences, " ", s.ClassName, new OnGUIEvent(DrawFindReferencesField));
				columnDrawer.AddContent((int)Column.StackTrace, s.FullStackTrace, s.ClassName);
				columnDrawer.AddContent((int)Column.Namespace, s.NamespaceFormatted, "");
				columnDrawer.AddContent((int)Column.Class, s.ClassNameFormatted, s.Namespace);

				if(s.Path.IndexOfAny(Path.GetInvalidPathChars()) == -1)
                {
					columnDrawer.AddContent((int)Column.ScriptReference, Path.GetFileName(s.Path), s.Path, new OnGUIEvent(DrawScriptReferenceField));
                }
				else // E.g.: (at <9577ac7a62ef43179789031239ba8798>:0)
				{
					#if DEV_MODE
					Debug.LogWarning("Path contained invalid characters: "+s.Path+"\nstackTrace: "+ stackTraceLines[n]);
					#endif
					columnDrawer.AddContent((int)Column.ScriptReference, s.Path, s.Path, new OnGUIEvent(DrawScriptReferenceField));
				}

				columnDrawer.AddContent((int)Column.MethodShort, s.MethodNameShortFormatted, s.MethodName);
				columnDrawer.AddContent((int)Column.Method, s.MethodNameFormatted, string.IsNullOrEmpty(s.Namespace) ? s.ClassName : s.Namespace + "." + s.ClassName);
				columnDrawer.AddContent((int)Column.ClassAndMethodShort, s.ClassAndMethodNameShortFormatted, s.MethodName);
				columnDrawer.AddContent((int)Column.ClassAndMethod, s.ClassAndMethodNameFormatted, s.Namespace);
				columnDrawer.AddContent((int)Column.Path, s.PathFormatted, s.Path);
				columnDrawer.AddContent((int)Column.PathShort, s.PathShortFormatted, s.Path);
				columnDrawer.AddContent((int)Column.LineNumber, s.LineNumberFormatted, s.Path);				
			}
		}

		private void DrawScriptReferenceField(Rect drawRect, int columnIndex, int rowIndex, int clickCount)
		{
			var stackTraceLine = stackTrace[rowIndex];
			if(stackTraceLine.ScriptReference == null)
			{
				GUI.Label(drawRect, stackTraceLine.ClassNameFormatted, stackTraceEntryStyle);
				return;
			}
			
			if(drawRect.Contains(Event.current.mousePosition) && DragAndDrop.objectReferences.Length > 0)
			{
				DragAndDrop.visualMode = DragAndDropVisualMode.Rejected;
			}

			// Hide the object picker button and the edges of the object field.
			var clip = drawRect;
			clip.y -= 2f;
			GUI.BeginClip(clip);

			drawRect.x = -1f;
			drawRect.y = -4f;
			drawRect.width += 18f;
			drawRect.height += 8f;
			EditorGUI.ObjectField(drawRect, GUIContent.none, stackTraceLine.ScriptReference, typeof(Object), false);

			GUI.EndClip();
		}

		private void DrawFindReferencesField(Rect drawRect, int visibleColumnIndex, int rowIndex, int clickCount)
		{
			drawRect.height -= 4f;
			drawRect.x += 4f;
			drawRect.width -= 4f;

			var iconSizeWas = EditorGUIUtility.GetIconSize();
			EditorGUIUtility.SetIconSize(new Vector2(16f, 16f));

			if(rowIndex < stackTrace.Count)
            {
				var classType = stackTrace[rowIndex].ClassType;
				if(classType != null && typeof(Object).IsAssignableFrom(classType) && !typeof(EditorWindow).IsAssignableFrom(classType))
				{
					GUI.Label(drawRect, searchIcon);
				}
			}
			#if DEV_MODE
			else { Debug.LogWarning("DrawFindReferencesField called with rowIndex "+rowIndex+ " < stackTrace.Count " + stackTrace.Count); }
			#endif

			EditorGUIUtility.SetIconSize(iconSizeWas);
		}

		public void OpenScript()
		{
			OpenScriptFileAtStackTraceRow(GetStackTraceMainRowIndex());
		}

		public bool TryExtractUnityObjectTypeFromStackTrace(Log log, out Type classType)
        {
			StackTraceData stackTraceData;
			var logType = log.type;
			string stackTraceTrimmed = log.stackTrace.Trim();
			if(string.IsNullOrEmpty(stackTraceTrimmed))
            {
				if(logType != LogType.Warning && logType != LogType.Error)
				{
					classType = null;
					return false;
				}
				if(StackTraceData.TryGet(log.textUnformatted, logType, stringBuilder, true, out stackTraceData) && stackTraceData.TryExtractUnityObjectTypeFromStackTrace(out classType))
				{
					return true;
				}
				classType = null;
				return false;
			}

			var stackTraceLines = stackTraceTrimmed.Split('\n');
			int count = stackTraceLines.Length;
			if(count == 0)
			{
				classType = null;
				return false;
			}

			string stackTraceLine = stackTraceLines[0].Trim();
			if(!stackTraceLine.StartsWith("UnityEngine.Debug:", StringComparison.Ordinal) && StackTraceData.TryGet(stackTraceLine, logType, stringBuilder, false, out stackTraceData) && stackTraceData.TryExtractUnityObjectTypeFromStackTrace(out classType))
			{
				return true;
			}

			if(count == 1)
			{
				classType = null;
				return false;
			}

			stackTraceLine = stackTraceLines[1].Trim();
			if(!stackTraceLine.StartsWith(DebugStackTracePrefix, StringComparison.Ordinal) && !stackTraceLine.StartsWith(DevStackTracePrefix, StringComparison.Ordinal) && !stackTraceLine.StartsWith(CriticalStackTracePrefix, StringComparison.Ordinal))
			{
				if(StackTraceData.TryGet(stackTraceLine, logType, stringBuilder, false, out stackTraceData) && stackTraceData.TryExtractUnityObjectTypeFromStackTrace(out classType))
				{
					return true;
				}
			}

			for(int n = 2; n < count; n++)
			{
				stackTraceLine = stackTraceLines[n].Trim();
				if(stackTraceLine.Length > 0 && StackTraceData.TryGet(stackTraceLine, logType, stringBuilder, false, out stackTraceData) && stackTraceData.TryExtractUnityObjectTypeFromStackTrace(out classType))
				{
					return true;
				}
			}

			classType = null;
			return false;
		}

		public bool TryExtractUnityObjectTypeFromStackTrace(out Type classType)
        {
			for(int n = 0, count = stackTrace.Count; n < count; n++)
			{
				if(stackTrace[n].TryExtractUnityObjectTypeFromStackTrace(out classType))
				{
					return true;
				}
			}
			classType = null;
			return false;
        }

		private int GetStackTraceMainRowIndex()
		{
			for(int n = 0, count = stackTrace.Count; n < count; n++)
			{
				var path = stackTrace[n].Path;
				if(!string.IsNullOrEmpty(path) && !path.EndsWith(".dll", StringComparison.OrdinalIgnoreCase))
				{
					return n;
				}
			}
			return -1;
		}

		private bool OpenScriptFileAtStackTraceRow(int rowIndex)
		{
			if(rowIndex < 0 || rowIndex >= stackTrace.Count)
			{
				var mainAsset = GetScriptAtStackTraceRow(-1);
				if(mainAsset = null)
                {
					return false;
                }
				AssetDatabase.OpenAsset(mainAsset);
				return true;
			}

			var assetAtRow = GetScriptAtStackTraceRow(rowIndex);
			if(assetAtRow == null)
            {
				return false;
            }

			AssetDatabase.OpenAsset(assetAtRow, stackTrace[rowIndex].LineNumber);
			return true;
		}

		[CanBeNull]
		private Object GetScriptAtStackTraceRow(int rowIndex)
		{
			if(rowIndex == -1)
            {
				rowIndex = GetStackTraceMainRowIndex();
			}
			if(rowIndex >= 0 && rowIndex < stackTrace.Count)
			{
				string path = stackTrace[rowIndex].Path;
				if(!string.IsNullOrEmpty(path) && !path.EndsWith(".dll", StringComparison.OrdinalIgnoreCase))
				{
					return AssetDatabase.LoadAssetAtPath<Object>(path);
				}
			}
			
			return null;
		}

		public enum Column
		{
			None,

			StackTrace,
			Namespace,
			Class,
			ScriptReference,
			FindReferences,
			Method,
			MethodShort,
			ClassAndMethodShort,
			ClassAndMethod,
			Path,
			PathShort,			
			LineNumber
		}

		[Serializable]
		private class StackTraceData
		{
			private static readonly Dictionary<string, Dictionary<string, string>> AssemblyPathsByClassNameThenNamespace = new Dictionary<string, Dictionary<string, string>>();

			public string FullStackTrace = "";

			public string Namespace = "";
			public string ClassName = "";
			public string MethodName = "";
			public string MethodNameShort = "";
			public int LineNumber = -1;

			public string ClassNameFormatted = "";
			public string MethodNameFormatted = "";
			public string MethodNameShortFormatted = "";
			public string ClassAndMethodNameFormatted = "";
			public string ClassAndMethodNameShortFormatted = "";
			public string LineNumberFormatted = "";

			[SerializeField]
			private string path = "";

			[SerializeField]
			private string pathFormatted = "";
			[SerializeField]
			private string pathShortFormatted = "";

			[NonSerialized]
			private Object scriptReference = null;
			[NonSerialized]
			private bool scriptReferenceLoaded = false;
			[NonSerialized]
			private Type classType;
			[NonSerialized]
			private bool classTypeDetermined;

			public string NamespaceFormatted
			{
				get
				{
					return Namespace;
				}
			}

			public string PathFormatted
			{
				get
				{
					if(pathFormatted.Length == 0 && Path.Length > 0)
					{
						pathFormatted = "<i>" + path + "</i>";
					}
					return pathFormatted;
				}
			}

			public string PathShortFormatted
			{
				get
				{
					if(pathShortFormatted.Length == 0 && Path.Length > 0)
					{
						pathShortFormatted = "<i>" + System.IO.Path.GetFileName(Path) + "</i>";
					}
					return pathShortFormatted;
				}
			}

			public string Path
			{
				get
				{
					if(path.Length == 0)
					{
						Dictionary<string, string > assemblyPathsByNamespace;
						if(!AssemblyPathsByClassNameThenNamespace.TryGetValue(ClassName, out assemblyPathsByNamespace) || !assemblyPathsByNamespace.TryGetValue(Namespace, out path))
						{
							var type = AppDomain.CurrentDomain.GetAssemblies()
								.SelectMany(a => a.GetTypes())
								.Where(t => string.Equals(t.Name, ClassName) && string.Equals(t.Namespace, Namespace))
								.FirstOrDefault();
							path = type == null ? "" : type.Assembly.Location.Replace('\\', '/');

							if(assemblyPathsByNamespace == null)
							{
								assemblyPathsByNamespace = new Dictionary<string, string>();
								AssemblyPathsByClassNameThenNamespace.Add(ClassName, assemblyPathsByNamespace);
							}
							assemblyPathsByNamespace.Add(Namespace, path);
						}
					}

					return path;
				}
			}

			public Object ScriptReference
			{
				get
				{
					if(!scriptReferenceLoaded)
					{
						scriptReferenceLoaded = true;

						string relativePath = Path;
						if(relativePath.Length == 0)
						{
							return null;
						}

						if(!relativePath.StartsWith("Assets", StringComparison.OrdinalIgnoreCase) && !relativePath.StartsWith("Packages", StringComparison.OrdinalIgnoreCase))
						{
							if(relativePath.Length <= Application.dataPath.Length || !relativePath.StartsWith(Application.dataPath))
							{
								return null;
							}
							relativePath = relativePath.Substring(Application.dataPath.Length + 1);
						}

						scriptReference = AssetDatabase.LoadAssetAtPath<Object>(relativePath);
					}
					return scriptReference;
				}
			}

			public Type ClassType
			{
				get
				{
					if(classTypeDetermined)
                    {
						return classType;
                    }
					classTypeDetermined = true;

					var unityObjectType = typeof(Object);

					if(ScriptReference != null)
					{
						var monoScript = scriptReference as MonoScript;
						if(monoScript != null)
						{
							classType = monoScript.GetClass();
							if(classType != null)
							{
								return classType;
							}
						}
					}

					if(ClassName.Length == 0)
					{
						return null;
					}

					string fullClassName;
					if(Namespace.Length > 0)
					{
						fullClassName = Namespace + "." + ClassName;
					}
					else
					{
						fullClassName = ClassName;
					}

					var unityObjectAssemblyName = unityObjectType.Assembly.GetName().Name;
					foreach(var assembly in AppDomain.CurrentDomain.GetAssemblies().Where((a)=>!a.IsDynamic && a.GetReferencedAssemblies().Any(n=>string.Equals(n.Name, unityObjectAssemblyName))))
					{
						foreach(var type in assembly.GetTypes())
						{
							if(unityObjectType.IsAssignableFrom(type) && string.Equals(type.FullName, fullClassName))
							{
								classType = type;
								return classType;
							}
						}
					}

					return null;
				}
			}

			public static bool TryGet([NotNull] string stackTrace, LogType logType, [NotNull] StringBuilder stringBuilder, bool isSingleErrorOrWarning, out StackTraceData stackTraceData)
			{
				stackTraceData = new StackTraceData(stackTrace, logType, stringBuilder, isSingleErrorOrWarning);
				return stackTraceData.ClassAndMethodNameFormatted.Length > 0 || stackTraceData.path.Length > 0;
			}

			private StackTraceData([NotNull]string stackTrace, LogType logType, [NotNull]StringBuilder stringBuilder, bool isSingleErrorOrWarning)
			{
				if(stackTrace.Length == 0)
				{
					return;
				}

				FullStackTrace = stackTrace;

				if(isSingleErrorOrWarning)
                {
					// Handle compile error messages without stack trace
					// Examples:
					// Assets\Sisus\Debug.Log Extensions\Source\DevCritical.cs(465,4): error CS0246: The type or namespace name 'Conditional' could not be found (are you missing a using directive or an assembly reference?)
					// Assets\Sisus\Debug.Log Extensions\Scripts\Editor\StackTraceDrawer.cs(1020,33): warning CS0414: The field 'StackTraceDrawer.StackTraceData.test' is assigned but its value is never used
					// Assets\Sisus\Debug.Log Extensions\Scripts\Editor\StackTraceDrawer.cs(1473,11): error CS1002: ; expected

					string errorOrWarning = stackTrace;

					int lineNumberAndCharIndexEnd = errorOrWarning.IndexOf("): ");
					if(lineNumberAndCharIndexEnd == -1)
					{
						#if DEV_MODE
						UnityEngine.Debug.LogWarning("Failed to parse StackTraceData from message of type "+logType+":\n" + errorOrWarning);
						#endif
						return;
					}
					int lineNumberEnd = errorOrWarning.LastIndexOf(',', lineNumberAndCharIndexEnd);
					if(lineNumberEnd == -1)
					{
						#if DEV_MODE
						UnityEngine.Debug.LogWarning("Failed to parse StackTraceData from errorMessage:\n" + errorOrWarning);
						#endif
						return;
					}

					const int pathStart = 0;
					int pathEnd = errorOrWarning.LastIndexOf('(', lineNumberEnd);
					if(pathEnd == -1)
					{
						#if DEV_MODE
						UnityEngine.Debug.LogWarning("Failed to parse StackTraceData from errorMessage:\n" + errorOrWarning);
						#endif
						return;
					}
					path = errorOrWarning.Substring(pathStart, pathEnd);

					stringBuilder.Append("<i>");
					stringBuilder.Append(path);
					stringBuilder.Append("</i>");
					pathFormatted = stringBuilder.ToString();
					stringBuilder.Length = 0;

					int lineNumberStart = pathEnd + 1;
					int lineNumber;
					if(int.TryParse(errorOrWarning.Substring(lineNumberStart, lineNumberEnd - lineNumberStart), out lineNumber))
					{
						LineNumber = lineNumber;
						
						stringBuilder.Append(Debug.formatter.BeginNumeric.Length > 0 ? Debug.formatter.BeginNumeric : "<color=blue>");
						stringBuilder.Append("<b>");
						stringBuilder.Append(lineNumber);
						stringBuilder.Append("</b></color>");
						LineNumberFormatted = stringBuilder.ToString();
						stringBuilder.Length = 0;
					}

					int classNameStart = errorOrWarning.LastIndexOf('\\', pathEnd) + 1;
					if(classNameStart <= 0)
					{
						MethodName = "???";
						MethodNameShort = "???";
						MethodNameFormatted = "<b>???</b>";
						MethodNameShortFormatted = "<b>???</b>";

						#if DEV_MODE
						UnityEngine.Debug.LogWarning("Failed to parse class name from errorMessage:\n" + errorOrWarning);
						#endif
						return;
					}

					int classNameEnd = errorOrWarning.IndexOf('.', classNameStart);
					ClassName = errorOrWarning.Substring(classNameStart, classNameEnd - classNameStart).Replace('/', '.');

					stringBuilder.Append("<b>");
					stringBuilder.Append(ClassName);
					stringBuilder.Append("</b>");
					ClassNameFormatted = stringBuilder.ToString();
					stringBuilder.Length = 0;

					var type = ClassType;
					if(type != null)
                    {
						Namespace = type.Namespace;
                    }

					int errorCodeStart = errorOrWarning.IndexOf("): ", classNameEnd + 3) + 3;
					if(errorCodeStart <= 2)
					{
						MethodName = "???";
					}
					else
                    {
						int errorCodeEnd = errorOrWarning.IndexOf(':', errorCodeStart + 1);
						if(errorCodeEnd == -1)
						{
							MethodName = "???";
						}
						else
                        {
							MethodName = errorOrWarning.Substring(errorCodeStart, errorCodeEnd - errorCodeStart);
						}
					}

					stringBuilder.Append("<b>");
					stringBuilder.Append(MethodName);
					stringBuilder.Append("</b>");
					MethodNameFormatted = stringBuilder.ToString();
					stringBuilder.Length = 0;

					MethodNameShort = MethodName;
					MethodNameShortFormatted = MethodNameFormatted;

					stringBuilder.Append(ClassName);
					stringBuilder.Append(": ");
					stringBuilder.Append(MethodNameFormatted);
					ClassAndMethodNameFormatted = stringBuilder.ToString();
					stringBuilder.Length = 0;

					ClassAndMethodNameShortFormatted = ClassAndMethodNameFormatted;
					return;
                }

				bool isNative = stackTrace.StartsWith("0x");
				if(isNative)
				{
					// Examples:
					// 0x00007ff6f9ef4f3c (Unity) StackWalker::GetCurrentCallstack
					// 0x00007ff6f967b126 (Unity) `InitPlayerLoopCallbacks'::`2'::EarlyUpdateScriptRunDelayedStartupFrameRegistrator::Forward
					// 0x000002991eb16a35 (Mono JIT Code) UnityEngine.Debug:LogError (object) (at ?)
					// 0x00007ff6f967b126 (Unity) `InitPlayerLoopCallbacks'::`2'::EarlyUpdateScriptRunDelayedStartupFrameRegistrator::Forward
					// 0x000002991eb338e0 (Mono JIT Code) (wrapper runtime-invoke) object:runtime_invoke_void__this__ (object,intptr,intptr,intptr) (at ?)
					// 0x000002991ec8b48b (Mono JIT Code) UnityEngine.DebugLogHandler:LogFormat (UnityEngine.LogType,UnityEngine.Object,string,object[]) (at ?)

					int sourceStart = stackTrace.IndexOf('(', 2) + 1;
					if(sourceStart == 0)
                    {
						#if DEV_MODE
						UnityEngine.Debug.LogWarning("Failed to parse StackTraceData from input:\n"+stackTrace);
						#endif
						return;
                    }
					int sourceEnd = stackTrace.IndexOf(") ", sourceStart, StringComparison.Ordinal);
					if(sourceEnd == -1)
                    {
						#if DEV_MODE
						UnityEngine.Debug.LogWarning("Failed to parse StackTraceData from input:\n"+stackTrace);
						#endif
						return;
                    }

					path = stackTrace.Substring(sourceStart, sourceEnd - sourceStart);

					int namespaceClassAndMethodStart = sourceEnd + 2;

					if(namespaceClassAndMethodStart >= stackTrace.Length)
                    {
						#if DEV_MODE
						UnityEngine.Debug.LogWarning("Failed to parse StackTraceData from input:\n"+stackTrace);
						#endif
						return;
                    }

					int fileNameStart = stackTrace.IndexOf('[', sourceEnd + 1) + 1;
					int fileNameEnd = fileNameStart == 0 ? -1 : stackTrace.IndexOf(':', fileNameStart + 1);
					// Examples:
					// 0x000002991eb1664b (Mono JIT Code) [Critical.cs:441] Sisus.Debugging.Critical:LogException (System.Exception)  (at ?)
					// 0x00007ff877532902 (mono-2.0-bdwgc) [object.c:2921] do_runtime_invoke 
					// 0x000002991ec8b48b (Mono JIT Code) UnityEngine.DebugLogHandler:LogFormat (UnityEngine.LogType,UnityEngine.Object,string,object[]) (at ?)
					if(fileNameEnd != -1)
					{
						int lineNumberEnd = stackTrace.IndexOf(']', fileNameStart + 1);
						if(lineNumberEnd == -1 || fileNameEnd > lineNumberEnd)
                        {
							#if DEV_MODE
							UnityEngine.Debug.LogWarning("Failed to parse StackTraceData from input:\n"+stackTrace);
							#endif
							return;
                        }

						namespaceClassAndMethodStart = lineNumberEnd + 2;

						stringBuilder.Append(stackTrace.Substring(fileNameStart, fileNameEnd - fileNameStart));
						stringBuilder.Append(" (");
						stringBuilder.Append(path);
						stringBuilder.Append(")");
						path = stringBuilder.ToString();
						stringBuilder.Length = 0;

						int lineNumberStart = fileNameEnd + 1;

						int lineNumber;
						if(int.TryParse(stackTrace.Substring(lineNumberStart, lineNumberEnd - lineNumberStart), out lineNumber))
						{
							LineNumber = lineNumber;

							stringBuilder.Append(Debug.formatter.BeginNumeric.Length > 0 ? Debug.formatter.BeginNumeric : "<color=blue>");
							stringBuilder.Append("<b>");
							stringBuilder.Append(lineNumber);
							stringBuilder.Append("</b></color>");
							LineNumberFormatted = stringBuilder.ToString();
							stringBuilder.Length = 0;
						}
					}

					stringBuilder.Append("<i>");
					stringBuilder.Append(path);
					stringBuilder.Append("</i>");
					pathFormatted = stringBuilder.ToString();
					stringBuilder.Length = 0;

					int methodNameStart = stackTrace.LastIndexOf(':', stackTrace.Length - 1, stackTrace.Length - namespaceClassAndMethodStart) + 1;
					// Example: 0x00007ff6f9663fe9 (Unity) PlayerLoop
					if(methodNameStart == 0)
                    {
						methodNameStart = namespaceClassAndMethodStart;
						MethodName = stackTrace.Substring(methodNameStart);
					}
					// Examples:
					// 0x00007ff6f9d69f55 (Unity) ScriptingInvocation::Invoke
					// 0x000002991eb16a35(Mono JIT Code) UnityEngine.Debug:LogError(object)(at ?)
					// 0x00007ff6f967b126 (Unity) `InitPlayerLoopCallbacks'::`2'::EarlyUpdateScriptRunDelayedStartupFrameRegistrator::Forward
					else
					{
						int argumentsStart = stackTrace.IndexOf('(', methodNameStart);
						// Example: 0x000002991eb16a35(Mono JIT Code) UnityEngine.Debug:LogError(object)(at ?)
						if(argumentsStart != -1)
						{
							int argumentsEnd = stackTrace.IndexOf(')', argumentsStart) + 1;
							if(argumentsEnd == -1)
							{
								#if DEV_MODE
								UnityEngine.Debug.LogWarning("Failed to parse StackTraceData from input:\n"+stackTrace);
								#endif
								return;
							}

							MethodName = stackTrace.Substring(methodNameStart, argumentsEnd - methodNameStart);
						}
						// Example: 0x00007ff6f9d69f55 (Unity) ScriptingInvocation::Invoke
						else
						{
							MethodName = stackTrace.Substring(methodNameStart);
						}

						int classNameStart = stackTrace.LastIndexOf('.', methodNameStart - 2) + 1;
						int classNameEnd;
						bool hasNamespace = classNameStart != 0 && classNameStart > sourceEnd;
						// Example: 0x000002991eb16a35(Mono JIT Code) UnityEngine.Debug:LogError(object)(at ?)
						if(hasNamespace)
                        {
							classNameEnd = methodNameStart - 1;

							int namespaceStart = sourceEnd + 2;
							int namespaceEnd = classNameStart - 1;

							Namespace = stackTrace.Substring(0, namespaceEnd);
						}
						// Example: 0x00007ff6f9d69f55 (Unity) ScriptingInvocation::Invoke
						else
						{
							classNameStart = sourceEnd + 2;
							classNameEnd = stackTrace.IndexOf(':', classNameStart + 1);
							if(classNameEnd == -1)
							{ 
								#if DEV_MODE
								UnityEngine.Debug.LogWarning("Failed to parse StackTraceData from input:\n"+stackTrace);
								#endif
								return;
							}
						}

						ClassName = stackTrace.Substring(classNameStart, classNameEnd - classNameStart).Replace('/', '.');

						stringBuilder.Append("<b>");
						stringBuilder.Append(ClassName);
						stringBuilder.Append("</b>");
						ClassNameFormatted = stringBuilder.ToString();
						stringBuilder.Length = 0;
					}
					
					stringBuilder.Append("<b>");
					stringBuilder.Append(MethodName);
					stringBuilder.Append("</b>");

					MethodNameFormatted = stringBuilder.ToString();
					stringBuilder.Length = 0;

					int methodNameShortEnd = MethodName.IndexOfAny(methodArgsStartChars);
					if(methodNameShortEnd == -1)
					{
						MethodNameShort = MethodName;
						MethodNameShortFormatted = MethodNameFormatted;
					}
					else
                    {
						MethodNameShort = MethodName.Substring(0, methodNameShortEnd);
						stringBuilder.Append("<b>");
						stringBuilder.Append(MethodNameShort);
						stringBuilder.Append("</b>");
						MethodNameShortFormatted = stringBuilder.ToString();
						stringBuilder.Length = 0;
					}

					if(ClassName.Length == 0)
					{
						ClassNameFormatted = "";
						ClassAndMethodNameFormatted = MethodNameFormatted;
						ClassAndMethodNameShortFormatted = MethodNameShortFormatted;
					}
					else
					{
						stringBuilder.Append(ClassName);
						stringBuilder.Append('.');
						stringBuilder.Append(MethodNameFormatted);
						ClassAndMethodNameFormatted = stringBuilder.ToString();
						stringBuilder.Length = 0;

						stringBuilder.Append(ClassName);
						stringBuilder.Append('.');
						stringBuilder.Append(MethodNameShortFormatted);
						ClassAndMethodNameShortFormatted = stringBuilder.ToString();
						stringBuilder.Length = 0;
					}
				}
				else
				{
					int parametersStart = stackTrace.IndexOf('(');

					// Probably a custom exception message.
					// Example: Rethrow as ArgumentException: Outer Exception
					if(parametersStart < 0)
					{
						#if DEV_MODE
						UnityEngine.Debug.Log(stackTrace);
						#endif
						return;
					}

					int methodEnd = stackTrace.LastIndexOf(") (", StringComparison.Ordinal) + 1;

					// Classes within DLLs have no asset path.
					// Also for example: UnityEngine.Events.UnityEvent`4<UnityEngine.Rect, int, int, int>:Invoke (UnityEngine.Rect,int,int,int)
					bool hasAssetPath = true;
					if(methodEnd == 0)
					{
						hasAssetPath = false;
						methodEnd = stackTrace.Length;
					}

					int methodNameStart;
					if(hasAssetPath)
					{
						// Example: Sisus.Example:Start () (at Assets/Example.cs:325)
						methodNameStart = stackTrace.LastIndexOf(':', parametersStart) + 1;

						if(methodNameStart == 0 || methodNameStart > parametersStart)
                        {
							// Handle exception style formatting.
							// Example: Sisus.Example.Start () (at Assets/Temp/Example.cs:358)
							methodNameStart = stackTrace.LastIndexOf('.', parametersStart) + 1;
						}
					}
					else
					{
						// Example: UnityEngine.Debug:LogException(Exception, Object)
						methodNameStart = stackTrace.LastIndexOfAny(new[] { ':', '.' }, parametersStart) + 1;

						if(methodEnd == stackTrace.Length)
						{
							methodEnd = stackTrace.LastIndexOf('(');
							if(methodEnd == -1)
                            {
								methodEnd = stackTrace.Length;
							}
						}
					}

					if(methodNameStart == 0)
					{
						ClassName = "";
						Namespace = "";
					}
					else
					{
						int classGenericArgumentsEnd = methodNameStart - 2;
						bool isGenericClass = classGenericArgumentsEnd >= 0 && stackTrace[classGenericArgumentsEnd] == '>';
						int classGenericArgumentsStart = -1;
						int namespaceEnd;
						if(isGenericClass)
                        {
							// Example: System.Runtime.CompilerServices.AsyncTaskMethodBuilder`1<MyClass`<System.Collections.Generic.List`1<int>>>:SetResult (System.Collections.Generic.List`1<int>) (at Assets/Scripts/Example1.cs:475)
							// Example: MyNamespace.Internal.MyClass/<Query>d__35`1<System.Collections.Generic.List`1<int>>:MoveNext () (at Assets/Scripts/Example2.cs:237)

							classGenericArgumentsStart = stackTrace.IndexOf('<', 0, classGenericArgumentsEnd);
							if(classGenericArgumentsStart != -1)
							{
								namespaceEnd = stackTrace.LastIndexOf('.', classGenericArgumentsStart - 1);
							}
							else
                            {
								namespaceEnd = -1;
							}
						}
						else
                        {
							namespaceEnd = stackTrace.LastIndexOf('.', methodNameStart - 2);
						}

						if(namespaceEnd != -1)
						{
							Namespace = stackTrace.Substring(0, namespaceEnd);
						}

						int classStart = namespaceEnd + 1;
						int classEnd = methodNameStart - 1;

						// Use '.' instead of '/' as separator for nested types.
						ClassName = stackTrace.Substring(classStart, classEnd - classStart).Replace('/', '.');

						if(isGenericClass)
                        {
							// Leave out "`1" etc. from generic class names
							ClassName = ClassName.Replace("`1<", "<").Replace("`2<", "<");
						}
					}
					MethodName = stackTrace.Substring(methodNameStart, methodEnd - methodNameStart);

					bool bolded = hasAssetPath;

					if(MethodName.Length == 0)
					{
						MethodNameFormatted = "";
						MethodNameShort = "";
						MethodNameShortFormatted = "";
						ClassNameFormatted = "";
						ClassAndMethodNameFormatted = "";
						ClassAndMethodNameShortFormatted = "";						
					}
					else
					{
						int methodNameShortEnd = MethodName.IndexOfAny(methodArgsStartChars);

						if(bolded)
						{
							stringBuilder.Append("<b>");
							stringBuilder.Append(MethodName);
							stringBuilder.Append("</b>");
							MethodNameFormatted = stringBuilder.ToString();
							stringBuilder.Length = 0;

							if(methodNameShortEnd == -1)
							{
								MethodNameShort = MethodName;
								MethodNameShortFormatted = MethodNameFormatted;
							}
							else
							{
								MethodNameShort = MethodName.Substring(0, methodNameShortEnd);
								stringBuilder.Append("<b>");
								stringBuilder.Append(MethodNameShort);
								stringBuilder.Append("</b>");
								MethodNameShortFormatted = stringBuilder.ToString();
								stringBuilder.Length = 0;
							}
						}
						else
						{
							MethodNameFormatted = MethodName;

							if(methodNameShortEnd == -1)
							{
								MethodNameShort = MethodName;
								MethodNameShortFormatted = MethodName;
							}
							else
							{
								MethodNameShort = MethodName.Substring(0, methodNameShortEnd);
								MethodNameShortFormatted = MethodNameShort;
							}
						}

						if(ClassName.Length == 0)
						{
							ClassNameFormatted = "";
							ClassAndMethodNameFormatted = MethodNameFormatted;
							ClassAndMethodNameShortFormatted = MethodNameShortFormatted;
						}
						else if(bolded)
						{
							stringBuilder.Append("<b>");
							stringBuilder.Append(ClassName);
							stringBuilder.Append("</b>");
							ClassNameFormatted = stringBuilder.ToString();
							stringBuilder.Length = 0;

							stringBuilder.Append(ClassName);
							stringBuilder.Append(".");
							stringBuilder.Append(MethodNameFormatted);
							ClassAndMethodNameFormatted = stringBuilder.ToString();
							stringBuilder.Length = 0;

							stringBuilder.Append(ClassName);
							stringBuilder.Append(".");
							stringBuilder.Append(MethodNameShortFormatted);
							ClassAndMethodNameShortFormatted = stringBuilder.ToString();
							stringBuilder.Length = 0;
						}
						else
						{
							ClassNameFormatted = ClassName;

							stringBuilder.Append(ClassName);
							stringBuilder.Append(".");
							stringBuilder.Append(MethodName);
							ClassAndMethodNameFormatted = stringBuilder.ToString();
							stringBuilder.Length = 0;

							stringBuilder.Append(ClassName);
							stringBuilder.Append(".");
							stringBuilder.Append(MethodNameShortFormatted);
							ClassAndMethodNameShortFormatted = stringBuilder.ToString();
							stringBuilder.Length = 0;
						}
					}

					if(!hasAssetPath)
					{
						return;
					}

					int pathStart = methodEnd + 5;

					if(pathStart >= stackTrace.Length)
					{
						#if DEV_MODE
						Debug.Log(stackTrace);
						#endif
						return;
					}

					int pathEnd = stackTrace.LastIndexOf(':');
					if(pathEnd == -1 || pathEnd < pathStart)
					{
						#if DEV_MODE
						//Debug.Log($"pathStart={pathStart}, pathEnd={pathEnd}, stackTrace.Length={stackTrace.Length}\nstackTrace: {stackTrace}\nlogType: {logType}");
						#endif
						return;
					}

					path = stackTrace.Substring(pathStart, pathEnd - pathStart);

					stringBuilder.Append("<i>");
					stringBuilder.Append(path);
					stringBuilder.Append("</i>");
					pathFormatted = stringBuilder.ToString();
					stringBuilder.Length = 0;

					pathShortFormatted = ShortenAndFormatPath(path, stringBuilder);

					int lineStart = pathEnd + 1;
					int lineEnd = stackTrace.Length - 1;
					int lineNumber;
					if(int.TryParse(stackTrace.Substring(lineStart, lineEnd - lineStart), out lineNumber))
					{
						LineNumber = lineNumber;

						stringBuilder.Append(Debug.formatter.BeginNumeric.Length > 0 ? Debug.formatter.BeginNumeric : "<color=blue>");
						stringBuilder.Append("<b>");
						stringBuilder.Append(lineNumber);
						stringBuilder.Append("</b></color>");
						LineNumberFormatted = stringBuilder.ToString();
						stringBuilder.Length = 0;
					}
				}
			}

			private static string ShortenAndFormatPath(string path, StringBuilder stringBuilder)
			{
				if(path.Length == 0)
				{
					return "";
				}

				int dir1 = path.IndexOf('/');
				if(dir1 != -1)
				{
					int dir2 = path.IndexOf('/', dir1 + 1);
					if(dir2 != -1)
					{
						int lastDir = path.LastIndexOf('/');
						if(lastDir != -1 && lastDir > dir2)
						{
							stringBuilder.Append("<i>");
							stringBuilder.Append(path.Substring(0, dir2));
							stringBuilder.Append("...");
							stringBuilder.Append(path.Substring(lastDir));
							stringBuilder.Append("</i>");
							string result = stringBuilder.ToString();
							stringBuilder.Length = 0;
							return result;
						}
					}
				}

				stringBuilder.Append("<i>");
				stringBuilder.Append(path);
				stringBuilder.Append("</i>");
				string result2 = stringBuilder.ToString();
				stringBuilder.Length = 0;
				return result2;
			}

			public bool TryExtractUnityObjectTypeFromStackTrace(out Type classType)
			{
				var unityObjectType = typeof(Object);

				if(ScriptReference != null)
                {
					var monoScript = scriptReference as MonoScript;
					if(monoScript != null)
					{
						classType = monoScript.GetClass();
						if(classType != null && unityObjectType.IsAssignableFrom(classType))
                        {
							return true;
                        }
					}
				}

				if(ClassName.Length == 0)
                {
					#if DEV_MODE
					Debug.Log("className.Length == 0");
					#endif
					classType = null;
					return false;
                }

				string fullClassName;
				if(Namespace.Length > 0)
				{
					fullClassName = Namespace + "." + ClassName;
				}
				else
                {
					fullClassName = ClassName;
				}

				var unityObjectAssemblyName = unityObjectType.Assembly.GetName().Name;
				foreach(var assembly in AppDomain.CurrentDomain.GetAssemblies().Where((a)=>!a.IsDynamic && a.GetReferencedAssemblies().Any(n=>string.Equals(n.Name, unityObjectAssemblyName))))
				{
					foreach(var type in assembly.GetTypes())
					{
						if(unityObjectType.IsAssignableFrom(type) && string.Equals(type.FullName, fullClassName))
						{
							classType = type;
							return true;
						}
					}
				}

				classType = null;
				return false;
			}
		}

		[Serializable]
		private class CopyPreferences
		{
			[SerializeField]
			public bool includeMessage = true;
			[SerializeField]
			private bool includeNamespace = true;
			[SerializeField]
			private bool includeClassName = true;
			[SerializeField]
			public bool includeMethodName = true;
			[SerializeField]
			public bool includePath = true;
			[SerializeField]
			public bool includeLineNumber = true;

			public bool addSeparators = true;

			public bool IncludeNamespace
			{
				get
				{
					return includeNamespace;
				}

				set
				{
					includeNamespace = value;
				}
			}

			public bool IncludeClassName
			{
				get
				{
					return includeClassName;
				}

				set
				{
					includeClassName = value;
				}
			}

			public bool IncludeFullStackTrace
			{
				get
				{
					return includeNamespace && includeClassName && includeMethodName && includePath && includeLineNumber;
				}

				set
				{
					includeNamespace = value;
					includeClassName = value;
					includeMethodName = value;
					includePath = value;
					includeLineNumber = value;
				}
			}
		}
	}
}