using UnityEngine;

namespace Pancake.UI
{
    [RequireComponent(typeof(QuestionView))]
    [EditorIcon("icon_popup")]
    public sealed class QuestionPopup : Popup<QuestionView>
    {
    }
}
