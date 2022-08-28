using System;
using System.Collections.Generic;
using Pancake.Core.Pattern;
using UnityEngine;

// ReSharper disable InconsistentNaming
namespace Pancake.Core.Tween
{
    [AddComponentMenu("")]
    public class TweenManager : AutoStartMonoSingleton<TweenManager>
    {
        private readonly List<Tween> _aliveTweens_Update = new List<Tween>();
        private readonly List<Tween> _aliveTweens_FixedUpdate = new List<Tween>();
        private readonly List<Tween> _aliveTweens_WaitForFixedUpdate = new List<Tween>();
        private readonly List<Tween> _aliveTweens_LateUpdate = new List<Tween>();
        private readonly List<Tween> _aliveTweens_WaitForEndOfFrame = new List<Tween>();

        private readonly List<Tween> _tweensToAdd_Update = new List<Tween>();
        private readonly List<Tween> _tweensToAdd_FixedUpdate = new List<Tween>();
        private readonly List<Tween> _tweensToAdd_WaitForFixedUpdate = new List<Tween>();
        private readonly List<Tween> _tweensToAdd_LateUpdate = new List<Tween>();
        private readonly List<Tween> _tweensToAdd_WaitForEndOfFrame = new List<Tween>();

        private readonly List<Tween> _tweensToRemove_Update = new List<Tween>();
        private readonly List<Tween> _tweensToRemove_FixedUpdate = new List<Tween>();
        private readonly List<Tween> _tweensToRemove_WaitForFixedUpdate = new List<Tween>();
        private readonly List<Tween> _tweensToRemove_LateUpdate = new List<Tween>();
        private readonly List<Tween> _tweensToRemove_WaitForEndOfFrame = new List<Tween>();

        private float _timeScale;

        public static float TimeScale { get => Instance._timeScale; set => Instance._timeScale = value; }

        private void Awake() { Init(); }

        private void OnDestroy() { Dispose(); }

        private void Init()
        {
            gameObject.hideFlags = HideFlags.HideInHierarchy;
            _timeScale = 1.0f;
            RuntimeUtilities.AddUpdate(UpdateMode.Update, UpdateTweens_Update);
            RuntimeUtilities.AddUpdate(UpdateMode.LateUpdate, UpdateTweens_LateUpdate);
            RuntimeUtilities.AddUpdate(UpdateMode.FixedUpdate, UpdateTweens_FixedUpdate);
            RuntimeUtilities.AddUpdate(UpdateMode.WaitForFixedUpdate, UpdateTweens_WaitForFixedUpdate);
            RuntimeUtilities.AddUpdate(UpdateMode.WaitForEndOfFrame, UpdateTweens_WaitForEndOfFrame);
        }

        private void Dispose()
        {
            RuntimeUtilities.RemoveUpdate(UpdateMode.Update, UpdateTweens_Update);
            RuntimeUtilities.RemoveUpdate(UpdateMode.LateUpdate, UpdateTweens_LateUpdate);
            RuntimeUtilities.RemoveUpdate(UpdateMode.FixedUpdate, UpdateTweens_FixedUpdate);
            RuntimeUtilities.RemoveUpdate(UpdateMode.WaitForFixedUpdate, UpdateTweens_WaitForFixedUpdate);
            RuntimeUtilities.RemoveUpdate(UpdateMode.WaitForEndOfFrame, UpdateTweens_WaitForEndOfFrame);

            _aliveTweens_Update.Clear();
            _aliveTweens_FixedUpdate.Clear();
            _aliveTweens_WaitForFixedUpdate.Clear();
            _aliveTweens_LateUpdate.Clear();
            _aliveTweens_WaitForEndOfFrame.Clear();

            _tweensToAdd_Update.Clear();
            _tweensToAdd_FixedUpdate.Clear();
            _tweensToAdd_WaitForFixedUpdate.Clear();
            _tweensToAdd_LateUpdate.Clear();
            _tweensToAdd_WaitForEndOfFrame.Clear();

            _tweensToRemove_Update.Clear();
            _tweensToRemove_FixedUpdate.Clear();
            _tweensToRemove_WaitForFixedUpdate.Clear();
            _tweensToRemove_LateUpdate.Clear();
            _tweensToRemove_WaitForEndOfFrame.Clear();
        }

        public static ISequence Sequence() { return new Sequence(); }

        public int GetAliveTweensCounts()
        {
            int aliveTweensCount = 0;

            for (int i = 0; i < _aliveTweens_Update.Count; ++i)
            {
                aliveTweensCount += _aliveTweens_Update[i].GetTweensCount();
            }

            for (int i = 0; i < _aliveTweens_FixedUpdate.Count; ++i)
            {
                aliveTweensCount += _aliveTweens_FixedUpdate[i].GetTweensCount();
            }

            for (int i = 0; i < _aliveTweens_WaitForFixedUpdate.Count; ++i)
            {
                aliveTweensCount += _aliveTweens_WaitForFixedUpdate[i].GetTweensCount();
            }

            for (int i = 0; i < _aliveTweens_LateUpdate.Count; ++i)
            {
                aliveTweensCount += _aliveTweens_LateUpdate[i].GetTweensCount();
            }

            for (int i = 0; i < _aliveTweens_WaitForEndOfFrame.Count; ++i)
            {
                aliveTweensCount += _aliveTweens_WaitForEndOfFrame[i].GetTweensCount();
            }

            return aliveTweensCount;
        }

        public int GetPlayingTweensCounts()
        {
            int aliveTweensCount = 0;

            for (int i = 0; i < _aliveTweens_Update.Count; ++i)
            {
                aliveTweensCount += _aliveTweens_Update[i].GetPlayingTweensCount();
            }

            for (int i = 0; i < _aliveTweens_FixedUpdate.Count; ++i)
            {
                aliveTweensCount += _aliveTweens_FixedUpdate[i].GetPlayingTweensCount();
            }

            for (int i = 0; i < _aliveTweens_WaitForFixedUpdate.Count; ++i)
            {
                aliveTweensCount += _aliveTweens_WaitForFixedUpdate[i].GetPlayingTweensCount();
            }

            for (int i = 0; i < _aliveTweens_LateUpdate.Count; ++i)
            {
                aliveTweensCount += _aliveTweens_LateUpdate[i].GetPlayingTweensCount();
            }

            for (int i = 0; i < _aliveTweens_WaitForEndOfFrame.Count; ++i)
            {
                aliveTweensCount += _aliveTweens_WaitForEndOfFrame[i].GetPlayingTweensCount();
            }

            return aliveTweensCount;
        }

        internal static void Add(Tween tween)
        {
            if (IsDestroyed) return;

            if (tween == null)
            {
                throw new ArgumentNullException($"Tried to play a null {nameof(Tween)} on {nameof(TweenManager)} instance");
            }

            if (tween.IsNested) return;

            if (tween.IsAlive)
            {
                Instance.TryStartTween(tween);
                return;
            }

            tween.IsAlive = true;

            switch (tween.UpdateMode)
            {
                case UpdateMode.Update:
                    Instance._tweensToAdd_Update.Add(tween);
                    break;
                case UpdateMode.LateUpdate:
                    Instance._tweensToAdd_LateUpdate.Add(tween);
                    break;
                case UpdateMode.FixedUpdate:
                    Instance._tweensToAdd_FixedUpdate.Add(tween);
                    break;
                case UpdateMode.WaitForFixedUpdate:
                    Instance._tweensToAdd_WaitForFixedUpdate.Add(tween);
                    break;
                case UpdateMode.WaitForEndOfFrame:
                    Instance._tweensToAdd_WaitForEndOfFrame.Add(tween);
                    break;
            }

            Instance.TryStartTween(tween);
        }

        private void TryStartTween(Tween tween)
        {
            if (!tween.IsPlaying) tween.Start();
        }

        private void UpdateTweens_Update()
        {
            foreach (Tween tween in _tweensToAdd_Update)
            {
                _aliveTweens_Update.Add(tween);
            }

            _tweensToAdd_Update.Clear();

            foreach (Tween tween in _aliveTweens_Update)
            {
                if (tween.IsPlaying && !tween.IsCompleted)
                {
                    tween.Update();
                }

                if (!tween.IsPlaying || tween.IsCompleted || tween.IsNested)
                {
                    _tweensToRemove_Update.Add(tween);
                }
            }

            foreach (Tween tween in _tweensToRemove_Update)
            {
                tween.IsAlive = false;

                _aliveTweens_Update.Remove(tween);
                _tweensToAdd_Update.Remove(tween);
            }

            _tweensToRemove_Update.Clear();
        }

        private void UpdateTweens_FixedUpdate()
        {
            foreach (Tween tween in _tweensToAdd_FixedUpdate)
            {
                _aliveTweens_FixedUpdate.Add(tween);
            }

            _tweensToAdd_FixedUpdate.Clear();

            foreach (Tween tween in _aliveTweens_FixedUpdate)
            {
                if (tween.IsPlaying && !tween.IsCompleted)
                {
                    tween.Update();
                }

                if (!tween.IsPlaying || tween.IsCompleted || tween.IsNested)
                {
                    _tweensToRemove_FixedUpdate.Add(tween);
                }
            }

            foreach (Tween tween in _tweensToRemove_FixedUpdate)
            {
                tween.IsAlive = false;

                _aliveTweens_FixedUpdate.Remove(tween);
                _tweensToAdd_FixedUpdate.Remove(tween);
            }

            _tweensToRemove_FixedUpdate.Clear();
        }

        private void UpdateTweens_LateUpdate()
        {
            foreach (Tween tween in _tweensToAdd_LateUpdate)
            {
                _aliveTweens_LateUpdate.Add(tween);
            }

            _tweensToAdd_LateUpdate.Clear();

            foreach (Tween tween in _aliveTweens_LateUpdate)
            {
                if (tween.IsPlaying && !tween.IsCompleted)
                {
                    tween.Update();
                }

                if (!tween.IsPlaying || tween.IsCompleted || tween.IsNested)
                {
                    _tweensToRemove_LateUpdate.Add(tween);
                }
            }

            foreach (Tween tween in _tweensToRemove_LateUpdate)
            {
                tween.IsAlive = false;

                _aliveTweens_LateUpdate.Remove(tween);
                _tweensToAdd_LateUpdate.Remove(tween);
            }

            _tweensToRemove_LateUpdate.Clear();
        }

        private void UpdateTweens_WaitForFixedUpdate()
        {
            foreach (Tween tween in _tweensToAdd_WaitForFixedUpdate)
            {
                _aliveTweens_WaitForFixedUpdate.Add(tween);
            }

            _tweensToAdd_WaitForFixedUpdate.Clear();

            foreach (Tween tween in _aliveTweens_WaitForFixedUpdate)
            {
                if (tween.IsPlaying && !tween.IsCompleted)
                {
                    tween.Update();
                }

                if (!tween.IsPlaying || tween.IsCompleted || tween.IsNested)
                {
                    _tweensToRemove_WaitForFixedUpdate.Add(tween);
                }
            }

            foreach (Tween tween in _tweensToRemove_WaitForFixedUpdate)
            {
                tween.IsAlive = false;

                _aliveTweens_WaitForFixedUpdate.Remove(tween);
                _tweensToAdd_WaitForFixedUpdate.Remove(tween);
            }

            _tweensToRemove_WaitForFixedUpdate.Clear();
        }

        private void UpdateTweens_WaitForEndOfFrame()
        {
            foreach (Tween tween in _tweensToAdd_WaitForEndOfFrame)
            {
                _aliveTweens_WaitForEndOfFrame.Add(tween);
            }

            _tweensToAdd_WaitForEndOfFrame.Clear();

            foreach (Tween tween in _aliveTweens_WaitForEndOfFrame)
            {
                if (tween.IsPlaying && !tween.IsCompleted)
                {
                    tween.Update();
                }

                if (!tween.IsPlaying || tween.IsCompleted || tween.IsNested)
                {
                    _tweensToRemove_WaitForEndOfFrame.Add(tween);
                }
            }

            foreach (Tween tween in _tweensToRemove_WaitForEndOfFrame)
            {
                tween.IsAlive = false;

                _aliveTweens_WaitForEndOfFrame.Remove(tween);
                _tweensToAdd_WaitForEndOfFrame.Remove(tween);
            }

            _tweensToRemove_WaitForEndOfFrame.Clear();
        }
    }
}