using System;
using UnityEngine;
using UnityEditor;
using JetBrains.Annotations;

namespace Pancake.Debugging.Console
{
	internal sealed class ConsoleWindowPlus : ConsoleWindowPlusBase, IHasCustomMenu, ISerializationCallbackReceiver
	{
		[SerializeField]
		private EditorWindow internalConsoleWindowInstance;

		private Type internalConsoleWindowType;
		private System.Reflection.MethodInfo internalConsoleWindowOnGUI;

		private float logCountsWidth = 165f;
		
		#if UNITY_2020_1_OR_NEWER
		private const float channelButtonXPos = 240f;
		private const float channelButtonYPos = 0f;
		private const float channelButtonHeight = 20f;
		private const float searchBarWidth = 241f;
		#elif UNITY_2019_3_OR_NEWER
		private const float channelButtonXPos = 398f;
		private const float channelButtonYPos = 0f;
		private const float channelButtonHeight = 20f;
		private const float searchBarWidth = 248f;
		#else
		private const float channelButtonXPos = 296f;
		private const float channelButtonYPos = -1f;
		private const float searchBarWidth = -60f;
		private const float channelButtonHeight = 17f;
		#endif

		private System.Reflection.MethodInfo getLogCountsByType;
		private object[] getLogCountsByTypeParams = new object[] { 1000, 1000, 1000 };
		private int errorCount;
		private int warningCount;
		private int logCount;

		public static void Open()
		{
			GetWindow<ConsoleWindowPlus>();
		}

		[UsedImplicitly]
		protected override void OnEnable()
		{
			internalConsoleWindowType = typeof(EditorWindow).Assembly.GetType("UnityEditor.ConsoleWindow");
			
			// Close any existing Console windows to avoid errors in older Unity versions where only one Console window is allowed to be open at a given time.
			var openConsoleWindows = Resources.FindObjectsOfTypeAll(internalConsoleWindowType);

			for(int n = openConsoleWindows.Length - 1; n >= 0; n--)
			{
				var openConsoleWindow = openConsoleWindows[n] as EditorWindow;
				if(openConsoleWindow != null && openConsoleWindow != internalConsoleWindowInstance)
				{
					try
					{
						openConsoleWindow.Close();
					}
					catch(NullReferenceException)
					{
						#if DEV_MODE
						Debug.LogWarning("ConsoleWindow.Close NullReferenceException.");
						#endif
					}
				}
			}

			if(internalConsoleWindowInstance != null)
			{
				DestroyImmediate(internalConsoleWindowInstance, true);
			}

			internalConsoleWindowInstance = CreateInstance(internalConsoleWindowType) as EditorWindow;

			internalConsoleWindowOnGUI = internalConsoleWindowType.GetMethod("OnGUI", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public);

			var logEntriesType = typeof(EditorWindow).Assembly.GetType("UnityEditor.LogEntries");
			if(logEntriesType == null)
			{
				logEntriesType = typeof(EditorWindow).Assembly.GetType("UnityEditorInternal.LogEntries");
			}
			if(logEntriesType != null)
			{
				getLogCountsByType = logEntriesType.GetMethod("GetCountsByType", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public);
			}

			base.OnEnable();
		}

		protected override void OnLogMessageReceived(string condition, string stackTrace, LogType type)
		{
			Repaint();
			internalConsoleWindowInstance.Repaint();
		}

		[UsedImplicitly]
		protected override void OnDisable()
		{
			base.OnDisable();
			CloseInternalConsoleWindow();
		}

		private void CloseInternalConsoleWindow()
		{
			if(internalConsoleWindowInstance == null)
			{
				return;
			}

			DestroyImmediate(internalConsoleWindowInstance, false);
			internalConsoleWindowInstance = null;
			EditorUtility.SetDirty(this);
		}

		private bool HasSpaceForChannelsButton()
		{
			if(getLogCountsByType != null)
			{
				getLogCountsByType.Invoke(null, getLogCountsByTypeParams);
			}

			int newErrorCount = Mathf.Min(1000, (int)getLogCountsByTypeParams[0]);
			int newWarningCount = Mathf.Min(1000, (int)getLogCountsByTypeParams[0]);
			int newLogCount = Mathf.Min(1000, (int)getLogCountsByTypeParams[0]);
			if(newErrorCount != errorCount || newWarningCount != warningCount || newLogCount != logCount)
			{
				errorCount = newErrorCount;
				warningCount = newWarningCount;
				logCount = newLogCount;

				var errorlabel = new GUIContent(errorCount >= 999 ? "  999+" : "  " + errorCount);
				var warninglabel = new GUIContent(warningCount >= 999 ? "  999+" : "  " + warningCount);
				var logLabel = new GUIContent(logCount >= 999 ? "  999+" : "  " + logCount);

				logCountsWidth = EditorStyles.toolbarPopup.CalcSize(logLabel).x + EditorStyles.toolbarPopup.CalcSize(warninglabel).x + EditorStyles.toolbarPopup.CalcSize(errorlabel).x;
			}

			return position.width > channelButtonXPos + shortChannelsButtonWidth + logCountsWidth + searchBarWidth;
		}

		private bool HasSpaceForFullChannelsButton()
		{
			return position.width > channelButtonXPos + fullChannelsButtonWidth + logCountsWidth + searchBarWidth;
		}

		[UsedImplicitly]
		protected override void OnGUI()
		{
			base.OnGUI();

			// Auto-close Console+ window if user has opened built-in Console window.
			if(focusedWindow != null && focusedWindow.GetType() == internalConsoleWindowType)
			{
				#if DEV_MODE
				Debug.Log("Auto-closing Console+ window because user opened built-in Console window...");
				#endif

				// The Console menu item causes the internalConsoleWindowInstance to open.
				// To avoid the newly opened window being destroyed in the OnDisable method
				// set the reference null.
				if(focusedWindow == internalConsoleWindowInstance)
				{
					internalConsoleWindowInstance = null;
				}

				Close();

				return;
			}

			if(internalConsoleWindowOnGUI == null || internalConsoleWindowInstance == null)
			{
				return;
			}

			// sync position to enable dynamic toolbar element hiding
			internalConsoleWindowInstance.position = position;

			internalConsoleWindowOnGUI.Invoke(internalConsoleWindowInstance, null);

			if(HasSpaceForChannelsButton())
			{
				bool useFullLabel = HasSpaceForFullChannelsButton();
				var buttonLabel = useFullLabel ? fullChannelsButtonLabel : shortChannelsButtonLabel;
				var buttonWidth = useFullLabel ? fullChannelsButtonWidth : shortChannelsButtonWidth;

				var buttonRect = new Rect(channelButtonXPos, channelButtonYPos, buttonWidth, channelButtonHeight);
				if(GUI.Button(buttonRect, buttonLabel, EditorStyles.toolbarPopup))
				{
					var menu = BuildChannelPopupMenu();
					menu.DropDown(buttonRect);
				}
			}
		}

		public void AddItemsToMenu(GenericMenu menu)
		{
			var internalConsoleWindowAddItemsToMenu = internalConsoleWindowInstance as IHasCustomMenu;
			if(internalConsoleWindowAddItemsToMenu != null)
			{
				internalConsoleWindowAddItemsToMenu.AddItemsToMenu(menu);
			}
		}
	}
}