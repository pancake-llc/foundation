using UnityEngine;

namespace Pancake.SOA
{
	[CreateAssetMenu(
	    fileName = "ColorCollection.asset",
	    menuName = SOArchitecture_Utility.COLLECTION_SUBMENU + "Structs/Color",
	    order = 120)]
	public class ColorCollection : Collection<Color>
	{
	}
}