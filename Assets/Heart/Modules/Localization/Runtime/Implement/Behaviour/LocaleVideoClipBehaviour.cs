using UnityEngine.Video;

namespace Pancake.Localization
{
    [EditorIcon("csharp")]
    public class LocaleVideoClipBehaviour : LocaleBehaviourGeneric<LocaleVideoClip, VideoClip>
    {
        private void Reset() { TrySetComponentAndPropertyIfNotSet<VideoPlayer>("video"); }
    }
}