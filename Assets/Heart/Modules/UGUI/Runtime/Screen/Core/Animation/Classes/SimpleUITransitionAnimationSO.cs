using Pancake.Apex;
using PrimeTween;
using UnityEngine;

namespace Pancake.UI
{
    [CreateAssetMenu(menuName = "Pancake/Misc/Simple UI Transition Asset", fileName = "simple_ui_transition_asset.asset")]
    [EditorIcon("scriptable_setting")]
    // ReSharper disable once InconsistentNaming
    public class SimpleUITransitionAnimationSO : UITransitionAnimationSO
    {
        [SerializeField] private float delay;
        [SerializeField] private float duration;
        [HorizontalLine] [SerializeField] private Ease ease = Ease.Default;
        [SerializeField] private Alignment beforeAlignment = Alignment.Center;
        [SerializeField] private Vector3 beforeScale = Vector3.one;
        [HorizontalLine] [SerializeField] private float beforeAlpha = 1f;
        [SerializeField] private Alignment afterAlignment = Alignment.Center;
        [SerializeField] private Vector3 afterScale = Vector3.one;
        [SerializeField] private float afterAlpha = 1f;

        private Vector3 _afterPosition;
        private Vector3 _beforePosition;
        private CanvasGroup _canvasGroup;


        public override float Duration => delay + duration;

        public override void SetTime(float time)
        {
            time = 0f.Max(time - delay);
            float progress = duration <= 0f ? 1f : (time / duration).Clamp01();
            var e = ease;
            if (ease == Ease.Custom || ease == Ease.Default) e = Ease.OutQuad;
            progress = StandardEasing.Evaluate(progress, e);
            var position = Vector3.Lerp(_beforePosition, _afterPosition, progress);
            var scale = Vector3.Lerp(beforeScale, afterScale, progress);
            float alpha = Math.Lerp(beforeAlpha, afterAlpha, progress);

            RectTransform.anchoredPosition = position;
            RectTransform.localScale = scale;
            _canvasGroup.alpha = alpha;
        }

        public override void Setup()
        {
            _beforePosition = beforeAlignment.ToPosition(RectTransform);
            _afterPosition = afterAlignment.ToPosition(RectTransform);
            if (!RectTransform.gameObject.TryGetComponent(out _canvasGroup)) _canvasGroup = RectTransform.gameObject.AddComponent<CanvasGroup>();
        }

        internal static SimpleUITransitionAnimationSO CreateInstance(
            float? duration = null,
            Ease? easeType = null,
            Alignment? beforeAlignment = null,
            Vector3? beforeScale = null,
            float? beforeAlpha = null,
            Alignment? afterAlignment = null,
            Vector3? afterScale = null,
            float? afterAlpha = null)
        {
            var anim = CreateInstance<SimpleUITransitionAnimationSO>();
            anim.SetParams(duration,
                easeType,
                beforeAlignment,
                beforeScale,
                beforeAlpha,
                afterAlignment,
                afterScale,
                afterAlpha);
            return anim;
        }

        private void SetParams(
            float? duration = null,
            Ease? easeType = null,
            Alignment? beforeAlignment = null,
            Vector3? beforeScale = null,
            float? beforeAlpha = null,
            Alignment? afterAlignment = null,
            Vector3? afterScale = null,
            float? afterAlpha = null)
        {
            if (duration.HasValue) this.duration = duration.Value;
            if (easeType.HasValue) ease = easeType.Value;
            if (beforeAlignment.HasValue) this.beforeAlignment = beforeAlignment.Value;
            if (beforeScale.HasValue) this.beforeScale = beforeScale.Value;
            if (beforeAlpha.HasValue) this.beforeAlpha = beforeAlpha.Value;
            if (afterAlignment.HasValue) this.afterAlignment = afterAlignment.Value;
            if (afterScale.HasValue) this.afterScale = afterScale.Value;
            if (afterAlpha.HasValue) this.afterAlpha = afterAlpha.Value;
        }
    }
}