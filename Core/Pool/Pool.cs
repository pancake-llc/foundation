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
        private bool _isInitialized;
        internal LinkedList<PoolMember> PoolMembers { get; private set; }

        /// <summary>
        /// Init the pool, run only oncetime
        /// prewarm the pool before use with size
        /// </summary>
        public void Init()
        {
            if (!Application.isPlaying || _isInitialized) return;

            _isInitialized = true;
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

        private PoolMember Create()
        {
            var poolMember = Instantiate(prefab, null).AddComponent<PoolMember>();
            if (dontDestroyOnLoad) DontDestroyOnLoad(poolMember);
            poolMember.Init(this, false, RegisterOnScenedUnloaded, UnregisterOnScenedUnloaded);
            return poolMember;
        }

        public void RemoveAll()
        {
            PoolMembers.Clear();
            OnSceneUnloadedEvent = null;
            _isInitialized = false;
        }

        private void OnsceneUnloaded(Scene scene) { OnSceneUnloadedEvent?.Invoke(scene.name); }
        private void UnregisterOnScenedUnloaded(Action<string> action) { OnSceneUnloadedEvent -= action; }
        private void RegisterOnScenedUnloaded(Action<string> action) { OnSceneUnloadedEvent += action; }
    }
}