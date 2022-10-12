namespace Pancake
{
    public sealed class TogglePropertyAttribute : ViewAttribute
    {
        public readonly string boolValue;

        public TogglePropertyAttribute(string boolValue) { this.boolValue = boolValue; }

        #region [Optional Parameters]

        /// <summary>
        /// Hide property, otherwise property will be disabled.
        /// </summary>
        public bool Hide { get; set; }

        #endregion
    }
}