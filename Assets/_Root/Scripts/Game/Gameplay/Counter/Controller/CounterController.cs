using System;
using Pancake.Common;
using Pancake.Elm;
using UnityEngine;

namespace Pancake.Game
{
    public class CounterController : MonoBehaviour
    {
        [SerializeField] private CounterRenderer render;
        [SerializeField] private int initCounter;

        private Elm<CounterModel, ECounterType> _counter;

        private void Start()
        {
            _counter = new Elm<CounterModel, ECounterType>(() => (new CounterModel {number = initCounter}, Cmd<ECounterType>.none), new CounterUpdater(), render, CreateSubscription());
            
            
            Func<CounterModel, Sub<IMessenger<ECounterType>>> CreateSubscription()
            {
                var timeSub = new Sub<DateTime>(new TimeSubscription())
                    .Map<IMessenger<ECounterType>>(time => new CounterTickMsg());

                var keyboardSub = new Sub<string>(new KeyboardSubscription())
                    .Map<IMessenger<ECounterType>>(key =>
                    {
                        if (key == "I") return new CounterIncreaseMsg();
                        if (key == "D") return new CounterDecreaseMsg();

                        return new CounterNothingMsg();
                    });

                return _ => Sub<IMessenger<ECounterType>>.Batch(new[] { timeSub, keyboardSub });
            }
            
        }
    }
    
    public class TimeSubscription : IEffect<DateTime>
    {
        private Timer _timer;

        public void Add(Action<DateTime> handler)
        {
            _timer = new CountdownTimer(5);
            _timer.Start();
            _timer.onTimerStart += () =>
            {
                Debug.Log("Startt");
                handler(DateTime.Now);
            };
            _timer.onTimerStop += () =>
            {
                Debug.Log("stop");
                
                handler(DateTime.Now);
            };
        }

        public void Remove(Action<DateTime> handler)
        {
            _timer.Stop();
        }
    }

    public class KeyboardSubscription : IEffect<string>
    {
        public void Add(Action<string> handler)
        {
           Debug.Log("Listening for key presses...");
            //while (true)
            {
                if (Input.GetKeyDown(KeyCode.I))
                {
                    handler("I");
                }else if (Input.GetKeyDown(KeyCode.D))
                {
                    handler("D");
                }
            }
        }

        public void Remove(Action<string> handler)
        {
            // Không cần làm gì trong trường hợp này
        }
    }
}