using System;
using UnityEngine;

namespace Pancake.StatModifier
{
    [Searchable]
    [Serializable]
    [CreateAssetMenu(menuName = "Pancake/Misc/[StatModifier] Base Stat", order = 11)]
    public class BaseStat : ScriptableObject
    {
        public float baseValue;
        public StringConstant statType;
    }
}