namespace Pancake.SaveData
{
    public enum EFileMode
    {
        Read,
        Write,
        Append
    }

    public enum ELocation
    {
        File,
        PlayerPrefs,
        InternalMS,
        Cache
    };

    public enum EDirectory
    {
        PersistentDataPath,
        DataPath
    }

    public enum EEncryptionType
    {
        None,
        Aes
    };

    public enum ECompressionType
    {
        None,
        Gzip
    };

    public enum EFormat
    {
        Json
    };
}