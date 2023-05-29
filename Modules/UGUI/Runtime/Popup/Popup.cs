using System.Collections.Generic;
using Pancake.Apex;
using Pancake.Scriptable;
using UnityEngine;

namespace Pancake.UI
{
    public class Popup : GameComponent
    {
        [SerializeField] private ScriptableEventGameObject eventDisplayPopup;
        
        private readonly Stack<UIPopup> _stacks = new Stack<UIPopup>();
        private int _sortingOrder;


        private void Close()
        {
            if (_stacks.Count == 0) 
            {
                Debug.LogWarning("[Popup] stack holder popup is empty, you can not close");
                return;
            }
            
            _stacks.Pop().Close();
            var order = 0;
            if (_stacks.Count >= 1)
            {
                var top = _stacks.Peek();
                top.Raise();
                if (_stacks.Count > 1) order = top.SortingOrder - 10;
            }

            _sortingOrder = order;
        }

        private void CloseAll()
        {
            int count = _stacks.Count;
            for (int i = 0; i < count; i++)
            {
                _stacks.Pop().Close();
            }

            _sortingOrder = 0;
        }


        private void Show(GameObject prefab)
        {
            
        }
    }

}