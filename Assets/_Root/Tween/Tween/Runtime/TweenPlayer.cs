using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Pancake.Core.Tween
{
    public enum WrapMode
    {
        Clamp,
        Loop,
        PingPong
    }


    public enum ArrivedAction
    {
        KeepPlaying = 0,
        StopOnForwardArrived = 1,
        StopOnBackArrived = 2,
        AlwaysStopOnArrived = 3
    }


    public enum PlayDirection
    {
        Forward,
        Back
    }


    /// <summary>
    /// TweenPlayer
    /// </summary>
    [AddComponentMenu("Miscellaneous/Tween Player")]
    public partial class TweenPlayer : ConfigurableUpdateComponent
    {
        private const float MIN_DURATION = 0.0001f;

        [SerializeField, Min(MIN_DURATION)] private float duration = 1f;

        /// <summary>
        /// Use unscaled delta time or normal delta time?
        /// </summary>
        public TimeMode timeMode = TimeMode.Unscaled;

        /// <summary>
        /// The wrap mode for playing.
        /// </summary>
        public WrapMode wrapMode = WrapMode.Clamp;

        /// <summary>
        /// Controls whether playback stops when the animation ends.
        /// </summary>
        public ArrivedAction arrivedAction = ArrivedAction.AlwaysStopOnArrived;

        /// <summary>
        /// Samples all animation states when this TweenPLayer awakes, it can avoid flashing caused by error initial states.
        /// </summary>
        public bool sampleOnAwake = true;

        [SerializeField] private UnityEvent onForwardArrived = default;
        [SerializeField] private UnityEvent onBackArrived = default;

        [SerializeField, SerializeReference] private List<TweenAnimation> animations = default;

        /// <summary>
        /// The direction of the playback.
        /// </summary>
        [NonSerialized] public PlayDirection direction;

        private float _normalizedTime = 0f;
        private int _state = 0; // -1: BackArrived, 0: Playing, +1: ForwardArrived

        /// <summary>
        /// The total duration time
        /// </summary>
        public float Duration { get => duration; set => duration = value > MIN_DURATION ? value : MIN_DURATION; }

        /// <summary>
        /// Add or remove callbacks when it's over.
        /// </summary>
        public event UnityAction OnForwardArrived
        {
            add => (onForwardArrived ?? (onForwardArrived = new UnityEvent())).AddListener(value);
            remove => onForwardArrived?.RemoveListener(value);
        }

        /// <summary>
        /// Add or remove callbacks when it gets to the starting point.
        /// </summary>
        public event UnityAction OnBackArrived
        {
            add => (onBackArrived ?? (onBackArrived = new UnityEvent())).AddListener(value);
            remove => onBackArrived?.RemoveListener(value);
        }

        /// <summary>
        /// Current time in range [0, 1]
        /// </summary>
        public float NormalizedTime
        {
            get => _normalizedTime;
            set
            {
                _normalizedTime = M.Clamp01(value);
                Sample(_normalizedTime);
            }
        }

        /// <summary>
        /// animationCount
        /// </summary>
        public int AnimationCount => animations == null ? 0 : animations.Count;

        /// <summary>
        /// Reverse playback direction.
        /// </summary>
        public void ReverseDirection() => direction = (direction == PlayDirection.Forward ? PlayDirection.Back : PlayDirection.Forward);


        public void SetForwardDirection() => direction = PlayDirection.Forward;


        public void SetBackDirection() => direction = PlayDirection.Back;


        public void SetForwardDirectionAndEnabled()
        {
            direction = PlayDirection.Forward;
            enabled = true;
        }


        public void SetBackDirectionAndEnabled()
        {
            direction = PlayDirection.Back;
            enabled = true;
        }

        /// <summary>
        /// Sample all animation states at a specified time.
        /// </summary>
        public void Sample(float normalizedTime)
        {
            if (animations != null)
            {
                for (int i = 0; i < animations.Count; i++)
                {
                    var item = animations[i];
                    if (item.enabled) item.Sample(normalizedTime);
                }
            }
        }

        /// <summary>
        /// Add an animation by a ease parameter.
        /// </summary>
        public T AddAnimation<T>() where T : TweenAnimation, new()
        {
            var anim = new T();
            (animations ?? (animations = new List<TweenAnimation>(4))).Add(anim);
            return anim;
        }

        /// <summary>
        /// Add an animation by a ease parameter.
        /// </summary>
        public TweenAnimation AddAnimation(Type type)
        {
            var anim = (TweenAnimation) Activator.CreateInstance(type);
            (animations ?? (animations = new List<TweenAnimation>(4))).Add(anim);
            return anim;
        }

        /// <summary>
        /// Get the animation at the index.
        /// </summary>
        public TweenAnimation GetAnimation(int index) => animations[index];

        /// <summary>
        /// Get an animation by the specified ease parameter.
        /// </summary>
        public T GetAnimation<T>() where T : TweenAnimation
        {
            if (animations != null)
            {
                foreach (var item in animations)
                {
                    if (item is T result) return result;
                }
            }

            return null;
        }

        /// <summary>
        /// Remove the animation at the index.
        /// </summary>
        public void RemoveAnimation(int index) => animations.RemoveAt(index);

        /// <summary>
        /// Remove the specified animation.
        /// </summary>
        public bool RemoveAnimation(TweenAnimation animation) => animations != null ? animations.Remove(animation) : false;

        /// <summary>
        /// Remove an animation by the specified ease parameter.
        /// </summary>
        public bool RemoveAnimation<T>() where T : TweenAnimation
        {
            if (animations != null)
            {
                for (int i = 0; i < animations.Count; i++)
                {
                    if (animations[i] is T)
                    {
                        animations.RemoveAt(i);
                        return true;
                    }
                }
            }

            return false;
        }


        private void Awake()
        {
            if (sampleOnAwake) Sample(_normalizedTime);
        }


        protected override void OnUpdate()
        {
#if UNITY_EDITOR
            if (_dragging) return;

            // Avoid rare error of prefab mode
            if (!this)
            {
                Playing = false;
                return;
            }
#endif

            float deltaTime = RuntimeUtilities.GetUnitedDeltaTime(timeMode);

            while (this && isActiveAndEnabled && deltaTime > M.Epsilon)
            {
                if (direction == PlayDirection.Forward)
                {
                    if (_normalizedTime < 1f)
                    {
                        _state = 0;
                    }
                    else if (wrapMode == WrapMode.Loop)
                    {
                        _normalizedTime = 0f;
                        _state = 0;
                    }

                    float time = _normalizedTime * duration + deltaTime;

                    // playing
                    if (time < duration)
                    {
                        NormalizedTime = time / duration;
                        return;
                    }

                    // arrived
                    NormalizedTime = 1f;
                    if (_state != +1)
                    {
                        _state = +1;

                        if ((arrivedAction & ArrivedAction.StopOnForwardArrived) != 0)
                            enabled = false;

                        onForwardArrived?.Invoke();
                    }

                    // wrap
                    switch (wrapMode)
                    {
                        case WrapMode.Clamp:
                            return;

                        case WrapMode.PingPong:
                            direction = PlayDirection.Back;
                            break;
                    }

                    deltaTime = time - duration;
                }
                else
                {
                    if (_normalizedTime > 0f)
                    {
                        _state = 0;
                    }
                    else if (wrapMode == WrapMode.Loop)
                    {
                        _normalizedTime = 1f;
                        _state = 0;
                    }

                    float time = _normalizedTime * duration - deltaTime;

                    // playing
                    if (time > 0f)
                    {
                        NormalizedTime = time / duration;
                        return;
                    }

                    // arrived
                    NormalizedTime = 0f;
                    if (_state != -1)
                    {
                        _state = -1;

                        if ((arrivedAction & ArrivedAction.StopOnBackArrived) != 0)
                            enabled = false;

                        onBackArrived?.Invoke();
                    }

                    // wrap
                    switch (wrapMode)
                    {
                        case WrapMode.Clamp:
                            return;

                        case WrapMode.PingPong:
                            direction = PlayDirection.Forward;
                            break;
                    }

                    deltaTime = -time;
                }
            }
        }
    } // class TweenPlayer
} // UnityExtensions.Tween