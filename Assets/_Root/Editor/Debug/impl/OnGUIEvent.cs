using UnityEngine;
using UnityEngine.Events;

namespace Pancake.Debugging.Console
{
	public class OnGUIEvent : UnityEvent<Rect, int, int, int>
	{
		public readonly bool IsEmpty;

		public OnGUIEvent()
		{
			IsEmpty = true;
		}

		/// <summary>
		/// OnGUI callback with parameters Rect drawRect, int visibleColumnIndex, int rowIndex, int clickCount.
		/// </summary>
		public OnGUIEvent(UnityAction<Rect, int, int, int> action) : base()
		{
			AddListener(action);
			IsEmpty = action == null;
		}
	}
}