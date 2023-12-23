using UnityEngine;

namespace Pancake.MobileInput
{
    [System.Serializable]
    public class PickableSelected
    {
        public Transform Selected { get; set; }

        public bool IsDoubleClick { get; set; }

        public bool IsLongTap { get; set; }
    }
}