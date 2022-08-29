using System;

namespace Pancake.Tween
{
    public class CallbackTween : Tween
    {
        private readonly Action _action;
        private readonly bool _callIfCompletingInstantly;

        protected override bool Loopable => false;

        public CallbackTween(Action action, bool callIfCompletingInstantly)
        {
            _action = action;
            _callIfCompletingInstantly = callIfCompletingInstantly;
        }

        protected override void OnTweenStart(bool isCompletingInstantly)
        {
            bool canCall = !isCompletingInstantly || _callIfCompletingInstantly;

            if (canCall)
            {
                _action?.Invoke();
            }

            NewMarkCompleted();
        }

        protected override void OnTweenUpdate() { }

        protected override void OnTweenKill() { }

        protected override void OnTweenComplete() { }

        protected override void OnTweenReset(bool kill, ResetMode resetMode) { }

        protected override void OnTweenStartLoop(ResetMode loopResetMode) { }

        internal override void OnTimeDelayChange(float timeDelay) { }

        internal override void OnTimeScaleChange(float timeScale) { }
        internal override void OnTimeModeChange(TimeMode timeMode) { }

        internal override void OnEaseDelegateChange(EaseDelegate easeFunction) { }

        public override float OnGetDuration() { return 0.0f; }

        public override float OnGetElapsed() { return 0.0f; }

        public override int OnGetTweensCount() { return 1; }

        public override int OnGetPlayingTweensCount() { return IsPlaying ? 1 : 0; }

#pragma warning disable CS0809
        /// <summary>
        /// do not use this function for <see cref="CallbackTween"/>
        /// </summary>
        /// <param name="timeDelay"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        [Obsolete]
        public override ITween Delay(float timeDelay) { throw new NotImplementedException($"Delay() can not use for {nameof(CallbackTween)}"); }
#pragma warning restore CS0809
    }
}