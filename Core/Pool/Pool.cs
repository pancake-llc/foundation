using System;
using System.Collections.Generic;
using Pancake.Attribute;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Pancake
{
    [EditorIcon("script_pool")]
    [Searchable]
    [CreateAssetMenu(fileName = "Pool", menuName = "Pancake/Scriptable/Pool")]
    public class Pool : ScriptableObject
    {
        [SerializeField, TextArea(3, 6)] private string developerDescription;
        [SerializeField] private GameObject prefab;
        [SerializeField] private int size;
        [SerializeField] private bool dontDestroyOnLoad;
        
        private event Action<string> OnSceneUnloadedEvent;
        private bool _initialize;
        internal LinkedList<PoolMember> PoolMembers { get; private set; }

        private void OnEnable() { Reset(); }

        private void Awake() { hideFlags = HideFlags.DontUnloadUnusedAsset; }

        /// <summary>
        /// Init the pool once time
        /// </summary>
        public void Init()
        {
            if (!Application.isPlaying || _initialize) return;
            _initialize = true;
            PoolMembers = new LinkedList<PoolMember>();
            for (var i = 0; i < size; i++)
            {
                PoolMembers.AddLast(Create());
            }

            SceneManager.sceneUnloaded -= OnsceneUnloaded;
            SceneManager.sceneUnloaded += OnsceneUnloaded;
        }

        public PoolMember Get()
        {
            if (PoolMembers.Count > 0)
            {
                var member = PoolMembers.First.Value;
                PoolMembers.RemoveFirst();
                member.SetContext(SceneManager.GetActiveScene().name);
                return member;
            }

            return Create();
        }

        public void Reset()
        {
            if (PoolMembers != null)
            {
                foreach (var member in PoolMembers)
                {
                    member.Return();
                }

                PoolMembers = null;
            }

            OnSceneUnloadedEvent = null;
            _initialize = false;
        }

        private PoolMember Create()
        {
            var poolMember = Instantiate(prefab, null).AddComponent<PoolMember>();
            if (dontDestroyOnLoad) DontDestroyOnLoad(poolMember);
            poolMember.Init(this, false, RegisterOnScenedUnloaded, UnregisterOnScenedUnloaded);
            return poolMember;
        }

        private void OnsceneUnloaded(Scene scene) { OnSceneUnloadedEvent?.Invoke(scene.name); }
        private void UnregisterOnScenedUnloaded(Action<string> action) { OnSceneUnloadedEvent -= action; }
        private void RegisterOnScenedUnloaded(Action<string> action) { OnSceneUnloadedEvent += action; }
    }
}