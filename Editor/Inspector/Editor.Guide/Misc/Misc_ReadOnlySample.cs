namespace Pancake.Editor.Guide
{
    using UnityEngine;

    public class Misc_ReadOnlySample : ScriptableObject
    {
        [ReadOnly] public Vector3 vec;
    }
}