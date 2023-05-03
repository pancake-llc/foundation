namespace Pancake.ApexEditor
{
    public interface IMemberDeclaringObject
    {
        /// <summary>
        /// Declaring object of serialized member.
        /// </summary>
        object GetDeclaringObject();
    }
}