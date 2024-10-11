using System;
using UnityEngine;

namespace Pancake
{
    [CreateAssetMenu(fileName = "const_string.asset", menuName = "Pancake/Scriptable/const string")]
    [EditorIcon("so_blue_const")]
    [Serializable]
    [Searchable]
    public class StringConstant : ScriptableConstant<string>
    {
    }
}