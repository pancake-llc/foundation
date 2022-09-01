using System;
using UnityEngine;
using UnityEditor;
using Object = UnityEngine.Object;

namespace Pancake.Debugging.Console
{
	[Serializable]
	public class Log
	{
		public string text;
		public string textUnformatted;
		public string stackTrace;
		public LogType type;
		public string[] channels;
		public int hash;
		public int timeMonth;
		public int timeDay;
		public int timeHour;
		public int timeMinute;
		public int timeSecond;
		public int timeMillisecond;
		public Object context;
		public bool isCompileErrorOrWarning;

		[NonSerialized]
		private GUIContent contextIcon;
		[NonSerialized]
		private bool contextIconLoaded;

		[NonSerialized]
		public GUIContent listViewLabel = null;

		[NonSerialized]
		public bool appearAnimationDone;

		public GUIContent ContextIcon
		{
			get
			{
				if(!contextIconLoaded)
				{
					contextIconLoaded = true;
					contextIcon = context == null ? GUIContent.none : new GUIContent("", EditorGUIUtility.ObjectContent(context, context.GetType()).image, context.name);
				}
				return contextIcon;
			}
		}

		public double AgeInSeconds
		{
			get
			{
				var now = DateTime.UtcNow;
				var timestamp = new DateTime(now.Year, timeMonth, timeDay, timeHour, timeMinute, timeSecond, timeMillisecond);
				return (now - timestamp).TotalSeconds;
			}
		}

		public Log() { }

		public Log(string text, string stackTrace, LogType type, Object context, bool isCompileErrorOrWarning)
		{
			if(text == null)
			{
				text = "";
			}

			this.text = text;
			this.stackTrace = stackTrace;
			this.type = type;
			this.context = context;
			this.isCompileErrorOrWarning = isCompileErrorOrWarning;

			var now = DateTime.UtcNow;
			timeMonth = now.Month;
			timeDay = now.Day;
			timeHour = now.Hour;
			timeMinute = now.Minute;
			timeSecond = now.Second;
			timeMillisecond = now.Millisecond;

			textUnformatted = text;

			int length = text.Length;
			if(length > 0)
			{
				int from = text.IndexOf('<');
				if(from != -1)
				{
					for(int to = textUnformatted.IndexOf('>', from + 1); to != -1; to = textUnformatted.IndexOf('>', from + 1))
					{
						textUnformatted = textUnformatted.Substring(0, from) + textUnformatted.Substring(to + 1);
						from = textUnformatted.IndexOf('<');
						if(from == -1)
						{
							break;
						}
					}
					length = textUnformatted.Length;
				}

				unchecked
				{
					const int LargePrimeNumber = 486187739;
					hash = textUnformatted.GetHashCode() + LargePrimeNumber * (int)type;
				}

				if(Debug.formatter.StartsWithChannelPrefix(textUnformatted))
				{
					from = 0;
					for(int to = textUnformatted.IndexOf(']', 1); to != -1; to = textUnformatted.IndexOf(']', from + 1))
					{
						string channel = textUnformatted.Substring(from + 1, to - from - 1);

						if(channels == null)
						{
							channels = new string[] { channel };
						}
						else
						{
							switch(channels.Length)
							{
								case 1:
									channels = new string[] { channels[0], channel };
									break;
								case 2:
									channels = new string[] { channels[0], channels[1], channel };
									break;
								case 3:
									channels = new string[] { channels[0], channels[1], channels[2], channel };
									break;
							}
						}

						from = to + 1;

						if(length <= from || textUnformatted[from] != '[')
						{
							break;
						}
					}
				}
			}
		}

		public Log(string textUnformatted, string textFormatted, string stackTrace, LogType type, Object context, bool isCompileErrorOrWarning)
		{
			if(textFormatted == null)
			{
				textFormatted = "";
			}
			if(textUnformatted == null)
			{
				textUnformatted = "";
			}

			text = textFormatted;
			this.stackTrace = stackTrace;
			this.type = type;
			this.context = context;
			this.isCompileErrorOrWarning = isCompileErrorOrWarning;

			var now = DateTime.UtcNow;
			timeMonth = now.Month;
			timeDay = now.Day;
			timeHour = now.Hour;
			timeMinute = now.Minute;
			timeSecond = now.Second;
			timeMillisecond = now.Millisecond;

			this.textUnformatted = textUnformatted;

			int length = text.Length;
			if(length > 0)
			{
				int from = text.IndexOf('<');
				if(from != -1)
				{
					for(int to = textUnformatted.IndexOf('>', from + 1); to != -1; to = textUnformatted.IndexOf('>', from + 1))
					{
						textUnformatted = textUnformatted.Substring(0, from) + textUnformatted.Substring(to + 1);
						from = textUnformatted.IndexOf('<');
						if(from == -1)
						{
							break;
						}
					}
					length = textUnformatted.Length;
				}

				hash = textUnformatted.GetHashCode();

				if(Debug.formatter.StartsWithChannelPrefix(textUnformatted))
				{
					from = 0;
					for(int to = textUnformatted.IndexOf(']', 1); to != -1; to = textUnformatted.IndexOf(']', from + 1))
					{
						string channel = textUnformatted.Substring(from + 1, to - from - 1);

						if(channels == null)
						{
							channels = new string[] { channel };
						}
						else
						{
							switch(channels.Length)
							{
								case 1:
									channels = new string[] { channels[0], channel };
									break;
								case 2:
									channels = new string[] { channels[0], channels[1], channel };
									break;
								case 3:
									channels = new string[] { channels[0], channels[1], channels[2], channel };
									break;
							}
						}

						from = to + 1;

						if(length <= from || textUnformatted[from] != '[')
						{
							break;
						}
					}
				}
			}
		}
	}
}