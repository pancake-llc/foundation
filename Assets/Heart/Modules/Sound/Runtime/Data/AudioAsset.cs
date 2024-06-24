using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Pancake.Sound
{
    public class AudioAsset : ScriptableObject, IAudioAsset
    {
        public AudioEntity[] entities;

#if UNITY_EDITOR
        [field: SerializeField] public string AssetName { get; set; }

        [SerializeField] private string assetGuid;

        public string AssetGuid
        {
            get
            {
                if (string.IsNullOrEmpty(assetGuid)) assetGuid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(this));
                return assetGuid;
            }
            set => assetGuid = value;
        }
#endif

        public IEnumerable<IAudioIdentity> GetAllAudioEntities()
        {
            entities ??= Array.Empty<AudioEntity>();

            foreach (var data in entities)
            {
                yield return data;
            }
        }
    }
}