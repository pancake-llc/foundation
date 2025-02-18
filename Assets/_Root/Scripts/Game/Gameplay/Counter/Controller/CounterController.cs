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
            _counter = new Elm<CounterModel, ECounterType>(() => (new CounterModel {number = initCounter}, Cmd<ECounterType>.none), new CounterUpdater(), render);
        }
    }
}