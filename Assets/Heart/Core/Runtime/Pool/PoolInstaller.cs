using UnityEngine;

namespace Pancake
{
    [DefaultExecutionOrder(-9999), DisallowMultipleComponent]
    internal sealed class PoolInstaller : MonoBehaviour
    {
        [SerializeField] private PoolContainer[] pools;

        private void Awake()
        {
            for (var i = 0; i < pools.Length; i++)
                pools[i].Populate();
        }

        [System.Serializable]
        private struct PoolContainer
        {
            [SerializeField] private GameObject prefab;
            [SerializeField, Min(1)] private int size;

            public void Populate() { prefab.Populate(size); }
        }
    }
}