using System;

namespace Pancake.Apex
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public sealed class NotEmptyAttribute : DecoratorAttribute
    {
        public NotEmptyAttribute()
        {
            const string DEFAULT_MESSAGE = "{name} cannot be empty!";
            Format = DEFAULT_MESSAGE;
            Height = 22;
            Style = MessageStyle.Error;
        }

        #region [Parameters]

        /// <summary>
        /// Custom message format. 
        /// Arguments: {name}
        /// </summary>
        public string Format { get; set; }

        /// <summary>
        /// Height of help box message.
        /// </summary>
        public float Height { get; set; }

        public MessageStyle Style { get; set; }

        #endregion
    }
}