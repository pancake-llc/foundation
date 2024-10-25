using UnityEngine.UI;

namespace Sisus.ComponentNames.Editor
{
	internal sealed class ImageHeader : CustomHeader<Image>
	{
		public override int ExecutionOrder => CustomHeaderExecutionOrders.BuiltIn;
		public override Suffix GetSuffix(Image target) => target.sprite ? target.sprite.name : null;
	}
}