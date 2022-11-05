using System;

namespace Pancake
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
    public sealed class GameObjectWithComponentAttribute : DecoratorAttribute
    {
        public readonly Type type;

        public GameObjectWithComponentAttribute(Type type)
        {
            this.type = type;
            const string DEFAULT_MESSAGE = "{name} required {type} component!";
            Format = DEFAULT_MESSAGE;
            Height = 30;
            Style = MessageStyle.Error;
        }

        #region [Parameters]

        /// <summary>
        /// Custom message format. 
        /// Arguments: {name}, {type}
        /// </summary>
        public string Format { get; set; }

        /// <summary>
        /// Height of help box message.
        /// </summary>
        public float Height { get; set; }

        /// <summary>
        /// Help box message style.
        /// </summary>
        public MessageStyle Style { get; set; }

        #endregion
    }
}