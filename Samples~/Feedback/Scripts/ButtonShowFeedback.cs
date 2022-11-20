using Pancake.Feedback;
using UnityEngine;

public class ButtonShowFeedback : MonoBehaviour
{
    public PopupFeedback prefab;
    public RectTransform canvaPopup;

    private PopupFeedback _popupFeedback;

    public void Show()
    {
        if (_popupFeedback == null) _popupFeedback = Instantiate(prefab, canvaPopup, false);

        _popupFeedback.gameObject.SetActive(true);
        _popupFeedback.Show();
    }
}