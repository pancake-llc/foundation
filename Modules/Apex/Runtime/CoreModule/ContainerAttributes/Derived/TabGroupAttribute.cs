namespace Pancake.Apex
{
    public sealed class TabGroupAttribute : ContainerAttribute
    {
        public readonly string Key;

        public TabGroupAttribute(string name, string Id)
            : base(name)
        {
            this.Key = Id;
        }
    }
}