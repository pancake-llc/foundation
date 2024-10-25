using UnityEngine;

namespace Sisus.ComponentNames.Editor
{
	internal sealed class LightHeader : CustomHeader<Light>
	{
		public override int ExecutionOrder => CustomHeaderExecutionOrders.BuiltIn;
		public override Name GetName(Light target) => target.type + " Light";
		public override Suffix GetSuffix(Light target) => Suffix.None; //"<color=#" + ColorUtility.ToHtmlStringRGB(target.color) + ">✸</color>";
		public override Tooltip GetTooltip(Light target) => target.type switch
		{
			LightType.Spot => "A light that’s located at a point in the Scene and emits light in a cone shape.",
			LightType.Directional => "A Light that’s located infinitely far away and emits light in one direction only.",
			LightType.Point => "A Light that’s located at a point in the Scene and emits light in all directions equally.",
			LightType.Disc => "A Light that’s defined by a disc in the Scene, and emits light in all directions uniformly across its surface area but only from one side of the disc.",
			LightType.Pyramid => "A Light that’s defined by pyramid in the Scene, and emits light in all directions uniformly across its surface area but only from one side of the pyramid.",
			LightType.Box => "A Light that’s defined by a box in the Scene, and emits light in all directions uniformly across its surface area but only from one side of the box.",
			LightType.Tube => "A Light that’s defined by a tube in the Scene, and emits light in all directions uniformly across its surface area but only from one side of the tube.",
			_ => "A Light that’s defined by a rectangle in the Scene, and emits light in all directions uniformly across its surface area but only from one side of the rectangle."
		};
	}
}