namespace Pancake.Rate
{
    using Scriptable;
    using System.Collections;
    using UnityEngine;

#if UNITY_IOS
using UnityEngine.iOS;
#elif UNITY_ANDROID && PANCAKE_REVIEW
    using Google.Play.Review;
#endif

    public class AppRatingComponent : GameComponent
    {
#if UNITY_ANDROID
#if PANCAKE_REVIEW
        private ReviewManager _reviewManager;
        private PlayReviewInfo _playReviewInfo;
        private Coroutine _coroutine;
#endif
        [SerializeField] private ScriptableEventNoParam initReviewEvent;
#endif
        [SerializeField] private ScriptableEventNoParam reviewEvent;

        protected override void OnEnabled()
        {
#if UNITY_ANDROID
            initReviewEvent.OnRaised += OnInitReview;
#endif
            reviewEvent.OnRaised += OnReview;
        }

        private void OnReview()
        {
            if (!Application.isMobilePlatform) return;
            
#if UNITY_ANDROID && PANCAKE_REVIEW
            StartCoroutine(LaunchReview());
#elif UNITY_IOS
            Device.RequestStoreReview();
#endif
        }

        private void OnInitReview()
        {
            if (!Application.isMobilePlatform) return;

#if UNITY_ANDROID && PANCAKE_REVIEW
            _coroutine = StartCoroutine(InitReview());
#endif
        }

        protected override void OnDisabled()
        {
#if UNITY_ANDROID
            initReviewEvent.OnRaised -= OnInitReview;
#endif
            reviewEvent.OnRaised -= OnReview;
        }

#if UNITY_ANDROID && PANCAKE_REVIEW
        private IEnumerator InitReview(bool force = false)
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

        private IEnumerator LaunchReview()
        {
            if (_playReviewInfo == null)
            {
                if (_coroutine != null) StopCoroutine(_coroutine);
                yield return StartCoroutine(InitReview(true));
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
    }
}