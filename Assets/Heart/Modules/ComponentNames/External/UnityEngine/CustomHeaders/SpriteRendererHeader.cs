using UnityEngine;

namespace Sisus.ComponentNames.Editor
{
	internal sealed class SpriteRendererHeader : CustomHeader<SpriteRenderer>
	{
		public override int ExecutionOrder => CustomHeaderExecutionOrders.BuiltIn;
		public override Suffix GetSuffix(SpriteRenderer target) => target.sprite ? target.sprite.name : Suffix.Default;
	}

	internal sealed class AudioSourceHeader : CustomHeader<AudioSource>
	{
		public override int ExecutionOrder => CustomHeaderExecutionOrders.BuiltIn;
		public override Suffix GetSuffix(AudioSource target) => target.clip ? target.clip.name : Suffix.None;
	}
}