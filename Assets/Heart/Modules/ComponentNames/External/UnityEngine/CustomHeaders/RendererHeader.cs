using UnityEngine;

namespace Sisus.ComponentNames.Editor
{
	internal sealed class RendererHeader : CustomHeader<Renderer>
	{
		public override int ExecutionOrder => CustomHeaderExecutionOrders.BuiltInFallback;
		public override Suffix GetSuffix(Renderer target) => target.sharedMaterial ? target.sharedMaterial.name : Suffix.Default;
	}
}