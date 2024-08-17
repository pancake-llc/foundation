namespace Pancake.StatModifier
{
    public class Query
    {
        public readonly StringConstant statType;
        public float value;

        public Query(StringConstant statType, float value)
        {
            this.statType = statType;
            this.value = value;
        }
    }
}