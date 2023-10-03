using Pancake.Scriptable;
using TMPro;

namespace Pancake.SceneFlow
{
    using UnityEngine;

    /// <summary>
    /// Display Text Current Level
    /// </summary>
    public class CurrentLevelComponent : GameComponent
    {
        [SerializeField] private IntVariable currentLevel;
        [SerializeField] private TextMeshProUGUI text;
        [SerializeField] private bool subscribe;

        protected override void OnEnabled()
        {
            OnValueChanged(currentLevel.Value);
            if (subscribe) currentLevel.OnValueChanged += OnValueChanged;
        }

        private void OnValueChanged(int level) { text.text = $"Level {level + 1}"; }

        protected override void OnDisabled()
        {
            if (subscribe) currentLevel.OnValueChanged -= OnValueChanged;
        }
    }
}