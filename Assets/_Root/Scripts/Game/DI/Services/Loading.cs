using System.Text;
using Pancake.Common;
using Pancake.Game.Interfaces;
using Pancake.Localization;
using Sisus.Init;
using UnityEngine;
using UnityEngine.UI;

namespace Pancake.Game
{
    [Service(typeof(ILoading), FindFromScene = true)]
    public class Loading : MonoBehaviour, ILoadComponent, ILoading
    {
        [SerializeField, Range(0f, 3f)] private float duration;
        [SerializeField] private Slider loadingBar;
        [SerializeField] private LocaleTextComponent localeTxtPercent;

        private float _currentTimeLoading;
        private StringBuilder _builder;
        private int _lastPercent = -1;
        public bool IsLoadingCompleted { get; private set; }

        void ILoadComponent.OnLoadComponents()
        {
            loadingBar = FindFirstObjectByType<Slider>();
            localeTxtPercent = loadingBar.GetComponentInChildren<LocaleTextComponent>();
        }

        private void Start()
        {
            loadingBar.value = 0;
            IsLoadingCompleted = false;
            _builder = new StringBuilder();
        }

        private void Update()
        {
            if (!IsLoadingCompleted)
            {
                if (loadingBar.value < 0.4f)
                {
                    loadingBar.value += 1 / duration / 3 * Time.deltaTime;
                    _currentTimeLoading += Time.deltaTime / 3f;
                }
                else
                {
                    loadingBar.value += 1 / duration * Time.deltaTime;
                    _currentTimeLoading += Time.deltaTime;
                }

                var now = (int) (loadingBar.value * 100).Round();
                if (_lastPercent != now)
                {
                    _lastPercent = now;
                    _builder.Clear();
                    _builder.Append((loadingBar.value * 100).Round());
                    localeTxtPercent.UpdateArgs(_builder.ToString());
                }

                if (_currentTimeLoading >= duration) IsLoadingCompleted = true;
            }
        }
    }
}