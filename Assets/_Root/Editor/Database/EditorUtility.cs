namespace Pancake.Database
{
    public static class EditorUtility
    {
        private const string ROOT_PATH_STORAGE = "Assets/_Root/DataStorages/";

        public static string StoragePath()
        {
            if (!ROOT_PATH_STORAGE.DirectoryExists()) ROOT_PATH_STORAGE.CreateDirectory();
            return ROOT_PATH_STORAGE;
        }
    }
}