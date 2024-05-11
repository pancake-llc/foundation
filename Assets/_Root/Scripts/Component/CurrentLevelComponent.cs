using Pancake.LevelSystem;
using Pancake.Localization;

namespace Pancake.SceneFlow
{
    using UnityEngine;

    /// <summary>
    /// Display Text Current Level
    /// </summary>
    public class CurrentLevelComponent : GameComponent
    {
        [SerializeField] private StringConstant levelType;
        [SerializeField] private LocaleTextComponent localeText;
        [SerializeField] private bool subscribe;

        private void Start() { OnValueChanged(LevelCoordinator.GetCurrentLevelIndex(levelType.Value)); }

        protected void OnEnable()
        {
            if (subscribe) LevelCoordinator.RegisterLevelIndexChanged(levelType.Value, OnValueChanged);
        }

        protected void OnDisable()
        {
            if (subscribe) LevelCoordinator.RegisterLevelIndexChanged(levelType.Value, OnValueChanged);
        }

        private void OnValueChanged(int level) { localeText.UpdateArgs($"{level + 1}"); }
    }
}