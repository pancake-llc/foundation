using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Pancake.Game
{
    public class CounterView : MonoBehaviour, IView<int>
    {
        [SerializeField] private TextMeshProUGUI text;
        [SerializeField] private Button buttonIncrease;

        public event Action IncreaseCounterEvent;

        public void Initialize()
        {
            // init default value
            buttonIncrease.onClick.AddListener(() => IncreaseCounterEvent?.Invoke());
        }

        public void Cleanup()
        {
            // clean up if needed
        }

        public void UpdateView(int data) { text.text = data.ToString(); }
    }
}