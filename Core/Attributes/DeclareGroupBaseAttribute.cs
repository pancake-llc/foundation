using System;

namespace Pancake
{
    public abstract class DeclareGroupBaseAttribute : Attribute
    {
        protected DeclareGroupBaseAttribute(string path) { Path = path ?? "None"; }

        public string Path { get; }
    }
}