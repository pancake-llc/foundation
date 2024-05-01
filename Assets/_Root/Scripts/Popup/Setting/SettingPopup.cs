using System.Threading.Tasks;
using UnityEngine;

namespace Pancake.UI
{
    [RequireComponent(typeof(SettingView))]
    [EditorIcon("icon_popup")]
    public sealed class SettingPopup : Popup<SettingView>
    {
    }
}