using System.Collections.Generic;

namespace Pancake.StatModifier
{
    public interface IModifier
    {
        float Calculate(float value);
    }

    public interface IModifierStrategyOrder
    {
        float Apply(List<StatModifier> statModifiers, float baseValue);
    }

    public interface IStatModifierFactory
    {
        StatModifier Create(EModifierType modifierType, string statType, float value, float duration = 0);
    }
}