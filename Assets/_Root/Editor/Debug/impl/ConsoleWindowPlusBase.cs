using System;
using UnityEngine;
using UnityEditor;
using JetBrains.Annotations;

namespace Pancake.Debugging.Console
{
	internal abstract class ConsoleWindowPlusBase : EditorWindow, ISerializationCallbackReceiver
	{
		protected static double lastQuitRequestTime = -100d;

		protected GUIContent shortChannelsButtonLabel = new GUIContent("Channels", "Select channels from which messages are received.");
		protected GUIContent fullChannelsButtonLabel = new GUIContent("Channels 1/1", "Select channels from which messages are received.");

		protected float fullChannelsButtonWidth = 100f;
		protected float shortChannelsButtonWidth = 90f;
		protected int enabledChannelsCount = -1;
		protected int totalChannelsCount = -1;

		[SerializeField]
		private Channels channelsSerialized;

		[NonSerialized]
		protected bool initialized = false;

		[NonSerialized]
		protected bool subscribedToEvents = false;

		protected virtual string Title
		{
			get
			{
				return "Console+";
			}
		}

		[UsedImplicitly]
		protected virtual void OnGUI()
		{
			if(!initialized)
			{
				initialized = true;
				UpdateChannelsButton();
				ResubscribeToEvents();
			}
		}
		
		protected virtual void OnEnable()
		{
			if(EditorGUIUtility.isProSkin)
			{
				titleContent = new GUIContent(Title, EditorGUIUtility.Load("d_UnityEditor.ConsoleWindow") as Texture);
			}
			else
			{
				titleContent = new GUIContent(Title, EditorGUIUtility.Load("UnityEditor.ConsoleWindow") as Texture);
			}

			ResubscribeToEvents();

			// Delay applying enabled/disabled channels so that settings and preferences have already had time to get applied.
			EditorApplication.delayCall += ()=>
			{
				if(this == null)
				{
					return;
				}
				RestoreSerializedChannels();
			};

			OnEnabledChannelsChanged();
		}

		protected void ResubscribeToEvents()
		{
			if(subscribedToEvents)
            {
				return;
            }
			subscribedToEvents = true;

			UnsubscribeFromEvents();
			SubscribeToEvents();
		}

		protected virtual void UnsubscribeFromEvents()
		{
			Application.logMessageReceived -= OnLogMessageReceived;
		}

		protected virtual void SubscribeToEvents()
		{
			Application.logMessageReceived += OnLogMessageReceived;
		}

		// This should be called during initialization, but after a short delay so that Debug has had time to initialize first
		// (otherwise all changes could be overridden during initialization phase).
		private void RestoreSerializedChannels()
		{
			if(channelsSerialized == null)
			{
				return;
			}

			Debug.channels.OnEnabledChannelsChanged -= OnEnabledChannelsChanged;

			Debug.channels.AllChannelsEnabledByDefault = channelsSerialized.AllChannelsEnabledByDefault;
			Debug.channels.MessagesWithNoChannelsEnabled = channelsSerialized.MessagesWithNoChannelsEnabled;

			foreach(var channel in channelsSerialized)
			{
				// Skip None channel
				if(channel.Length == 0)
                {
					continue;
                }

				Debug.channels.RegisterChannel(channel);
				Debug.channels.SetChannelColor(channel, channelsSerialized.GetChannelColor(channel));

				switch(channelsSerialized.GetForceEnabledState(channel))
				{
					case ChannelEnabled.ForceEnabled:
						Debug.channels.EnableChannel(channel);
						break;
					case ChannelEnabled.ForceDisabled:
						Debug.channels.DisableChannel(channel);
						break;
				}
			}

			Debug.channels.OnEnabledChannelsChanged += OnEnabledChannelsChanged;

			OnEnabledChannelsChanged();
		}

		[UsedImplicitly]
		protected virtual void OnDisable()
		{
			initialized = false;

			UnsubscribeFromEvents();
		}

		protected abstract void OnLogMessageReceived(string message, string stackTrace, LogType type);

		protected void OnEnabledChannelsChanged()
		{
			OnEnabledChannelsChanged(Debug.channels);
		}

		protected virtual void OnEnabledChannelsChanged(Channels channels)
		{
			UpdateChannelsButton();
		}

		private void UpdateChannelsButton()
		{
			// Prevents NullReferenceException when calling EditorStyles.toolbarPopup during OnEnable, plus ensures Debug has had time to setup before this gets called.
			if(!initialized && Event.current == null)
			{
				return;
			}

			int setEnabledChannelsCount = GetEnabledChannelsCount();
			int setTotalChannelsCount = Debug.channels.Count + 1; // plus one because of "None" channel.
			
			if(setEnabledChannelsCount != enabledChannelsCount || setTotalChannelsCount != totalChannelsCount)
			{
				fullChannelsButtonLabel.text = "Channels  " + setEnabledChannelsCount + " / " + setTotalChannelsCount;
				shortChannelsButtonLabel = new GUIContent("Channels");

				enabledChannelsCount = setEnabledChannelsCount;
				totalChannelsCount = setTotalChannelsCount;

				fullChannelsButtonWidth = EditorStyles.toolbarPopup.CalcSize(fullChannelsButtonLabel).x;
				shortChannelsButtonWidth = EditorStyles.toolbarPopup.CalcSize(shortChannelsButtonLabel).x;
				
				Repaint();
			}
		}

		private int GetEnabledChannelsCount()
		{
			// Handle "None"
			int count = Debug.channels.MessagesWithNoChannelsEnabled ? 1 : 0;

			// Handle rest of the channels
			foreach(var channel in Debug.channels)
			{
				if(Debug.channels.IsEnabled(channel))
				{
					count++;
				}
			}
			return count;
		}

		protected GenericMenu BuildChannelPopupMenu()
		{
			var menu = new GenericMenu();

			if(Event.current.button == 0)
			{
				menu.AddItem(new GUIContent("None"), Debug.channels.MessagesWithNoChannelsEnabled, ToggleShowMessagesWithNoChannel);
			}
			else
			{
				menu.AddItem(new GUIContent("None"), Debug.channels.MessagesWithNoChannelsEnabled, OnlyShowMessagesWithNoChannel);
			}

			foreach(string channel in Debug.channels)
			{
				if(!string.Equals(channel, Dev.PersonalChannelName))
				{
					continue;
				}
				var label = new GUIContent(channel + " (Personal)");
				bool enabled = Debug.channels.IsEnabled(channel);
				if(Event.current.button == 0)
				{
					menu.AddItem(label, enabled, ToggleChannel, channel);
				}
				else
				{
					menu.AddItem(label, enabled, SetAsOnlyChannel, channel);
				}
				break;
			}

			foreach(string channel in Debug.channels)
			{
				if(string.Equals(channel, Dev.PersonalChannelName))
				{
					continue;
				}

				var label = new GUIContent(channel);
				bool enabled = channel.Length == 0 ? Debug.channels.MessagesWithNoChannelsEnabled : Debug.channels.IsEnabled(channel);
				if(Event.current.button == 0)
				{
					menu.AddItem(label, enabled, ToggleChannel, channel);
				}
				else
				{
					menu.AddItem(label, enabled, SetAsOnlyChannel, channel);
				}
			}

			menu.AddSeparator("");
			menu.AddItem(new GUIContent("Enable All"), AllChannelsAreEnabled(), EnableAllChannels);
			menu.AddItem(new GUIContent("Disable All"), AllChannelsAreDisabled(), DisableAllChannels);

			menu.AddSeparator("");
			menu.AddItem(new GUIContent("Reset Channels List"), false, ResetChannelsList);

			return menu;
		}

		protected bool AllChannelsAreEnabled()
		{
			foreach(var channel in Debug.channels)
			{
				if(!Debug.channels.IsEnabled(channel))
				{
					return false;
				}
			}
			return true;
		}

		protected bool AllChannelsAreDisabled()
		{
			foreach(var channel in Debug.channels)
			{
				if(Debug.channels.IsEnabled(channel))
				{
					return false;
				}
			}
			return true;
		}

		protected void EnableAllChannels()
		{
			Debug.channels.OnEnabledChannelsChanged -= OnEnabledChannelsChanged;

			Debug.channels.AllChannelsEnabledByDefault = true;
			Debug.channels.MessagesWithNoChannelsEnabled = true;

			foreach(var channel in Debug.channels)
			{
				Debug.channels.EnableChannel(channel);
			}

			Debug.channels.OnEnabledChannelsChanged += OnEnabledChannelsChanged;

			OnEnabledChannelsChanged();
		}

		protected void DisableAllChannels()
		{
			Debug.channels.OnEnabledChannelsChanged -= OnEnabledChannelsChanged;

			Debug.channels.AllChannelsEnabledByDefault = false;
			Debug.channels.MessagesWithNoChannelsEnabled = false;

			foreach(var channel in Debug.channels)
			{
				Debug.channels.DisableChannel(channel);
			}

			Debug.channels.OnEnabledChannelsChanged += OnEnabledChannelsChanged;

			OnEnabledChannelsChanged();
		}

		protected void ResetChannelsList()
		{
			if(!EditorUtility.DisplayDialog("Reset Channels List?", "Are you sure you want to reset channels list to default state?", "Reset", "Cancel"))
			{
				return;
			}

			Debug.channels.OnEnabledChannelsChanged -= OnEnabledChannelsChanged;

			Debug.channels.ResetToDefaults();

			Debug.channels.OnEnabledChannelsChanged += OnEnabledChannelsChanged;

			OnEnabledChannelsChanged();
		}

		protected void ToggleShowMessagesWithNoChannel()
		{
			Debug.channels.MessagesWithNoChannelsEnabled = !Debug.channels.MessagesWithNoChannelsEnabled;

			OnEnabledChannelsChanged();
		}

		protected void OnlyShowMessagesWithNoChannel()
		{
			Debug.channels.OnEnabledChannelsChanged -= OnEnabledChannelsChanged;

			Debug.channels.AllChannelsEnabledByDefault = false;
			Debug.channels.MessagesWithNoChannelsEnabled = true;

			foreach(var channel in Debug.channels)
			{
				Debug.DisableChannel(channel);
			}

			Debug.channels.OnEnabledChannelsChanged += OnEnabledChannelsChanged;

			OnEnabledChannelsChanged();
		}

		protected void ToggleChannel(object parameter)
		{
			Debug.channels.OnEnabledChannelsChanged -= OnEnabledChannelsChanged;

			string channel = (string)parameter;

			if(Debug.channels.IsEnabled(channel))
			{
				Debug.channels.DisableChannel(channel);
			}
			else
			{
				Debug.channels.EnableChannel(channel);
			}

			Debug.channels.OnEnabledChannelsChanged += OnEnabledChannelsChanged;

			OnEnabledChannelsChanged();
		}

		protected void SetAsOnlyChannel(object parameter)
		{
			Debug.channels.OnEnabledChannelsChanged -= OnEnabledChannelsChanged;

			Debug.channels.MessagesWithNoChannelsEnabled = false;
			Debug.channels.AllChannelsEnabledByDefault = false;

			string channelToEnable = (string)parameter;

			foreach(var channel in Debug.channels)
			{
				if(string.Equals(channel, channelToEnable, StringComparison.OrdinalIgnoreCase))
				{
					Debug.channels.EnableChannel(channel);
				}
				else
				{
					Debug.channels.DisableChannel(channel);
				}
			}

			Debug.channels.OnEnabledChannelsChanged += OnEnabledChannelsChanged;

			OnEnabledChannelsChanged();
		}

		[UsedImplicitly]
		public virtual void OnBeforeSerialize()
		{
			channelsSerialized = Debug.channels;
		}

		[UsedImplicitly]
		public virtual void OnAfterDeserialize() { }
	}
}