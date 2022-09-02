using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using JetBrains.Annotations;
using System.Text;
using Pancake.Debugging.Console;

namespace Pancake.Debugging
{
	[InitializeOnLoad]
	public static class DebugLogExtensionsPreferences
	{
		private const int UseProjectSettings = 0;
		private const int AllEnabledByDefault = 1;
		private const int AllDisabledByDefault = 2;

		private static readonly GUIContent WhitelistLabel = new GUIContent("Whitelisted Channels", "List of channels from which you want to see all messages, regardless of project settings.");
		private static readonly GUIContent BlacklistLabel = new GUIContent("Blacklisted Channels", "List of channels from which you don't want to see any messages, regardless of project settings.");

		private static readonly GUIContent ChannelOptionsLabel = new GUIContent("Unlisted Channels", "Determines whether or not messages using channels that are not included in whitelist or blacklist should be shown by default.");
		private static readonly GUIContent[] ChannelOptions = new GUIContent[] { new GUIContent("Use Project Settings"), new GUIContent("Enabled By Default"), new GUIContent("Disabled By Default") };

		private static readonly GUIContent DefaultChannelColorsLabel = new GUIContent("Default Channel Colors", "List of colors for use with channels tags in cases where a channel does not have a specific color tied to it.");

		private static readonly GUIContent ListDisplayStyleLabel = new GUIContent("List Display Style", "Default: Your messages are logged as they are with no changes.\n\nClean: An empty line is added at end of every log message, hiding the stack trace from the Console's main view when using 2 Lines display mode.\n\nLarge Font: All log messages by default use a larger fully utilizing all vertical space in Console's main view when using 2 Lines display mode.\n\nAuto: User Large Font mode for single-line messages and Clean mode for multi-line messages.");
		private static readonly GUIContent SingleLineMaxCharCountLabel = new GUIContent("Single Line Max Char Count", "Maximum number of characters in a list-based message before it should automatically be split into multiple rows.");

		private static GUIStyle subtitleStyle;

		private static KeyConfig toggleView;

		private static List<string> blacklist = new List<string>();
		private static List<string> whitelist = new List<string>();
		private static List<Color> channelColors = new List<Color>();

		private static ReorderableList reorderableBlacklist;
		private static ReorderableList reorderableWhitelist;
		private static ReorderableList reorderableChannelColors;

		private static Color colorTrue;
		private static Color colorFalse;
		private static Color colorString;
		private static Color colorNumeric;
		private static Color colorNameValueSeparator;

		private static GUIContent colorTrueLabel = new GUIContent("True", "For \"True\".");
		private static GUIContent colorFalseLabel = new GUIContent("False / Negative", "For \"False\" as well as negative numbers.");
		private static GUIContent colorStringLabel = new GUIContent("String", "For text inside \"\" and character inside ''.");
		private static GUIContent colorNumericLabel = new GUIContent("Numbers", "For numbers.");
		private static GUIContent colorNameValueSeparatorLabel = new GUIContent("Name Value Separator", "For the name value separator.");

		private static bool proSkin;

		private static bool scrollAnimationsCached = true;
		private static bool logUpdatedEffectsCached = true;
		private static bool pushToBottomInCollapsedModeCached = false;
		private static bool hideDetailAreaWhenNoLogEntrySelectedCached = true;

		private static string ColorTrueKey
		{
			get
			{
				return proSkin ? "DebugLogExtensions.ColorTrue2" : "DebugLogExtensions.ColorTrue";
			}
		}

		private static string ColorFalseKey
		{
			get
			{
				return proSkin ? "DebugLogExtensions.ColorFalse2" : "DebugLogExtensions.ColorFalse";
			}
		}

		private static string ColorStringKey
		{
			get
			{
				return proSkin ? "DebugLogExtensions.ColorString2" : "DebugLogExtensions.ColorString";
			}
		}

		private static string ColorNumericKey
		{
			get
			{
				return proSkin ? "DebugLogExtensions.ColorNumeric2" : "DebugLogExtensions.ColorNumeric";
			}
		}

		private static string ColorNameValueSeparatorKey
		{
			get
			{
				return proSkin ? "DebugLogExtensions.ColorNameValueSeparator2" : "DebugLogExtensions.ColorNameValueSeparator";
			}
		}

		private static string ChannelColorsKey
		{
			get
			{
				return proSkin ? "DebugLogExtensions.ChannelColors2" : "DebugLogExtensions.ChannelColors";
			}
		}

		private static Color ColorTrueDefault
		{
			get
			{
				return proSkin ? Color.green : (Color)new Color32(5, 145, 0, 255);
			}
		}

		private static string ColorTrueDefaultText
		{
			get
			{
				return "#" + ColorUtility.ToHtmlStringRGB(ColorTrueDefault);
			}
		}

		private static Color ColorFalseDefault
		{
			get
			{
				return proSkin ? Color.red : (Color)new Color32(200, 0, 0, 255);
			}
		}

		private static string ColorFalseDefaultText
		{
			get
			{
				return "#" + ColorUtility.ToHtmlStringRGB(ColorFalseDefault);
			}
		}

		private static Color ColorStringDefault
		{
			get
			{
				return proSkin ? new Color32(245, 167, 38, 255) : new Color32(154, 9, 209, 255);
			}
		}

		private static string ColorStringDefaultText
		{
			get
			{
				return "#" + ColorUtility.ToHtmlStringRGB(ColorStringDefault);
			}
		}

		private static Color ColorNumericDefault
		{
			get
			{
				return proSkin ? new Color32(0, 191, 255, 255) : new Color32(0, 146, 159, 255);
			}
		}

		private static string ColorNumericDefaultText
		{
			get
			{
				return "#" + ColorUtility.ToHtmlStringRGB(ColorNumericDefault);
			}
		}

		private static Color ColorNameValueSeparatorDefault
		{
			get
			{
				return proSkin ? Color.white : Color.grey;
			}
		}

		private static string ColorNameValueSeparatorDefaultText
		{
			get
			{
				return "#" + ColorUtility.ToHtmlStringRGB(ColorNameValueSeparatorDefault);
			}
		}

		private static Formatting Formatting
		{
			get
			{
				return (Formatting)EditorPrefs.GetInt("DebugLogExtensions.Formatting", (int)Formatting.Clean);
			}
		}

		private static int MaxLengthBeforeLineSplitting
		{
			get
			{
				return EditorPrefs.GetInt("DebugLogExtensions.SplitLength", 175);
			}
		}

		private static string NameValueSeparator
		{
			get
			{
				return EditorPrefs.GetString("DebugLogExtensions.NameValueSeparator", "=");
			}
		}

		private static string MultipleEntrySeparator
		{
			get
			{
				return EditorPrefs.GetString("DebugLogExtensions.MultipleValueSeparator", ", ");
			}
		}

		private static char BeginCollection
		{
			get
			{
				string text = EditorPrefs.GetString("DebugLogExtensions.BeginCollection", "");
				return text.Length == 0 ? '{' : text[0];
			}
		}

		private static char EndCollection
		{
			get
			{
				string text = EditorPrefs.GetString("DebugLogExtensions.EndCollection", "");
				return text.Length == 0 ? '}' : text[0];
			}
		}

		private static bool AllChannelsEnabledByDefault
		{
			get
			{
				switch(HowToDetermineEnabledChannels)
				{
					case UseProjectSettings:
						return DebugLogExtensionsProjectSettings.Get().unlistedChannelsEnabledByDefault;
					case AllDisabledByDefault:
						return false;
					default:
						return true;
				}
			}
		}

		private static int HowToDetermineEnabledChannels
		{
			get
			{
				return PlayerPrefs.GetInt("DebugLogExtensions.DeterminingEnabledChannels", 0);
			}

			set
			{
				switch(value)
				{
					case UseProjectSettings:
						PlayerPrefs.DeleteKey("DebugLogExtensions.DeterminingEnabledChannels");
						return;
					default:
						PlayerPrefs.SetInt("DebugLogExtensions.DeterminingEnabledChannels", value);
						return;
					
				}
			}
		}

		public static bool CompactModeEnabled
        {
			get
			{
				return EditorPrefs.GetBool("DebugLogExtensions.CompactMode", false);
			}

			set
			{
				if(value)
				{
					EditorPrefs.SetBool("DebugLogExtensions.CompactMode", true);
				}
				else
				{
					EditorPrefs.DeleteKey("DebugLogExtensions.CompactMode");
				}
			}
		}

		public static UseMonospaceFont UseMonospaceFont
		{
			get
			{
				#if UNITY_2021_2_OR_NEWER
				return (UseMonospaceFont)EditorPrefs.GetInt("DebugLogExtensions.UseMonospaceFont", (int)UseMonospaceFont.DetailsViewOnly);
				#else
				return UseMonospaceFont.Never;
				#endif
			}

			#if UNITY_2021_2_OR_NEWER
			set
			{
				if(value == UseMonospaceFont.DetailsViewOnly)
				{
					EditorPrefs.DeleteKey("DebugLogExtensions.UseMonospaceFont");
				}
				else
				{
					EditorPrefs.SetInt("DebugLogExtensions.UseMonospaceFont", (int)value);
				}
			}
			#endif
		}

		
		public static bool ScrollAnimations
		{
			get
			{
				return scrollAnimationsCached;
			}

			set
			{
				if(scrollAnimationsCached == value)
				{
					return;
				}

				scrollAnimationsCached = value;
				if(value)
				{
					EditorPrefs.DeleteKey("DebugLogExtensions.ScrollAnimations");
				}
				else
				{
					EditorPrefs.SetBool("DebugLogExtensions.ScrollAnimations", false);
				}
			}
		}

		public static bool HideDetailAreaWhenNoLogEntrySelected
		{
			get
			{
				return hideDetailAreaWhenNoLogEntrySelectedCached;
			}

			set
			{
				if(hideDetailAreaWhenNoLogEntrySelectedCached == value)
				{
					return;
				}

				hideDetailAreaWhenNoLogEntrySelectedCached = value;
				if(value)
				{
					EditorPrefs.SetBool("DebugLogExtensions.AutoHideDetailArea", true);
				}
				else
				{
					EditorPrefs.DeleteKey("DebugLogExtensions.AutoHideDetailArea");
				}
			}
		}

		public static bool LogUpdatedEffects
		{
			get
			{
				return logUpdatedEffectsCached;
			}

			set
			{
				if(logUpdatedEffectsCached == value)
				{
					return;
				}

				logUpdatedEffectsCached = value;
				if(value)
				{
					EditorPrefs.DeleteKey("DebugLogExtensions.LogUpdatedEffects");
				}
				else
				{
					EditorPrefs.SetBool("DebugLogExtensions.LogUpdatedEffects", false);
				}
			}
		}
		
		public static bool PushToBottomInCollapsedMode
		{
			get
			{
				return pushToBottomInCollapsedModeCached;
			}

			set
			{
				if(pushToBottomInCollapsedModeCached == value)
				{
					return;
				}

				pushToBottomInCollapsedModeCached = value;
				if(value)
				{
					EditorPrefs.SetBool("DebugLogExtensions.PushToBottomInCollapsedMode", true);
				}
				else
				{
					EditorPrefs.DeleteKey("DebugLogExtensions.PushToBottomInCollapsedMode");
				}
			}
		}

		private static bool Colorize
		{
			get
			{
				return EditorPrefs.GetBool("DebugLogExtensions.Colorize", true);
			}
		}

		[UsedImplicitly]
		static DebugLogExtensionsPreferences()
		{
			Apply();
		}

		public static void Apply()
		{
			if(EditorApplication.isCompiling || EditorApplication.isUpdating || BuildPipeline.isBuildingPlayer)
			{
				EditorApplication.delayCall += Apply;
				return;
			}

			proSkin = EditorGUIUtility.isProSkin;

			scrollAnimationsCached = EditorPrefs.GetBool("DebugLogExtensions.ScrollAnimations", true);
			logUpdatedEffectsCached = EditorPrefs.GetBool("DebugLogExtensions.LogUpdatedEffects", true);
			pushToBottomInCollapsedModeCached = EditorPrefs.GetBool("DebugLogExtensions.PushToBottomInCollapsedMode", false);

			var settings = DebugLogExtensionsProjectSettings.Get();
			if(settings == null)
			{
				#if DEV_MODE
				Debug.LogWarning("Delaying DebugLogExtensionsPreferences.Apply because settings still null...");
				#endif

				EditorApplication.delayCall += Apply;
				return;
			}

			colorTrue = ColorTrueDefault;
			string colorTrueAsText = ColorTrueDefaultText;
			if(EditorPrefs.HasKey(ColorTrueKey))
			{
				Color setColor;
				colorTrueAsText = "#" + EditorPrefs.GetString(ColorTrueKey, "");
				if(ColorUtility.TryParseHtmlString(colorTrueAsText, out setColor))
				{
					colorTrue = setColor;
				}
				#if DEV_MODE
				else { UnityEngine.Debug.LogWarning("Color parse failed!"); }
				#endif
			}

			colorFalse = ColorFalseDefault;
			string colorFalseAsText = ColorFalseDefaultText;
			if(EditorPrefs.HasKey(ColorFalseKey))
			{
				Color setColor;
				colorFalseAsText = "#" + EditorPrefs.GetString(ColorFalseKey, "");
				if(ColorUtility.TryParseHtmlString(colorFalseAsText, out setColor))
				{
					colorFalse = setColor;
				}
				#if DEV_MODE
				else { UnityEngine.Debug.LogWarning("Color parse failed!"); }
				#endif
			}

			colorString = ColorStringDefault;
			string colorStringAsText = ColorStringDefaultText;
			if(EditorPrefs.HasKey(ColorStringKey))
			{
				Color setColor;
				colorStringAsText = "#" + EditorPrefs.GetString(ColorStringKey, "");
				if(ColorUtility.TryParseHtmlString(colorStringAsText, out setColor))
				{
					colorString = setColor;
				}
				#if DEV_MODE
				else { UnityEngine.Debug.LogWarning("Color parse failed!"); }
				#endif
			}

			colorNumeric = ColorNumericDefault;
			string colorNumericAsText = ColorNumericDefaultText;
			if(EditorPrefs.HasKey(ColorNumericKey))
			{
				Color setColor;
				colorNumericAsText = "#" + EditorPrefs.GetString(ColorNumericKey, "");
				if(ColorUtility.TryParseHtmlString(colorNumericAsText, out setColor))
				{
					colorNumeric = setColor;
				}
				#if DEV_MODE
				else { UnityEngine.Debug.LogWarning("Color parse failed!"); }
				#endif
			}

			colorNameValueSeparator = ColorNameValueSeparatorDefault;
			string colorNameValueSeparatorAsText = ColorNameValueSeparatorDefaultText;
			if(EditorPrefs.HasKey(ColorNameValueSeparatorKey))
			{
				Color setColor;
				colorNameValueSeparatorAsText = "#" + EditorPrefs.GetString(ColorNameValueSeparatorKey, "");
				if(ColorUtility.TryParseHtmlString(colorNameValueSeparatorAsText, out setColor))
				{
					colorNameValueSeparator = setColor;
				}
				#if DEV_MODE
				else { UnityEngine.Debug.LogWarning("Color parse failed!"); }
				#endif
			}

			string[] defaultChannelColors;
			if(EditorPrefs.HasKey(ChannelColorsKey))
			{
				channelColors.Clear();
				defaultChannelColors = EditorPrefs.GetString(ChannelColorsKey, "").Split(';');

				#if DEV_MODE
				UnityEngine.Debug.Assert(defaultChannelColors.Length >= 6);
				#endif

				for(int n = 0, count = defaultChannelColors.Length; n < count; n++)
				{
					Color color;
					if(ColorUtility.TryParseHtmlString("#" + defaultChannelColors[n], out color))
					{
						channelColors.Add(color);
					}
					#if DEV_MODE
					else { UnityEngine.Debug.LogWarning("Color parse failed!"); }
					#endif
				}
			}
			else
			{
				if(proSkin)
				{
					channelColors = new List<Color>(){ new Color32(99, 252, 54, 255), Color.magenta, new Color32(255, 165, 0, 255), Color.red, Color.cyan, Color.yellow, new Color32(173, 216, 230, 255) };
				}
				else
				{
					channelColors = new List<Color>() { new Color32(0, 128, 128, 255), Color.magenta, new Color32(128, 0, 0, 255), Color.green, new Color32(255, 165, 0, 255), new Color32(128, 0, 128, 255), Color.blue, Color.yellow, new Color32(165, 42, 42, 255) };
				}

				int count = channelColors.Count;
				defaultChannelColors = new string[count];
				for(int n = 0; n < count; n++)
				{
					defaultChannelColors[n] = "#" + ColorUtility.ToHtmlStringRGB(channelColors[n]);
				}
			}

			reorderableChannelColors = new ReorderableList(channelColors, typeof(Color), true, true, true, true)
			{
				drawHeaderCallback = DrawChannelColorsHeader,
				drawElementCallback = DrawChannelColor,
				onRemoveCallback = RemoveChannelColor,
				onAddCallback = AddChannelColor,
				elementHeight = 20f
			};

			Debug.channels.SetDefaultChannelColors(defaultChannelColors);

			Debug.formatter = new DebugFormatter(Formatting, MaxLengthBeforeLineSplitting, Colorize, NameValueSeparator, MultipleEntrySeparator, BeginCollection, EndCollection, AllChannelsEnabledByDefault, colorTrueAsText, colorFalseAsText, colorStringAsText, colorNumericAsText, colorNameValueSeparatorAsText);

			if(PlayerPrefs.HasKey("DebugLogExtensions.Blacklist"))
			{
				blacklist.Clear();
				blacklist.AddRange(PlayerPrefs.GetString("DebugLogExtensions.Blacklist").Split(';'));
			}
			reorderableBlacklist = new ReorderableList(blacklist, typeof(string), true, true, true, true)
			{
				drawHeaderCallback = DrawBlacklistHeader,
				drawElementCallback = DrawBlacklistElement,
				onRemoveCallback = RemoveBlacklistChannel,
				onAddCallback = AddBlacklistChannel,
				elementHeight = 20f
			};

			if(PlayerPrefs.HasKey("DebugLogExtensions.Whitelist"))
			{
				whitelist.Clear();
				whitelist.AddRange(PlayerPrefs.GetString("DebugLogExtensions.Whitelist").Split(';'));
			}
			reorderableWhitelist = new ReorderableList(whitelist, typeof(string), true, true, true, true)
			{
				drawHeaderCallback = DrawWhitelistHeader,
				drawElementCallback = DrawWhitelistElement,
				onRemoveCallback = RemoveWhitelistChannel,
				onAddCallback = AddWhitelistChannel,
				elementHeight = 20f
			};

			// Apply channel enabledness based on user preferences (overriding any previously applied project settings).
			for(int n = blacklist.Count - 1; n >= 0; n--)
			{
				Debug.channels.DisableChannel(blacklist[n]);
			}
			for(int n = whitelist.Count - 1; n >= 0; n--)
			{
				Debug.channels.EnableChannel(whitelist[n]);
			}

			if(toggleView.KeyCode == default(KeyCode))
			{
				toggleView = new KeyConfig(KeyConfig.ToggleViewKey, KeyCode.Insert, false, false, false);
				toggleView.Load();
			}
			else
			{
				toggleView.Save();
			}

			// Automatically whitelist personal channel in preferences if it is not already found in whitelist or blacklist.
			var personalChannel = Dev.PersonalChannelName;
			bool devUniqueChannelFound = whitelist.Contains(personalChannel, StringComparer.OrdinalIgnoreCase) || blacklist.Contains(personalChannel, StringComparer.OrdinalIgnoreCase);
			if(!devUniqueChannelFound)
			{
				whitelist.Insert(0, personalChannel);
			}
		}

		[SettingsProvider, UsedImplicitly]
		private static SettingsProvider CreateSettingsProvider()
		{
			var provider = new SettingsProvider("Preferences/Heart/Debug Log", SettingsScope.User)
			{
				label = "Debug Log",
				guiHandler = DrawSettingsGUI,

				// Populate the search keywords to enable smart search filtering and label highlighting
				keywords = new HashSet<string>(new[] { "Formatting", "List Display Style", "Colorize", "Single Line Max Char Count", "Channels", "Enabled Channels", "Disabled Channels" })
			};

			return provider;
		}
		
		private static void DrawSettingsGUI(string searchContext)
		{
			DrawSettingsGUI();
		}

		private static void DrawSettingsGUI()
		{
			if(proSkin != EditorGUIUtility.isProSkin || reorderableWhitelist == null)
			{
				Apply();

				if(reorderableWhitelist == null)
                {
					return;
                }
			}

			if(subtitleStyle == null)
			{
				subtitleStyle = new GUIStyle(EditorStyles.largeLabel);
				subtitleStyle.wordWrap = true;
			}

			GUILayout.Label("These are your personal preferences and will not affect other users of the project.", subtitleStyle);

			GUILayout.Space(5f);

			GUILayout.Label("Your personal channel prefix is [" + Dev.PersonalChannelName + "].", subtitleStyle);

			GUILayout.Space(10f);

			EditorGUI.indentLevel = 1;

			float labelWidthWas = EditorGUIUtility.labelWidth;
			EditorGUIUtility.labelWidth = 190f;

			GUILayout.Label("Channels", EditorStyles.boldLabel);

			reorderableWhitelist.DoLayoutList();

			GUILayout.Space(3f);

			reorderableBlacklist.DoLayoutList();
			EditorGUI.indentLevel = 1;

			int channelOptionWas = HowToDetermineEnabledChannels;
			var setChannelOption = EditorGUILayout.Popup(ChannelOptionsLabel, channelOptionWas, ChannelOptions);
			if(setChannelOption != channelOptionWas)
			{
				HowToDetermineEnabledChannels = setChannelOption;
				Apply();
			}

			GUILayout.Space(10f);

			GUILayout.Label("Formatting", EditorStyles.boldLabel);

			var formatting = Formatting;
			var setFormatting = (Formatting)EditorGUILayout.EnumPopup(ListDisplayStyleLabel, formatting);
			if(setFormatting != formatting)
			{
				if(setFormatting == Formatting.Clean)
				{
					EditorPrefs.DeleteKey("DebugLogExtensions.Formatting");
				}
				else
				{
					EditorPrefs.SetInt("DebugLogExtensions.Formatting", (int)setFormatting);
				}
				Apply();
			}

			var splitLength = MaxLengthBeforeLineSplitting;
			var setSplitLength = EditorGUILayout.IntField(SingleLineMaxCharCountLabel, splitLength);
			if(setSplitLength != splitLength)
			{
				if(setSplitLength == 175)
				{
					EditorPrefs.DeleteKey("DebugLogExtensions.SplitLength");
				}
				else
				{
					EditorPrefs.SetInt("DebugLogExtensions.SplitLength", setSplitLength);
				}
				Apply();
			}

			var nameValueSeparator = NameValueSeparator;
			var setNameValueSeparator = EditorGUILayout.DelayedTextField("Name Value Separator", nameValueSeparator);
			if(setNameValueSeparator != nameValueSeparator)
			{
				if(setNameValueSeparator == "=" || string.IsNullOrEmpty(setNameValueSeparator))
				{
					EditorPrefs.DeleteKey("DebugLogExtensions.NameValueSeparator");
				}
				else
				{
					EditorPrefs.SetString("DebugLogExtensions.NameValueSeparator", setNameValueSeparator);
				}
				Apply();
			}

			var multipleValueSeparator = MultipleEntrySeparator;
			var setMultipleValueSeparator = EditorGUILayout.DelayedTextField("Multiple Entry Separator", multipleValueSeparator);
			if(setMultipleValueSeparator != multipleValueSeparator)
			{
				if(setMultipleValueSeparator == "=" || string.IsNullOrEmpty(setMultipleValueSeparator))
				{
					EditorPrefs.DeleteKey("DebugLogExtensions.MultipleValueSeparator");
				}
				else
				{
					EditorPrefs.SetString("DebugLogExtensions.MultipleValueSeparator", setMultipleValueSeparator);
				}
				Apply();
			}

			var beginCollection = BeginCollection;
			var setBeginCollectionText = EditorGUILayout.DelayedTextField("Begin Collection", beginCollection.ToString());
			var setBeginCollection = setBeginCollectionText.Length != 1 ? beginCollection : setBeginCollectionText[0];
			if(setBeginCollection != beginCollection)
			{
				if(setBeginCollection == '{')
				{
					EditorPrefs.DeleteKey("DebugLogExtensions.BeginCollection");
				}
				else
				{
					EditorPrefs.SetString("DebugLogExtensions.BeginCollection", setBeginCollection.ToString());
				}
				Apply();
			}

			var endCollection = EndCollection;
			var setEndCollectionText = EditorGUILayout.DelayedTextField("End Collection", endCollection.ToString());
			var setEndCollection = setEndCollectionText.Length != 1 ? endCollection : setEndCollectionText[0];
			if(setEndCollection != endCollection)
			{
				if(setEndCollection == '{')
				{
					EditorPrefs.DeleteKey("DebugLogExtensions.EndCollection");
				}
				else
				{
					EditorPrefs.SetString("DebugLogExtensions.EndCollection", setEndCollection.ToString());
				}
				Apply();
			}

			GUILayout.Space(10f);

			GUILayout.Label("Shortcuts", EditorStyles.boldLabel);
			DrawKeyConfigGUI();

			GUILayout.Space(10f);

			GUILayout.Label("Console+", EditorStyles.boldLabel);

			bool scrollAnimations = ScrollAnimations;
			var setScrollAnimations = EditorGUILayout.Toggle(new GUIContent("Enable Smooth Scrolling", "Should the Console list scroll to the bottom with a smooth animation when new messages appear in it?"), scrollAnimations);
			if(setScrollAnimations != scrollAnimations)
			{
				ScrollAnimations = setScrollAnimations;
				Apply();
			}

			bool hideDetailAreaWhenNoLogEntrySelected = HideDetailAreaWhenNoLogEntrySelected;
			var setHideDetailAreaWhenNoLogEntrySelected = EditorGUILayout.Toggle(new GUIContent("Auto-Hide Detail Area", "Should the bottom detail area be automatically hidden when no messages are selected?"), hideDetailAreaWhenNoLogEntrySelected);
			if(setHideDetailAreaWhenNoLogEntrySelected != hideDetailAreaWhenNoLogEntrySelected)
			{
				HideDetailAreaWhenNoLogEntrySelected = setHideDetailAreaWhenNoLogEntrySelected;
				Apply();
			}

			bool logUpdatedEffects = LogUpdatedEffects;
			var setLogUpdatedEffects = EditorGUILayout.Toggle(new GUIContent("Log Updated Effects", "Should new entries that appear in the Console list be highlighted momentarily?"), logUpdatedEffects);
			if(setLogUpdatedEffects != logUpdatedEffects)
			{
				LogUpdatedEffects = setLogUpdatedEffects;
				Apply();
			}

			bool pushToBottomInCollapsedMode = PushToBottomInCollapsedMode;
			var setPushToBottomInCollapsedMode = EditorGUILayout.Toggle(new GUIContent("Push Newest To Bottom", "In collapsed mode should logs be pushed to bottom when there is a new occurrence or should they always remain in place of first occurrence?"), pushToBottomInCollapsedMode);
			if(setPushToBottomInCollapsedMode != pushToBottomInCollapsedMode)
			{
				PushToBottomInCollapsedMode = setPushToBottomInCollapsedMode;
				Apply();
			}

			GUILayout.Space(10f);

			GUILayout.Label("Colors", EditorStyles.boldLabel);

			bool colorize = Colorize;
			var setColorize = EditorGUILayout.Toggle("Colorize", colorize);
			if(setColorize != colorize)
			{
				if(setColorize)
				{
					EditorPrefs.DeleteKey("DebugLogExtensions.Colorize");
				}
				else
				{
					EditorPrefs.SetBool("DebugLogExtensions.Colorize", false);
				}
				Apply();
			}

			if(colorize)
			{
				var setColorTrue = EditorGUILayout.ColorField(colorTrueLabel, colorTrue, false, false, false);
				if(setColorTrue != colorTrue)
				{
					colorTrue = setColorTrue;
					if(setColorTrue == ColorTrueDefault)
					{
						EditorPrefs.DeleteKey(ColorTrueKey);
					}
					else
					{
						EditorPrefs.SetString(ColorTrueKey, ColorUtility.ToHtmlStringRGB(setColorTrue));
					}
					Apply();
				}

				var setColorFalse = EditorGUILayout.ColorField(colorFalseLabel, colorFalse, false, false, false);
				if(setColorFalse != colorFalse)
				{
					colorFalse = setColorFalse;
					if(setColorFalse == ColorFalseDefault)
					{
						EditorPrefs.DeleteKey(ColorFalseKey);
					}
					else
					{
						EditorPrefs.SetString(ColorFalseKey, ColorUtility.ToHtmlStringRGB(setColorFalse));
					}
					Apply();
				}

				var setColorString = EditorGUILayout.ColorField(colorStringLabel, colorString, false, false, false);
				if(setColorString != colorString)
				{
					colorString = setColorString;
					if(setColorString == ColorStringDefault)
					{
						EditorPrefs.DeleteKey(ColorStringKey);
					}
					else
					{
						EditorPrefs.SetString(ColorStringKey, ColorUtility.ToHtmlStringRGB(setColorString));
					}
					Apply();
				}

				var setColorNumeric = EditorGUILayout.ColorField(colorNumericLabel, colorNumeric, false, false, false);
				if(setColorNumeric != colorNumeric)
				{
					colorNumeric = setColorNumeric;
					if(setColorNumeric == ColorNumericDefault)
					{
						EditorPrefs.DeleteKey(ColorNumericKey);
					}
					else
					{
						EditorPrefs.SetString(ColorNumericKey, ColorUtility.ToHtmlStringRGB(setColorNumeric));
					}
					Apply();
				}

				var setColorNameValueSeparator = EditorGUILayout.ColorField(colorNameValueSeparatorLabel, colorNameValueSeparator, false, false, false);
				if(setColorNameValueSeparator != colorNameValueSeparator)
				{
					colorNameValueSeparator = setColorNameValueSeparator;
					if(setColorNameValueSeparator == ColorNameValueSeparatorDefault)
					{
						EditorPrefs.DeleteKey(ColorNameValueSeparatorKey);
					}
					else
					{
						EditorPrefs.SetString(ColorNameValueSeparatorKey, ColorUtility.ToHtmlStringRGB(setColorNameValueSeparator));
					}
					Apply();
				}

				EditorGUILayout.BeginHorizontal();
				{
					EditorGUILayout.PrefixLabel("Test Colors");

					EditorGUI.indentLevel = 0;

					if(GUILayout.Button("Test Content Colors"))
					{
						var color = "#" + ColorUtility.ToHtmlStringRGB(colorTrue);
						UnityEngine.Debug.Log("<color=" + color + ">True</color> : " + color);
						color = "#" + ColorUtility.ToHtmlStringRGB(colorFalse);
						UnityEngine.Debug.Log("<color=" + color + ">False</color> : " + color);
						color = "#" + ColorUtility.ToHtmlStringRGB(colorString);
						UnityEngine.Debug.Log("<color=" + color + ">\"Text\"</color> : " + color);
						color = "#" + ColorUtility.ToHtmlStringRGB(colorNumeric);
						UnityEngine.Debug.Log("<color=" + color + ">1234567890</color> : " + color);
						color = "#" + ColorUtility.ToHtmlStringRGB(colorNameValueSeparator);
						UnityEngine.Debug.Log("field<color=" + color + ">"+ nameValueSeparator + "</color>value : " + color);
					}
				}
				EditorGUILayout.EndHorizontal();
			}

			GUILayout.Space(5f);

			reorderableChannelColors.DoLayoutList();

			GUILayout.Space(5f);

			GUILayout.BeginHorizontal();
			GUILayout.Space(20f);
			if(GUILayout.Button("Reset All To Defaults"))
			{
				if(EditorUtility.DisplayDialog("Reset All To Defaults?", "Are you sure you want to reset all personal preferences to default values?\n\nThis will not affect any other users of the project.", "Reset All", "Cancel"))
				{
					PlayerPrefs.DeleteKey("DebugLogExtensions.DeterminingEnabledChannels");
					PlayerPrefs.DeleteKey("DebugLogExtensions.Whitelist");
					PlayerPrefs.DeleteKey("DebugLogExtensions.Blacklist");

					EditorPrefs.DeleteKey("DebugLogExtensions.ChannelColors");

					EditorPrefs.DeleteKey("DebugLogExtensions.Formatting");
					EditorPrefs.DeleteKey("DebugLogExtensions.SplitLength");
					EditorPrefs.DeleteKey("DebugLogExtensions.NameValueSeparator");
					EditorPrefs.DeleteKey("DebugLogExtensions.MultipleValueSeparator");

					EditorPrefs.DeleteKey("DebugLogExtensions.Colorize");
					EditorPrefs.DeleteKey(ColorTrueKey);
					EditorPrefs.DeleteKey(ColorFalseKey);
					EditorPrefs.DeleteKey(ColorStringKey);
					EditorPrefs.DeleteKey(ColorNumericKey);
					EditorPrefs.DeleteKey(ColorNameValueSeparatorKey);

					Apply();
				}
			}
			GUILayout.Space(20f);
			GUILayout.EndHorizontal();

			EditorGUI.indentLevel = 0;
			EditorGUIUtility.labelWidth = labelWidthWas;
		}

		public static void DrawKeyConfigGUI()
		{
			GUILayout.BeginHorizontal();
			{
				EditorGUILayout.PrefixLabel("Toggle GUI");

				int indentLevelWas = EditorGUI.indentLevel;
				EditorGUI.indentLevel = 0;

				var setKey = (KeyCode)EditorGUILayout.EnumPopup(toggleView.keyCodeOverride);
				if(!toggleView.KeyCode.Equals(setKey))
				{
					toggleView.SetKeyCodeOverride(setKey);
					Apply();
				}

				GUILayout.Label(new GUIContent("Ctrl", "Require Control modifier?"), GUILayout.Width(30f));

				bool controlWas = toggleView.controlOverride;
				bool setControl = EditorGUILayout.Toggle(controlWas);
				if(setControl != controlWas)
				{
					toggleView.SetControlOverride(setControl);
					Apply();
				}

				GUILayout.Label(new GUIContent("Alt", "Require Alt modifier?"), GUILayout.Width(30f));

				bool altWas = toggleView.altOverride;
				bool setAlt = EditorGUILayout.Toggle(altWas);
				if(setAlt != altWas)
				{
					toggleView.SetAltOverride(setAlt);
					Apply();
				}

				GUILayout.Label(new GUIContent("Shift", "Require Shift modifier?"), GUILayout.Width(30f));

				bool shiftWas = toggleView.shiftOverride;
				bool setShift = EditorGUILayout.Toggle(shiftWas);
				if(setShift != shiftWas)
				{
					toggleView.SetShiftOverride(setShift);
					Apply();
				}

				EditorGUI.indentLevel = indentLevelWas;
			}
			GUILayout.EndHorizontal();
		}

		private static void DrawBlacklistHeader(Rect rect)
		{
			GUI.Label(rect, BlacklistLabel);
		}

		private static void DrawWhitelistHeader(Rect rect)
		{
			GUI.Label(rect, WhitelistLabel);
		}

		private static void DrawChannelColorsHeader(Rect rect)
		{
			GUI.Label(rect, DefaultChannelColorsLabel);
			
			rect.x += rect.width - 150f;
			rect.width = 150f;

			if(GUI.Button(rect, "Test Channel Colors"))
			{
				for(int n = 0, count = channelColors.Count; n < count; n++)
				{
					var color = ColorUtility.ToHtmlStringRGB(channelColors[n]);
					UnityEngine.Debug.Log("<color=#"+ color + ">[Color"+(n + 1)+"]</color> #" + color);
				}
			}
		}

		private static void DrawBlacklistElement(Rect rect, int index, bool isActive, bool isFocused)
		{
			DrawChannelsElement(blacklist, "DebugLogExtensions.Blacklist", rect, index);
		}

		private static void DrawWhitelistElement(Rect rect, int index, bool isActive, bool isFocused)
		{
			DrawChannelsElement(whitelist, "DebugLogExtensions.Whitelist", rect, index);
		}

		private static void DrawChannelsElement(List<string> channels, string prefsKey, Rect rect, int index)
		{
			rect.y += 2f;
			rect.height -= 4f;

			EditorGUI.indentLevel = 0;
			string idWas = channels[index];
			string setId = EditorGUI.DelayedTextField(rect, idWas);
			if(!string.Equals(idWas, setId))
			{
				setId = setId.Replace("[", "").Replace("]", "");
				if(!string.Equals(idWas, setId))
				{
					channels[index] = setId;
					PlayerPrefs.SetString(prefsKey, string.Join(";", channels));
					Apply();
				}
			}
		}

		private static void DrawChannelColor(Rect rect, int index, bool isActive, bool isFocused)
		{
			rect.y += 2f;
			rect.height -= 4f;

			EditorGUI.indentLevel = 0;
			var colorWas = channelColors[index];
			var setColor = EditorGUI.ColorField(rect, GUIContent.none, colorWas, false, false, false);
			if(colorWas != setColor)
			{
				channelColors[index] = setColor;
				int count = channelColors.Count;
				var sb = new StringBuilder(count * 8);
				if(count > 0)
				{
					sb.Append(channelColors[0]);
					for(int n = 1; n < count; n++)
					{
						sb.Append(';');
						sb.Append(ColorUtility.ToHtmlStringRGB(channelColors[n]));
					}
				}
				PlayerPrefs.SetString("DebugLogExtensions.ChannelColors", sb.ToString());
				Apply();
			}
		}

		private static void AddBlacklistChannel(ReorderableList list)
		{
			AddChannel(list, ref blacklist, "DebugLogExtensions.Blacklist");
		}

		private static void AddWhitelistChannel(ReorderableList list)
		{
			AddChannel(list, ref whitelist, "DebugLogExtensions.Whitelist");
		}

		private static void AddChannel(ReorderableList list, ref List<string> channels, string prefsKey)
		{
			channels.Add("");
		}

		private static void AddChannelColor(ReorderableList list)
		{
			var color = UnityEngine.Random.ColorHSV();
			color.a = 1f;
			channelColors.Add(color);
		}

		private static void RemoveBlacklistChannel(ReorderableList list)
		{
			RemoveChannel(list, ref blacklist, "DebugLogExtensions.Blacklist");
		}

		private static void RemoveWhitelistChannel(ReorderableList list)
		{
			RemoveChannel(list, ref whitelist, "DebugLogExtensions.Whitelist");
		}

		private static void RemoveChannel(ReorderableList list, ref List<string> channels, string prefsKey)
		{
			int countWas = channels.Count;
			if(countWas == 0)
			{
				return;
			}

			int setCount = countWas - 1;

			if(setCount == 0)
			{
				PlayerPrefs.DeleteKey(prefsKey);
				channels.Clear();
				return;
			}

			int selected = list.index;
			channels.RemoveAt(selected);
			PlayerPrefs.SetString(prefsKey, string.Join(";", channels));
		}

		private static void RemoveChannelColor(ReorderableList list)
		{
			int countWas = channelColors.Count;
			if(countWas == 0)
			{
				return;
			}

			int setCount = countWas - 1;

			if(setCount == 0)
			{
				PlayerPrefs.DeleteKey("DebugLogExtensions.ChannelColors");
				channelColors.Clear();
				return;
			}

			int selected = list.index;
			channelColors.RemoveAt(selected);

			var sb = new StringBuilder(setCount * 8);
			if(setCount > 0)
			{
				sb.Append(channelColors[0]);
				for(int n = 1; n < setCount; n++)
				{
					sb.Append(';');
					sb.Append(ColorUtility.ToHtmlStringRGB(channelColors[n]));
				}
			}
			PlayerPrefs.SetString("DebugLogExtensions.ChannelColors", sb.ToString());
			Apply();
		}
	}
}