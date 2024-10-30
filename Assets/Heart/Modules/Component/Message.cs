using Pancake.Localization;
using UnityEngine;

namespace Pancake.Component
{
    public readonly struct UpdateCurrencyMessage : IMessage
    {
        public string TypeCurrency { get; }

        public UpdateCurrencyMessage(string typeCurrency) { TypeCurrency = typeCurrency; }
    }

    public readonly struct UpdateCurrencyWithValueMessage : IMessage
    {
        public string TypeCurrency { get; }
        public int Value { get; }

        public UpdateCurrencyWithValueMessage(string typeCurrency, int value)
        {
            TypeCurrency = typeCurrency;
            Value = value;
        }
    }

    public readonly struct SpawnInGameNotiMessage : IMessage
    {
        public LocaleText LocaleText { get; }

        public SpawnInGameNotiMessage(LocaleText localeText) { LocaleText = localeText; }
    }

    public readonly struct UIHideBeforeMessage : IMessage
    {
        public string Group { get; }

        public UIHideBeforeMessage(string group) { Group = group; }
    }

    public readonly struct UIShowAfterMessage : IMessage
    {
        public string Group { get; }

        public UIShowAfterMessage(string group) { Group = group; }
    }

    public readonly struct VfxMagnetMessage : IMessage
    {
        public string Type { get; }
        public Vector3 Position { get; }
        public int Value { get; }

        public VfxMagnetMessage(string type, Vector3 position, int value)
        {
            Type = type;
            Position = position;
            Value = value;
        }
    }
}