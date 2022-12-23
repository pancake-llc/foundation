using System;
using System.Collections.Generic;

namespace Pancake.Tween
{
    public class Sequence : Tween, ISequence
    {
        private readonly SequenceTweenRepository _tweenRepository = new SequenceTweenRepository();
        private readonly List<Tween> _playingTweens = new List<Tween>();

        protected override bool Loopable => true;

        private bool _durationCalculated;
        private float _cachedCalculatedDuration;

        protected override void OnTweenStart(bool isCompletingInstantly) { StartTweens(isCompletingInstantly); }

        protected override void OnTweenUpdate()
        {
            #region DELAY

            if (ValidateDelay()) return;

            #endregion

            if (_playingTweens.Count == 0)
            {
                NewMarkCompleted();

                return;
            }

            Tween tween = _playingTweens[0];

            tween.Update();

            if (tween.IsPlaying) return;

            _playingTweens.RemoveAt(0);

            if (_playingTweens.Count > 0)
            {
                Tween nextTween = _playingTweens[0];

                nextTween.Start();
            }
            else
            {
                base.Update();
            }
        }

        protected override void OnTweenKill()
        {
            foreach (Tween tween in _playingTweens)
            {
                tween.Kill();
            }

            _playingTweens.Clear();
        }

        protected override void OnTweenComplete()
        {
            foreach (Tween tween in _tweenRepository.Tweens)
            {
                if (tween.IsCompleted)
                {
                    continue;
                }

                if (!tween.IsPlaying)
                {
                    tween.Start(isCompletingInstantly: true);
                }

                tween.Complete();
            }

            _playingTweens.Clear();

            NewMarkCompleted();
        }

        protected override void OnTweenReset(bool kill, ResetMode resetMode)
        {
            for (int i = _tweenRepository.Tweens.Count - 1; i >= 0; --i)
            {
                Tween tween = _tweenRepository.Tweens[i];

                tween.Reset(kill, resetMode);
            }
        }

        protected override void OnTweenStartLoop(ResetMode loopResetMode) { StartTweens(isCompletingInstantly: false); }

        internal override void OnTimeDelayChange(float timeDelay) { }

        internal override void OnTimeScaleChange(float timeScale)
        {
            foreach (Tween tween in _tweenRepository.Tweens)
            {
                tween.OnTimeScaleChange(timeScale);
            }
        }

        internal override void OnTimeModeChange(TimeMode timeMode)
        {
            foreach (Tween tween in _tweenRepository.Tweens)
            {
                tween.OnTimeModeChange(timeMode);
            }
        }

        internal override void OnEaseDelegateChange(EaseDelegate easeFunction)
        {
            foreach (Tween tween in _tweenRepository.Tweens)
            {
                tween.OnEaseDelegateChange(easeFunction);
            }

            EaseFunction = easeFunction;
        }

        public override float OnGetDuration()
        {
            if (_durationCalculated)
            {
                return _cachedCalculatedDuration;
            }

            _durationCalculated = true;

            _cachedCalculatedDuration = 0.0f;

            foreach (Tween tween in _tweenRepository.Tweens)
            {
                _cachedCalculatedDuration += tween.GetDuration();
            }

            return _cachedCalculatedDuration;
        }

        public override float OnGetElapsed()
        {
            float totalDuration = 0.0f;

            foreach (Tween tween in _tweenRepository.Tweens)
            {
                totalDuration += tween.GetElapsed();
            }

            return totalDuration;
        }

        public override int OnGetTweensCount()
        {
            int totalTweens = 1;

            foreach (Tween tween in _tweenRepository.Tweens)
            {
                totalTweens += tween.GetTweensCount();
            }

            return totalTweens;
        }

        public override int OnGetPlayingTweensCount()
        {
            if (!IsPlaying)
            {
                return 0;
            }

            int totalTweens = 1;

            foreach (Tween tween in _tweenRepository.Tweens)
            {
                totalTweens += tween.OnGetPlayingTweensCount();
            }

            return totalTweens;
        }

        public override void OnGoto(float elapsed)
        {
            void GotoLocal(Tween tween, float value)
            {
                if (tween.IsPlaying) tween.Goto(value);
                else
                {
                    tween.OnStartLate(OnStartLateLocal);

                    void OnStartLateLocal()
                    {
                        tween.OnStartLate(null);
                        tween.Goto(value);
                    }
                }
            }

            foreach (Tween tween in _tweenRepository.Tweens)
            {
                if (tween is GroupTween groupTween)
                {
                    float percent = elapsed / groupTween.GetDuration();
                    GotoLocal(groupTween, percent);
                }
                else
                {
                    float d = tween.GetDuration();

                    if (elapsed > d)
                    {
                        elapsed -= d;
                        GotoLocal(tween, d);
                    }
                    else
                    {
                        GotoLocal(tween, elapsed);
                        break;
                    }
                }
            }
        }

        public void Append(ITween tween)
        {
            Tween castedTween = tween as Tween;

            if (castedTween == null)
            {
                throw new ArgumentNullException($"Tried to {nameof(Append)} a null {nameof(Tween)} on {nameof(Sequence)}");
            }

            bool canAdd = TweenUtils.CanAddTween(this, tween);

            if (!canAdd)
            {
                return;
            }

            _tweenRepository.Append(castedTween);

            castedTween.IsNested = true;

            _durationCalculated = false;
        }

        public void Join(ITween tween)
        {
            Tween castedTween = tween as Tween;

            if (castedTween == null)
            {
                throw new ArgumentNullException($"Tried to {nameof(Join)} a null {nameof(Tween)} on {nameof(Sequence)}");
            }

            bool canAdd = TweenUtils.CanAddTween(this, tween);

            if (!canAdd)
            {
                return;
            }

            _tweenRepository.Join(castedTween);

            castedTween.IsNested = true;

            _durationCalculated = false;
        }

        public void AppendCallback(Action callback, bool callIfCompletingInstantly = true) { Append(new CallbackTween(callback, callIfCompletingInstantly)); }

        public void JoinCallback(Action callback, bool callIfCompletingInstantly = true) { Join(new CallbackTween(callback, callIfCompletingInstantly)); }

        public void AppendResetableCallback(Action callback, Action reset, bool callIfCompletingInstantly = true)
        {
            Append(new ResetableCallbackTween(callback, reset, callIfCompletingInstantly));
        }

        public void JoinResetableCallback(Action callback, Action reset, bool callIfCompletingInstantly = true)
        {
            Join(new ResetableCallbackTween(callback, reset, callIfCompletingInstantly));
        }

        private void StartTweens(bool isCompletingInstantly)
        {
            _playingTweens.Clear();
            _playingTweens.AddRange(_tweenRepository.Tweens);

            for (int i = _playingTweens.Count - 1; i >= 0; --i)
            {
                Tween tween = _playingTweens[i];

                if (i == 0)
                {
                    tween.Start(isCompletingInstantly);
                }
            }

            if (_playingTweens.Count == 0)
            {
                NewMarkCompleted();
            }
        }
    }
}