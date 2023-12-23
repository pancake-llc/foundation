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

        /// <summary>
        /// Style value canbe <br/>
        /// default <br/>
        /// Highlight <br/>
        /// Header <br/>
        /// Group <br/>
        /// </summary>
        public string Style { get; set; }

        #endregion
    }
}