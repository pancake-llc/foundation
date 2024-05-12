using Pancake.Localization;

namespace Pancake.Component
{
    public struct UpdateCurrencyEvent : IEvent
    {
        public string typeCurrency;
    }

    public struct UpdateCurrencyWithValueEvent : IEvent
    {
        public string typeCurrency;
        public int value;
    }

    public struct SpawnInGameNotiEvent : IEvent
    {
        public LocaleText localeText;
    }
}