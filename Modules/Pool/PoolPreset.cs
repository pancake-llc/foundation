using System.Collections.Generic;
using UnityEngine;

namespace Pancake
{
    [CreateAssetMenu(fileName = "PoolPreset", menuName = "Pancake/Create Pool Preset", order = 0)]
    public class PoolPreset : ScriptableObject
    {
        [SerializeField, TextArea] private string developerDescription;
        [SerializeField] private string poolName;
        [SerializeField] private List<PoolObject> poolObjects = new List<PoolObject>(256);

        public IReadOnlyList<PoolObject> PoolObjects => poolObjects;

        public string GetName => poolName;
    }
}