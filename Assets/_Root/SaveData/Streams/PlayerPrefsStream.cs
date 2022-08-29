using System.IO;
using UnityEngine;

namespace Pancake.SaveData
{
    internal class PlayerPrefsStream : MemoryStream
    {
        private string _path;
        private bool _append;
        private bool _isWriteStream = false;
        private bool _isDisposed = false;

        // This constructor should be used for read streams only.
        public PlayerPrefsStream(string path)
            : base(GetData(path, false))
        {
            _path = path;
            _append = false;
        }

        // This constructor should be used for write streams only.
        public PlayerPrefsStream(string path, int bufferSize, bool append = false)
            : base(bufferSize)
        {
            _path = path;
            _append = append;
            _isWriteStream = true;
        }

        private static byte[] GetData(string path, bool isWriteStream)
        {
            if (!PlayerPrefs.HasKey(path)) throw new FileNotFoundException("File \"" + path + "\" could not be found in PlayerPrefs");
            return System.Convert.FromBase64String(PlayerPrefs.GetString(path));
        }

        protected override void Dispose(bool disposing)
        {
            if (_isDisposed) return;
            _isDisposed = true;
            if (_isWriteStream && Length > 0)
            {
                if (_append)
                {
                    // Convert data back to bytes before appending, as appending Base-64 strings directly can corrupt the data.
                    var sourceBytes = System.Convert.FromBase64String(PlayerPrefs.GetString(_path));
                    var appendBytes = ToArray();
                    var finalBytes = new byte[sourceBytes.Length + appendBytes.Length];
                    System.Buffer.BlockCopy(sourceBytes,
                        0,
                        finalBytes,
                        0,
                        sourceBytes.Length);
                    System.Buffer.BlockCopy(appendBytes,
                        0,
                        finalBytes,
                        sourceBytes.Length,
                        appendBytes.Length);

                    PlayerPrefs.SetString(_path, System.Convert.ToBase64String(finalBytes));

                    PlayerPrefs.Save();
                }
                else
                    PlayerPrefs.SetString(_path + IO.TEMPORARY_FILE_SUFFIX, System.Convert.ToBase64String(ToArray()));

                // Save the timestamp to a separate key.
                PlayerPrefs.SetString("timestamp_" + _path, System.DateTime.UtcNow.Ticks.ToString());
            }

            base.Dispose(disposing);
        }
    }
}