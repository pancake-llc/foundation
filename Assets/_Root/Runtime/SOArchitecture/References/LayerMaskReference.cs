using UnityEngine;

namespace Pancake.SOA
{
	[System.Serializable]
	public sealed class LayerMaskReference : BaseReference<LayerMask, LayerMaskVariable>
	{
	    public LayerMaskReference() : base() { }
	    public LayerMaskReference(LayerMask value) : base(value) { }
	}
}