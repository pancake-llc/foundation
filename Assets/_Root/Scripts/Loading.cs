using System;
using System.Globalization;
using Pancake.Apex;
using Pancake.Scriptable;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Pancake.SceneFlow
{
    [HideMonoScript]
    [EditorIcon("script_progress")]
    public class Loading : GameComponent
    {
        [SerializeField, Range(0f, 3f)] private float duration;
        [SerializeField] private Slider loadingBar;
        [SerializeField] private TextMeshProUGUI txtPercent;
        [SerializeField] private string format = "Loading {0}%";
        [SerializeField] private BoolVariable loadingCompleted;


        private float _currentTimeLoading;

        private void Start()
        {
            loadingBar.value = 0;
            loadingCompleted.Value = false;
        }

        protected override void Tick()
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

                txtPercent.text = string.Format(format, (loadingBar.value * 100).Round().ToString(CultureInfo.InvariantCulture));
                if (_currentTimeLoading >= duration)
                {
                    loadingCompleted.Value = true;
                }
            }
        }
    }
}