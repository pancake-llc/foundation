#if PANCAKE_SPINE
using System;
using System.Collections;
using Pancake.Common;
using Spine.Unity;
using UnityEngine;

namespace Pancake.Spine
{
    [EditorIcon("icon_default")]
    public class TrackChainSpineGraphic : GameComponent
    {
        [SerializeField] private SkeletonGraphic skeleton;
        [SerializeField] private bool loopLastestTrack;
        [SerializeField] private EStartupMode startupMode = EStartupMode.OnEnabled;
        [SerializeField] private TrackData[] datas;

        private AsyncProcessHandle _handle;

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

        protected void OnDisable() { App.StopAndClean(ref _handle); }

        public void Play() { _handle = App.StartCoroutine(IePlay()); }

        private IEnumerator IePlay()
        {
            for (var i = 0; i < datas.Length; i++)
            {
                if (i == datas.Length - 1)
                {
                    skeleton.PlayOnly(datas[i].trackName, loopLastestTrack);
                }
                else
                {
                    uint count = datas[i].loopCount;
                    while (count > 0)
                    {
                        skeleton.PlayOnly(datas[i].trackName, count != 1);
                        if (datas[i].overrideDuration)
                        {
                            yield return new WaitForSeconds(datas[i].duration);
                        }
                        else
                        {
                            string trackName = datas[i].trackName;
                            yield return new WaitForSeconds(skeleton.Duration(trackName));
                        }

                        count--;
                    }
                }
            }
        }
    }
}
#endif