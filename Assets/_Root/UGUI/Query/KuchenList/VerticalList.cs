using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace Pancake.UIQuery
{
    public class VerticalList<T1> : IKuchenList where T1 : IMappedObject, new()
    {
        private readonly ScrollRect _scrollRect;
        private readonly T1 _original1;
        private readonly (string Name, float Size)[] _originalInfoCache;
        private List<UIFactory<T1>> _contents = new List<UIFactory<T1>>();
        private readonly List<(float, float)> _contentPositions = new List<(float, float)>();
        private readonly Dictionary<int, IMappedObject> _createdObjects = new Dictionary<int, IMappedObject>();
        private readonly Dictionary<Type, List<IMappedObject>> _cachedObjects = new Dictionary<Type, List<IMappedObject>>();
        private readonly RectTransform _viewportRectTransformCache;
        private readonly ListAdditionalInfo _additionalInfo;
        public float Spacing { get; private set; }
        public int SpareElement { get; private set; }
        public IReadOnlyDictionary<int, IMappedObject> CreatedObjects => _createdObjects;
        public int ContentsCount => _contents.Count;
        public ScrollRect ScrollRect => _scrollRect;
        public RectTransform ContentRectTransform => _scrollRect.content;
        public Action<int, IMappedObject> OnCreateObject { get; set; }
        public IMappedObject[] MappedObjects => new[] {(IMappedObject) _original1,};

        public T1 Original1 => _original1;

        private Margin _margin = new Margin();
        public IReadonlyMargin Margin => _margin;

        private readonly HashSet<GameObject> _inactiveMarked = new HashSet<GameObject>();

        public float NormalizedPosition { get => _scrollRect.verticalNormalizedPosition; set => _scrollRect.verticalNormalizedPosition = value; }

        public VerticalList(ScrollRect scrollRect, T1 original1)
        {
            this._scrollRect = scrollRect;

            _originalInfoCache = new (string Name, float Size)[1];

            this._original1 = original1;
            this._original1.Mapper.Get().SetActive(false);
            _cachedObjects.Add(typeof(T1), new List<IMappedObject>());
            _originalInfoCache[0] = (original1.Mapper.Get().name, original1.Mapper.Get<RectTransform>().rect.height);

            var kuchenList = this._scrollRect.gameObject.AddComponent<KuchenList>();
            kuchenList.List = new ListOperator(this);

            var viewport = scrollRect.viewport;
            _viewportRectTransformCache = viewport != null ? viewport : scrollRect.GetComponent<RectTransform>();

            _additionalInfo = scrollRect.GetComponent<ListAdditionalInfo>();

            var verticalLayoutGroup = scrollRect.content.GetComponent<VerticalLayoutGroup>();
            if (verticalLayoutGroup != null)
            {
                verticalLayoutGroup.enabled = false;
                Spacing = verticalLayoutGroup.spacing;
                _margin = new Margin {Top = verticalLayoutGroup.padding.top, Bottom = verticalLayoutGroup.padding.bottom};
            }

            var contentSizeFitter = scrollRect.content.GetComponent<ContentSizeFitter>();
            if (contentSizeFitter != null)
            {
                contentSizeFitter.enabled = false;
            }
        }

        private class ListOperator : IKuchenListMonoBehaviourBridge
        {
            private readonly VerticalList<T1> _list;

            public ListOperator(VerticalList<T1> list) { this._list = list; }

            public void DeactivateAll() { _list.DeactivateAll(); }

            public void UpdateView() { _list.UpdateView(); }
        }

        private void DeactivateAll()
        {
            foreach (var item in _createdObjects.Values)
            {
                if (item is IReusableMappedObject reusable) reusable.Deactivate();
            }

            _createdObjects.Clear();
        }

        // RectTransformUtility.CalculateRelativeRectTransformBoundsを使うと、
        // inactiveMarkedの分だけズレてしまうので自前実装
        private Bounds CalculateRelativeRectTransformBounds(Transform root, Transform child)
        {
            var componentsInChildren = new List<RectTransform>();
            componentsInChildren.Add(child.GetComponent<RectTransform>());
            foreach (Transform a in child)
            {
                if (_inactiveMarked.Contains(a.gameObject)) continue;
                componentsInChildren.Add(a.GetComponent<RectTransform>());
            }

            if ((uint) componentsInChildren.Count <= 0U)
                return new Bounds(Vector3.zero, Vector3.zero);
            var vector31 = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
            var vector32 = new Vector3(float.MinValue, float.MinValue, float.MinValue);
            var worldToLocalMatrix = root.worldToLocalMatrix;
            var index1 = 0;
            for (var length = componentsInChildren.Count; index1 < length; ++index1)
            {
                componentsInChildren[index1].GetWorldCorners(KuchenListInternal.ReuseCorners);
                for (var index2 = 0; index2 < 4; ++index2)
                {
                    var lhs = worldToLocalMatrix.MultiplyPoint3x4(KuchenListInternal.ReuseCorners[index2]);
                    vector31 = Vector3.Min(lhs, vector31);
                    vector32 = Vector3.Max(lhs, vector32);
                }
            }

            var rectTransformBounds = new Bounds(vector31, Vector3.zero);
            rectTransformBounds.Encapsulate(vector32);
            return rectTransformBounds;
        }

        private void UpdateView()
        {
            var displayRect = _viewportRectTransformCache.rect;
            var contentRect = CalculateRelativeRectTransformBounds(_viewportRectTransformCache, _scrollRect.content);
            var start = contentRect.max.y - displayRect.max.y;
            var displayRectHeight = displayRect.height;
            var end = start + displayRectHeight;

            var displayMinIndex = int.MaxValue;
            var displayMaxIndex = int.MinValue;
            for (var i = 0; i < _contentPositions.Count; ++i)
            {
                if (start > _contentPositions[i].Item1) continue;
                if (_contentPositions[i].Item1 > end) break;
                displayMinIndex = Mathf.Min(displayMinIndex, i);
                displayMaxIndex = Mathf.Max(displayMaxIndex, i);
            }

            if (displayMinIndex == int.MaxValue)
            {
                displayMinIndex = _contentPositions.Count - 1;
                displayMaxIndex = _contentPositions.Count - 1;
            }

            displayMinIndex = Mathf.Max(displayMinIndex - 1 - SpareElement, 0);
            displayMaxIndex = Mathf.Min(displayMaxIndex + SpareElement, _contents.Count - 1);

            var removedList = new List<int>();
            foreach (var tmp in _createdObjects)
            {
                var index = tmp.Key;
                var map = tmp.Value;
                if (displayMinIndex <= index && index <= displayMaxIndex) continue;

                CollectObject(map);
                removedList.Add(index);
            }

            foreach (var removed in removedList)
            {
                _createdObjects.Remove(removed);
            }

            for (var i = displayMinIndex; i <= displayMaxIndex; ++i)
            {
                if (_createdObjects.ContainsKey(i)) continue;

                RectTransform newObject = null;
                IMappedObject newMappedObject = null;
                var content = _contents[i];
                if (content.Callback1 != null) (newObject, newMappedObject) = GetOrCreateNewObject(_original1, content.Callback1, _contentPositions[i].Item1);
                if (content.Spacer != null) continue;
                if (newObject == null) throw new Exception($"newObject == null");
                _createdObjects[i] = newMappedObject;
                OnCreateObject?.Invoke(i, newMappedObject);
            }

            foreach (var a in _inactiveMarked) a.SetActive(false);
            _inactiveMarked.Clear();
        }

        private void UpdateListContents()
        {
            // clear elements
            var isFirst = _createdObjects.Values.Count == 0;
            foreach (var map in _createdObjects.Values)
            {
                CollectObject(map);
            }

            _createdObjects.Clear();
            _contentPositions.Clear();

            // create elements
            var calcPosition = Margin.Top;
            var prevElementName = "";
            var elementName = "";
            var specialSpacings = (_additionalInfo != null && _additionalInfo.specialSpacings != null) ? _additionalInfo.specialSpacings : new SpecialSpacing[] { };
            for (var i = 0; i < _contents.Count; ++i)
            {
                var content = _contents[i];
                var elementSize = 0f;

                if (content.Callback1 != null)
                {
                    elementName = _originalInfoCache[0].Name;
                    elementSize = _originalInfoCache[0].Size;
                }

                if (content.Spacer != null)
                {
                    elementName = "";
                    elementSize = content.Spacer.Size;
                }

                float? spacing = null;
                var specialSpacing = specialSpacings.FirstOrDefault(x => x.item1 == prevElementName && x.item2 == elementName);
                if (specialSpacing != null) spacing = specialSpacing.spacing;
                if (spacing == null && i != 0) spacing = Spacing;

                calcPosition += spacing ?? 0f;
                _contentPositions.Add((calcPosition, elementSize));
                calcPosition += elementSize;

                prevElementName = elementName;
            }

            calcPosition += Margin.Bottom;

            // calc content size
            var c = _scrollRect.content;
            var s = c.sizeDelta;
            c.sizeDelta = new Vector2(s.x, calcPosition);

            var anchoredPosition = c.anchoredPosition;
            if (isFirst)
            {
                var scrollRectSizeDeltaY = _scrollRect.GetComponent<RectTransform>().rect.y;
                if (c.pivot.y > 1f - 0.0001f) c.anchoredPosition = new Vector2(anchoredPosition.x, -scrollRectSizeDeltaY);
                if (c.pivot.y < 0f + 0.0001f) c.anchoredPosition = new Vector2(anchoredPosition.x, scrollRectSizeDeltaY);
                _scrollRect.velocity = Vector2.zero;
            }
        }

        private void CollectObject(IMappedObject target)
        {
            if (target is IReusableMappedObject reusable) reusable.Deactivate();
            _inactiveMarked.Add(target.Mapper.Get());

            if (target is T1) _cachedObjects[typeof(T1)].Add(target);
        }

        private (RectTransform, IMappedObject) GetOrCreateNewObject<T>(T original, Action<T> contentCallback, float position) where T : IMappedObject, new()
        {
            var cache = _cachedObjects[typeof(T)];
            T newObject;
            if (cache.Count > 0)
            {
                newObject = (T) cache[0];
                cache.RemoveAt(0);
            }
            else
            {
                newObject = original.Duplicate();
            }

            var newRectTransform = newObject.Mapper.Get<RectTransform>();
            newRectTransform.SetParent(_scrollRect.content);
            var newGameObject = newObject.Mapper.Get();
            if (_inactiveMarked.Contains(newGameObject))
            {
                _inactiveMarked.Remove(newGameObject);
            }
            else
            {
                newGameObject.SetActive(true);
            }

            var p = newRectTransform.anchoredPosition;
            var r = newRectTransform.rect;
            newRectTransform.anchoredPosition = new Vector3(p.x, _scrollRect.content.sizeDelta.y / 2f - position - r.height / 2f, 0f);

            if (newObject is IReusableMappedObject reusable) reusable.Activate();
            contentCallback(newObject);

            return (newRectTransform, newObject);
        }

        public IListContentEditor<T1> Edit(EditMode editMode = EditMode.Clear) { return new ListContentEditor(this, editMode); }

        public class ListContentEditor : IListContentEditor<T1>
        {
            private readonly VerticalList<T1> _parent;
            public List<UIFactory<T1>> Contents { get; set; }
            public float Spacing { get; set; }
            public Margin Margin { get; set; }
            public int SpareElement { get; set; }

            public ListContentEditor(VerticalList<T1> parent, EditMode editMode)
            {
                this._parent = parent;
                Contents = parent._contents;
                Spacing = parent.Spacing;
                Margin = parent._margin;
                SpareElement = parent.SpareElement;

                if (editMode == EditMode.Clear) Contents.Clear();
            }

            public void Dispose()
            {
                _parent._contents = Contents;
                _parent.Spacing = Spacing;
                _parent._margin = Margin;
                _parent.SpareElement = SpareElement;
                _parent.UpdateListContents();
            }

            public void Add(Action<T1> factory) { Contents.Add(new UIFactory<T1>(factory)); }
        }

        public void DestroyCachedGameObjects()
        {
            foreach (var cachedObject in _cachedObjects)
            {
                foreach (var go in cachedObject.Value)
                {
                    Object.Destroy(go.Mapper.Get());
                }

                cachedObject.Value.Clear();
            }
        }

        public Vector2? CalcScrollPosition(int index, ScrollToType type = ScrollToType.Top, float additionalSpacing = 0f)
        {
            var c = _scrollRect.content;
            var anchoredPosition = c.anchoredPosition;
            var scrollRectSizeDeltaY = _scrollRect.GetComponent<RectTransform>().rect.y;
            var content = _contentPositions[index];
            var contentHeight = _scrollRect.content.rect.height;
            var viewportHeight = _viewportRectTransformCache.rect.height;
            if (viewportHeight > contentHeight) return null;

            if (c.pivot.y > 1f - 0.0001f)
            {
                var p = -scrollRectSizeDeltaY + content.Item1;
                var limitMin = viewportHeight / 2f;
                var limitMax = -limitMin + contentHeight;
                var top = Mathf.Clamp(p - Spacing - additionalSpacing, limitMin, limitMax);
                var bottom = Mathf.Clamp(p - viewportHeight + content.Item2 + Spacing + additionalSpacing, limitMin, limitMax);
                var center = Mathf.Clamp(p - (viewportHeight - content.Item2) / 2f, limitMin, limitMax);

                if (type == ScrollToType.Top) return new Vector2(anchoredPosition.x, top);
                else if (type == ScrollToType.Bottom) return new Vector2(anchoredPosition.x, bottom);
                else if (type == ScrollToType.Center) return new Vector2(anchoredPosition.x, center);
                else if (type == ScrollToType.Near)
                {
                    var current = c.anchoredPosition.y;
                    if (current > top) return new Vector2(anchoredPosition.x, top);
                    else if (current < bottom) return new Vector2(anchoredPosition.x, bottom);
                    return null;
                }
            }

            if (c.pivot.y < 0f + 0.0001f)
            {
                var p = scrollRectSizeDeltaY - (contentHeight - content.Item1 - content.Item2);
                var limitMax = -viewportHeight / 2f;
                var limitMin = -limitMax - contentHeight;
                var top = Mathf.Clamp(p + Spacing + additionalSpacing, limitMin, limitMax);
                var bottom = Mathf.Clamp(p + viewportHeight - content.Item2 - Spacing - additionalSpacing, limitMin, limitMax);
                var center = Mathf.Clamp(p + (viewportHeight - content.Item2) / 2f, limitMin, limitMax);

                if (type == ScrollToType.Top) return new Vector2(anchoredPosition.x, top);
                else if (type == ScrollToType.Bottom) return new Vector2(anchoredPosition.x, bottom);
                else if (type == ScrollToType.Center) return new Vector2(anchoredPosition.x, center);
                else if (type == ScrollToType.Near)
                {
                    var current = c.anchoredPosition.y;
                    if (current < top) return new Vector2(anchoredPosition.x, top);
                    else if (current > bottom) return new Vector2(anchoredPosition.x, bottom);
                    return null;
                }
            }

            return null;
        }

        public void ScrollTo(int index, ScrollToType type = ScrollToType.Top, float additionalSpacing = 0f)
        {
            var scrollPosition = CalcScrollPosition(index, type, additionalSpacing);
            if (scrollPosition != null) ContentRectTransform.anchoredPosition = scrollPosition.Value;
            _scrollRect.velocity = Vector2.zero;
        }

        public void UpdateAllElements()
        {
            foreach (var tmp in _createdObjects)
            {
                var map = tmp.Value;
                CollectObject(map);
            }

            _createdObjects.Clear();
        }

        public void UpdateElement(int index)
        {
            if (!_createdObjects.ContainsKey(index)) return;
            CollectObject(_createdObjects[index]);
            _createdObjects.Remove(index);
        }
    }

    public class VerticalList<T1, T2> : IKuchenList where T1 : IMappedObject, new() where T2 : IMappedObject, new()
    {
        private readonly ScrollRect _scrollRect;
        private readonly T1 _original1;
        private readonly T2 _original2;
        private readonly (string Name, float Size)[] _originalInfoCache;
        private List<UIFactory<T1, T2>> _contents = new List<UIFactory<T1, T2>>();
        private readonly List<(float, float)> _contentPositions = new List<(float, float)>();
        private readonly Dictionary<int, IMappedObject> _createdObjects = new Dictionary<int, IMappedObject>();
        private readonly Dictionary<Type, List<IMappedObject>> _cachedObjects = new Dictionary<Type, List<IMappedObject>>();
        private readonly RectTransform _viewportRectTransformCache;
        private readonly ListAdditionalInfo _additionalInfo;
        public float Spacing { get; private set; }
        public int SpareElement { get; private set; }
        public IReadOnlyDictionary<int, IMappedObject> CreatedObjects => _createdObjects;
        public int ContentsCount => _contents.Count;
        public ScrollRect ScrollRect => _scrollRect;
        public RectTransform ContentRectTransform => _scrollRect.content;
        public Action<int, IMappedObject> OnCreateObject { get; set; }
        public IMappedObject[] MappedObjects => new[] {(IMappedObject) _original1, (IMappedObject) _original2,};

        public T1 Original1 => _original1;
        public T2 Original2 => _original2;

        private Margin _margin = new Margin();
        public IReadonlyMargin Margin => _margin;

        private readonly HashSet<GameObject> _inactiveMarked = new HashSet<GameObject>();

        public float NormalizedPosition { get => _scrollRect.verticalNormalizedPosition; set => _scrollRect.verticalNormalizedPosition = value; }

        public VerticalList(ScrollRect scrollRect, T1 original1, T2 original2)
        {
            this._scrollRect = scrollRect;

            _originalInfoCache = new (string Name, float Size)[2];

            this._original1 = original1;
            this._original1.Mapper.Get().SetActive(false);
            _cachedObjects.Add(typeof(T1), new List<IMappedObject>());
            _originalInfoCache[0] = (original1.Mapper.Get().name, original1.Mapper.Get<RectTransform>().rect.height);

            this._original2 = original2;
            this._original2.Mapper.Get().SetActive(false);
            _cachedObjects.Add(typeof(T2), new List<IMappedObject>());
            _originalInfoCache[1] = (original2.Mapper.Get().name, original2.Mapper.Get<RectTransform>().rect.height);

            var kuchenList = this._scrollRect.gameObject.AddComponent<KuchenList>();
            kuchenList.List = new ListOperator(this);

            var viewport = scrollRect.viewport;
            _viewportRectTransformCache = viewport != null ? viewport : scrollRect.GetComponent<RectTransform>();

            _additionalInfo = scrollRect.GetComponent<ListAdditionalInfo>();

            var verticalLayoutGroup = scrollRect.content.GetComponent<VerticalLayoutGroup>();
            if (verticalLayoutGroup != null)
            {
                verticalLayoutGroup.enabled = false;
                Spacing = verticalLayoutGroup.spacing;
                _margin = new Margin {Top = verticalLayoutGroup.padding.top, Bottom = verticalLayoutGroup.padding.bottom};
            }

            var contentSizeFitter = scrollRect.content.GetComponent<ContentSizeFitter>();
            if (contentSizeFitter != null)
            {
                contentSizeFitter.enabled = false;
            }
        }

        private class ListOperator : IKuchenListMonoBehaviourBridge
        {
            private readonly VerticalList<T1, T2> _list;

            public ListOperator(VerticalList<T1, T2> list) { this._list = list; }

            public void DeactivateAll() { _list.DeactivateAll(); }

            public void UpdateView() { _list.UpdateView(); }
        }

        private void DeactivateAll()
        {
            foreach (var item in _createdObjects.Values)
            {
                if (item is IReusableMappedObject reusable) reusable.Deactivate();
            }

            _createdObjects.Clear();
        }

        // RectTransformUtility.CalculateRelativeRectTransformBoundsを使うと、
        // inactiveMarkedの分だけズレてしまうので自前実装
        private Bounds CalculateRelativeRectTransformBounds(Transform root, Transform child)
        {
            var componentsInChildren = new List<RectTransform>();
            componentsInChildren.Add(child.GetComponent<RectTransform>());
            foreach (Transform a in child)
            {
                if (_inactiveMarked.Contains(a.gameObject)) continue;
                componentsInChildren.Add(a.GetComponent<RectTransform>());
            }

            if ((uint) componentsInChildren.Count <= 0U)
                return new Bounds(Vector3.zero, Vector3.zero);
            var vector31 = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
            var vector32 = new Vector3(float.MinValue, float.MinValue, float.MinValue);
            var worldToLocalMatrix = root.worldToLocalMatrix;
            var index1 = 0;
            for (var length = componentsInChildren.Count; index1 < length; ++index1)
            {
                componentsInChildren[index1].GetWorldCorners(KuchenListInternal.ReuseCorners);
                for (var index2 = 0; index2 < 4; ++index2)
                {
                    var lhs = worldToLocalMatrix.MultiplyPoint3x4(KuchenListInternal.ReuseCorners[index2]);
                    vector31 = Vector3.Min(lhs, vector31);
                    vector32 = Vector3.Max(lhs, vector32);
                }
            }

            var rectTransformBounds = new Bounds(vector31, Vector3.zero);
            rectTransformBounds.Encapsulate(vector32);
            return rectTransformBounds;
        }

        private void UpdateView()
        {
            var displayRect = _viewportRectTransformCache.rect;
            var contentRect = CalculateRelativeRectTransformBounds(_viewportRectTransformCache, _scrollRect.content);
            var start = contentRect.max.y - displayRect.max.y;
            var displayRectHeight = displayRect.height;
            var end = start + displayRectHeight;

            var displayMinIndex = int.MaxValue;
            var displayMaxIndex = int.MinValue;
            for (var i = 0; i < _contentPositions.Count; ++i)
            {
                if (start > _contentPositions[i].Item1) continue;
                if (_contentPositions[i].Item1 > end) break;
                displayMinIndex = Mathf.Min(displayMinIndex, i);
                displayMaxIndex = Mathf.Max(displayMaxIndex, i);
            }

            if (displayMinIndex == int.MaxValue)
            {
                displayMinIndex = _contentPositions.Count - 1;
                displayMaxIndex = _contentPositions.Count - 1;
            }

            displayMinIndex = Mathf.Max(displayMinIndex - 1 - SpareElement, 0);
            displayMaxIndex = Mathf.Min(displayMaxIndex + SpareElement, _contents.Count - 1);

            var removedList = new List<int>();
            foreach (var tmp in _createdObjects)
            {
                var index = tmp.Key;
                var map = tmp.Value;
                if (displayMinIndex <= index && index <= displayMaxIndex) continue;

                CollectObject(map);
                removedList.Add(index);
            }

            foreach (var removed in removedList)
            {
                _createdObjects.Remove(removed);
            }

            for (var i = displayMinIndex; i <= displayMaxIndex; ++i)
            {
                if (_createdObjects.ContainsKey(i)) continue;

                RectTransform newObject = null;
                IMappedObject newMappedObject = null;
                var content = _contents[i];
                if (content.Callback1 != null) (newObject, newMappedObject) = GetOrCreateNewObject(_original1, content.Callback1, _contentPositions[i].Item1);
                if (content.Callback2 != null) (newObject, newMappedObject) = GetOrCreateNewObject(_original2, content.Callback2, _contentPositions[i].Item1);
                if (content.Spacer != null) continue;
                if (newObject == null) throw new Exception($"newObject == null");
                _createdObjects[i] = newMappedObject;
                OnCreateObject?.Invoke(i, newMappedObject);
            }

            foreach (var a in _inactiveMarked) a.SetActive(false);
            _inactiveMarked.Clear();
        }

        private void UpdateListContents()
        {
            // clear elements
            var isFirst = _createdObjects.Values.Count == 0;
            foreach (var map in _createdObjects.Values)
            {
                CollectObject(map);
            }

            _createdObjects.Clear();
            _contentPositions.Clear();

            // create elements
            var calcPosition = Margin.Top;
            var prevElementName = "";
            var elementName = "";
            var specialSpacings = (_additionalInfo != null && _additionalInfo.specialSpacings != null) ? _additionalInfo.specialSpacings : new SpecialSpacing[] { };
            for (var i = 0; i < _contents.Count; ++i)
            {
                var content = _contents[i];
                var elementSize = 0f;

                if (content.Callback1 != null)
                {
                    elementName = _originalInfoCache[0].Name;
                    elementSize = _originalInfoCache[0].Size;
                }

                if (content.Callback2 != null)
                {
                    elementName = _originalInfoCache[1].Name;
                    elementSize = _originalInfoCache[1].Size;
                }

                if (content.Spacer != null)
                {
                    elementName = "";
                    elementSize = content.Spacer.Size;
                }

                float? spacing = null;
                var specialSpacing = specialSpacings.FirstOrDefault(x => x.item1 == prevElementName && x.item2 == elementName);
                if (specialSpacing != null) spacing = specialSpacing.spacing;
                if (spacing == null && i != 0) spacing = Spacing;

                calcPosition += spacing ?? 0f;
                _contentPositions.Add((calcPosition, elementSize));
                calcPosition += elementSize;

                prevElementName = elementName;
            }

            calcPosition += Margin.Bottom;

            // calc content size
            var c = _scrollRect.content;
            var s = c.sizeDelta;
            c.sizeDelta = new Vector2(s.x, calcPosition);

            var anchoredPosition = c.anchoredPosition;
            if (isFirst)
            {
                var scrollRectSizeDeltaY = _scrollRect.GetComponent<RectTransform>().rect.y;
                if (c.pivot.y > 1f - 0.0001f) c.anchoredPosition = new Vector2(anchoredPosition.x, -scrollRectSizeDeltaY);
                if (c.pivot.y < 0f + 0.0001f) c.anchoredPosition = new Vector2(anchoredPosition.x, scrollRectSizeDeltaY);
                _scrollRect.velocity = Vector2.zero;
            }
        }

        private void CollectObject(IMappedObject target)
        {
            if (target is IReusableMappedObject reusable) reusable.Deactivate();
            _inactiveMarked.Add(target.Mapper.Get());

            if (target is T1) _cachedObjects[typeof(T1)].Add(target);
            if (target is T2) _cachedObjects[typeof(T2)].Add(target);
        }

        private (RectTransform, IMappedObject) GetOrCreateNewObject<T>(T original, Action<T> contentCallback, float position) where T : IMappedObject, new()
        {
            var cache = _cachedObjects[typeof(T)];
            T newObject;
            if (cache.Count > 0)
            {
                newObject = (T) cache[0];
                cache.RemoveAt(0);
            }
            else
            {
                newObject = original.Duplicate();
            }

            var newRectTransform = newObject.Mapper.Get<RectTransform>();
            newRectTransform.SetParent(_scrollRect.content);
            var newGameObject = newObject.Mapper.Get();
            if (_inactiveMarked.Contains(newGameObject))
            {
                _inactiveMarked.Remove(newGameObject);
            }
            else
            {
                newGameObject.SetActive(true);
            }

            var p = newRectTransform.anchoredPosition;
            var r = newRectTransform.rect;
            newRectTransform.anchoredPosition = new Vector3(p.x, _scrollRect.content.sizeDelta.y / 2f - position - r.height / 2f, 0f);

            if (newObject is IReusableMappedObject reusable) reusable.Activate();
            contentCallback(newObject);

            return (newRectTransform, newObject);
        }

        public IListContentEditor<T1, T2> Edit(EditMode editMode = EditMode.Clear) { return new ListContentEditor(this, editMode); }

        public class ListContentEditor : IListContentEditor<T1, T2>
        {
            private readonly VerticalList<T1, T2> _parent;
            public List<UIFactory<T1, T2>> Contents { get; set; }
            public float Spacing { get; set; }
            public Margin Margin { get; set; }
            public int SpareElement { get; set; }

            public ListContentEditor(VerticalList<T1, T2> parent, EditMode editMode)
            {
                this._parent = parent;
                Contents = parent._contents;
                Spacing = parent.Spacing;
                Margin = parent._margin;
                SpareElement = parent.SpareElement;

                if (editMode == EditMode.Clear) Contents.Clear();
            }

            public void Dispose()
            {
                _parent._contents = Contents;
                _parent.Spacing = Spacing;
                _parent._margin = Margin;
                _parent.SpareElement = SpareElement;
                _parent.UpdateListContents();
            }

            public void Add(Action<T1> factory) { Contents.Add(new UIFactory<T1, T2>(factory)); }

            public void Add(Action<T2> factory) { Contents.Add(new UIFactory<T1, T2>(factory)); }
        }

        public void DestroyCachedGameObjects()
        {
            foreach (var cachedObject in _cachedObjects)
            {
                foreach (var go in cachedObject.Value)
                {
                    Object.Destroy(go.Mapper.Get());
                }

                cachedObject.Value.Clear();
            }
        }

        public Vector2? CalcScrollPosition(int index, ScrollToType type = ScrollToType.Top, float additionalSpacing = 0f)
        {
            var c = _scrollRect.content;
            var anchoredPosition = c.anchoredPosition;
            var scrollRectSizeDeltaY = _scrollRect.GetComponent<RectTransform>().rect.y;
            var content = _contentPositions[index];
            var contentHeight = _scrollRect.content.rect.height;
            var viewportHeight = _viewportRectTransformCache.rect.height;
            if (viewportHeight > contentHeight) return null;

            if (c.pivot.y > 1f - 0.0001f)
            {
                var p = -scrollRectSizeDeltaY + content.Item1;
                var limitMin = viewportHeight / 2f;
                var limitMax = -limitMin + contentHeight;
                var top = Mathf.Clamp(p - Spacing - additionalSpacing, limitMin, limitMax);
                var bottom = Mathf.Clamp(p - viewportHeight + content.Item2 + Spacing + additionalSpacing, limitMin, limitMax);
                var center = Mathf.Clamp(p - (viewportHeight - content.Item2) / 2f, limitMin, limitMax);

                if (type == ScrollToType.Top) return new Vector2(anchoredPosition.x, top);
                else if (type == ScrollToType.Bottom) return new Vector2(anchoredPosition.x, bottom);
                else if (type == ScrollToType.Center) return new Vector2(anchoredPosition.x, center);
                else if (type == ScrollToType.Near)
                {
                    var current = c.anchoredPosition.y;
                    if (current > top) return new Vector2(anchoredPosition.x, top);
                    else if (current < bottom) return new Vector2(anchoredPosition.x, bottom);
                    return null;
                }
            }

            if (c.pivot.y < 0f + 0.0001f)
            {
                var p = scrollRectSizeDeltaY - (contentHeight - content.Item1 - content.Item2);
                var limitMax = -viewportHeight / 2f;
                var limitMin = -limitMax - contentHeight;
                var top = Mathf.Clamp(p + Spacing + additionalSpacing, limitMin, limitMax);
                var bottom = Mathf.Clamp(p + viewportHeight - content.Item2 - Spacing - additionalSpacing, limitMin, limitMax);
                var center = Mathf.Clamp(p + (viewportHeight - content.Item2) / 2f, limitMin, limitMax);

                if (type == ScrollToType.Top) return new Vector2(anchoredPosition.x, top);
                else if (type == ScrollToType.Bottom) return new Vector2(anchoredPosition.x, bottom);
                else if (type == ScrollToType.Center) return new Vector2(anchoredPosition.x, center);
                else if (type == ScrollToType.Near)
                {
                    var current = c.anchoredPosition.y;
                    if (current < top) return new Vector2(anchoredPosition.x, top);
                    else if (current > bottom) return new Vector2(anchoredPosition.x, bottom);
                    return null;
                }
            }

            return null;
        }

        public void ScrollTo(int index, ScrollToType type = ScrollToType.Top, float additionalSpacing = 0f)
        {
            var scrollPosition = CalcScrollPosition(index, type, additionalSpacing);
            if (scrollPosition != null) ContentRectTransform.anchoredPosition = scrollPosition.Value;
            _scrollRect.velocity = Vector2.zero;
        }

        public void UpdateAllElements()
        {
            foreach (var tmp in _createdObjects)
            {
                var map = tmp.Value;
                CollectObject(map);
            }

            _createdObjects.Clear();
        }

        public void UpdateElement(int index)
        {
            if (!_createdObjects.ContainsKey(index)) return;
            CollectObject(_createdObjects[index]);
            _createdObjects.Remove(index);
        }
    }

    public class VerticalList<T1, T2, T3> : IKuchenList where T1 : IMappedObject, new() where T2 : IMappedObject, new() where T3 : IMappedObject, new()
    {
        private readonly ScrollRect _scrollRect;
        private readonly T1 _original1;
        private readonly T2 _original2;
        private readonly T3 _original3;
        private readonly (string Name, float Size)[] _originalInfoCache;
        private List<UIFactory<T1, T2, T3>> _contents = new List<UIFactory<T1, T2, T3>>();
        private readonly List<(float, float)> _contentPositions = new List<(float, float)>();
        private readonly Dictionary<int, IMappedObject> _createdObjects = new Dictionary<int, IMappedObject>();
        private readonly Dictionary<Type, List<IMappedObject>> _cachedObjects = new Dictionary<Type, List<IMappedObject>>();
        private readonly RectTransform _viewportRectTransformCache;
        private readonly ListAdditionalInfo _additionalInfo;
        public float Spacing { get; private set; }
        public int SpareElement { get; private set; }
        public IReadOnlyDictionary<int, IMappedObject> CreatedObjects => _createdObjects;
        public int ContentsCount => _contents.Count;
        public ScrollRect ScrollRect => _scrollRect;
        public RectTransform ContentRectTransform => _scrollRect.content;
        public Action<int, IMappedObject> OnCreateObject { get; set; }
        public IMappedObject[] MappedObjects => new[] {(IMappedObject) _original1, (IMappedObject) _original2, (IMappedObject) _original3,};

        public T1 Original1 => _original1;
        public T2 Original2 => _original2;
        public T3 Original3 => _original3;

        private Margin _margin = new Margin();
        public IReadonlyMargin Margin => _margin;

        private readonly HashSet<GameObject> _inactiveMarked = new HashSet<GameObject>();

        public float NormalizedPosition { get => _scrollRect.verticalNormalizedPosition; set => _scrollRect.verticalNormalizedPosition = value; }

        public VerticalList(ScrollRect scrollRect, T1 original1, T2 original2, T3 original3)
        {
            this._scrollRect = scrollRect;

            _originalInfoCache = new (string Name, float Size)[3];

            this._original1 = original1;
            this._original1.Mapper.Get().SetActive(false);
            _cachedObjects.Add(typeof(T1), new List<IMappedObject>());
            _originalInfoCache[0] = (original1.Mapper.Get().name, original1.Mapper.Get<RectTransform>().rect.height);

            this._original2 = original2;
            this._original2.Mapper.Get().SetActive(false);
            _cachedObjects.Add(typeof(T2), new List<IMappedObject>());
            _originalInfoCache[1] = (original2.Mapper.Get().name, original2.Mapper.Get<RectTransform>().rect.height);

            this._original3 = original3;
            this._original3.Mapper.Get().SetActive(false);
            _cachedObjects.Add(typeof(T3), new List<IMappedObject>());
            _originalInfoCache[2] = (original3.Mapper.Get().name, original3.Mapper.Get<RectTransform>().rect.height);

            var kuchenList = this._scrollRect.gameObject.AddComponent<KuchenList>();
            kuchenList.List = new ListOperator(this);

            var viewport = scrollRect.viewport;
            _viewportRectTransformCache = viewport != null ? viewport : scrollRect.GetComponent<RectTransform>();

            _additionalInfo = scrollRect.GetComponent<ListAdditionalInfo>();

            var verticalLayoutGroup = scrollRect.content.GetComponent<VerticalLayoutGroup>();
            if (verticalLayoutGroup != null)
            {
                verticalLayoutGroup.enabled = false;
                Spacing = verticalLayoutGroup.spacing;
                _margin = new Margin {Top = verticalLayoutGroup.padding.top, Bottom = verticalLayoutGroup.padding.bottom};
            }

            var contentSizeFitter = scrollRect.content.GetComponent<ContentSizeFitter>();
            if (contentSizeFitter != null)
            {
                contentSizeFitter.enabled = false;
            }
        }

        private class ListOperator : IKuchenListMonoBehaviourBridge
        {
            private readonly VerticalList<T1, T2, T3> _list;

            public ListOperator(VerticalList<T1, T2, T3> list) { this._list = list; }

            public void DeactivateAll() { _list.DeactivateAll(); }

            public void UpdateView() { _list.UpdateView(); }
        }

        private void DeactivateAll()
        {
            foreach (var item in _createdObjects.Values)
            {
                if (item is IReusableMappedObject reusable) reusable.Deactivate();
            }

            _createdObjects.Clear();
        }

        // RectTransformUtility.CalculateRelativeRectTransformBoundsを使うと、
        // inactiveMarkedの分だけズレてしまうので自前実装
        private Bounds CalculateRelativeRectTransformBounds(Transform root, Transform child)
        {
            var componentsInChildren = new List<RectTransform>();
            componentsInChildren.Add(child.GetComponent<RectTransform>());
            foreach (Transform a in child)
            {
                if (_inactiveMarked.Contains(a.gameObject)) continue;
                componentsInChildren.Add(a.GetComponent<RectTransform>());
            }

            if ((uint) componentsInChildren.Count <= 0U)
                return new Bounds(Vector3.zero, Vector3.zero);
            var vector31 = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
            var vector32 = new Vector3(float.MinValue, float.MinValue, float.MinValue);
            var worldToLocalMatrix = root.worldToLocalMatrix;
            var index1 = 0;
            for (var length = componentsInChildren.Count; index1 < length; ++index1)
            {
                componentsInChildren[index1].GetWorldCorners(KuchenListInternal.ReuseCorners);
                for (var index2 = 0; index2 < 4; ++index2)
                {
                    var lhs = worldToLocalMatrix.MultiplyPoint3x4(KuchenListInternal.ReuseCorners[index2]);
                    vector31 = Vector3.Min(lhs, vector31);
                    vector32 = Vector3.Max(lhs, vector32);
                }
            }

            var rectTransformBounds = new Bounds(vector31, Vector3.zero);
            rectTransformBounds.Encapsulate(vector32);
            return rectTransformBounds;
        }

        private void UpdateView()
        {
            var displayRect = _viewportRectTransformCache.rect;
            var contentRect = CalculateRelativeRectTransformBounds(_viewportRectTransformCache, _scrollRect.content);
            var start = contentRect.max.y - displayRect.max.y;
            var displayRectHeight = displayRect.height;
            var end = start + displayRectHeight;

            var displayMinIndex = int.MaxValue;
            var displayMaxIndex = int.MinValue;
            for (var i = 0; i < _contentPositions.Count; ++i)
            {
                if (start > _contentPositions[i].Item1) continue;
                if (_contentPositions[i].Item1 > end) break;
                displayMinIndex = Mathf.Min(displayMinIndex, i);
                displayMaxIndex = Mathf.Max(displayMaxIndex, i);
            }

            if (displayMinIndex == int.MaxValue)
            {
                displayMinIndex = _contentPositions.Count - 1;
                displayMaxIndex = _contentPositions.Count - 1;
            }

            displayMinIndex = Mathf.Max(displayMinIndex - 1 - SpareElement, 0);
            displayMaxIndex = Mathf.Min(displayMaxIndex + SpareElement, _contents.Count - 1);

            var removedList = new List<int>();
            foreach (var tmp in _createdObjects)
            {
                var index = tmp.Key;
                var map = tmp.Value;
                if (displayMinIndex <= index && index <= displayMaxIndex) continue;

                CollectObject(map);
                removedList.Add(index);
            }

            foreach (var removed in removedList)
            {
                _createdObjects.Remove(removed);
            }

            for (var i = displayMinIndex; i <= displayMaxIndex; ++i)
            {
                if (_createdObjects.ContainsKey(i)) continue;

                RectTransform newObject = null;
                IMappedObject newMappedObject = null;
                var content = _contents[i];
                if (content.Callback1 != null) (newObject, newMappedObject) = GetOrCreateNewObject(_original1, content.Callback1, _contentPositions[i].Item1);
                if (content.Callback2 != null) (newObject, newMappedObject) = GetOrCreateNewObject(_original2, content.Callback2, _contentPositions[i].Item1);
                if (content.Callback3 != null) (newObject, newMappedObject) = GetOrCreateNewObject(_original3, content.Callback3, _contentPositions[i].Item1);
                if (content.Spacer != null) continue;
                if (newObject == null) throw new Exception($"newObject == null");
                _createdObjects[i] = newMappedObject;
                OnCreateObject?.Invoke(i, newMappedObject);
            }

            foreach (var a in _inactiveMarked) a.SetActive(false);
            _inactiveMarked.Clear();
        }

        private void UpdateListContents()
        {
            // clear elements
            var isFirst = _createdObjects.Values.Count == 0;
            foreach (var map in _createdObjects.Values)
            {
                CollectObject(map);
            }

            _createdObjects.Clear();
            _contentPositions.Clear();

            // create elements
            var calcPosition = Margin.Top;
            var prevElementName = "";
            var elementName = "";
            var specialSpacings = (_additionalInfo != null && _additionalInfo.specialSpacings != null) ? _additionalInfo.specialSpacings : new SpecialSpacing[] { };
            for (var i = 0; i < _contents.Count; ++i)
            {
                var content = _contents[i];
                var elementSize = 0f;

                if (content.Callback1 != null)
                {
                    elementName = _originalInfoCache[0].Name;
                    elementSize = _originalInfoCache[0].Size;
                }

                if (content.Callback2 != null)
                {
                    elementName = _originalInfoCache[1].Name;
                    elementSize = _originalInfoCache[1].Size;
                }

                if (content.Callback3 != null)
                {
                    elementName = _originalInfoCache[2].Name;
                    elementSize = _originalInfoCache[2].Size;
                }

                if (content.Spacer != null)
                {
                    elementName = "";
                    elementSize = content.Spacer.Size;
                }

                float? spacing = null;
                var specialSpacing = specialSpacings.FirstOrDefault(x => x.item1 == prevElementName && x.item2 == elementName);
                if (specialSpacing != null) spacing = specialSpacing.spacing;
                if (spacing == null && i != 0) spacing = Spacing;

                calcPosition += spacing ?? 0f;
                _contentPositions.Add((calcPosition, elementSize));
                calcPosition += elementSize;

                prevElementName = elementName;
            }

            calcPosition += Margin.Bottom;

            // calc content size
            var c = _scrollRect.content;
            var s = c.sizeDelta;
            c.sizeDelta = new Vector2(s.x, calcPosition);

            var anchoredPosition = c.anchoredPosition;
            if (isFirst)
            {
                var scrollRectSizeDeltaY = _scrollRect.GetComponent<RectTransform>().rect.y;
                if (c.pivot.y > 1f - 0.0001f) c.anchoredPosition = new Vector2(anchoredPosition.x, -scrollRectSizeDeltaY);
                if (c.pivot.y < 0f + 0.0001f) c.anchoredPosition = new Vector2(anchoredPosition.x, scrollRectSizeDeltaY);
                _scrollRect.velocity = Vector2.zero;
            }
        }

        private void CollectObject(IMappedObject target)
        {
            if (target is IReusableMappedObject reusable) reusable.Deactivate();
            _inactiveMarked.Add(target.Mapper.Get());

            if (target is T1) _cachedObjects[typeof(T1)].Add(target);
            if (target is T2) _cachedObjects[typeof(T2)].Add(target);
            if (target is T3) _cachedObjects[typeof(T3)].Add(target);
        }

        private (RectTransform, IMappedObject) GetOrCreateNewObject<T>(T original, Action<T> contentCallback, float position) where T : IMappedObject, new()
        {
            var cache = _cachedObjects[typeof(T)];
            T newObject;
            if (cache.Count > 0)
            {
                newObject = (T) cache[0];
                cache.RemoveAt(0);
            }
            else
            {
                newObject = original.Duplicate();
            }

            var newRectTransform = newObject.Mapper.Get<RectTransform>();
            newRectTransform.SetParent(_scrollRect.content);
            var newGameObject = newObject.Mapper.Get();
            if (_inactiveMarked.Contains(newGameObject))
            {
                _inactiveMarked.Remove(newGameObject);
            }
            else
            {
                newGameObject.SetActive(true);
            }

            var p = newRectTransform.anchoredPosition;
            var r = newRectTransform.rect;
            newRectTransform.anchoredPosition = new Vector3(p.x, _scrollRect.content.sizeDelta.y / 2f - position - r.height / 2f, 0f);

            if (newObject is IReusableMappedObject reusable) reusable.Activate();
            contentCallback(newObject);

            return (newRectTransform, newObject);
        }

        public IListContentEditor<T1, T2, T3> Edit(EditMode editMode = EditMode.Clear) { return new ListContentEditor(this, editMode); }

        public class ListContentEditor : IListContentEditor<T1, T2, T3>
        {
            private readonly VerticalList<T1, T2, T3> _parent;
            public List<UIFactory<T1, T2, T3>> Contents { get; set; }
            public float Spacing { get; set; }
            public Margin Margin { get; set; }
            public int SpareElement { get; set; }

            public ListContentEditor(VerticalList<T1, T2, T3> parent, EditMode editMode)
            {
                this._parent = parent;
                Contents = parent._contents;
                Spacing = parent.Spacing;
                Margin = parent._margin;
                SpareElement = parent.SpareElement;

                if (editMode == EditMode.Clear) Contents.Clear();
            }

            public void Dispose()
            {
                _parent._contents = Contents;
                _parent.Spacing = Spacing;
                _parent._margin = Margin;
                _parent.SpareElement = SpareElement;
                _parent.UpdateListContents();
            }

            public void Add(Action<T1> factory) { Contents.Add(new UIFactory<T1, T2, T3>(factory)); }

            public void Add(Action<T2> factory) { Contents.Add(new UIFactory<T1, T2, T3>(factory)); }

            public void Add(Action<T3> factory) { Contents.Add(new UIFactory<T1, T2, T3>(factory)); }
        }

        public void DestroyCachedGameObjects()
        {
            foreach (var cachedObject in _cachedObjects)
            {
                foreach (var go in cachedObject.Value)
                {
                    Object.Destroy(go.Mapper.Get());
                }

                cachedObject.Value.Clear();
            }
        }

        public Vector2? CalcScrollPosition(int index, ScrollToType type = ScrollToType.Top, float additionalSpacing = 0f)
        {
            var c = _scrollRect.content;
            var anchoredPosition = c.anchoredPosition;
            var scrollRectSizeDeltaY = _scrollRect.GetComponent<RectTransform>().rect.y;
            var content = _contentPositions[index];
            var contentHeight = _scrollRect.content.rect.height;
            var viewportHeight = _viewportRectTransformCache.rect.height;
            if (viewportHeight > contentHeight) return null;

            if (c.pivot.y > 1f - 0.0001f)
            {
                var p = -scrollRectSizeDeltaY + content.Item1;
                var limitMin = viewportHeight / 2f;
                var limitMax = -limitMin + contentHeight;
                var top = Mathf.Clamp(p - Spacing - additionalSpacing, limitMin, limitMax);
                var bottom = Mathf.Clamp(p - viewportHeight + content.Item2 + Spacing + additionalSpacing, limitMin, limitMax);
                var center = Mathf.Clamp(p - (viewportHeight - content.Item2) / 2f, limitMin, limitMax);

                if (type == ScrollToType.Top) return new Vector2(anchoredPosition.x, top);
                else if (type == ScrollToType.Bottom) return new Vector2(anchoredPosition.x, bottom);
                else if (type == ScrollToType.Center) return new Vector2(anchoredPosition.x, center);
                else if (type == ScrollToType.Near)
                {
                    var current = c.anchoredPosition.y;
                    if (current > top) return new Vector2(anchoredPosition.x, top);
                    else if (current < bottom) return new Vector2(anchoredPosition.x, bottom);
                    return null;
                }
            }

            if (c.pivot.y < 0f + 0.0001f)
            {
                var p = scrollRectSizeDeltaY - (contentHeight - content.Item1 - content.Item2);
                var limitMax = -viewportHeight / 2f;
                var limitMin = -limitMax - contentHeight;
                var top = Mathf.Clamp(p + Spacing + additionalSpacing, limitMin, limitMax);
                var bottom = Mathf.Clamp(p + viewportHeight - content.Item2 - Spacing - additionalSpacing, limitMin, limitMax);
                var center = Mathf.Clamp(p + (viewportHeight - content.Item2) / 2f, limitMin, limitMax);

                if (type == ScrollToType.Top) return new Vector2(anchoredPosition.x, top);
                else if (type == ScrollToType.Bottom) return new Vector2(anchoredPosition.x, bottom);
                else if (type == ScrollToType.Center) return new Vector2(anchoredPosition.x, center);
                else if (type == ScrollToType.Near)
                {
                    var current = c.anchoredPosition.y;
                    if (current < top) return new Vector2(anchoredPosition.x, top);
                    else if (current > bottom) return new Vector2(anchoredPosition.x, bottom);
                    return null;
                }
            }

            return null;
        }

        public void ScrollTo(int index, ScrollToType type = ScrollToType.Top, float additionalSpacing = 0f)
        {
            var scrollPosition = CalcScrollPosition(index, type, additionalSpacing);
            if (scrollPosition != null) ContentRectTransform.anchoredPosition = scrollPosition.Value;
            _scrollRect.velocity = Vector2.zero;
        }

        public void UpdateAllElements()
        {
            foreach (var tmp in _createdObjects)
            {
                var map = tmp.Value;
                CollectObject(map);
            }

            _createdObjects.Clear();
        }

        public void UpdateElement(int index)
        {
            if (!_createdObjects.ContainsKey(index)) return;
            CollectObject(_createdObjects[index]);
            _createdObjects.Remove(index);
        }
    }

    public class VerticalList<T1, T2, T3, T4> : IKuchenList
        where T1 : IMappedObject, new() where T2 : IMappedObject, new() where T3 : IMappedObject, new() where T4 : IMappedObject, new()
    {
        private readonly ScrollRect _scrollRect;
        private readonly T1 _original1;
        private readonly T2 _original2;
        private readonly T3 _original3;
        private readonly T4 _original4;
        private readonly (string Name, float Size)[] _originalInfoCache;
        private List<UIFactory<T1, T2, T3, T4>> _contents = new List<UIFactory<T1, T2, T3, T4>>();
        private readonly List<(float, float)> _contentPositions = new List<(float, float)>();
        private readonly Dictionary<int, IMappedObject> _createdObjects = new Dictionary<int, IMappedObject>();
        private readonly Dictionary<Type, List<IMappedObject>> _cachedObjects = new Dictionary<Type, List<IMappedObject>>();
        private readonly RectTransform _viewportRectTransformCache;
        private readonly ListAdditionalInfo _additionalInfo;
        public float Spacing { get; private set; }
        public int SpareElement { get; private set; }
        public IReadOnlyDictionary<int, IMappedObject> CreatedObjects => _createdObjects;
        public int ContentsCount => _contents.Count;
        public ScrollRect ScrollRect => _scrollRect;
        public RectTransform ContentRectTransform => _scrollRect.content;
        public Action<int, IMappedObject> OnCreateObject { get; set; }
        public IMappedObject[] MappedObjects => new[] {(IMappedObject) _original1, (IMappedObject) _original2, (IMappedObject) _original3, (IMappedObject) _original4,};

        public T1 Original1 => _original1;
        public T2 Original2 => _original2;
        public T3 Original3 => _original3;
        public T4 Original4 => _original4;

        private Margin _margin = new Margin();
        public IReadonlyMargin Margin => _margin;

        private readonly HashSet<GameObject> _inactiveMarked = new HashSet<GameObject>();

        public float NormalizedPosition { get => _scrollRect.verticalNormalizedPosition; set => _scrollRect.verticalNormalizedPosition = value; }

        public VerticalList(ScrollRect scrollRect, T1 original1, T2 original2, T3 original3, T4 original4)
        {
            this._scrollRect = scrollRect;

            _originalInfoCache = new (string Name, float Size)[4];

            this._original1 = original1;
            this._original1.Mapper.Get().SetActive(false);
            _cachedObjects.Add(typeof(T1), new List<IMappedObject>());
            _originalInfoCache[0] = (original1.Mapper.Get().name, original1.Mapper.Get<RectTransform>().rect.height);

            this._original2 = original2;
            this._original2.Mapper.Get().SetActive(false);
            _cachedObjects.Add(typeof(T2), new List<IMappedObject>());
            _originalInfoCache[1] = (original2.Mapper.Get().name, original2.Mapper.Get<RectTransform>().rect.height);

            this._original3 = original3;
            this._original3.Mapper.Get().SetActive(false);
            _cachedObjects.Add(typeof(T3), new List<IMappedObject>());
            _originalInfoCache[2] = (original3.Mapper.Get().name, original3.Mapper.Get<RectTransform>().rect.height);

            this._original4 = original4;
            this._original4.Mapper.Get().SetActive(false);
            _cachedObjects.Add(typeof(T4), new List<IMappedObject>());
            _originalInfoCache[3] = (original4.Mapper.Get().name, original4.Mapper.Get<RectTransform>().rect.height);

            var kuchenList = this._scrollRect.gameObject.AddComponent<KuchenList>();
            kuchenList.List = new ListOperator(this);

            var viewport = scrollRect.viewport;
            _viewportRectTransformCache = viewport != null ? viewport : scrollRect.GetComponent<RectTransform>();

            _additionalInfo = scrollRect.GetComponent<ListAdditionalInfo>();

            var verticalLayoutGroup = scrollRect.content.GetComponent<VerticalLayoutGroup>();
            if (verticalLayoutGroup != null)
            {
                verticalLayoutGroup.enabled = false;
                Spacing = verticalLayoutGroup.spacing;
                _margin = new Margin {Top = verticalLayoutGroup.padding.top, Bottom = verticalLayoutGroup.padding.bottom};
            }

            var contentSizeFitter = scrollRect.content.GetComponent<ContentSizeFitter>();
            if (contentSizeFitter != null)
            {
                contentSizeFitter.enabled = false;
            }
        }

        private class ListOperator : IKuchenListMonoBehaviourBridge
        {
            private readonly VerticalList<T1, T2, T3, T4> _list;

            public ListOperator(VerticalList<T1, T2, T3, T4> list) { this._list = list; }

            public void DeactivateAll() { _list.DeactivateAll(); }

            public void UpdateView() { _list.UpdateView(); }
        }

        private void DeactivateAll()
        {
            foreach (var item in _createdObjects.Values)
            {
                if (item is IReusableMappedObject reusable) reusable.Deactivate();
            }

            _createdObjects.Clear();
        }

        // RectTransformUtility.CalculateRelativeRectTransformBoundsを使うと、
        // inactiveMarkedの分だけズレてしまうので自前実装
        private Bounds CalculateRelativeRectTransformBounds(Transform root, Transform child)
        {
            var componentsInChildren = new List<RectTransform>();
            componentsInChildren.Add(child.GetComponent<RectTransform>());
            foreach (Transform a in child)
            {
                if (_inactiveMarked.Contains(a.gameObject)) continue;
                componentsInChildren.Add(a.GetComponent<RectTransform>());
            }

            if ((uint) componentsInChildren.Count <= 0U)
                return new Bounds(Vector3.zero, Vector3.zero);
            var vector31 = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
            var vector32 = new Vector3(float.MinValue, float.MinValue, float.MinValue);
            var worldToLocalMatrix = root.worldToLocalMatrix;
            var index1 = 0;
            for (var length = componentsInChildren.Count; index1 < length; ++index1)
            {
                componentsInChildren[index1].GetWorldCorners(KuchenListInternal.ReuseCorners);
                for (var index2 = 0; index2 < 4; ++index2)
                {
                    var lhs = worldToLocalMatrix.MultiplyPoint3x4(KuchenListInternal.ReuseCorners[index2]);
                    vector31 = Vector3.Min(lhs, vector31);
                    vector32 = Vector3.Max(lhs, vector32);
                }
            }

            var rectTransformBounds = new Bounds(vector31, Vector3.zero);
            rectTransformBounds.Encapsulate(vector32);
            return rectTransformBounds;
        }

        private void UpdateView()
        {
            var displayRect = _viewportRectTransformCache.rect;
            var contentRect = CalculateRelativeRectTransformBounds(_viewportRectTransformCache, _scrollRect.content);
            var start = contentRect.max.y - displayRect.max.y;
            var displayRectHeight = displayRect.height;
            var end = start + displayRectHeight;

            var displayMinIndex = int.MaxValue;
            var displayMaxIndex = int.MinValue;
            for (var i = 0; i < _contentPositions.Count; ++i)
            {
                if (start > _contentPositions[i].Item1) continue;
                if (_contentPositions[i].Item1 > end) break;
                displayMinIndex = Mathf.Min(displayMinIndex, i);
                displayMaxIndex = Mathf.Max(displayMaxIndex, i);
            }

            if (displayMinIndex == int.MaxValue)
            {
                displayMinIndex = _contentPositions.Count - 1;
                displayMaxIndex = _contentPositions.Count - 1;
            }

            displayMinIndex = Mathf.Max(displayMinIndex - 1 - SpareElement, 0);
            displayMaxIndex = Mathf.Min(displayMaxIndex + SpareElement, _contents.Count - 1);

            var removedList = new List<int>();
            foreach (var tmp in _createdObjects)
            {
                var index = tmp.Key;
                var map = tmp.Value;
                if (displayMinIndex <= index && index <= displayMaxIndex) continue;

                CollectObject(map);
                removedList.Add(index);
            }

            foreach (var removed in removedList)
            {
                _createdObjects.Remove(removed);
            }

            for (var i = displayMinIndex; i <= displayMaxIndex; ++i)
            {
                if (_createdObjects.ContainsKey(i)) continue;

                RectTransform newObject = null;
                IMappedObject newMappedObject = null;
                var content = _contents[i];
                if (content.Callback1 != null) (newObject, newMappedObject) = GetOrCreateNewObject(_original1, content.Callback1, _contentPositions[i].Item1);
                if (content.Callback2 != null) (newObject, newMappedObject) = GetOrCreateNewObject(_original2, content.Callback2, _contentPositions[i].Item1);
                if (content.Callback3 != null) (newObject, newMappedObject) = GetOrCreateNewObject(_original3, content.Callback3, _contentPositions[i].Item1);
                if (content.Callback4 != null) (newObject, newMappedObject) = GetOrCreateNewObject(_original4, content.Callback4, _contentPositions[i].Item1);
                if (content.Spacer != null) continue;
                if (newObject == null) throw new Exception($"newObject == null");
                _createdObjects[i] = newMappedObject;
                OnCreateObject?.Invoke(i, newMappedObject);
            }

            foreach (var a in _inactiveMarked) a.SetActive(false);
            _inactiveMarked.Clear();
        }

        private void UpdateListContents()
        {
            // clear elements
            var isFirst = _createdObjects.Values.Count == 0;
            foreach (var map in _createdObjects.Values)
            {
                CollectObject(map);
            }

            _createdObjects.Clear();
            _contentPositions.Clear();

            // create elements
            var calcPosition = Margin.Top;
            var prevElementName = "";
            var elementName = "";
            var specialSpacings = (_additionalInfo != null && _additionalInfo.specialSpacings != null) ? _additionalInfo.specialSpacings : new SpecialSpacing[] { };
            for (var i = 0; i < _contents.Count; ++i)
            {
                var content = _contents[i];
                var elementSize = 0f;

                if (content.Callback1 != null)
                {
                    elementName = _originalInfoCache[0].Name;
                    elementSize = _originalInfoCache[0].Size;
                }

                if (content.Callback2 != null)
                {
                    elementName = _originalInfoCache[1].Name;
                    elementSize = _originalInfoCache[1].Size;
                }

                if (content.Callback3 != null)
                {
                    elementName = _originalInfoCache[2].Name;
                    elementSize = _originalInfoCache[2].Size;
                }

                if (content.Callback4 != null)
                {
                    elementName = _originalInfoCache[3].Name;
                    elementSize = _originalInfoCache[3].Size;
                }

                if (content.Spacer != null)
                {
                    elementName = "";
                    elementSize = content.Spacer.Size;
                }

                float? spacing = null;
                var specialSpacing = specialSpacings.FirstOrDefault(x => x.item1 == prevElementName && x.item2 == elementName);
                if (specialSpacing != null) spacing = specialSpacing.spacing;
                if (spacing == null && i != 0) spacing = Spacing;

                calcPosition += spacing ?? 0f;
                _contentPositions.Add((calcPosition, elementSize));
                calcPosition += elementSize;

                prevElementName = elementName;
            }

            calcPosition += Margin.Bottom;

            // calc content size
            var c = _scrollRect.content;
            var s = c.sizeDelta;
            c.sizeDelta = new Vector2(s.x, calcPosition);

            var anchoredPosition = c.anchoredPosition;
            if (isFirst)
            {
                var scrollRectSizeDeltaY = _scrollRect.GetComponent<RectTransform>().rect.y;
                if (c.pivot.y > 1f - 0.0001f) c.anchoredPosition = new Vector2(anchoredPosition.x, -scrollRectSizeDeltaY);
                if (c.pivot.y < 0f + 0.0001f) c.anchoredPosition = new Vector2(anchoredPosition.x, scrollRectSizeDeltaY);
                _scrollRect.velocity = Vector2.zero;
            }
        }

        private void CollectObject(IMappedObject target)
        {
            if (target is IReusableMappedObject reusable) reusable.Deactivate();
            _inactiveMarked.Add(target.Mapper.Get());

            if (target is T1) _cachedObjects[typeof(T1)].Add(target);
            if (target is T2) _cachedObjects[typeof(T2)].Add(target);
            if (target is T3) _cachedObjects[typeof(T3)].Add(target);
            if (target is T4) _cachedObjects[typeof(T4)].Add(target);
        }

        private (RectTransform, IMappedObject) GetOrCreateNewObject<T>(T original, Action<T> contentCallback, float position) where T : IMappedObject, new()
        {
            var cache = _cachedObjects[typeof(T)];
            T newObject;
            if (cache.Count > 0)
            {
                newObject = (T) cache[0];
                cache.RemoveAt(0);
            }
            else
            {
                newObject = original.Duplicate();
            }

            var newRectTransform = newObject.Mapper.Get<RectTransform>();
            newRectTransform.SetParent(_scrollRect.content);
            var newGameObject = newObject.Mapper.Get();
            if (_inactiveMarked.Contains(newGameObject))
            {
                _inactiveMarked.Remove(newGameObject);
            }
            else
            {
                newGameObject.SetActive(true);
            }

            var p = newRectTransform.anchoredPosition;
            var r = newRectTransform.rect;
            newRectTransform.anchoredPosition = new Vector3(p.x, _scrollRect.content.sizeDelta.y / 2f - position - r.height / 2f, 0f);

            if (newObject is IReusableMappedObject reusable) reusable.Activate();
            contentCallback(newObject);

            return (newRectTransform, newObject);
        }

        public IListContentEditor<T1, T2, T3, T4> Edit(EditMode editMode = EditMode.Clear) { return new ListContentEditor(this, editMode); }

        public class ListContentEditor : IListContentEditor<T1, T2, T3, T4>
        {
            private readonly VerticalList<T1, T2, T3, T4> _parent;
            public List<UIFactory<T1, T2, T3, T4>> Contents { get; set; }
            public float Spacing { get; set; }
            public Margin Margin { get; set; }
            public int SpareElement { get; set; }

            public ListContentEditor(VerticalList<T1, T2, T3, T4> parent, EditMode editMode)
            {
                this._parent = parent;
                Contents = parent._contents;
                Spacing = parent.Spacing;
                Margin = parent._margin;
                SpareElement = parent.SpareElement;

                if (editMode == EditMode.Clear) Contents.Clear();
            }

            public void Dispose()
            {
                _parent._contents = Contents;
                _parent.Spacing = Spacing;
                _parent._margin = Margin;
                _parent.SpareElement = SpareElement;
                _parent.UpdateListContents();
            }

            public void Add(Action<T1> factory) { Contents.Add(new UIFactory<T1, T2, T3, T4>(factory)); }

            public void Add(Action<T2> factory) { Contents.Add(new UIFactory<T1, T2, T3, T4>(factory)); }

            public void Add(Action<T3> factory) { Contents.Add(new UIFactory<T1, T2, T3, T4>(factory)); }

            public void Add(Action<T4> factory) { Contents.Add(new UIFactory<T1, T2, T3, T4>(factory)); }
        }

        public void DestroyCachedGameObjects()
        {
            foreach (var cachedObject in _cachedObjects)
            {
                foreach (var go in cachedObject.Value)
                {
                    Object.Destroy(go.Mapper.Get());
                }

                cachedObject.Value.Clear();
            }
        }

        public Vector2? CalcScrollPosition(int index, ScrollToType type = ScrollToType.Top, float additionalSpacing = 0f)
        {
            var c = _scrollRect.content;
            var anchoredPosition = c.anchoredPosition;
            var scrollRectSizeDeltaY = _scrollRect.GetComponent<RectTransform>().rect.y;
            var content = _contentPositions[index];
            var contentHeight = _scrollRect.content.rect.height;
            var viewportHeight = _viewportRectTransformCache.rect.height;
            if (viewportHeight > contentHeight) return null;

            if (c.pivot.y > 1f - 0.0001f)
            {
                var p = -scrollRectSizeDeltaY + content.Item1;
                var limitMin = viewportHeight / 2f;
                var limitMax = -limitMin + contentHeight;
                var top = Mathf.Clamp(p - Spacing - additionalSpacing, limitMin, limitMax);
                var bottom = Mathf.Clamp(p - viewportHeight + content.Item2 + Spacing + additionalSpacing, limitMin, limitMax);
                var center = Mathf.Clamp(p - (viewportHeight - content.Item2) / 2f, limitMin, limitMax);

                if (type == ScrollToType.Top) return new Vector2(anchoredPosition.x, top);
                else if (type == ScrollToType.Bottom) return new Vector2(anchoredPosition.x, bottom);
                else if (type == ScrollToType.Center) return new Vector2(anchoredPosition.x, center);
                else if (type == ScrollToType.Near)
                {
                    var current = c.anchoredPosition.y;
                    if (current > top) return new Vector2(anchoredPosition.x, top);
                    else if (current < bottom) return new Vector2(anchoredPosition.x, bottom);
                    return null;
                }
            }

            if (c.pivot.y < 0f + 0.0001f)
            {
                var p = scrollRectSizeDeltaY - (contentHeight - content.Item1 - content.Item2);
                var limitMax = -viewportHeight / 2f;
                var limitMin = -limitMax - contentHeight;
                var top = Mathf.Clamp(p + Spacing + additionalSpacing, limitMin, limitMax);
                var bottom = Mathf.Clamp(p + viewportHeight - content.Item2 - Spacing - additionalSpacing, limitMin, limitMax);
                var center = Mathf.Clamp(p + (viewportHeight - content.Item2) / 2f, limitMin, limitMax);

                if (type == ScrollToType.Top) return new Vector2(anchoredPosition.x, top);
                else if (type == ScrollToType.Bottom) return new Vector2(anchoredPosition.x, bottom);
                else if (type == ScrollToType.Center) return new Vector2(anchoredPosition.x, center);
                else if (type == ScrollToType.Near)
                {
                    var current = c.anchoredPosition.y;
                    if (current < top) return new Vector2(anchoredPosition.x, top);
                    else if (current > bottom) return new Vector2(anchoredPosition.x, bottom);
                    return null;
                }
            }

            return null;
        }

        public void ScrollTo(int index, ScrollToType type = ScrollToType.Top, float additionalSpacing = 0f)
        {
            var scrollPosition = CalcScrollPosition(index, type, additionalSpacing);
            if (scrollPosition != null) ContentRectTransform.anchoredPosition = scrollPosition.Value;
            _scrollRect.velocity = Vector2.zero;
        }

        public void UpdateAllElements()
        {
            foreach (var tmp in _createdObjects)
            {
                var map = tmp.Value;
                CollectObject(map);
            }

            _createdObjects.Clear();
        }

        public void UpdateElement(int index)
        {
            if (!_createdObjects.ContainsKey(index)) return;
            CollectObject(_createdObjects[index]);
            _createdObjects.Remove(index);
        }
    }
}