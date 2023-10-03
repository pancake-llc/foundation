namespace Pancake.Apex
{
    public class MaxVector2Attribute : ValidatorAttribute
    {
        public readonly float x;
        public readonly float y;

        public readonly string xProperty;
        public readonly string yProperty;

        public MaxVector2Attribute(float x, float y)
        {
            this.x = x;
            this.y = y;

            xProperty = string.Empty;
            yProperty = string.Empty;
        }

        public MaxVector2Attribute(string xProperty, string yProperty)
        {
            this.xProperty = xProperty;
            this.yProperty = yProperty;
        }

        public MaxVector2Attribute(float x, string yProperty)
        {
            this.x = x;
            this.yProperty = yProperty;

            xProperty = string.Empty;
        }

        public MaxVector2Attribute(string xProperty, float y)
        {
            this.xProperty = xProperty;
            this.y = y;

            yProperty = string.Empty;
        }
    }
}