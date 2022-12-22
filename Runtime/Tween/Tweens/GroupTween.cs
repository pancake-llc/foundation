using System;
using System.Collections.Generic;

namespace Pancake.Tween
{
    public class GroupTween : Tween
    {
        private readonly List<Tween> _tweens = new List<Tween>();
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

            for (int i = _playingTweens.Count - 1; i >= 0; --i)
            {
                Tween tween = _playingTweens[i];

                tween.Update();

                if (!tween.IsPlaying)
                {
                    _playingTweens.RemoveAt(i);
                }
            }

            if (_playingTweens.Count == 0)
            {
                NewMarkCompleted();
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
            foreach (Tween tween in _playingTweens)
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
            for (int i = _tweens.Count - 1; i >= 0; --i)
            {
                Tween tween = _tweens[i];

                tween.Reset(kill, resetMode);
            }
        }

        protected override void OnTweenStartLoop(ResetMode loopResetMode) { StartTweens(isCompletingInstantly: false); }

        internal override void OnTimeDelayChange(float timeDelay) { }

        internal override void OnTimeScaleChange(float timeScale)
        {
            foreach (Tween tween in _tweens)
            {
                tween.OnTimeScaleChange(timeScale);
            }
        }

        internal override void OnTimeModeChange(TimeMode timeMode)
        {
            foreach (Tween tween in _tweens)
            {
                tween.OnTimeModeChange(timeMode);
            }
        }

        internal override void OnEaseDelegateChange(EaseDelegate easeFunction)
        {
            foreach (Tween tween in _tweens)
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

            foreach (Tween tween in _tweens)
            {
                _cachedCalculatedDuration += tween.GetDuration();
            }

            return _cachedCalculatedDuration;
        }

        public override float OnGetElapsed()
        {
            float totalDuration = 0.0f;

            foreach (Tween tween in _tweens)
            {
                totalDuration += tween.GetElapsed();
            }

            return totalDuration;
        }

        public override int OnGetTweensCount()
        {
            int totalTweens = 1;

            foreach (Tween tween in _tweens)
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

            foreach (Tween tween in _tweens)
            {
                totalTweens += tween.OnGetPlayingTweensCount();
            }

            return totalTweens;
        }

        public override void OnGoto(float elapsed)
        {
            foreach (Tween tween in _tweens)
            {
                tween.Goto(elapsed);
            }
        }

        public void Add(ITween tween)
        {
            Tween castedTween = tween as Tween;

            if (castedTween == null)
            {
                throw new ArgumentNullException($"Tried to {nameof(Add)} a null {nameof(Tween)} on {nameof(GroupTween)}");
            }

            if (IsPlaying)
            {
                return;
            }

            if (tween.IsPlaying)
            {
                return;
            }

            if (castedTween.IsNested)
            {
                return;
            }

            castedTween.IsNested = true;

            _tweens.Add(castedTween);

            _durationCalculated = false;
        }

        private void StartTweens(bool isCompletingInstantly)
        {
            _playingTweens.Clear();
            _playingTweens.AddRange(_tweens);

            for (int i = _playingTweens.Count - 1; i >= 0; --i)
            {
                Tween tween = _playingTweens[i];

                tween.Start(isCompletingInstantly);

                if (!tween.IsPlaying)
                {
                    _playingTweens.RemoveAt(i);
                }
            }

            if (_playingTweens.Count == 0)
            {
                NewMarkCompleted();
            }
        }
    }
}