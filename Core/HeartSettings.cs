using Pancake.Attribute;
using UnityEngine;

namespace Pancake
{
    [HideMono]
    public class HeartSettings : ScriptableSettings<HeartSettings>
    {
        [SerializeField, LabelText("Privacy on FirstOpen")] private bool enablePrivacyFirstOpen;
    }
}