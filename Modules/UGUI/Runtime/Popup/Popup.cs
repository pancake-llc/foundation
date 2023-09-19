using System;
using System.Collections.Generic;
using System.Threading;
using Pancake.Apex;
using Pancake.Linq;
using Pancake.Scriptable;
using UnityEngine;

namespace Pancake.UI
{
    [EditorIcon("script_popup")]
    [HideMonoScript]
    public class Popup : GameComponent
    {
        [SerializeField, Array] private List<UIPopup> popups = new List<UIPopup>();
        [SerializeField] private PopupShowEvent showPopupEvent;
        [SerializeField] private ScriptableEventNoParam closePopupEvent;

        private readonly Stack<UIPopup> _stacks = new Stack<UIPopup>();
        private Dictionary<string, UIPopup> _container = new Dictionary<string, UIPopup>();

        private void Start()
        {
            showPopupEvent.OnRaised += Show;
            closePopupEvent.OnRaised += Close;
        }

        private void Close()
        {
            if (_stacks.Count == 0)
            {
                Debug.LogWarning("[Popup] stack holder popup is empty, you can not close");
                return;
            }

            _stacks.Pop().Close();
            if (_stacks.Count >= 1)
            {
                var top = _stacks.Peek();
                top.Raise();
            }
        }

        private void CloseAll()
        {
            int count = _stacks.Count;
            for (int i = 0; i < count; i++)
            {
                _stacks.Pop().Close();
            }
        }

        private UIPopup Show(string name, Transform parent, bool callInit = true, CancellationToken token = default)
        {
            _container.TryGetValue(name, out var existInstance);
            if (existInstance == null)
            {
                var prefab = popups.Filter(p => p.name == name).FirstOrDefault();
                var instance = Instantiate(prefab, parent);
                _container.TryAdd(name, instance);
                return Show(instance, callInit, token);
            }

            return Show(existInstance, callInit, token);
        }

        private UIPopup Show(UIPopup instance, bool callInit, CancellationToken token)
        {
            var lastOrder = 0;
            if (_stacks.Count > 0)
            {
                var top = _stacks.Peek();
                if (top.Equals(instance))
                {
                    Debug.LogWarning("[Popup] you trying show popup is already displayed!");
                    return instance;
                }

                top.Collapse();
                lastOrder = top.SortingOrder;
            }

            instance.UpdateSortingOrder(lastOrder + 10);
            _stacks.Push(instance);
            if (callInit) instance.Init(); // Initialize if necessary before show
            instance.Show(token);
            return instance;
        }

        private void Release(string type)
        {
            _container.TryGetValue(type, out var instance);
            if (instance == null) return;

            Destroy(instance.gameObject);
            _container.Remove(type);
        }
    }
}