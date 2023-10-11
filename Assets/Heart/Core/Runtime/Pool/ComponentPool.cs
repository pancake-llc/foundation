using UnityEngine;

namespace Pancake
{
    /// <summary>
    /// Implements a Pool for Component types.
    /// </summary>
    /// <typeparam name="T">Specifies the component to pool.</typeparam>
    public abstract class ComponentPool<T> : ScriptablePool<T> where T : Component
    {
        private Transform _root;
        private Transform _parent;

        private Transform Root
        {
            get
            {
                if (_root == null)
                {
                    _root = new GameObject(name).transform;
                    _root.SetParent(_parent);
                }

                return _root;
            }
        }

        public override IFactory<T> Factory { get; set; }

        /// <summary>
        /// Parents the pool root transform to <paramref name="t"/>.
        /// </summary>
        /// <param name="t">The Transform to which this pool should become a child.</param>
        /// <remarks>NOTE: Setting the parent to an object marked DontDestroyOnLoad will effectively make this pool DontDestroyOnLoad.<br/>
        /// This can only be circumvented by manually destroying the object or its parent or by setting the parent to an object not marked DontDestroyOnLoad.</remarks>
        public void SetParent(Transform t)
        {
            _parent = t;
            Root.SetParent(_parent);
        }

        public override T Request()
        {
            var member = base.Request();
            member.gameObject.SetActive(true);
            return member;
        }

        public override void Return(T member)
        {
            member.transform.SetParent(Root);
            member.gameObject.SetActive(false);
            base.Return(member);
        }

        protected override T Create()
        {
            var member = base.Create();
            member.transform.SetParent(Root);
            member.gameObject.SetActive(false);
            return member;
        }

        public override void OnDisable()
        {
            base.OnDisable();
            if (_root != null) _root.gameObject.Destroy();
        }
    }
}