using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using Newtonsoft.Json;
using UnityEngine;

namespace Pancake
{
    /// <summary>
    /// Master class holder data in memory
    /// </summary>
    public static class Data
    {
        private static bool isInitialized;
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

            LoadAll();
            App.AddFocusCallback(OnApplicationFocus);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static string Serialize<T>(T data) { return JsonConvert.SerializeObject(data); }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static T Deserialize<T>(string json) { return JsonConvert.DeserializeObject<T>(json); }

        private static void OnApplicationFocus(bool focus)
        {
            if (!focus) SaveAll();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void RequireNullCheck()
        {
            if (datas == null) LoadAll();
            if (datas == null) throw new NullReferenceException();
        }

        private static string GetPath => Path.Combine(Application.persistentDataPath, $"masterdata_{profile}.json");

        #endregion

        #region Public API

        public static bool IsInitialized => isInitialized;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ChangeProfile(int profile)
        {
            if (Data.profile == profile) return;

            SaveAll();
            Data.profile = profile;
            LoadAll();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SaveAll()
        {
            OnSaveEvent?.Invoke();

            string json = Serialize(datas);
            File.WriteAllText(GetPath, json);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void LoadAll()
        {
            if (!File.Exists(GetPath))
            {
                var stream = File.Create(GetPath);
                stream.Close();
            }

            string json = File.ReadAllText(GetPath);
            datas = Deserialize<Dictionary<string, string>>(json) ?? new Dictionary<string, string>(INIT_SIZE);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="default">If value of <paramref name="key"/> can not be found or empty! will return the default value of data type!</param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T Load<T>(string key, T @default = default)
        {
            RequireNullCheck();

            datas.TryGetValue(key, out string value);
            return !string.IsNullOrEmpty(value) ? Deserialize<T>(value) : @default;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryLoad<T>(string key, out T data)
        {
            RequireNullCheck();

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
            RequireNullCheck();

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