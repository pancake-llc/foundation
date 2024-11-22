using UnityEngine;

namespace Pancake.Game
{
    public class CounterController : MonoBehaviour
    {
        [SerializeField] private CounterView view;
        [SerializeField] private int counter;

        private CounterPresenter _presenter;

        private void Start()
        {
            _presenter = new CounterPresenter.Builder().WithData(counter).Build(view);
        }
    }
}