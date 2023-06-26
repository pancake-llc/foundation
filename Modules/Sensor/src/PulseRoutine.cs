using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Pancake.Sensor
{
    public interface IPulseRoutine
    {
        PulseRoutine.Modes PulseMode { get; set; }
        float PulseInterval { get; set; }
    }

    [Serializable]
    public class PulseRoutine
    {
        public enum Modes
        {
            Manual,
            FixedInterval,
            EachFrame
        }

        public enum UpdateFunctions
        {
            Update,
            FixedUpdate
        }

        [Serializable]
        public class ObservableMode : Observable<Modes>
        {
        }

        public ObservableMode Mode = new ObservableMode() {Value = Modes.EachFrame};

        public UpdateFunctions UpdateFunction = UpdateFunctions.Update;

        public ObservableFloat Interval = new ObservableFloat() {Value = 1f};

        public float dt
        {
            get
            {
                if (Mode.Value == Modes.EachFrame)
                {
                    return Time.deltaTime;
                }
                else if (Mode.Value == Modes.FixedInterval)
                {
                    return Interval.Value;
                }

                return 0;
            }
        }

        BasePulsableSensor pulsable;
        float steppedPulseDelay;
        Coroutine pulseRoutine;

        public void Awake(BasePulsableSensor pulsable)
        {
            this.pulsable = pulsable;

            if (Mode == null)
            {
                Mode = new ObservableMode();
            }

            if (Interval == null)
            {
                Interval = new ObservableFloat();
            }

            steppedPulseDelay = UnityEngine.Random.Range(0f, 1f);
        }

        public void OnEnable()
        {
            Mode.OnChanged += PulseModeChangedHandler;
            Interval.OnChanged += PulseModeChangedHandler;

            PulseModeChangedHandler();
        }

        public void OnDisable()
        {
            Mode.OnChanged -= PulseModeChangedHandler;
            Interval.OnChanged -= PulseModeChangedHandler;
        }

        public void OnValidate()
        {
            Mode?.OnValidate();
            Interval?.OnValidate();
        }

        void PulseModeChangedHandler()
        {
            if (!Application.isPlaying)
            {
                return;
            }

            RunPulseMode(Mode.Value, Interval.Value);
        }

        void RunPulseMode(Modes mode, float interval = 0)
        {
            if (pulseRoutine != null)
            {
                pulsable.StopCoroutine(pulseRoutine);
                pulseRoutine = null;
            }

            if (mode == Modes.EachFrame)
            {
                pulseRoutine = pulsable.StartCoroutine(PulseEachFrameRoutine());
            }
            else if (mode == Modes.FixedInterval)
            {
                pulseRoutine = pulsable.StartCoroutine(PulseFixedIntervalRoutine(interval));
            }
        }

        WaitForFixedUpdate waitForFixedUpdate = new WaitForFixedUpdate();

        IEnumerator PulseFixedIntervalRoutine(float interval)
        {
            var prevPulseTime = Time.time + (steppedPulseDelay * interval);
            var waitCondition = new WaitUntil(delegate
            {
                var timeSincePulse = Time.time - prevPulseTime;

                if (timeSincePulse < interval)
                {
                    return false;
                }

                var nPulsesIncremented = Mathf.FloorToInt(timeSincePulse / interval);
                prevPulseTime += (nPulsesIncremented * interval);

                return true;
            });

            while (true)
            {
                yield return waitCondition;
                if (UpdateFunction == UpdateFunctions.FixedUpdate)
                {
                    yield return waitForFixedUpdate;
                }

                pulsable.Pulse();
            }
        }

        IEnumerator PulseEachFrameRoutine()
        {
            while (true)
            {
                yield return null;
                if (UpdateFunction == UpdateFunctions.FixedUpdate)
                {
                    yield return waitForFixedUpdate;
                }

                pulsable.Pulse();
            }
        }
    }
}