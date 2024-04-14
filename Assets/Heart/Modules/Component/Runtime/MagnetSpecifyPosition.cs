using System.Collections;
using Pancake;
using Pancake.Common;
using Pancake.Component;
using UnityEngine;

public class MagnetSpecifyPosition : MonoBehaviour
{
    private RectTransform _rectTransform;
    private VfxMagnetComponent _component;
    private readonly WaitForEndOfFrame _endOfFrame = new();


    private void OnEnable()
    {
        if (_rectTransform == null) _rectTransform = GetComponent<RectTransform>();
        if (_component == null) _component = FindFirstObjectByType<VfxMagnetComponent>();
        App.StartCoroutine(IeExecute());
    }

    private IEnumerator IeExecute()
    {
        yield return new WaitForSeconds(0.1f);
        if (_component != null) _component.transform.position = _rectTransform.ToWorldPosition();
    }
}