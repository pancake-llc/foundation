namespace Pancake.Apex
{
    public sealed class ProgressBarAttribute : ViewAttribute
    {
        public readonly string text;

        public ProgressBarAttribute()
        {
            text = string.Empty;
            Height = 20;
        }

        public ProgressBarAttribute(string text)
            : this()
        {
            this.text = text;
        }

        #region [Optional Properties]

        public float Height { get; set; }

        #endregion
    }
}