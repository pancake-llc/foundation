using System;

namespace Pancake.Attribute
{
    public abstract class DeclareGroupBaseAttribute : System.Attribute
    {
        protected DeclareGroupBaseAttribute(string path) { Path = path ?? "None"; }

        public string Path { get; }
    }
}