#if PANCAKE_SPINE
using System;
using System.Collections;
using Pancake.Apex;
using Pancake.Scriptable;
using Pancake.Threading.Tasks;
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

    [EditorIcon("csharp")]
    public class TrackChainSpineAnimation : GameComponent
    {
        [SerializeField] private SkeletonAnimation skeleton;
        [SerializeField] private bool loopLastestTrack;
        [SerializeField] private StartupMode startupMode = StartupMode.OnEnabled;

        [SerializeField, ShowIf(nameof(startupMode), StartupMode.Manual), Label("   Play Event")]
        private ScriptableEventNoParam playAnimationEvent;

        [SerializeField, Array] private TrackData[] datas;

        private AsyncProcessHandle _handle;

        private void Awake()
        {
            if (startupMode == StartupMode.Awake) Play();
        }

        private void Start()
        {
            if (startupMode == StartupMode.Start) Play();
        }

        protected override void OnEnabled()
        {
            if (startupMode == StartupMode.OnEnabled) Play();
            if (startupMode == StartupMode.Manual) playAnimationEvent.OnRaised += Play;
        }

        protected override void OnDisabled()
        {
            if (_handle != null)
            {
#if UNITY_EDITOR
                // avoid case app be destroy soon than other component
                try
                {
#endif
                    App.StopCoroutine(_handle);
#if UNITY_EDITOR
                }
                catch (Exception)
                {
                    // ignored
                }
#endif
            }

            if (startupMode == StartupMode.Manual) playAnimationEvent.OnRaised -= Play;
        }

        private void Play() { _handle = App.StartCoroutine(IePlay()); }

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