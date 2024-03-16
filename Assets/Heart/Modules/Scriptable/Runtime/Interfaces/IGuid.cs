namespace Pancake.Scriptable
{
    public interface IGuid
    {
        string Guid { get; set; }
        ECreationMode GuidCreateMode { get; set; }
    }
}