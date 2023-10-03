namespace Pancake.Apex
{
    public sealed class FoldoutAttribute : ContainerAttribute
    {
        public FoldoutAttribute(string name)
            : base(name)
        {
            Style = string.Empty;
        }

        #region [Optional Parameters]

        public string Style { get; set; }

        #endregion
    }
}