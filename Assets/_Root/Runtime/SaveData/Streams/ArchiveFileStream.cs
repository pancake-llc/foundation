using System.IO;

namespace Pancake.SaveData
{


    public class ArchiveFileStream : FileStream
    {
        private bool _isDisposed = false;

        public ArchiveFileStream(string path, EFileMode fileMode, int bufferSize, bool useAsync)
            : base(GetPath(path, fileMode),
                GetFileMode(fileMode),
                GetFileAccess(fileMode),
                FileShare.None,
                bufferSize,
                useAsync)
        {
        }

        // Gets a temporary path if necessary.
        protected static string GetPath(string path, EFileMode fileMode)
        {
            string directoryPath = IO.GetDirectoryPath(path);
            // Attempt to create the directory incase it does not exist if we are storing data.
            if (fileMode != EFileMode.Read && directoryPath != IO.PersistentDataPath) IO.CreateDirectory(directoryPath);
            if (fileMode != EFileMode.Write) return path;
            return path + IO.TEMPORARY_FILE_SUFFIX;
        }

        protected static FileMode GetFileMode(EFileMode fileMode)
        {
            if (fileMode == EFileMode.Read) return FileMode.Open;
            if (fileMode == EFileMode.Write) return FileMode.Create;
            return FileMode.Append;
        }

        protected static FileAccess GetFileAccess(EFileMode fileMode)
        {
            if (fileMode == EFileMode.Read) return FileAccess.Read;
            if (fileMode == EFileMode.Write) return FileAccess.Write;
            return FileAccess.Write;
        }

        protected override void Dispose(bool disposing)
        {
            // Ensure we only perform disposable once.
            if (_isDisposed) return;
            _isDisposed = true;

            base.Dispose(disposing);
        }
    }
}