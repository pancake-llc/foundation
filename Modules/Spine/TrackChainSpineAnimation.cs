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

    public class TrackChainSpineAnimation : GameComponent
    {
        [SerializeField] private SkeletonAnimation skeleton;
        [SerializeField] private bool playOnAwake;
        [SerializeField] private bool loopLastestTrack;

        [SerializeField, Array] private TrackData[] datas;

        [Header("EVENT")] [SerializeField] private ScriptableEventNoParam playAnimationEvent;

        private IEnumerator _coroutine;

        private async void Awake()
        {
            await UniTask.WaitUntil(() => skeleton != null && skeleton.skeletonDataAsset != null);

            if (playOnAwake) Play();
        }

        protected override void OnEnabled()
        {
            if (playAnimationEvent != null) playAnimationEvent.OnRaised += Play;
        }

        protected override void OnDisabled()
        {
            if (_coroutine != null) App.StopCoroutine(_coroutine);
            if (playAnimationEvent != null) playAnimationEvent.OnRaised -= Play;
        }

        private void Play()
        {
            _coroutine = IePlay();
            App.StartCoroutine(_coroutine);
        }


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