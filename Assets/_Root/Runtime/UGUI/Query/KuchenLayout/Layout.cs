using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Pancake.UIQuery
{
    public class Layout<T> : IMappedObjectList where T : IMappedObject, new()
    {
        public T Original => original;
        public T[] Elements => elements.ToArray();
        public IMappedObject[] MappedObjects => new[] { (IMappedObject) original };

        private readonly T original;
        private readonly ILayouter layouter;
        private List<T> elements;
        private readonly List<T> cachedElements;

        public Layout(T original, ILayouter layouter = null)
        {
            if (layouter == null) layouter = new NoneLayouter();

            this.original = original;
            this.layouter = layouter;
            this.elements = new List<T>();
            this.cachedElements = new List<T>();

            this.original.Mapper.Get().SetActive(false);
        }

        public LayoutEditor Edit(EditMode editMode = EditMode.Clear)
        {
            var inactiveMarked = new HashSet<GameObject>();
            if (editMode == EditMode.Clear)
            {
                // Reverseして頭から詰めていくことで、同じ場所に同じ要素を表示した場合に同じGameObjectが使用されるようになる
                foreach (var element in Elements.Reverse())
                {
                    var gameObject = element.Mapper.Get();
                    gameObject.GetComponent<LayoutElement>().Deactivate();
                    inactiveMarked.Add(gameObject);
                    cachedElements.Insert(0, element);
                }
                elements.Clear();
            }
            return new LayoutEditor(this, inactiveMarked);
        }

        public void Clear(bool purgeCache = false)
        {
            foreach (var element in Elements)
            {
                var gameObject = element.Mapper.Get();
                gameObject.GetComponent<LayoutElement>().Deactivate();
                gameObject.SetActive(false);
                cachedElements.Add(element);
            }
            elements.Clear();

            if (purgeCache)
            {
                foreach (var element in cachedElements)
                {
                    Object.Destroy(element.Mapper.Get());
                }
                cachedElements.Clear();
            }
        }

        public class LayoutEditor : IDisposable
        {
            public List<T> Elements { get; }
            private readonly Layout<T> parent;
            private readonly HashSet<GameObject> inactiveMarked;

            public LayoutEditor(Layout<T> parent, HashSet<GameObject> inactiveMarked)
            {
                this.parent = parent;
                this.inactiveMarked = inactiveMarked;
                Elements = parent.elements.ToList();
            }

            public T Create()
            {
                T newObject;
                LayoutElement layoutElement;
                if (parent.cachedElements.Count > 0)
                {
                    newObject = parent.cachedElements[0];
                    parent.cachedElements.RemoveAt(0);
                    layoutElement = newObject.Mapper.Get().GetComponent<LayoutElement>();
                }
                else
                {
                    newObject = parent.original.Duplicate();
                    layoutElement = newObject.Mapper.Get().AddComponent<LayoutElement>();
                }

                var newGameObject = newObject.Mapper.Get();
                if (inactiveMarked.Contains(newGameObject))
                {
                    inactiveMarked.Remove(newGameObject);
                }
                else
                {
                    newGameObject.SetActive(true);
                }

                Elements.Add(newObject);
                if (newObject is IReusableMappedObject reusableMappedObject)
                {
                    reusableMappedObject.Activate();
                    layoutElement.ReusableMappedObject = reusableMappedObject;
                }
                return newObject;
            }

            public void Dispose()
            {
                parent.elements = Elements;
                var mappers = parent.Elements.Select(x => x.Mapper).ToArray();
                foreach (var m in mappers) m.Get().transform.SetAsLastSibling();
                parent.layouter.Layout(parent.original.Mapper, mappers);
                foreach (var a in inactiveMarked) a.SetActive(false);
                inactiveMarked.Clear();
            }
        }
    }

    public class LayoutElement : MonoBehaviour
    {
        public IReusableMappedObject ReusableMappedObject { get; set; }

        public void OnDestroy()
        {
            Deactivate();
        }

        public void Deactivate()
        {
            ReusableMappedObject?.Deactivate();
            ReusableMappedObject = null;
        }
    }
}

namespace Pancake.UIQuery
{
    public interface ILayouter
    {
        void Layout(IMapper original, IMapper[] elements);
    }
}
