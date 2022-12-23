namespace Pancake
{
    /// <summary>
    /// Wrapper simmilar LateUpdate method of MonoBehaviour
    /// </summary>
    public interface ILateTickSystem
    {
        void LateTick();
    }
}