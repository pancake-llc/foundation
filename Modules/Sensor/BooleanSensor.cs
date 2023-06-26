using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Pancake.Sensor
{
    /**
     * The Boolean Sensor is another Compound Sensor that combines the signals of it's input sensors. It merges the 
     * signals of each sensor via logical And or logical Or.
     */
    [AddComponentMenu("Sensors/Boolean Sensor")]
    public class BooleanSensor : Sensor
    {
        #region Configurations

        public enum OperationType
        {
            And,
            Or
        }

        [System.Serializable]
        public class ObservableOperationType : Observable<OperationType>
        {
        }

        [Tooltip("The list of input sensors. Changing the list will cause this sensor to immediately re-evaluate it's output signals.")]
        public ObservableSensorList InputSensors = new ObservableSensorList();

        [SerializeField] ObservableOperationType operation = new ObservableOperationType() {Value = OperationType.And};

        #endregion

        #region Events

        public override event System.Action OnPulsed;

        #endregion

        #region Public

        // Change the operation type at runtime
        public OperationType Operation { get => operation.Value; set => operation.Value = value; }

        // In reality you shouldn't need to pulse this sensor manually. It subscribes to the OnSignalChange events of
        // the input sensors and updates as soon as the input sensors update.
        public override void Pulse()
        {
            if (!isActiveAndEnabled)
            {
                return;
            }

            RedoFromScratch();

            workList.Clear();
            foreach (var detectionCount in detectionCounts)
            {
                if (SatisfiesBooleanOp(detectionCount.Value))
                {
                    workList.Add(CombineInputSignals(detectionCount.Key));
                }
            }

            UpdateAllSignals(workList);

            OnPulsed?.Invoke();
        }

        public override void PulseAll()
        {
            foreach (var sensor in InputSensors)
            {
                if (sensor != null)
                {
                    sensor.PulseAll();
                }
            }

            Pulse();
        }

        #endregion

        #region Internals

        Dictionary<GameObject, int> detectionCounts = new Dictionary<GameObject, int>();
        List<Signal> workList = new List<Signal>();
        int sensorCount;

        protected override void Awake()
        {
            base.Awake();

            InputSensors.OnChanged += InputsChangedHandler;
            InputSensors.ItemAdded += SensorAddedHandler;
            InputSensors.ItemRemoved += SensorRemovedHandler;
            operation.OnChanged += InputsChangedHandler;

            foreach (var sensor in InputSensors)
            {
                SensorAddedHandler(sensor);
            }
        }

        void OnDestroy()
        {
            InputSensors.OnChanged -= InputsChangedHandler;
            InputSensors.ItemAdded -= SensorAddedHandler;
            InputSensors.ItemRemoved -= SensorRemovedHandler;
            operation.OnChanged -= InputsChangedHandler;

            foreach (var sensor in InputSensors)
            {
                SensorRemovedHandler(sensor);
            }
        }

        void OnValidate()
        {
            if (InputSensors != null)
            {
                InputSensors.OnValidate();
            }

            if (operation != null)
            {
                operation.OnValidate();
            }
        }

        void SensorAddedHandler(Sensor sensor)
        {
            if (sensor != null)
            {
                sensor.OnDetected.AddListener(OnDetectionHandler);
                sensor.OnLostDetection.AddListener(OnLostDetectionHandler);
                sensor.OnSignalChanged += OnSignalChangedHandler;
            }

            sensorCount += 1;
        }

        void SensorRemovedHandler(Sensor sensor)
        {
            if (sensor != null)
            {
                sensor.OnDetected.RemoveListener(OnDetectionHandler);
                sensor.OnLostDetection.RemoveListener(OnLostDetectionHandler);
                sensor.OnSignalChanged -= OnSignalChangedHandler;
            }

            sensorCount -= 1;
        }

        void OnDetectionHandler(GameObject go, Sensor sensor)
        {
            if (SatisfiesBooleanOp(IncrementDetectionCount(go)))
            {
                UpdateSignalImmediate(CombineInputSignals(go));
            }
        }

        void OnSignalChangedHandler(Signal signal, Sensor sensor)
        {
            if (IsDetected(signal.Object))
            {
                UpdateSignalImmediate(CombineInputSignals(signal.Object));
            }
        }

        void OnLostDetectionHandler(GameObject go, Sensor sensor)
        {
            if (!SatisfiesBooleanOp(DecrementDetectionCount(go)))
            {
                LostSignalImmediate(go);
            }
        }

        int IncrementDetectionCount(GameObject forObject)
        {
            var n = 0;
            detectionCounts.TryGetValue(forObject, out n);
            n += 1;
            detectionCounts[forObject] = n;
            return n;
        }

        int DecrementDetectionCount(GameObject forObject)
        {
            var n = detectionCounts[forObject];
            n -= 1;
            if (n > 0)
            {
                detectionCounts[forObject] = n;
            }
            else
            {
                detectionCounts.Remove(forObject);
            }

            return n;
        }

        Signal CombineInputSignals(GameObject forObject)
        {
            Signal signal;
            Signal combinedSignal = new Signal(forObject);
            bool isFirst = true;
            foreach (var input in InputSensors)
            {
                if (input.TryGetSignal(forObject, out signal))
                {
                    if (isFirst)
                    {
                        combinedSignal.Shape = signal.Shape;
                        isFirst = false;
                    }
                    else
                    {
                        combinedSignal.Shape.Encapsulate(signal.Shape);
                    }

                    combinedSignal.Strength = Mathf.Max(combinedSignal.Strength, signal.Strength);
                }
            }

            return combinedSignal;
        }

        void InputsChangedHandler() { Pulse(); }

        void RedoFromScratch()
        {
            detectionCounts.Clear();

            sensorCount = 0;

            foreach (var sensor in InputSensors)
            {
                if (sensor == null)
                {
                    continue;
                }

                sensorCount += 1;
                foreach (var detection in sensor.Detections)
                {
                    IncrementDetectionCount(detection);
                }
            }
        }

        bool SatisfiesBooleanOp(int n)
        {
            if (Operation == OperationType.And)
            {
                return n == sensorCount;
            }
            else
            {
                return n > 0;
            }
        }

        #endregion
    }
}