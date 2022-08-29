using UnityEngine;

namespace Pancake.SaveData
{
    public class DefaultSetting : ScriptableObject
    {
        [SerializeField] public Setting settings = new Setting();
        public bool logInfo;
        public bool logWarnings = true;
        public bool logErrors = true;
    }
}