using System;
using System.Collections.Generic;
using Pancake.Sound;
using UnityEditor;

namespace PancakeEditor.Sound
{
    public class IdGenerator : IUniqueIdGenerator
    {
        private bool _isInit;
        private IReadOnlyList<AudioAsset> _assetList;

        private void Init()
        {
            if (!EditorAudioEx.TryGetCoreData(out var coreData)) return;
            _assetList = coreData.Assets;
            _isInit = true;
        }

        public int GetSimpleUniqueId(EAudioType audioType)
        {
            if (!_isInit) Init();

            if (audioType == EAudioType.None) return default;

            int lastID = default;
            foreach (var asset in _assetList)
            {
                foreach (var entity in asset.GetAllAudioEntities())
                {
                    if (AudioExtension.GetAudioType(entity.Id) == audioType && entity.Id > lastID) lastID = entity.Id;
                }
            }

            if (lastID == default) return audioType.GetInitialId();

            return lastID + 1;
        }
    }

    public interface IUniqueIdGenerator
    {
        int GetSimpleUniqueId(EAudioType audioType);
    }
}