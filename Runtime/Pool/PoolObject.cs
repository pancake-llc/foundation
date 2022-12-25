using System;
using UnityEngine;

namespace Pancake
{
    [Serializable]
    public sealed class PoolObject
    {
        [field: SerializeField] public GameObject Prefab { get; }
        [field: SerializeField] public int Size { get; }
    }
}