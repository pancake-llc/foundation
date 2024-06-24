using System.Collections.Generic;
using Pancake.Sound;
using UnityEditor;

namespace PancakeEditor.Sound
{
    public class IdGenerator : IUniqueIdGenerator
    {
        private bool _isInit;
        private List<IAudioAsset> _assetList;

        private void Init()
        {
            _assetList = new List<IAudioAsset>();

            foreach (string guid in LibraryDataContainer.Data.Settings.guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (AssetDatabase.LoadAssetAtPath(path, typeof(IAudioAsset)) is IAudioAsset asset) _assetList.Add(asset);
            }

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