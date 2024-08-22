using System.Collections.Generic;
using System.Linq;
using Pancake.Linq;

namespace Pancake.StatModifier
{
    public class ModifierNormalStrategyOrder : IModifierStrategyOrder
    {
        public float Apply(List<StatModifier> statModifiers, float baseValue)
        {
            foreach (var modifier in statModifiers.Filter(modifier => modifier.Modifier is AddModifier))
            {
                baseValue = modifier.Modifier.Calculate(baseValue);
            }

            foreach (var modifier in statModifiers.Where(modifier => modifier.Modifier is MultiplyModifier))
            {
                baseValue = modifier.Modifier.Calculate(baseValue);
            }

            return baseValue;
        }
    }
}