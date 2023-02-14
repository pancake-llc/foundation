using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using Pancake.OdinSerializer;
using UnityEngine;

namespace Pancake
{
    /// <summary>
    /// 
    /// </summary>
    public static class Data
    {
        // [Serializable] is unnecessary for OdinSerializer
        internal sealed class DataSegment
        {
            public byte[] value;
        }

        private static bool isInitialized;
        private static string path = string.Empty;
        private static int profile;
        private static Dictionary<string, DataSegment> datas;
        private const int INIT_SIZE = 64;
        private const DataFormat FORMAT = DataFormat.Binary;

        public static event Action OnSaveEvent;

        #region Internal Stuff

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void Init()
        {
            if (isInitialized) return;
            isInitialized = true;

            GeneratePath();
            Load();
            Runtime.AddFocusCallback(OnApplicationFocus);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static byte[] Serialize<T>(T data)
        {
            byte[] bytes = SerializationUtility.SerializeValue(data, FORMAT);
            return bytes;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static T Deserialize<T>(byte[] bytes)
        {
            var data = SerializationUtility.DeserializeValue<T>(bytes, FORMAT);
            return data;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void GeneratePath() { path = Path.Combine(Application.persistentDataPath, $"masterdata_{profile}.data"); }

        private static void OnApplicationFocus(bool focus)
        {
            if (!focus)
            {
                Save();
            }
        }

        #endregion

        #region Public API

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ChangeProfile(int value)
        {
            if (profile == value) return;

            Save();
            profile = value;
            GeneratePath();
            Load();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Save()
        {
            OnSaveEvent?.Invoke();

            byte[] bytes = Serialize(datas);
            File.WriteAllBytes(path, bytes);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Load()
        {
            if (!path.FileExists())
            {
                var stream = File.Create(path);
                stream?.Close();
            }

            byte[] bytes = File.ReadAllBytes(path);
            datas = Deserialize<Dictionary<string, DataSegment>>(bytes) ?? new Dictionary<string, DataSegment>(INIT_SIZE);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T Load<T>(string key)
        {
            datas.TryGetValue(key, out var value);
            if (value == null) throw Error.NotFound(key);
            return Deserialize<T>(value.value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryLoad<T>(string key, out T data)
        {
            bool hasKey;
            if (datas.TryGetValue(key, out var value))
            {
                data = Deserialize<T>(value.value);
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
            if (datas.TryGetValue(key, out var value))
            {
                value.value = Serialize<T>(data);
            }
            else
            {
                var dataSegment = new DataSegment {value = Serialize<T>(data)};
                datas.Add(key, dataSegment);
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