using System;
using System.Reflection;
using UnityEditor;
using JetBrains.Annotations;

namespace Pancake.Debugging.Console
{
	public static class PlayerConnectionLogReceiver
	{
		public enum ConnectionState
		{
			Disconnected,
			CleanLog,
			FullLog
		}

		[CanBeNull]
		private static readonly Type playerConnectionLogReceiverType;
		[CanBeNull]
		private static readonly FieldInfo playerConnectionLogReceiverInstanceField;
		[CanBeNull]
		private static readonly PropertyInfo stateProperty;

		[CanBeNull]
		private static object Instance
		{
			get
			{
				if(playerConnectionLogReceiverInstanceField == null)
				{
					return null;
				}

				return playerConnectionLogReceiverInstanceField.GetValue(null);
			}
		}

		public static ConnectionState State
		{
			get
			{
				if(stateProperty == null)
				{
					return default(ConnectionState);
				}

				var instance = Instance;
				if(instance == null)
				{
					return default(ConnectionState);
				}

				return (ConnectionState)(int)stateProperty.GetValue(instance);
			}

			set
			{
				if(stateProperty == null)
				{
					return;
				}

				var instance = Instance;
				if(instance == null)
				{
					return;
				}

				stateProperty.SetValue(instance, (int)value);
			}
		}

		static PlayerConnectionLogReceiver()
		{
			playerConnectionLogReceiverType = typeof(EditorWindow).Assembly.GetType("UnityEditor.PlayerConnectionLogReceiver");
			if(playerConnectionLogReceiverType == null)
			{
				#if DEV_MODE
				Debug.LogWarning("UnityEditor.PlayerConnectionLogReceiver not found.");
				#endif
				return;
			}

			playerConnectionLogReceiverInstanceField = playerConnectionLogReceiverType.GetField("instance", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
			if(playerConnectionLogReceiverInstanceField == null)
			{
				#if DEV_MODE
				Debug.LogWarning("UnityEditor.PlayerConnectionLogReceiver.instance field not found.");
				#endif
			}

			stateProperty = playerConnectionLogReceiverType.GetProperty("State", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
			if(stateProperty == null)
			{
				#if DEV_MODE
				Debug.LogWarning("UnityEditor.PlayerConnectionLogReceiver.State property not found.");
				#endif
			}
		}

		public static bool PlayerLoggingEnabled()
		{
			return State != ConnectionState.Disconnected;
		}

		public static bool FullLoggingEnabled()
		{
			return State == ConnectionState.FullLog;
		}
	}
}