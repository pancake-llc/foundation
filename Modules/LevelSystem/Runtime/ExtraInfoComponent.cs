using System;
using Pancake.Apex;
using UnityEngine;

namespace Pancake.LevelSystem
{
    [HideMonoScript]
    [EditorIcon("script_mono")]
    public class ExtraInfoComponent : GameComponent
    {
        [SerializeField, Array] private ExtraInfo[] extraInfos;

        public void Init(ExtraInfo[] infos)
        {
            extraInfos = new ExtraInfo[infos.Length];
            infos.CopyTo(extraInfos, 0);
        }

        public ExtraInfo[] GetAllExtraInfos()
        {
            if (extraInfos == null) return Array.Empty<ExtraInfo>();
            var copy = new ExtraInfo[extraInfos.Length];
            extraInfos.CopyTo(copy, 0);
            return copy;
        }

        /// <summary>
        /// Get level extra by id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ExtraInfo GetExtraInfo(string id)
        {
            if (extraInfos == null) return null;

            foreach (var info in extraInfos)
            {
                if (info.id == id) return info;
            }

            return null;
        }

        /// <summary>
        /// Get extra info and try convert it to int
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public int? GetInt(string id)
        {
            var ex = GetExtraInfo(id);
            if (ex == null) return null;

            if (int.TryParse(ex.value, out int value)) return value;

            return null;
        }

        /// <summary>
        /// Get extra info and try convert it to double
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public double? GetDouble(string id)
        {
            var ex = GetExtraInfo(id);
            if (ex == null) return null;

            if (double.TryParse(ex.value, out double value)) return value;

            return null;
        }

        /// <summary>
        /// Get extra info and try convert it to double
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public float? GetFloat(string id)
        {
            var ex = GetExtraInfo(id);
            if (ex == null) return null;

            if (float.TryParse(ex.value, out float value)) return value;

            return null;
        }

        /// <summary>
        /// Get extra info and try convert it to double
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public string GetString(string id) { return GetExtraInfo(id)?.value; }
    }
}