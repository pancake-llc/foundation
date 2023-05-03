using System;

namespace Pancake.Apex
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public sealed class ObjectPreviewAttribute : DecoratorAttribute
    {
        public ObjectPreviewAttribute() { Height = 150.0f; }

        #region [Parameters]

        /// <summary>
        /// Height of the preview window.
        /// </summary>
        public float Height { get; set; }

        #endregion
    }
}