using System;
using System.IO;
using Newtonsoft.Json;
using UnityEngine;

namespace PancakeEditor
{
    public class UserSetting<T> where T : class, new()
    {
        internal const string DEFAULT_PATH = "UserSettings/{0}.asset";

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
            if (!Directory.Exists("UserSettings")) Directory.CreateDirectory("UserSettings");

            try
            {
                File.WriteAllText(string.Format(DEFAULT_PATH, _name), JsonConvert.SerializeObject(_settings));
            }
            catch (Exception e)
            {
                Debug.LogError($"Unable to save setting with path : {string.Format(DEFAULT_PATH, _name)}\n" + e.Message);
            }
        }

        public void LoadSetting()
        {
            _settings = new T();
            string path = string.Format(DEFAULT_PATH, _name);
            if (!File.Exists(path))
            {
                File.WriteAllText(path, JsonConvert.SerializeObject(_settings));
                return;
            }

            string json = File.ReadAllText(path);
            if (string.IsNullOrEmpty(json)) return;
            _settings = JsonConvert.DeserializeObject<T>(json) ?? new T();
        }

        public void DeleteSetting()
        {
            string path = string.Format(DEFAULT_PATH, _name);
            if (File.Exists(path)) File.Delete(path);
        }
    }
}