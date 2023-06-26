using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Pancake.Sensor
{
    /*
     *  Sensors can run in two detection modes
     *  - Colliders: The sensor detects the GameObject attached to any collider it intersects.
     *  - RigidBodies: The sensor detects the GameObject owning the attached RigidBody of any collider it intersects.
     */
    public enum DetectionModes
    {
        Colliders,
        RigidBodies
    }

    #region Concrete Generic Classes

    [Serializable]
    public class ObservableSensor : Observable<Sensor>
    {
    }

    [Serializable]
    public class ObservableSensorList : ObservableList<Sensor>
    {
    }

    [Serializable]
    public class SensorEventHandler : UnityEvent<Sensor>
    {
    }

    [Serializable]
    public class SensorDetectionEventHandler : UnityEvent<GameObject, Sensor>
    {
    }

    #endregion

    /*
     *  Base class implemented by all sensor types with common functions for querying and filtering
     *  the sensors list of detected objects.
     */
    public abstract class Sensor : BasePulsableSensor
    {
        // Enumerator over all detected GameObjects. Can be used in foreach without GC allocated
        public SignalPipeline.TargetsEnumerable Detections => signalPipeline.OutputTargets;

        // Enumerator over all detected Signals. Can be used in foreach without GC allocated
        public SignalPipeline.OutputsEnumerable Signals => signalPipeline.Outputs;

        /**
         * A list of SignalProcessors will transform the detected Signals into their final representation.
         * Currently it's used to filter signals by tag or to reduce signals to their common rigid bodies.
         * It's possible to insert custom processors and really customise the sensor behaviour, although
         * this is more of an advanced feature. I'll be fleshing it out later.
         */
        public List<ISignalProcessor> SignalProcessors => signalPipeline.SignalProcessors;

        #region Events

        // Event is invoked for each detected object
        public SensorDetectionEventHandler OnDetected;

        // Event is invoked for each object that has lost detection
        public SensorDetectionEventHandler OnLostDetection;

        // Event is invoked if an object is detected and previously there were no detections
        public UnityEvent OnSomeDetection;

        // Event is invoked when the sensor has lost all detections
        public UnityEvent OnNoDetection;

        // Delegate events can be subscribed to instead of the UnityEvents above.
        public event Action<Signal, Sensor> OnSignalAdded;
        public event Action<Signal, Sensor> OnSignalChanged;
        public event Action<Signal, Sensor> OnSignalLost;

        #endregion

        #region Public Methods

        List<Signal> clearList = new List<Signal>();
        public override void Clear() { signalPipeline.UpdateAllInputs(clearList); }

        // Returns the Signal associated with a given GameObject. It is unsafe to call on undetected objects.
        public Signal GetSignal(GameObject go) => signalPipeline.GetOutput(go);

        // Safely retrieve the Signal for a given GameObject. Returns true or false whether the object is
        // detected or not.
        public bool TryGetSignal(GameObject go, out Signal signal) => signalPipeline.TryGetOutput(go, out signal);

        // Returns true when the GameObject is currently detected by the sensor, false otherwise.
        public bool IsDetected(GameObject go) => signalPipeline.ContainsOutput(go);

        List<GameObject> golist = new List<GameObject>();

        // Returns a list of Colliders that were detected on the GameObject.
        public List<Collider> GetDetectedColliders(GameObject forObject, List<Collider> storeIn)
        {
            golist.Clear();
            var inputObjects = signalPipeline.GetInputObjects(forObject, golist);
            if (inputObjects.Count == 0)
            {
                // There are no mapped input-signals, so assume this is the input-signal
                return GetInputColliders(forObject, storeIn);
            }

            foreach (var inputGo in inputObjects)
            {
                GetInputColliders(inputGo, storeIn);
            }

            return storeIn;
        }

        // Returns a list of Collider2Ds that were detected on the GameObject.
        public List<Collider2D> GetDetectedColliders(GameObject forObject, List<Collider2D> storeIn)
        {
            golist.Clear();
            var inputObjects = signalPipeline.GetInputObjects(forObject, golist);
            if (inputObjects.Count == 0)
            {
                // There are no mapped input-signals, so assume this is the input-signal
                return GetInputColliders(forObject, storeIn);
            }

            foreach (var inputGo in inputObjects)
            {
                GetInputColliders(inputGo, storeIn);
            }

            return storeIn;
        }

        /**
         * There are many query functions for reading what signals the sensor detects, but they all follow some common forms:
         * Get[...] -- Returns a list of detected [...]
         * Get[...] -- Same as above except the list is sorted by distance to the sensor
         * Get[...]ByDistanceToPoint -- Same as above except the list is sorted by distance to some point
         * GetNearest[...] -- Returns only the nearest [...] to the sensor.
         * GetNearest[...]ToPoint -- returns on the nearest [...] to some point.
         * 
         * The [...] wil be one of 'Signals', 'Detections', 'DetectedComponents'. Where this specifies what is to be returned.
         * The 'DetectedComponent' functions are useful as it will filter out signals missing the component and return references
         * to that component.
         * 
         * There are also optional parameters for 'tag' or predicate function to further refine the results returned.
         * 
         * For functions that return a List you may optionally provide your own List instance. If you omit this the Sensor will
         * reuse a single instance of List for each function calls. This makes it very easy to call a function and enumerate over
         * the results without worrying about GC. But it does mean each function call will override the results of the previous call.
         * If you need to persist the results of the query then you will need to provide your own list to store the results in.
         */
        public List<Signal> GetSignals(List<Signal> storeIn = null)
        {
            storeIn = storeIn ?? signalWorkList;
            storeIn.Clear();
            foreach (var signal in Signals)
            {
                storeIn.Add(signal);
            }

            return storeIn;
        }

        public List<Signal> GetSignals(string withTag, List<Signal> storeIn = null)
        {
            storeIn = storeIn ?? signalWorkList;
            storeIn.Clear();
            foreach (var signal in Signals)
            {
                if (signal.Object.CompareTag(withTag))
                {
                    storeIn.Add(signal);
                }
            }

            return storeIn;
        }

        public List<Signal> GetSignals(Predicate<Signal> predicate, List<Signal> storeIn = null)
        {
            storeIn = storeIn ?? signalWorkList;
            storeIn.Clear();
            foreach (var signal in Signals)
            {
                if (predicate(signal))
                {
                    storeIn.Add(signal);
                }
            }

            return storeIn;
        }

        public List<GameObject> GetDetections(List<GameObject> storeIn = null) => SignalsToObjects(GetSignals(), storeIn ?? objectWorkList);
        public List<GameObject> GetDetections(string withTag, List<GameObject> storeIn = null) => SignalsToObjects(GetSignals(withTag), storeIn ?? objectWorkList);

        public List<GameObject> GetDetections(Predicate<Signal> predicate, List<GameObject> storeIn = null) =>
            SignalsToObjects(GetSignals(predicate), storeIn ?? objectWorkList);

        public List<T> GetDetectedComponents<T>(List<T> storeIn) where T : Component => SignalsToComponents(GetSignals(), storeIn);
        public List<T> GetDetectedComponents<T>(string withTag, List<T> storeIn) where T : Component => SignalsToComponents(GetSignals(withTag), storeIn);
        public List<Component> GetDetectedComponents(Type t, List<Component> storeIn = null) => SignalsToComponents(GetSignals(), t, storeIn ?? componentWorkList);

        public List<Component> GetDetectedComponents(Type t, string withTag, List<Component> storeIn = null) =>
            SignalsToComponents(GetSignals(withTag), t, storeIn ?? componentWorkList);

        public List<Signal> GetSignalsByDistance(List<Signal> storeIn = null) => OrderedByDistance(GetSignals(storeIn));
        public List<Signal> GetSignalsByDistance(string withTag, List<Signal> storeIn = null) => OrderedByDistance(GetSignals(withTag, storeIn));
        public List<Signal> GetSignalsByDistance(Predicate<Signal> predicate, List<Signal> storeIn = null) => OrderedByDistance(GetSignals(predicate, storeIn));
        public List<Signal> GetSignalsBySignalStrength(List<Signal> storeIn = null) => OrderedBySignalStrength(GetSignals(storeIn));
        public List<Signal> GetSignalsBySignalStrength(string withTag, List<Signal> storeIn = null) => OrderedBySignalStrength(GetSignals(withTag, storeIn));

        public List<Signal> GetSignalsBySignalStrength(Predicate<Signal> predicate, List<Signal> storeIn = null) =>
            OrderedBySignalStrength(GetSignals(predicate, storeIn));

        public List<GameObject> GetDetectionsByDistance(List<GameObject> storeIn = null) => SignalsToObjects(GetSignalsByDistance(), storeIn ?? objectWorkList);

        public List<GameObject> GetDetectionsByDistance(string withTag, List<GameObject> storeIn = null) =>
            SignalsToObjects(GetSignalsByDistance(withTag), storeIn ?? objectWorkList);

        public List<GameObject> GetDetectionsByDistance(Predicate<Signal> predicate, List<GameObject> storeIn = null) =>
            SignalsToObjects(GetSignalsByDistance(predicate), storeIn ?? objectWorkList);

        public List<GameObject> GetDetectionsBySignalStrength(List<GameObject> storeIn = null) =>
            SignalsToObjects(GetSignalsBySignalStrength(), storeIn ?? objectWorkList);

        public List<GameObject> GetDetectionsBySignalStrength(string withTag, List<GameObject> storeIn = null) =>
            SignalsToObjects(GetSignalsBySignalStrength(withTag), storeIn ?? objectWorkList);

        public List<GameObject> GetDetectionsBySignalStrength(Predicate<Signal> predicate, List<GameObject> storeIn = null) =>
            SignalsToObjects(GetSignalsBySignalStrength(predicate), storeIn ?? objectWorkList);

        public List<T> GetDetectedComponentsByDistance<T>(List<T> storeIn) where T : Component => SignalsToComponents(GetSignalsByDistance(), storeIn);

        public List<T> GetDetectedComponentsByDistance<T>(string withTag, List<T> storeIn) where T : Component =>
            SignalsToComponents(GetSignalsByDistance(withTag), storeIn);

        public List<Component> GetDetectedComponentsByDistance(Type t, List<Component> storeIn = null) =>
            SignalsToComponents(GetSignalsByDistance(), t, storeIn ?? componentWorkList);

        public List<Component> GetDetectedComponentsByDistance(Type t, string withTag, List<Component> storeIn = null) =>
            SignalsToComponents(GetSignalsByDistance(withTag), t, storeIn ?? componentWorkList);

        public List<T> GetDetectedComponentsBySignalStrength<T>(List<T> storeIn) where T : Component => SignalsToComponents(GetSignalsBySignalStrength(), storeIn);

        public List<T> GetDetectedComponentsBySignalStrength<T>(string withTag, List<T> storeIn) where T : Component =>
            SignalsToComponents(GetSignalsBySignalStrength(withTag), storeIn);

        public List<Component> GetDetectedComponentsBySignalStrength(Type t, List<Component> storeIn = null) =>
            SignalsToComponents(GetSignalsBySignalStrength(), t, storeIn ?? componentWorkList);

        public List<Component> GetDetectedComponentsBySignalStrength(Type t, string withTag, List<Component> storeIn = null) =>
            SignalsToComponents(GetSignalsBySignalStrength(withTag), t, storeIn ?? componentWorkList);

        public List<Signal> GetSignalsByDistanceToPoint(Vector3 point, List<Signal> storeIn = null) => OrderedByDistanceToPoint(GetSignals(storeIn), point);

        public List<Signal> GetSignalsByDistanceToPoint(Vector3 point, string withTag, List<Signal> storeIn = null) =>
            OrderedByDistanceToPoint(GetSignals(withTag, storeIn), point);

        public List<Signal> GetSignalsByDistanceToPoint(Vector3 point, Predicate<Signal> predicate, List<Signal> storeIn = null) =>
            OrderedByDistanceToPoint(GetSignals(predicate, storeIn), point);

        public List<GameObject> GetDetectionsByDistanceToPoint(Vector3 point, List<GameObject> storeIn = null) =>
            SignalsToObjects(GetSignalsByDistanceToPoint(point), storeIn ?? objectWorkList);

        public List<GameObject> GetDetectionsByDistanceToPoint(Vector3 point, string withTag, List<GameObject> storeIn = null) =>
            SignalsToObjects(GetSignalsByDistanceToPoint(point, withTag), storeIn ?? objectWorkList);

        public List<GameObject> GetDetectionsByDistanceToPoint(Vector3 point, Predicate<Signal> predicate, List<GameObject> storeIn = null) =>
            SignalsToObjects(GetSignalsByDistanceToPoint(point, predicate), storeIn ?? objectWorkList);

        public List<T> GetDetectedComponentsByDistanceToPoint<T>(Vector3 point, List<T> storeIn) => SignalsToComponents(GetSignalsByDistanceToPoint(point), storeIn);

        public List<T> GetDetectedComponentsByDistanceToPoint<T>(Vector3 point, string withTag, List<T> storeIn) =>
            SignalsToComponents(GetSignalsByDistanceToPoint(point, withTag), storeIn);

        public List<Component> GetDetectedComponentsByDistanceToPoint(Vector3 point, Type t, List<Component> storeIn = null) =>
            SignalsToComponents(GetSignalsByDistanceToPoint(point), t, storeIn ?? new List<Component>());

        public List<Component> GetDetectedComponentsByDistanceToPoint(Vector3 point, Type t, string withTag, List<Component> storeIn = null) =>
            SignalsToComponents(GetSignalsByDistanceToPoint(point, withTag), t, storeIn ?? new List<Component>());

        public Signal GetNearestSignal() => FirstOrDefault(GetSignalsByDistance());
        public Signal GetNearestSignal(string withTag) => FirstOrDefault(GetSignalsByDistance(withTag));
        public Signal GetNearestSignal(Predicate<Signal> predicate) => FirstOrDefault(GetSignalsByDistance(predicate));
        public Signal GetStrongestSignal() => FirstOrDefault(GetSignalsBySignalStrength());
        public Signal GetStrongestSignal(string withTag) => FirstOrDefault(GetSignalsBySignalStrength(withTag));
        public Signal GetStrongestSignal(Predicate<Signal> predicate) => FirstOrDefault(GetSignalsBySignalStrength(predicate));
        public GameObject GetNearestDetection() => FirstOrDefault(GetDetectionsByDistance());
        public GameObject GetNearestDetection(string withTag) => FirstOrDefault(GetDetectionsByDistance(withTag));
        public GameObject GetNearestDetection(Predicate<Signal> predicate) => FirstOrDefault(GetDetectionsByDistance(predicate));
        public GameObject GetStrongestDetection() => FirstOrDefault(GetDetectionsBySignalStrength());
        public GameObject GetStrongestDetection(string withTag) => FirstOrDefault(GetDetectionsBySignalStrength(withTag));
        public GameObject GetStrongestDetection(Predicate<Signal> predicate) => FirstOrDefault(GetDetectionsBySignalStrength(predicate));

        public T GetNearestComponent<T>() where T : Component
        {
            foreach (var go in GetDetectionsByDistance())
            {
                if (go.TryGetComponent<T>(out var c))
                {
                    return c;
                }
            }

            return null;
        }

        public T GetNearestComponent<T>(Predicate<T> predicate) where T : Component
        {
            foreach (var go in GetDetectionsByDistance())
            {
                if (go.TryGetComponent<T>(out var c) && predicate(c))
                {
                    return c;
                }
            }

            return null;
        }

        public T GetNearestComponent<T>(string withTag) where T : Component
        {
            foreach (var go in GetDetectionsByDistance(withTag))
            {
                if (go.TryGetComponent<T>(out var c))
                {
                    return c;
                }
            }

            return null;
        }

        public Component GetNearestComponent(Type t)
        {
            foreach (var go in GetDetectionsByDistance())
            {
                if (go.TryGetComponent(t, out var c))
                {
                    return c;
                }
            }

            return null;
        }

        public Component GetNearestComponent(Type t, Predicate<Component> predicate)
        {
            foreach (var go in GetDetectionsByDistance())
            {
                if (go.TryGetComponent(t, out var c) && predicate(c))
                {
                    return c;
                }
            }

            return null;
        }

        public Component GetNearestComponent(Type t, string withTag)
        {
            foreach (var go in GetDetectionsByDistance(withTag))
            {
                if (go.TryGetComponent(t, out var c))
                {
                    return c;
                }
            }

            return null;
        }

        public T GetStrongestComponent<T>() where T : Component
        {
            foreach (var go in GetDetectionsBySignalStrength())
            {
                if (go.TryGetComponent<T>(out var c))
                {
                    return c;
                }
            }

            return null;
        }

        public T GetStrongestComponent<T>(Predicate<T> predicate) where T : Component
        {
            foreach (var go in GetDetectionsBySignalStrength())
            {
                if (go.TryGetComponent<T>(out var c) && predicate(c))
                {
                    return c;
                }
            }

            return null;
        }

        public T GetStrongestComponent<T>(string withTag) where T : Component
        {
            foreach (var go in GetDetectionsBySignalStrength(withTag))
            {
                if (go.TryGetComponent<T>(out var c))
                {
                    return c;
                }
            }

            return null;
        }

        public Component GetStrongestComponent(Type t)
        {
            foreach (var go in GetDetectionsBySignalStrength())
            {
                if (go.TryGetComponent(t, out var c))
                {
                    return c;
                }
            }

            return null;
        }

        public Component GetStrongestComponent(Type t, Predicate<Component> predicate)
        {
            foreach (var go in GetDetectionsBySignalStrength())
            {
                if (go.TryGetComponent(t, out var c) && predicate(c))
                {
                    return c;
                }
            }

            return null;
        }

        public Component GetStrongestComponent(Type t, string withTag)
        {
            foreach (var go in GetDetectionsBySignalStrength(withTag))
            {
                if (go.TryGetComponent(t, out var c))
                {
                    return c;
                }
            }

            return null;
        }

        public Signal GetNearestSignalToPoint(Vector3 point) => FirstOrDefault(GetSignalsByDistanceToPoint(point));
        public Signal GetNearestSignalToPoint(Vector3 point, string withTag) => FirstOrDefault(GetSignalsByDistanceToPoint(point, withTag));
        public Signal GetNearestSignalToPoint(Vector3 point, Predicate<Signal> predicate) => FirstOrDefault(GetSignalsByDistanceToPoint(point, predicate));
        public GameObject GetNearestDetectionToPoint(Vector3 point) => FirstOrDefault(GetDetectionsByDistanceToPoint(point));
        public GameObject GetNearestDetectionToPoint(Vector3 point, string withTag) => FirstOrDefault(GetDetectionsByDistanceToPoint(point, withTag));
        public GameObject GetNearestDetectionToPoint(Vector3 point, Predicate<Signal> predicate) => FirstOrDefault(GetDetectionsByDistanceToPoint(point, predicate));

        public T GetNearestComponentToPoint<T>(Vector3 point) where T : Component
        {
            foreach (var go in GetDetectionsByDistanceToPoint(point))
            {
                if (go.TryGetComponent<T>(out var c))
                {
                    return c;
                }
            }

            return null;
        }

        public T GetNearestComponentToPoint<T>(Vector3 point, string withTag) where T : Component
        {
            foreach (var go in GetDetectionsByDistanceToPoint(point, withTag))
            {
                if (go.TryGetComponent<T>(out var c))
                {
                    return c;
                }
            }

            return null;
        }

        public Component GetNearestComponentToPoint(Vector3 point, Type t)
        {
            foreach (var go in GetDetectionsByDistanceToPoint(point))
            {
                if (go.TryGetComponent(t, out var c))
                {
                    return c;
                }
            }

            return null;
        }

        public Component GetNearestComponentToPoint(Vector3 point, Type t, string withTag)
        {
            foreach (var go in GetDetectionsByDistanceToPoint(point, withTag))
            {
                if (go.TryGetComponent(t, out var c))
                {
                    return c;
                }
            }

            return null;
        }

        #endregion

        #region Protected

        SignalPipeline _signalPipeline;

        protected SignalPipeline signalPipeline
        {
            get
            {
                if (_signalPipeline == null)
                {
                    _signalPipeline = new SignalPipeline();
                    _signalPipeline.OnAdd += delegate(Signal signal)
                    {
                        OnSignalAdded?.Invoke(signal, this);
                        OnDetected.Invoke(signal.Object, this);
                    };
                    _signalPipeline.OnChange += delegate(Signal prev, Signal next) { OnSignalChanged?.Invoke(next, this); };
                    _signalPipeline.OnRemove += delegate(Signal signal)
                    {
                        OnSignalLost?.Invoke(signal, this);
                        OnLostDetection.Invoke(signal.Object, this);
                    };
                    _signalPipeline.OnSome += delegate { OnSomeDetection.Invoke(); };
                    _signalPipeline.OnNone += delegate { OnNoDetection.Invoke(); };
                    InitialiseSignalProcessors();
                }

                return _signalPipeline;
            }
        }

        protected virtual void InitialiseSignalProcessors() { }

        protected virtual List<Collider> GetInputColliders(GameObject inputObject, List<Collider> storeIn) => storeIn;
        protected virtual List<Collider2D> GetInputColliders(GameObject inputObject, List<Collider2D> storeIn) => storeIn;

        protected virtual void Awake()
        {
            if (OnDetected == null)
            {
                OnDetected = new SensorDetectionEventHandler();
            }

            if (OnLostDetection == null)
            {
                OnLostDetection = new SensorDetectionEventHandler();
            }

            if (OnSomeDetection == null)
            {
                OnSomeDetection = new UnityEvent();
            }

            if (OnNoDetection == null)
            {
                OnNoDetection = new UnityEvent();
            }
        }

        protected void UpdateAllSignals(List<Signal> nextSignals) { signalPipeline.UpdateAllInputs(nextSignals); }

        protected void UpdateSignalImmediate(Signal signal) { signalPipeline.UpdateInput(signal); }

        protected void LostSignalImmediate(GameObject forObject) { signalPipeline.RemoveInput(forObject); }

        protected virtual void OnDrawGizmosSelected()
        {
            if (!isActiveAndEnabled) return;

            if (ShowDetectionGizmos)
            {
                foreach (Signal signal in Signals)
                {
                    SensorGizmos.DetectedObjectGizmo(signal.Bounds);
                }
            }
        }

        #endregion

        #region Internals

        List<Signal> signalWorkList = new List<Signal>();
        List<GameObject> objectWorkList = new List<GameObject>();
        List<Component> componentWorkList = new List<Component>();

        List<GameObject> SignalsToObjects(List<Signal> signals, List<GameObject> storeIn)
        {
            storeIn.Clear();
            foreach (var signal in signals)
            {
                storeIn.Add(signal.Object);
            }

            return storeIn;
        }

        List<T> SignalsToComponents<T>(List<Signal> signals, List<T> storeIn)
        {
            storeIn.Clear();
            foreach (var signal in signals)
            {
                if (signal.Object.TryGetComponent<T>(out var c))
                {
                    storeIn.Add(c);
                }
            }

            return storeIn;
        }

        T FirstOrDefault<T>(List<T> list)
        {
            if (list.Count > 0)
            {
                return list[0];
            }

            return default;
        }

        List<Component> SignalsToComponents(List<Signal> signals, Type t, List<Component> storeIn)
        {
            storeIn.Clear();
            foreach (var signal in signals)
            {
                if (signal.Object.TryGetComponent(t, out var c))
                {
                    storeIn.Add(c);
                }
            }

            return storeIn;
        }

        struct DistanceElement
        {
            public float Distance;
            public Signal Signal;
        }

        List<DistanceElement> distanceWorkList = new List<DistanceElement>();
        List<Signal> OrderedByDistance(List<Signal> signals) { return OrderedByDistanceToPoint(signals, transform.position); }

        List<Signal> OrderedByDistanceToPoint(List<Signal> signals, Vector3 point)
        {
            distanceWorkList.Clear();

            for (int i = 0; i < signals.Count; i++)
            {
                distanceWorkList.Add(new DistanceElement() {Distance = signals[i].DistanceTo(point), Signal = signals[i]});
            }

            distanceWorkList.Sort(DistanceElementComparison);

            for (var i = 0; i < distanceWorkList.Count; i++)
            {
                signals[i] = distanceWorkList[i].Signal;
            }

            return signals;
        }

        List<Signal> OrderedBySignalStrength(List<Signal> signals)
        {
            signals.Sort(SignalStrengthComparison);
            return signals;
        }

        static Comparison<DistanceElement> DistanceElementComparison = new Comparison<DistanceElement>(CompareSignalElements);

        static int CompareSignalElements(DistanceElement x, DistanceElement y)
        {
            if (x.Distance > y.Distance)
            {
                return 1;
            }
            else if (x.Distance < y.Distance)
            {
                return -1;
            }

            return 0;
        }

        static Comparison<Signal> SignalStrengthComparison = new Comparison<Signal>(CompareSignalStrengths);

        static int CompareSignalStrengths(Signal x, Signal y)
        {
            if (x.Strength > y.Strength)
            {
                return -1;
            }
            else if (x.Strength < y.Strength)
            {
                return 1;
            }

            return 0;
        }
    }

    #endregion
}