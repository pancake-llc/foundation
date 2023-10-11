using System;

namespace Pancake.Apex
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
    public sealed class GameObjectWithComponentAttribute : DecoratorAttribute
    {
        public readonly Type type;

        public GameObjectWithComponentAttribute(Type type)
        {
            this.type = type;
            Format = "{name} required {type} component!";
            Style = MessageStyle.Error;
        }

        #region [Parameters]

        /// <summary>
        /// Custom message format. 
        /// Arguments: {name}, {type}
        /// </summary>
        public string Format { get; set; }

        /// <summary>
        /// Help box message style.
        /// </summary>
        public MessageStyle Style { get; set; }

        #endregion
    }
}