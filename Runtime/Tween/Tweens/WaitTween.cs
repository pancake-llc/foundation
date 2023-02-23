using System;

namespace Pancake.Tween
{
    public class WaitTween : Tween
    {
        protected override bool Loopable => true;

        public WaitTween(float duration) { timeDelay = duration; }

        protected override void OnTweenStart(bool isCompletingInstantly) { elapsedDelay = 0.0f; }

        protected override void OnTweenUpdate()
        {
            float deltaTime = Runtime.GetDeltaTime(TimeMode);
            float dt = deltaTime * TweenManager.TimeScale * TimeScale;

            elapsedDelay += dt;

            if (elapsedDelay >= timeDelay)
            {
                NewMarkCompleted();
            }
        }

        protected override void OnTweenKill() { }

        protected override void OnTweenComplete()
        {
            elapsedDelay = timeDelay;

            NewMarkCompleted();
        }

        protected override void OnTweenReset(bool kill, ResetMode resetMode) { elapsedDelay = 0.0f; }

        protected override void OnTweenStartLoop(ResetMode loopResetMode) { elapsedDelay = 0.0f; }
        internal override void OnTimeDelayChange(float timeDelay) { }

        internal override void OnTimeScaleChange(float timeScale) { }
        internal override void OnTimeModeChange(TimeMode timeMode) { }

        internal override void OnEaseDelegateChange(EaseDelegate easeFunction) { }

        public override float OnGetDuration() { return timeDelay; }

        public override float OnGetElapsed() { return elapsedDelay; }

        public override int OnGetTweensCount() { return 1; }

        public override int OnGetPlayingTweensCount() { return IsPlaying ? 1 : 0; }

        public override void OnGoto(float elapsed)
        {
            elapsedDelay = elapsed;
        }

#pragma warning disable CS0809
        /// <summary>
        /// do not use this function for <see cref="WaitTween"/>
        /// </summary>
        /// <param name="timeDelay"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        [Obsolete]
        public override ITween Delay(float timeDelay) { throw new NotImplementedException($"Delay() can not use for {nameof(WaitTween)}"); }
#pragma warning restore CS0809
    }
}