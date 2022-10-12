namespace Pancake.Editor
{
    public interface IInlineDecoratorSide
    {
        /// <summary>
        /// On which side should the space be reserved?
        /// </summary>
        InlineDecoratorSide GetSide();
    }
}