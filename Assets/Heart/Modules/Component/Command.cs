#if PANCAKE_ROUTER
using Pancake.Localization;
using UnityEngine;
using VitalRouter;

namespace Pancake.Component
{
    public readonly struct UpdateCurrencyCommand : ICommand
    {
        public string TypeCurrency { get; }

        public UpdateCurrencyCommand(string typeCurrency) { TypeCurrency = typeCurrency; }
    }

    public readonly struct UpdateCurrencyWithValueCommand : ICommand
    {
        public string TypeCurrency { get; }
        public int Value { get; }

        public UpdateCurrencyWithValueCommand(string typeCurrency, int value)
        {
            TypeCurrency = typeCurrency;
            Value = value;
        }
    }

    public readonly struct SpawnInGameNotiCommand : ICommand
    {
        public LocaleText LocaleText { get; }

        public SpawnInGameNotiCommand(LocaleText localeText) { LocaleText = localeText; }
    }

    public readonly struct UIHideBeforeCommand : ICommand
    {
        public string Group { get; }

        public UIHideBeforeCommand(string group) { Group = group; }
    }

    public readonly struct UIShowAfterCommand : ICommand
    {
        public string Group { get; }

        public UIShowAfterCommand(string group) { Group = group; }
    }

    public readonly struct VfxMagnetCommand : ICommand
    {
        public string Type { get; }
        public Vector3 Position { get; }
        public int Value { get; }

        public VfxMagnetCommand(string type, Vector3 position, int value)
        {
            Type = type;
            Position = position;
            Value = value;
        }
    }
}
#endif