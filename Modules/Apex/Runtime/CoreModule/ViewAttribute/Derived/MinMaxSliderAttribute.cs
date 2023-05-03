namespace Pancake.Apex
{
    public sealed class MinMaxSliderAttribute : ViewAttribute
    {
        public readonly float min;
        public readonly float max;

        public MinMaxSliderAttribute(float min, float max)
        {
            this.min = min;
            this.max = max;
        }
    }
}