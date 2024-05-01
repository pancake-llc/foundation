using System.Globalization;
using Pancake.Common;
using Pancake.Localization;
using Pancake.Scriptable;
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
        [SerializeField] private BoolVariable loadingCompleted;


        private float _currentTimeLoading;

        private void Start()
        {
            loadingBar.value = 0;
            loadingCompleted.Value = false;
        }

        private void Update()
        {
            if (!loadingCompleted.Value)
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
                if (_currentTimeLoading >= duration) loadingCompleted.Value = true;
            }
        }
    }
}