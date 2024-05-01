using UnityEngine;

namespace Pancake.UI
{
    [RequireComponent(typeof(CreditView))]
    [EditorIcon("icon_popup")]
    public sealed class CreditPopup : Popup<CreditView>
    {
    }
}
