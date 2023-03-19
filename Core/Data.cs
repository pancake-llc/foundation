using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using Newtonsoft.Json;
using UnityEngine;

namespace Pancake
{
    /// <summary>
    /// 
    /// </summary>
    public static class Data
    {
        private static bool isInitialized;
        private static string path = string.Empty;
        private static int profile;
        private static Dictionary<string, string> datas;
        private const int INIT_SIZE = 64;

        public static event Action OnSaveEvent;

        #region Internal Stuff

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void Init()
        {
            if (isInitialized) return;
            isInitialized = true;

            GeneratePath();
            LoadAll();
            App.AddFocusCallback(OnApplicationFocus);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static string Serialize<T>(T data) { return JsonConvert.SerializeObject(data); }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static T Deserialize<T>(string json) { return JsonConvert.DeserializeObject<T>(json); }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void GeneratePath() { path = Path.Combine(Application.persistentDataPath, $"masterdata_{profile}.json"); }

        private static void OnApplicationFocus(bool focus)
        {
            if (!focus) SaveAll();
        }

        #endregion

        #region Public API

        public static bool IsInitialized => isInitialized;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ChangeProfile(int profile)
        {
            if (Data.profile == profile) return;

            SaveAll();
            Data.profile = profile;
            GeneratePath();
            LoadAll();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SaveAll()
        {
            OnSaveEvent?.Invoke();

            string json = Serialize(datas);
            File.WriteAllText(path, json);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void LoadAll()
        {
            if (!File.Exists(path))
            {
                var stream = File.Create(path);
                stream.Close();
            }

            var json = File.ReadAllText(path);
            datas = Deserialize<Dictionary<string, string>>(json) ?? new Dictionary<string, string>(INIT_SIZE);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T Load<T>(string key, T @default = default)
        {
            if (datas == null)
            {
                GeneratePath();
                LoadAll();
            }

            if (datas == null) throw new NullReferenceException();
            
            datas.TryGetValue(key, out string value);
            if (value != null && !string.IsNullOrEmpty(value)) return Deserialize<T>(value);
            Debug.Log($"Value of key <color=#52D5F2>'{key}'</color> can not be found or empty! So will return the default value of data type!");
            return @default;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryLoad<T>(string key, out T data)
        {
            if (datas == null)
            {
                GeneratePath();
                LoadAll();
            }

            if (datas == null) throw new NullReferenceException();
            
            bool hasKey;
            if (datas.TryGetValue(key, out string value))
            {
                data = Deserialize<T>(value);
                hasKey = true;
            }
            else
            {
                data = default;
                hasKey = false;
            }

            return hasKey;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Save<T>(string key, T data)
        {
            if (datas.TryGetValue(key, out string _))
            {
                datas[key] = Serialize(data);
            }
            else
            {
                string json = Serialize(data);
                datas.Add(key, json);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool HasKey(string key) => datas.ContainsKey(key);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void DeleteKey(string key) => datas.Remove(key);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void DeleteAll() => datas.Clear();

        #endregion
    }
}