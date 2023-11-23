using UnityEngine.Video;

namespace Pancake.Localization
{
    public class LocaleVideoClipBehaviour : LocaleBehaviourGeneric<LocaleVideoClip, VideoClip>
    {
        private void Reset() { TrySetComponentAndPropertyIfNotSet<VideoPlayer>("video"); }
    }
}