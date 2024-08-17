namespace Pancake.Common
{
    public interface IVisitable
    {
        void Accept(IVisitor visitor);
    }
}