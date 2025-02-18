using System;
using Pancake.Elm;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Pancake.Game
{
    public class CounterRenderer : MonoBehaviour, IRenderer<CounterModel, ECounterType>
    {
        [SerializeField] private TextMeshProUGUI text;
        [SerializeField] private Button buttonIncrease;

        private Dispatcher<ECounterType> _dispatcher;

        public void Init(Dispatcher<ECounterType> dispatcher)
        {
            _dispatcher = dispatcher;
            //buttonIncrease.onClick.AddListener(() => dispatcher.Invoke(new CounterIncreaseMsg()));
        }

        public void Render(CounterModel model)
        {
            //text.text = model.number.ToString();
        }

        private void Update()
        {
            _dispatcher.Invoke(new CounterIncreaseMsg());
        }
    }
}