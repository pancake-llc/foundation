using System;

using Pancake.Scriptable;
using UnityEngine;

namespace Pancake.SceneFlow
{
    [EditorIcon("scriptable_variable")]
    [Serializable]
    [CreateAssetMenu(menuName = "Pancake/Game/Variables/Bool Daily Reset")]
    public class BoolDailyVariable : ScriptableVariable<bool>
    {
#if UNITY_EDITOR
#pragma warning disable 0414
        [SerializeField, TextArea] private string message = "The value will be reset after each new day \nRequiring the Saved attribute to be enabled";
#pragma warning restore 0414
#endif
        private string _lastTimeUpdated;

        public override void Load()
        {
            base.Load();
            
            string currentShortDate = DateTime.Now.ToShortDateString();
            DateTime.TryParse(currentShortDate, out var shortDate);
            var oneDay = new TimeSpan(24, 0, 0);
            _lastTimeUpdated = Data.Load(Guid + "_time", currentShortDate);
            DateTime.TryParse(_lastTimeUpdated, out var lastTime);

            var comparison = shortDate - lastTime;

            if (comparison >= oneDay) Value = InitialValue;
            else Value = Data.Load(Guid, InitialValue);
        }

        public override void Save()
        {
            Data.Save(Guid + "_time", _lastTimeUpdated);
            Data.Save(Guid, Value);
            base.Save();
        }

        public void RegisterTime(bool _) { _lastTimeUpdated = DateTime.Now.ToShortDateString(); }
#if UNITY_EDITOR
        protected override void OnValidate()
        {
            if (value == PreviousValue) return;
            ValueChanged();
        }
#endif
    }
}