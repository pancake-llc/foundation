using System;

namespace Pancake.Apex
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public sealed class DisplayAsStringAttribute : ViewAttribute
    {
        #region [Optional]

        public string Style { get; set; }

        #endregion
    }
}