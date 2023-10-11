using System;

namespace Pancake.ApexEditor
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
    public sealed class DrawerTarget : Attribute
    {
        public readonly Type type;

        public DrawerTarget(Type type) { this.type = type; }

        #region [Optional Parameters]

        public bool Subclasses { get; set; }

        #endregion
    }
}