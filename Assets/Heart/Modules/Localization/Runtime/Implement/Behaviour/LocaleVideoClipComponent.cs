using UnityEngine.Video;

namespace Pancake.Localization
{
    [EditorIcon("csharp")]
    public class LocaleVideoClipComponent : LocaleComponentGeneric<LocaleVideoClip, VideoClip>
    {
        private void Reset() { TrySetComponentAndPropertyIfNotSet<VideoPlayer>("video"); }
    }
}