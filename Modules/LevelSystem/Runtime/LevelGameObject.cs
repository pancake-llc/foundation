using System;
using UnityEngine;

namespace Pancake.LevelSystem
{
    [Serializable]
    public class LevelGameObject
    {
        /// <summary>
        /// local position
        /// </summary>
        public Vector3 pos;

        /// <summary>
        /// local rotation
        /// </summary>
        public Quaternion rot;

        /// <summary>
        /// local scale
        /// </summary>
        public Vector3 sc;

        /// <summary>
        /// prefab id
        /// </summary>
        public string id;

        /// <summary>
        /// child
        /// </summary>
        public LevelGameObject[] c;

        /// <summary>
        /// extra info
        /// </summary>
        public LevelExtraInfo[] ex;
    }
}