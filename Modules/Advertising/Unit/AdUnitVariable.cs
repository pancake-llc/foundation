using System;
using Pancake.Apex;
using UnityEngine;

namespace Pancake.Monetization
{
    [Searchable]
    [Serializable]
    [EditorIcon("scriptable_variable")]
    public abstract class AdUnitVariable : ScriptableObject
    {
        [Message("When you change the platform need to double check if the id is empty?")]
#if UNITY_ANDROID
        [SerializeField] protected string androidId;
#elif UNITY_IOS
        [SerializeField] protected string iOSId;
#endif

        [NonSerialized] public Action loadedCallback;
        [NonSerialized] public Action faildedToLoadCallback;
        [NonSerialized] public Action displayedCallback;
        [NonSerialized] public Action faildedToDisplayCallback;
        [NonSerialized] public Action closedCallback;
        [NonSerialized] public Action<double, string, string, string, string> paidedCallback; // units are dollars

        public string Id
        {
            get
            {
#if UNITY_ANDROID
                return androidId;
#elif UNITY_IOS
                return iOSId;
#else
                return string.Empty;
#endif
            }
        }

        public abstract void Load();

        public abstract bool IsReady();

        public virtual AdUnitVariable Show()
        {
            if (!Application.isMobilePlatform || string.IsNullOrEmpty(Id) || !IsReady() || AdStatic.IsRemoveAd) return this;
            ShowImpl();
            return this;
        }

        protected abstract void ShowImpl();
        public abstract void Destroy();
    }
}