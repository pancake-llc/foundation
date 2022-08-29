using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Pancake.UIQuery
{
    public interface IKuchenList : IMappedObjectList
    {
        float Spacing { get; }
        int SpareElement { get; }
        IReadOnlyDictionary<int, IMappedObject> CreatedObjects { get; }
        int ContentsCount { get; }
        IReadonlyMargin Margin { get; }
        ScrollRect ScrollRect { get; }
        Action<int, IMappedObject> OnCreateObject { get; set; }

        float NormalizedPosition { get; set; }

        Vector2? CalcScrollPosition(int index, ScrollToType type = ScrollToType.Top, float additionalSpacing = 0f);
        void ScrollTo(int index, ScrollToType type = ScrollToType.Top, float additionalSpacing = 0f);
        void DestroyCachedGameObjects();

        void UpdateAllElements();
        void UpdateElement(int index);
    }

    public enum ScrollToType
    {
        Top,
        Bottom,
        Near,
        Center,
    }

    public interface IListContentEditor<T1> : IDisposable where T1 : IMappedObject
    {
        List<UIFactory<T1>> Contents { get; set; }
        float Spacing { get; set; }
        Margin Margin { get; set; }
        int SpareElement { get; set; }
        void Add(Action<T1> factory);
    }

    public interface IListContentEditor<T1, T2> : IDisposable where T1 : IMappedObject where T2 : IMappedObject
    {
        List<UIFactory<T1, T2>> Contents { get; set; }
        float Spacing { get; set; }
        Margin Margin { get; set; }
        int SpareElement { get; set; }
        void Add(Action<T1> factory);
        void Add(Action<T2> factory);
    }

    public interface IListContentEditor<T1, T2, T3> : IDisposable where T1 : IMappedObject where T2 : IMappedObject where T3 : IMappedObject
    {
        List<UIFactory<T1, T2, T3>> Contents { get; set; }
        float Spacing { get; set; }
        Margin Margin { get; set; }
        int SpareElement { get; set; }
        void Add(Action<T1> factory);
        void Add(Action<T2> factory);
        void Add(Action<T3> factory);
    }

    public interface IListContentEditor<T1, T2, T3, T4> : IDisposable where T1 : IMappedObject where T2 : IMappedObject where T3 : IMappedObject where T4 : IMappedObject
    {
        List<UIFactory<T1, T2, T3, T4>> Contents { get; set; }
        float Spacing { get; set; }
        Margin Margin { get; set; }
        int SpareElement { get; set; }
        void Add(Action<T1> factory);
        void Add(Action<T2> factory);
        void Add(Action<T3> factory);
        void Add(Action<T4> factory);
    }
}
