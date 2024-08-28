using System;
using Sisus.Init;

namespace Pancake.StatModifier
{
    [Service(typeof(IStatModifierFactory))]
    public class StatModifierFactory : IStatModifierFactory
    {
        public StatModifier Create(EModifierType modifierType, StringConstant statType, float value, float duration = 0f)
        {
            IModifier modifier = modifierType switch
            {
                EModifierType.Add => new AddModifier(value),
                EModifierType.Multiply => new MultiplyModifier(value),
                _ => throw new ArgumentOutOfRangeException(nameof(modifierType), modifierType, null)
            };
            return new StatModifier(statType, modifier, duration);
        }
    }
}