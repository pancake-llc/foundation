using System;
using LitMotion;
using Pancake.Elm;
using UnityEngine;

namespace Pancake.Game
{
    public class CounterController : MonoBehaviour
    {
        [SerializeField] private CounterRenderer render;
        [SerializeField] private int initCounter;

        private Elm<CounterModel, ECounterType> _counter;
        private TimeSubscription _timeSubscription;
        private KeyboardSubscription _keyboardSubscription;

        private void Start()
        {
            _timeSubscription = new TimeSubscription();
            _keyboardSubscription = new KeyboardSubscription();
            //var sub = new Sub<IMessenger<ECounterType>>(_timeSubscription);
            var sub = Sub<IMessenger<ECounterType>>.Batch(new Sub<IMessenger<ECounterType>>[] {new(_timeSubscription), new(_keyboardSubscription)});
            _counter = new Elm<CounterModel, ECounterType>(() => (new CounterModel {number = initCounter}, Cmd<ECounterType>.none),
                new CounterUpdater(),
                render,
                _ => sub);


            Sub<ECounterType> map = new Sub<IMessenger<ECounterType>>(_timeSubscription).Map(m => m.GetMessage());
        }
    }

    public class TimeSubscription : IEffect<IMessenger<ECounterType>>
    {
        private event Action<IMessenger<ECounterType>> OnOccurrence;

        public void Add(Action<IMessenger<ECounterType>> handler) => OnOccurrence += handler;

        public void Remove(Action<IMessenger<ECounterType>> handler) => OnOccurrence -= handler;

        public TimeSubscription()
        {
            LMotion.Create(0, 0, 1f)
                .WithLoops(-1)
                .WithOnLoopComplete(_ =>
                {
                    Debug.Log("Called TimeSubscription After 1s");
                    OnOccurrence?.Invoke(new CounterIncreaseMsg());
                })
                .RunWithoutBinding();
        }
    }

    public class KeyboardSubscription : IEffect<IMessenger<ECounterType>>
    {
        private event Action<IMessenger<ECounterType>> OnOccurrence;

        public void Add(Action<IMessenger<ECounterType>> handler) => OnOccurrence += handler;

        public void Remove(Action<IMessenger<ECounterType>> handler) => OnOccurrence -= handler;

        public KeyboardSubscription()
        {
            LMotion.Create(0, 0, 1f)
                .WithLoops(-1)
                .Bind(_ =>
                {
                    if (Input.GetKeyDown(KeyCode.Space))
                    {
                        Debug.Log("Pressed Space");
                        OnOccurrence.Invoke(new CounterDecreaseMsg());
                    }
                });
        }
    }
}