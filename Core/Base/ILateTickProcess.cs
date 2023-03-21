namespace Pancake
{
    /// <summary>
    /// Wrapper simmilar LateUpdate method of MonoBehaviour
    /// </summary>
    public interface ILateTickProcess
    {
        void OnLateTick();
    }
}