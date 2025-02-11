namespace Pancake.StatModifier
{
    public class Query
    {
        public readonly string statType;
        public float value;

        public Query(string statType, float value)
        {
            this.statType = statType;
            this.value = value;
        }
    }
}