using System;
using UnityEngine;

namespace Pancake
{
    [CreateAssetMenu(fileName = "const_float.asset", menuName = "Pancake/Scriptable/const float")]
    [EditorIcon("so_blue_const")]
    [Serializable]
    [Searchable]
    public class FloatConstant : ScriptableConstant<float>
    {
    }
}