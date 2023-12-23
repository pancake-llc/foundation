using System;
using Pancake.Apex;

namespace Pancake
{
    using UnityEngine;

    /// <summary>
    /// Implements a pool for GameObject
    /// </summary>
    [Searchable]
    [EditorIcon("scriptable_pool")]
    [CreateAssetMenu(fileName = "GameOjectPool", menuName = "Pancake/Misc/Game Object Pool")]
    public class GameObjectPool : ScriptablePool<GameObject>
    {
        [SerializeField] private GameObjectFactory factory;

        [Message("If rootIsUI is true the pool root object will be generated with RectTransform")] [SerializeField]
        private bool rootIsUI;

        private Transform _root;
        private Transform _parent;

        private Transform Root
        {
            get
            {
                if (_root != null) return _root;
                if (rootIsUI)
                {
                    var r = new GameObject(name).AddComponent<RectTransform>();
                    _root = r;
                    r.FillWithParent(_parent);
                }
                else
                {
                    _root = new GameObject(name).transform;
                    _root.SetParent(_parent);
                }

                return _root;
            }
        }

        public override IFactory<GameObject> Factory { get => factory; set => factory = value as GameObjectFactory; }

        /// <summary>
        /// Parents the pool root transform to <paramref name="t"/>.
        /// </summary>
        /// <param name="t">The Transform to which this pool should become a child.</param>
        /// <param name="resetPosition"></param>
        /// <remarks>NOTE: Setting the parent to an object marked DontDestroyOnLoad will effectively make this pool DontDestroyOnLoad.<br/>
        /// This can only be circumvented by manually destroying the object or its parent or by setting the parent to an object not marked DontDestroyOnLoad.</remarks>
        public void SetParent(Transform t, bool resetPosition = false)
        {
            _parent = t;
            Root.SetParent(_parent);
            if (resetPosition)
            {
                Root.localPosition = Vector3.zero;
                Root.localScale = Vector3.one;
            }
        }

        public override GameObject Request()
        {
            var member = base.Request();
            member.gameObject.SetActive(true);
            return member;
        }

        public override void Return(GameObject member)
        {
            member.transform.SetParent(Root);
            member.gameObject.SetActive(false);
            base.Return(member);
        }

        protected override GameObject Create()
        {
            var member = base.Create();
            member.transform.SetParent(Root);
            member.gameObject.SetActive(false);
            return member;
        }

        protected override void CleanPool()
        {
            try
            {
                foreach (var instance in container)
                {
                    if (instance != null) instance.Destroy();
                }
            }
            catch (Exception)
            {
               // ignored
            }
        }

        public override void OnDisable()
        {
            base.OnDisable();
            if (_root != null) _root.gameObject.Destroy();
        }
    }
}