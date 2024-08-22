using UnityEngine.Video;

namespace Pancake.Localization
{
    [EditorIcon("icon_default")]
    public class LocaleVideoClipComponent : LocaleComponentGeneric<LocaleVideoClip, VideoClip>
    {
        private void Reset() { TrySetComponentAndPropertyIfNotSet<VideoPlayer>("video"); }
    }
}