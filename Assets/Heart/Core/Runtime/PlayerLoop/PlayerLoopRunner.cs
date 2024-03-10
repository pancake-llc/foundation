using System;
using System.Collections.Generic;

namespace Pancake.PlayerLoop
{
    public class PlayerLoopItem
    {
        public object target;
        public Action action;
    }

    internal class PlayerLoopRunner
    {
        public int ItemCount => _playerLoopItemList.Count;

        private readonly List<PlayerLoopItem> _playerLoopItemList = new();
        private readonly Dictionary<object, PlayerLoopItem> _playerLoopItemDictionary = new();
        private readonly bool _autoClear;

        public PlayerLoopRunner(bool autoClear = false) { _autoClear = autoClear; }

        public void Run()
        {
            foreach (var item in _playerLoopItemList.ToArray())
            {
                item.action?.Invoke();
            }

            if (_autoClear)
            {
                _playerLoopItemList.Clear();
                _playerLoopItemDictionary.Clear();
            }
        }

        public void Register(object target, Action action)
        {
            if (_playerLoopItemDictionary.ContainsKey(target)) return;

            var playerLoopItem = new PlayerLoopItem() {target = target, action = action};
            _playerLoopItemList.Add(playerLoopItem);
            _playerLoopItemDictionary.Add(playerLoopItem.target, playerLoopItem);
        }

        public void Unregister(object target)
        {
            if (!_playerLoopItemDictionary.ContainsKey(target)) return;

            int targetIndex = _playerLoopItemList.FindIndex(item => item.target == target);
            if (targetIndex != -1) _playerLoopItemList.RemoveAt(targetIndex);

            _playerLoopItemDictionary.Remove(target);
        }

        public void Clear()
        {
            _playerLoopItemList.Clear();
            _playerLoopItemDictionary.Clear();
        }
    }
}