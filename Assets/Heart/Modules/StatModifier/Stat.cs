namespace Pancake.StatModifier
{
    public class Stat
    {
        private readonly float _value;
        private readonly string _statType;

        public StatMediator Mediator { get; }

        public float Value
        {
            get
            {
                var q = new Query(_statType, _value);
                Mediator.PerformQuery(this, q);
                return q.value;
            }
        }

        public Stat(StatMediator mediator, string statType, float value)
        {
            Mediator = mediator;
            _value = value;
            _statType = statType;
        }

        public Stat(StatMediator mediator, BaseStat baseStat)
        {
            Mediator = mediator;
            _value = baseStat.baseValue;
            _statType = baseStat.statType;
        }
    }
}