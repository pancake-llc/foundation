using System;
using Pancake.Common;
using System.Collections;
using UnityEngine;

namespace Pancake.Rate
{
#if UNITY_IOS
    using UnityEngine.iOS;
#elif UNITY_ANDROID && PANCAKE_REVIEW
    using Google.Play.Review;
#endif

    [EditorIcon("icon_default")]
    public class AppRatingComponent : GameComponent
    {
#if UNITY_ANDROID
#if PANCAKE_REVIEW
        private ReviewManager _reviewManager;
        private PlayReviewInfo _playReviewInfo;
        private Coroutine _coroutine;
#endif
        private static event Action InitReviewEvent;
#endif

        private static event Action ReviewEvent;

        protected void OnEnable()
        {
#if UNITY_ANDROID
            InitReviewEvent += OnInitReview;
#endif
            ReviewEvent += OnReview;
        }

        private void OnReview()
        {
            if (!Application.isMobilePlatform) return;

#if UNITY_ANDROID && PANCAKE_REVIEW
            StartCoroutine(IeLaunchReview());
#elif UNITY_IOS
            Device.RequestStoreReview();
#endif
        }

        private void OnInitReview()
        {
            if (!Application.isMobilePlatform) return;

#if UNITY_ANDROID && PANCAKE_REVIEW
            _coroutine = StartCoroutine(IeInitReview());
#endif
        }

        protected void OnDisable()
        {
#if UNITY_ANDROID
            InitReviewEvent -= OnInitReview;
#endif
            ReviewEvent -= OnReview;
        }

#if UNITY_ANDROID && PANCAKE_REVIEW
        private IEnumerator IeInitReview(bool force = false)
        {
            if (_reviewManager == null) _reviewManager = new ReviewManager();

            var requestFlowOperation = _reviewManager.RequestReviewFlow();
            yield return requestFlowOperation;
            if (requestFlowOperation.Error != ReviewErrorCode.NoError)
            {
                if (force) C.GotoStore();
                yield break;
            }

            _playReviewInfo = requestFlowOperation.GetResult();
        }

        private IEnumerator IeLaunchReview()
        {
            if (_playReviewInfo == null)
            {
                if (_coroutine != null) StopCoroutine(_coroutine);
                yield return StartCoroutine(IeInitReview(true));
            }

            var launchFlowOperation = _reviewManager.LaunchReviewFlow(_playReviewInfo);
            yield return launchFlowOperation;
            _playReviewInfo = null;
            if (launchFlowOperation.Error != ReviewErrorCode.NoError)
            {
                C.GotoStore();
                yield break;
            }
        }
#endif

        public static void InitReview() { InitReviewEvent?.Invoke(); }
        public static void LaunchReview() { ReviewEvent?.Invoke(); }
    }
}