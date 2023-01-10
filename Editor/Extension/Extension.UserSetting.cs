using System;
using System.IO;
using UnityEngine;

namespace Pancake.Editor
{
    public static partial class InEditor
    {
        private const string DEFAULT_USER_SETTING_PATH = "UserSettings/{0}.asset";

        public class UserSetting<T> where T : class, new()
        {
            private T _settings;
            private readonly string _name;

            public UserSetting(string name) { _name = name; }

            public UserSetting() { _name = typeof(T).Name; }

            public T Settings
            {
                get
                {
                    if (_settings != null) return _settings;

                    LoadSetting();
                    return _settings;
                }
                set
                {
                    _settings = value;
                    SaveSetting();
                }
            }

            public void SaveSetting()
            {
                if (!"UserSettings".DirectoryExists()) Directory.CreateDirectory("UserSettings");

                try
                {
                    File.WriteAllText(string.Format(DEFAULT_USER_SETTING_PATH, _name), JsonUtility.ToJson(_settings, false));
                }
                catch (Exception e)
                {
                    Debug.LogError($"Unable to save {typeof(T).Name} to UserSettings!\n" + e.Message);
                }
            }

            public void LoadSetting()
            {
                _settings = new T();
                string path = string.Format(DEFAULT_USER_SETTING_PATH, _name);
                if (!path.FileExists())
                {
                    File.WriteAllText(path, JsonUtility.ToJson(_settings, false));
                    return;
                }

                string json = File.ReadAllText(path);
                _settings = JsonUtility.FromJson<T>(json);
            }

            public void DeleteSetting()
            {
                string path = string.Format(DEFAULT_USER_SETTING_PATH, _name);
                if (path.FileExists()) File.Delete(path);
            }
        }
    }
}