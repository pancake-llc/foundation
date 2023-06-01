using Pancake.Apex;

namespace Pancake
{
    /// <summary>
    /// provides a way to select the name of the class from a collected list of names of all classes that inherit from T
    /// </summary>
    public sealed class NamePickupAttribute : ViewAttribute
    {
        public string NameClassInherit { get; set; }

        public NamePickupAttribute(string nameClassInherit) { NameClassInherit = nameClassInherit; }
    }
}