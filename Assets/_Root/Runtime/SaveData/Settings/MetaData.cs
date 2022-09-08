using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Pancake.SaveData
{
    public class MetaData : System.ICloneable
    {
        #region Default settings

        private static MetaData @default;
        private static DefaultSetting defaultSetting;
        private const string DEFAULT_SETTINGS_PATH = "ArchiveSetting";

        public static DefaultSetting DefaultSetting
        {
            get
            {
                if (defaultSetting == null)
                {
                    defaultSetting = Resources.Load<DefaultSetting>(DEFAULT_SETTINGS_PATH);

#if UNITY_EDITOR
                    if (defaultSetting == null)
                    {
                        defaultSetting = ScriptableObject.CreateInstance<DefaultSetting>();

                        CreateDefaultSettingsFolder();
                        AssetDatabase.CreateAsset(defaultSetting, $"{PATH_SAVE_SETTING}/{DEFAULT_SETTINGS_PATH}.asset");
                        AssetDatabase.SaveAssets();
                    }
#endif
                }

                return defaultSetting;
            }
        }

        public static MetaData Default
        {
            get
            {
                if (@default == null)
                {
                    if (DefaultSetting != null) @default = DefaultSetting.settings;
                }

                return @default;
            }
        }

        #endregion

        #region Fields

        private static readonly string[] ResourcesExtensions = new string[] {".txt", ".htm", ".html", ".xml", ".bytes", ".json", ".csv", ".yaml", ".fnt"};

        private ELocation _location;

        /// <summary>The location where we wish to store data. As it's not possible to save/load from File in WebGL, if the default location is File it will use PlayerPrefs instead.</summary>
        public ELocation Location
        {
            get
            {
                if (_location == ELocation.File && (Application.platform == RuntimePlatform.WebGLPlayer || Application.platform == RuntimePlatform.tvOS))
                    return ELocation.PlayerPrefs;
                return _location;
            }
            set { _location = value; }
        }

        /// <summary>The path associated with this MetaData object, if any.</summary>
        public string path = "Data.pak";

        /// <summary>The type of encryption to use when encrypting data, if any.</summary>
        public EEncryptionType encryptionType = EEncryptionType.None;

        /// <summary>The type of encryption to use when encrypting data, if any.</summary>
        public ECompressionType compressionType = ECompressionType.None;

        /// <summary>The password to use when encrypting data.</summary>
        public string encryptionPassword = "moonfalldown";

        /// <summary>The default directory in which to store files, and the location which relative paths should be relative to.</summary>
        public EDirectory eDirectory = EDirectory.PersistentDataPath;

        /// <summary>What format to use when serialising and deserialising data.</summary>
        public EFormat format = EFormat.Json;

        /// <summary>Whether we want to pretty print JSON.</summary>
        public bool prettyPrint;

        /// <summary>Any stream buffers will be set to this length in bytes.</summary>
        public int bufferSize = 2048;

        /// <summary>The text encoding to use for text-based format. Note that changing this may invalidate previous save data.</summary>
        public System.Text.Encoding encoding = System.Text.Encoding.UTF8;

        /// <summary>Whether we should check that the data we are loading from a file matches the method we are using to load it.</summary>
        [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
        public bool typeChecking = true;

        /// <summary>Enabling this ensures that only serialisable fields are serialised. Otherwise, possibly unsafe fields and properties will be serialised.</summary>
        [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
        public bool safeReflection = true;

        /// <summary>How many levels of hierarchy Save Data will serialise. This is used to protect against cyclic references.</summary>
        [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
        public int serializationDepthLimit = 64;

        /// <summary>The names of the Assemblies we should try to load our CustomTypes from.</summary>
        [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
        public string[] assemblyNames = new string[] {"Assembly-CSharp-firstpass", "Assembly-CSharp", "pancake@heart"};

        /// <summary>Gets the full, absolute path which this MetaData object identifies.</summary>
        public string FullPath
        {
            get
            {
                if (path == null) throw new System.NullReferenceException("The specified path to load Metadata cannot be null");

                if (IsAbsolute(path)) return path;

                if (Location == ELocation.File)
                {
                    if (eDirectory == EDirectory.PersistentDataPath)
                        return IO.PersistentDataPath + "/" + path;
                    if (eDirectory == EDirectory.DataPath)
                        return Application.dataPath + "/" + path;
                    throw new System.NotImplementedException("File directory \"" + eDirectory + "\" has not been implemented.");
                }

                return path;
            }
        }

        #endregion

        #region Constructors

        /// <summary>Creates a new MetaData object with the given path.</summary>
        /// <param name="path">The path associated with this MetaData object.</param>
        /// <param name="metadata">The settings we want to use to override the default settings.</param>
        public MetaData(string path = null, MetaData metadata = null)
            : this(true)
        {
            // if there are settings to merge, merge them.
            if (metadata != null) metadata.CopyInto(this);

            if (path != null) this.path = path;
        }

        /// <summary>Creates a new MetaData object with the given path.</summary>
        /// <param name="path">The path associated with this MetaData object.</param>
        /// <param name="enums">Accepts an EEncryptionType, ECompressionType, ELocation, EDirectory .</param>
        public MetaData(string path, params System.Enum[] enums)
            : this(enums)
        {
            if (path != null) this.path = path;
        }

        /// <summary>Creates a new MetaData object with the given path.</summary>
        /// <param name="enums">Accepts an EEncryptionType, ECompressionType, ELocation, EDirectory.</param>
        public MetaData(params System.Enum[] enums)
            : this(true)
        {
            foreach (var e in enums)
            {
                switch (e)
                {
                    case EEncryptionType type:
                        encryptionType = type;
                        break;
                    case ELocation location:
                        Location = location;
                        break;
                    case ECompressionType type:
                        compressionType = type;
                        break;
                    case EFormat fm:
                        format = fm;
                        break;
                    case EDirectory dir:
                        eDirectory = dir;
                        break;
                }
            }
        }

        /// <summary>Creates a new MetaData object with the given encryption settings.</summary>
        /// <param name="encryptionType">The type of encryption to use, if any.</param>
        /// <param name="encryptionPassword">The password to use when encrypting data.</param>
        public MetaData(EEncryptionType encryptionType, string encryptionPassword)
            : this(true)
        {
            this.encryptionType = encryptionType;
            this.encryptionPassword = encryptionPassword;
        }

        /// <summary>Creates a new MetaData object with the given path and encryption settings.</summary>
        /// <param name="path">The path associated with this MetaData object.</param>
        /// <param name="encryptionType">The type of encryption to use, if any.</param>
        /// <param name="encryptionPassword">The password to use when encrypting data.</param>
        /// <param name="metadata">The settings we want to use to override the default settings.</param>
        public MetaData(string path, EEncryptionType encryptionType, string encryptionPassword, MetaData metadata = null)
            : this(path, metadata)
        {
            this.encryptionType = encryptionType;
            this.encryptionPassword = encryptionPassword;
        }

        /* Base constructor which allows us to bypass defaults so it can be called by Editor serialization */
        [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
        public MetaData(bool applyDefaults)
        {
            if (applyDefaults)
                if (Default != null)
                    @default.CopyInto(this);
        }

        #endregion

        #region Editor methods

#if UNITY_EDITOR
        public const string PATH_SAVE_SETTING = "Assets/_Root/Resources";

        internal static void CreateDefaultSettingsFolder()
        {
            if (AssetDatabase.IsValidFolder(PATH_SAVE_SETTING)) return;
            System.IO.Directory.CreateDirectory(PATH_SAVE_SETTING);
        }
#endif

        #endregion

        #region Utility methods

        private static bool IsAbsolute(string path)
        {
            if (path.Length > 0 && (path[0] == '/' || path[0] == '\\')) return true;
            if (path.Length > 1 && path[1] == ':') return true;
            return false;
        }

        [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
        public object Clone()
        {
            var settings = new MetaData();
            CopyInto(settings);
            return settings;
        }

        private void CopyInto(MetaData newSettings)
        {
            newSettings._location = _location;
            newSettings.eDirectory = eDirectory;
            newSettings.format = format;
            newSettings.prettyPrint = prettyPrint;
            newSettings.path = path;
            newSettings.encryptionType = encryptionType;
            newSettings.encryptionPassword = encryptionPassword;
            newSettings.compressionType = compressionType;
            newSettings.bufferSize = bufferSize;
            newSettings.encoding = encoding;
            newSettings.typeChecking = typeChecking;
            newSettings.safeReflection = safeReflection;
            newSettings.assemblyNames = assemblyNames;
            newSettings.serializationDepthLimit = serializationDepthLimit;
        }

        #endregion
    }
}