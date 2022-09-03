using UnityEngine;

namespace Pancake.SOA
{
	[System.Serializable]
	public sealed class AnimationCurveReference : BaseReference<AnimationCurve, AnimationCurveVariable>
	{
	    public AnimationCurveReference() : base() { }
	    public AnimationCurveReference(AnimationCurve value) : base(value) { }
	}
}