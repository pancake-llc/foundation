using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Pancake.UI
{
    public abstract class UIPopup : GameComponent
    {
        [SerializeField] protected UnityEvent onBeforeShow;
        [SerializeField] protected UnityEvent onAfterShow;
        [SerializeField] protected UnityEvent onBeforeClose;
        [SerializeField] protected UnityEvent onAfterClose;

        [SerializeField] protected bool closeByClickContainer;
        [SerializeField] protected bool closeByClickBackground;
        [SerializeField] protected bool closeByBackButton;
        [SerializeField] private List<Button> closeButtons = new List<Button>();
    }
}