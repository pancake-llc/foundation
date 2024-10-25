using UnityEngine.UI;

namespace Sisus.ComponentNames.Editor
{
	internal sealed class ToggleHeader : CustomHeader<Toggle>
	{
		public override int ExecutionOrder => CustomHeaderExecutionOrders.BuiltIn;
		public override Suffix GetSuffix(Toggle target) => target.isOn ? "On" : "Off";
	}
}