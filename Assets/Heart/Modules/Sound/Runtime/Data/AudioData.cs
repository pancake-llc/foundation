using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Pancake.Sound
{
    public class AudioData : ScriptableObject
    {
        [SerializeField] private List<AudioAsset> assets = new();
        public IReadOnlyList<AudioAsset> Assets => assets;

#if UNITY_EDITOR
        public List<string> GetGuidList()
        {
            List<string> list = new List<string>();
            foreach (var asset in assets)
            {
                list.Add(asset.AssetGuid);
            }

            return list;
        }

        public void AddAsset(AudioAsset asset)
        {
            if (asset) assets.Add(asset);
        }

        public void RemoveEmpty()
        {
            for (int i = assets.Count - 1; i >= 0; i--)
            {
                if (!assets[i]) assets.RemoveAt(i);
            }
        }

        public void ReorderAssets(List<string> guids)
        {
            if (assets.Count != guids.Count)
            {
                Debug.LogError(AudioConstant.LOG_HEADER + "Asset count is not match!");
                return;
            }

            assets = assets.OrderBy(x => guids.IndexOf(x.AssetGuid)).ToList();
        }
#endif
    }
}