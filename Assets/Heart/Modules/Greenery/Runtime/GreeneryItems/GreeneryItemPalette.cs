namespace Pancake.Greenery
{
    using System.Collections.Generic;
    using UnityEngine;

    [CreateAssetMenu(fileName = "GreeneryItemPalette", menuName = "Pancake/Greenery/Greenery Item Palette", order = 0)]
    public class GreeneryItemPalette : ScriptableObject
    {
        public List<GreeneryItem> greeneryItems;
    }
}