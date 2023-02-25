using System;
using UnityEngine;

namespace Pancake
{
    [Serializable]
    public sealed class PoolObject
    {
        [SerializeField] private GameObject prefab;
        [SerializeField] private int size;

        public GameObject Prefab => prefab;

        public int Size => size;
    }
}