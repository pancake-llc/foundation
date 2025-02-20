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

        public void Init(Dispatcher<ECounterType> dispatcher)
        {
            buttonIncrease.onClick.AddListener(() => dispatcher.Invoke(new CounterIncreaseMsg()));
        }

        public void Render(CounterModel model) { text.text = model.number.ToString(); }
    }
}