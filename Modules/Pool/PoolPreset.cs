using System.Collections.Generic;
using Pancake.Attribute;
using UnityEngine;

namespace Pancake
{
    [CreateAssetMenu(fileName = "PoolPreset", menuName = "Pancake/Create Pool Preset", order = 0)]
    [Searchable]
    public class PoolPreset : ScriptableObject
    {
        [SerializeField, TextArea] private string developerDescription;
        [SerializeField] private string poolName;
        [SerializeField] private List<PoolObject> poolObjects = new List<PoolObject>(256);

        public IReadOnlyList<PoolObject> PoolObjects => poolObjects;

        public string GetName => poolName;
    }
}