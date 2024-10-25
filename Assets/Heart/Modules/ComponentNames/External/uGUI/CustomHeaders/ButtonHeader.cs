using UnityEngine.UI;

namespace Sisus.ComponentNames.Editor
{
	internal sealed class ButtonHeader : CustomHeader<Button>
	{
		public override int ExecutionOrder => CustomHeaderExecutionOrders.BuiltIn;

		public override Suffix GetSuffix(Button target) => target.onClick.GetPersistentEventCount() switch
		{
			0 => "Listeners: None",
			1 => FirstPersistentListenerToString(target),
			var count => FirstPersistentListenerToString(target) + " + " + (count - 1) + " More"
		};

		private static string FirstPersistentListenerToString(Button target)
			=> target.onClick.GetPersistentMethodName(0) is not { Length: > 0 } methodName
			 ? "Listener: No Function"
			 : "⇒ " + methodName;
	}
}