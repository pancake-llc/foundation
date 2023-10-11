namespace Pancake.Apex
{
    public sealed class ColorAttribute : ManipulatorAttribute
    {
        public readonly float r;
        public readonly float g;
        public readonly float b;
        public readonly float a;

        public ColorAttribute(float r, float g, float b, float a)
        {
            this.r = r;
            this.g = g;
            this.b = b;
            this.a = a;
        }

        #region [Optional Parameters]

        public ColorTarget Target { get; set; }

        #endregion
    }
}