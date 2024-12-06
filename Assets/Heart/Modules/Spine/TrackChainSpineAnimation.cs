#if PANCAKE_SPINE
using System;
using Sirenix.OdinInspector;
using Spine.Unity;
using UnityEngine;

namespace Pancake.Spine
{
    [Serializable]
    public class TrackData
    {
        [SpineAnimation(dataField = "skeleton")] public string trackName;
        public bool overrideDuration;
        [ShowIf("overrideDuration")] public float duration;
        public uint loopCount;
    }

    [EditorIcon("icon_default")]
    public class TrackChainSpineAnimation : GameComponent
    {
        [SerializeField] private SkeletonAnimation skeleton;
        [SerializeField] private bool loopLastestTrack;
        [SerializeField] private EStartupMode startupMode = EStartupMode.OnEnabled;
        [SerializeField] private TrackData[] datas;

        private void Awake()
        {
            if (startupMode == EStartupMode.Awake) Play();
        }

        private void Start()
        {
            if (startupMode == EStartupMode.Start) Play();
        }

        protected void OnEnable()
        {
            if (startupMode == EStartupMode.OnEnabled) Play();
        }

        public void Play() { Playable(); }

        private async void Playable()
        {
            for (var i = 0; i < datas.Length; i++)
            {
                if (i == datas.Length - 1)
                {
                    skeleton.Play(datas[i].trackName, loopLastestTrack);
                }
                else
                {
                    uint count = datas[i].loopCount;
                    while (count > 0)
                    {
                        skeleton.Play(datas[i].trackName, count != 1);
                        if (datas[i].overrideDuration) await Awaitable.WaitForSecondsAsync(datas[i].duration);
                        else await Awaitable.WaitForSecondsAsync(skeleton.Duration(skeleton.AnimationName));

                        count--;
                    }
                }
            }
        }
    }
}
#endif