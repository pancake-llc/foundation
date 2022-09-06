using System;

namespace Needle
{
	public readonly struct GenericHyperlink : IHyperlinkCallbackReceiver
	{
		private readonly string key;
		private readonly string text;
		private readonly Action callback;

		public GenericHyperlink(string key, string text, Action callback, int priority = 0)
		{
			this.key = key;
			this.text = text;
			this.callback = callback;
			ConsoleHyperlink.RegisterClickedCallback(this, priority);
		}

		public bool OnHyperlinkClicked(string path, string line)
		{
			if (path == key)
			{
				callback?.Invoke();
				return callback != null;
			}

			return false;
		}

		public override string ToString()
		{
			return "<a href=\"" + key + "\">" + text + "</a>";
		}
	}
}