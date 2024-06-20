using System.Collections.Generic;

namespace Pancake.Sound
{
    public interface IAudioAsset
    {
        IEnumerable<IAudioIdentity> GetAllAudioEntities();
        
#if UNITY_EDITOR
        string AssetGuid { get; }
        string AssetName { get; }
#endif
    }
}