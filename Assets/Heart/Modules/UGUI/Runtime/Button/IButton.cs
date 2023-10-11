using UnityEngine;

namespace Pancake.UI
{
    public interface IButton
    {
    }

    public interface IUniTMP
    {
        TMPro.TextMeshProUGUI Label { get; }
    }

    public interface IButtonAffect
    {
        /// <summary>
        /// default scale
        /// </summary>
        Vector3 DefaultScale { get; set; }

        /// <summary>
        /// is affect to self
        /// </summary>
        bool IsAffectToSelf { get; }

        /// <summary>
        /// object affect if IsAffectToSelf equal false.
        /// </summary>
        Transform AffectObject { get; }
    }
}