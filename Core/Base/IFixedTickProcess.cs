namespace Pancake
{
    /// <summary>
    /// Wrapper simmilar FixedUpdate method of MonoBehaviour
    /// </summary>
    public interface IFixedTickProcess
    {
        void OnFixedTick();
    }
}