using System;
using Pancake.Common;

namespace Pancake.StatModifier
{
    public class StatModifier : IDisposable
    {
        public event Action<StatModifier> OnDispose = delegate { };

        public StringConstant StatType { get; }
        public IModifier Modifier { get; }
        public bool MarkedForRemoval { get; internal set; }
        private readonly CountdownTimer _timer;

        public StatModifier(StringConstant statType, IModifier modifier, float duration = 0f)
        {
            StatType = statType;
            Modifier = modifier;

            if (duration <= 0) return;

            _timer = new CountdownTimer(duration);
            _timer.onTimerStop += () => MarkedForRemoval = true;
            _timer.Start();
        }

        public void OnUpdate(float deltaTime) => _timer?.OnUpdate(deltaTime);

        public void Handle(object sender, Query query)
        {
            if (query.statType.Equals(StatType)) query.value = Modifier.Calculate(query.value);
        }

        public void Dispose()
        {
            _timer?.Stop();
            OnDispose?.Invoke(this);
        }
    }
}