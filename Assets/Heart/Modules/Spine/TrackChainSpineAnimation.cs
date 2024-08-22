#if PANCAKE_SPINE
using System;
using System.Collections;
using Alchemy.Inspector;
using Pancake.Common;
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
        [SerializeField] private StartupMode startupMode = StartupMode.OnEnabled;
        [SerializeField] private TrackData[] datas;

        private AsyncProcessHandle _handle;

        private void Awake()
        {
            if (startupMode == StartupMode.Awake) Play();
        }

        private void Start()
        {
            if (startupMode == StartupMode.Start) Play();
        }

        protected void OnEnable()
        {
            if (startupMode == StartupMode.OnEnabled) Play();
        }

        protected void OnDisable() { App.StopAndClean(ref _handle); }

        public void Play() { _handle = App.StartCoroutine(IePlay()); }

        private IEnumerator IePlay()
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
                        if (datas[i].overrideDuration)
                        {
                            yield return new WaitForSeconds(datas[i].duration);
                        }
                        else
                        {
                            yield return new WaitForSeconds(skeleton.Duration(skeleton.AnimationName));
                        }

                        count--;
                    }
                }
            }
        }
    }
}
#endif