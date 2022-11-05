namespace Pancake.Editor
{
    public interface IMemberTarget
    {
        /// <summary>
        /// Target object of serialized member.
        /// </summary>
        object GetMemberTarget();
    }
}