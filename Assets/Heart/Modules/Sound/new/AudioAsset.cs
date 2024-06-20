using System;
using System.Collections.Generic;
using UnityEngine;

namespace Pancake.Sound
{
    public class AudioAsset : ScriptableObject
    {
        public AudioEntity[] entities;
#if UNITY_EDITOR
        [SerializeField] private string assetName;
        [SerializeField] private string assetGuid;

        public string AssetGuid
        {
            get
            {
                if (string.IsNullOrEmpty(assetGuid)) assetGuid = UnityEditor.AssetDatabase.AssetPathToGUID(UnityEditor.AssetDatabase.GetAssetPath(this));
                return assetGuid;
            }
            set => assetGuid = value;
        }
#endif

        public IEnumerable<IAudioIdentity> GetAllAudioEntities()
        {
            entities ??= Array.Empty<AudioEntity>();

            foreach (var entity in entities)
            {
                yield return entity;
            }
        }
    }
}