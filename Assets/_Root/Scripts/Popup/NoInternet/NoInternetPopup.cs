using UnityEngine;

namespace Pancake.UI
{
    [RequireComponent(typeof(NoInternetView))]
    [EditorIcon("icon_popup")]
    public sealed class NoInternetPopup : Popup<NoInternetView>
    {
    }
}
