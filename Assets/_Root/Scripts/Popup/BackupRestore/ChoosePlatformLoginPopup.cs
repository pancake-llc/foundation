using Pancake.UI;
using UnityEngine;

namespace Pancake.SceneFlow
{
    [RequireComponent(typeof(ChoosePlatformLoginView))]
    [EditorIcon("icon_popup")]
    public sealed class ChoosePlatformLoginPopup : Popup<ChoosePlatformLoginView>
    {
    }
}
