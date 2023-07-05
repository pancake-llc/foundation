using System;
using Pancake.Scriptable;

namespace Pancake.LevelSystem
{
    using UnityEngine;

    [CreateAssetMenu(fileName = "scriptable_level_list_callback.asset", menuName = "Pancake/Scriptable/Lists/level callback")]
    [EditorIcon("scriptable_list")]
    public class ScriptableLevelListCallback : ScriptableList<Action>
    {
        public bool isLevelLoaded;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="action"></param>
        public void Subscribe(Action action)
        {
            if (isLevelLoaded)
            {
                action();
                return;
            }

            Add(action);
        }
    }
}