namespace Pancake.Scriptable
{
    /// <summary>
    /// Interface for objects that can be saved and loaded
    /// </summary>
    public interface ISave
    {
        void Save();
        void Load();
    }
}