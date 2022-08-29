using System;

namespace Pancake.UIQuery
{
    public class UIFactory<T1> where T1 : IMappedObject
    {
        public Action<T1> Callback1 { get; }
        public Spacer Spacer { get; }

        public UIFactory(Action<T1> callback1)
        {
            Callback1 = callback1;
        }

        public UIFactory(Spacer spacer)
        {
            Spacer = spacer;
        }
    }

    public class UIFactory<T1, T2> where T1 : IMappedObject where T2 : IMappedObject
    {
        public Action<T1> Callback1 { get; }
        public Action<T2> Callback2 { get; }
        public Spacer Spacer { get; }

        public UIFactory(Action<T1> callback1)
        {
            Callback1 = callback1;
        }

        public UIFactory(Action<T2> callback2)
        {
            Callback2 = callback2;
        }

        public UIFactory(Spacer spacer)
        {
            Spacer = spacer;
        }
    }

    public class UIFactory<T1, T2, T3> where T1 : IMappedObject where T2 : IMappedObject where T3 : IMappedObject
    {
        public Action<T1> Callback1 { get; }
        public Action<T2> Callback2 { get; }
        public Action<T3> Callback3 { get; }
        public Spacer Spacer { get; }

        public UIFactory(Action<T1> callback1)
        {
            Callback1 = callback1;
        }

        public UIFactory(Action<T2> callback2)
        {
            Callback2 = callback2;
        }

        public UIFactory(Action<T3> callback3)
        {
            Callback3 = callback3;
        }

        public UIFactory(Spacer spacer)
        {
            Spacer = spacer;
        }
    }

    public class UIFactory<T1, T2, T3, T4> where T1 : IMappedObject where T2 : IMappedObject where T3 : IMappedObject where T4 : IMappedObject
    {
        public Action<T1> Callback1 { get; }
        public Action<T2> Callback2 { get; }
        public Action<T3> Callback3 { get; }
        public Action<T4> Callback4 { get; }
        public Spacer Spacer { get; }

        public UIFactory(Action<T1> callback1)
        {
            Callback1 = callback1;
        }

        public UIFactory(Action<T2> callback2)
        {
            Callback2 = callback2;
        }

        public UIFactory(Action<T3> callback3)
        {
            Callback3 = callback3;
        }

        public UIFactory(Action<T4> callback4)
        {
            Callback4 = callback4;
        }

        public UIFactory(Spacer spacer)
        {
            Spacer = spacer;
        }
    }

}