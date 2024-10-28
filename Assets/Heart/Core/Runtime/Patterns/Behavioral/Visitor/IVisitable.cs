namespace Pancake.Pattern
{
    public interface IVisitable
    {
        void Accept(IVisitor visitor);
    }
}