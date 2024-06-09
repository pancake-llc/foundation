using System.Globalization;
using Pancake.Common;
using Pancake.Localization;
using UnityEngine;
using UnityEngine.UI;

namespace Pancake.SceneFlow
{
    [EditorIcon("icon_progress")]
    public class Loading : GameComponent
    {
        [SerializeField, Range(0f, 3f)] private float duration;
        [SerializeField] private Slider loadingBar;
        [SerializeField] private LocaleTextComponent localeTxtPercent;

        private float _currentTimeLoading;
        private bool _loadingCompleted;

        private void Start()
        {
            loadingBar.value = 0;
            _loadingCompleted = false;
        }

        private void Update()
        {
            if (!_loadingCompleted)
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

                localeTxtPercent.UpdateArgs((loadingBar.value * 100).Round().ToString(CultureInfo.InvariantCulture));
                if (_currentTimeLoading >= duration) _loadingCompleted = true;
            }
        }
    }
}