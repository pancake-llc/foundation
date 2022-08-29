using System;
using UnityEngine;
using UnityEngine.UI;

namespace Pancake.UIQuery
{
    [RequireComponent(typeof(ScrollRect))]
    public class ListAdditionalInfo : MonoBehaviour
    {
        public SpecialSpacing[] specialSpacings;
    }

    [Serializable]
    public class SpecialSpacing
    {
        public string item1;
        public string item2;
        public float spacing;
    }
}