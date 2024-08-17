using System.Collections.Generic;
using Pancake.Linq;

namespace Pancake.StatModifier
{
    public class StatMediator
    {
        private readonly List<StatModifier> _modifiers = new();
        private readonly Dictionary<StringConstant, List<StatModifier>> _modifiersCache = new();
        private readonly IModifierStrategyOrder _order = new ModifierNormalStrategyOrder();

        public void PerformQuery(object sender, Query query)
        {
            if (!_modifiersCache.ContainsKey(query.statType))
            {
                _modifiersCache[query.statType] = _modifiers.Filter(modifier => modifier.StatType == query.statType);
            }

            query.value = _order.Apply(_modifiersCache[query.statType], query.value);
        }

        private void InvalidateCache(StringConstant statType) { _modifiersCache.Remove(statType); }

        private void InternalOnDispose(StatModifier modifier)
        {
            InvalidateCache(modifier.StatType);
            _modifiers.Remove(modifier);
        }

        public void AddModifier(StatModifier modifier)
        {
            _modifiers.Add(modifier);
            InvalidateCache(modifier.StatType);
            modifier.MarkedForRemoval = false;
            modifier.OnDispose += InternalOnDispose;
        }

        public void RemoveModifier(StatModifier modifier)
        {
            modifier.OnDispose -= InternalOnDispose;
            modifier.MarkedForRemoval = true;
            InvalidateCache(modifier.StatType);
            _modifiers.Remove(modifier);
        }

        public void RemoveAllModifier()
        {
            foreach (var modifier in _modifiers)
            {
                modifier.OnDispose -= InternalOnDispose;
                modifier.MarkedForRemoval = true;
                InvalidateCache(modifier.StatType);
            }

            _modifiers.Clear();
        }

        public void Update(float deltaTime)
        {
            foreach (var modifier in _modifiers)
            {
                modifier.OnUpdate(deltaTime);
            }

            foreach (var modifier in _modifiers.Filter(modifier => modifier.MarkedForRemoval))
            {
                modifier.Dispose();
            }
        }
    }
}