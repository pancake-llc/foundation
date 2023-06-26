using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Pancake.Sensor
{
    /**
     * The Arc Sensor detects objects that intersect a curve. It works just like the Ray Sensor in that it detects all 
     * objects up to the first obstruction. The arc is broken up into line segments and a raycast is done on each.
     */
    [AddComponentMenu("Sensors/Arc Sensor")]
    public class ArcSensor : Sensor, IRayCastingSensor, IPulseRoutine
    {
        #region Configurations

        public enum ParameterisationType
        {
            Bezier,
            Ballistic
        }

        [Tooltip("Choose how the arc is represented.")] public ParameterisationType Parameterisation;

        // Configurations for Bezier type arc
        public BezierCurve Bezier = new BezierCurve(new Vector3(0, 0, 0), new Vector3(0, 0, 1), new Vector3(0, 1, 1), 10);

        // Configurations for Ballistic type arc
        public BallisticCurve Ballistic = new BallisticCurve(new Vector3(0, 0, 5), new Vector3(0, -10, 0), 1f, 10);

        [Tooltip("Is the curve parameterised in world space or local space.")]
        public bool WorldSpace = false;

        [Tooltip("A layer mask specifying which physics layers objects will be detected on.")]
        public LayerMask DetectsOnLayers;

        [Tooltip("A layer mask specifying which physics layers objects will obstruct the ray on.")]
        public LayerMask ObstructedByLayers;

        [Tooltip("In Collider mode the sensor detects GameObjects attached to colliders. In RigidBody mode it detects the Collider.AttachedRigidbody.")]
        public DetectionModes DetectionMode;

        [SerializeField] SignalFilter signalFilter = new SignalFilter();

        [Tooltip("Ignores all trigger colliders. Will not detect them or be obstructed by them.")]
        public bool IgnoreTriggerColliders;

        [SerializeField] PulseRoutine pulseRoutine;

        #endregion

        #region Events

        [SerializeField] ObstructionEvent onObstruction;
        public ObstructionEvent OnObstruction => onObstruction;

        [SerializeField] ObstructionEvent onClear;
        public ObstructionEvent OnClear => onClear;

        public override event Action OnPulsed;

        #endregion

        #region Public

        // Edit the IgnoreList at runtime. Anything in the list will not be detected
        public List<GameObject> IgnoreList => signalFilter.IgnoreList;

        // Enable/Disable the tag filtering at runtime
        public bool EnableTagFilter { get => signalFilter.EnableTagFilter; set => signalFilter.EnableTagFilter = value; }

        // Change the allowed tags at runtime
        public string[] AllowedTags { get => signalFilter.AllowedTags; set => signalFilter.AllowedTags = value; }

        // Change the pulse mode at runtime
        public PulseRoutine.Modes PulseMode { get => pulseRoutine.Mode.Value; set => pulseRoutine.Mode.Value = value; }

        // Change the pulse interval at runtime
        public float PulseInterval { get => pulseRoutine.Interval.Value; set => pulseRoutine.Interval.Value = value; }

        // The array size allocated for storing results from Physics.RaycastNonAlloc. Will automatically
        // be doubled in size when more space is needed.
        public int CurrentBufferSize => physics != null ? physics.Buffer.Length : 0;

        // Returns true if the ray sensor is being obstructed and false otherwise
        public bool IsObstructed => GetObstructionRayHit().IsObstructing;

        public List<Vector3> UnobstructedArcPoints => arcPoints;
        public List<Vector3> ObstructedArcPoints => obstructedArcPoints;

        public override void Pulse()
        {
            if (!isActiveAndEnabled)
            {
                return;
            }

            if (physics == null)
            {
                physics = new PhysicsNonAlloc<RaycastHit>();
            }

            mapToRB.IsRigidBodyMode = DetectionMode == DetectionModes.RigidBodies;

            TestArc();

            UpdateAllSignals(workList);
            SendObstructionEvents();

            OnPulsed?.Invoke();
        }

        public override void PulseAll() => Pulse();

        public override void Clear()
        {
            base.Clear();
            clearDetectedObjects();
            SendObstructionEvents();
            SampleArc();
        }

        // Returns the RayHit data associated with the detected GameObject
        public RayHit GetDetectionRayHit(GameObject detectedGameObject)
        {
            var result = RayHit.None;
            goWorkList.Clear();
            signalPipeline.GetInputObjects(detectedGameObject, goWorkList);
            foreach (var input in goWorkList)
            {
                RaycastHit hit;
                if (detectedObjectHits.TryGetValue(input, out hit))
                {
                    if (result.Equals(RayHit.None) || result.Distance > hit.distance)
                    {
                        result = new RayHit()
                        {
                            IsObstructing = false,
                            Point = hit.point,
                            Normal = hit.normal,
                            Distance = hit.distance,
                            DistanceFraction = hit.distance / totalLength,
                            Collider = hit.collider
                        };
                    }
                }
            }

            return result;
        }

        // Returns the RayHit data associated with the obstructing GameObject
        public RayHit GetObstructionRayHit()
        {
            if (!isObstructed || obstructionRayHit.collider == null)
            {
                return RayHit.None;
            }

            return new RayHit()
            {
                IsObstructing = true,
                Point = obstructionRayHit.point,
                Normal = obstructionRayHit.normal,
                Distance = obstructionRayHit.distance,
                DistanceFraction = obstructionRayHit.distance / totalLength,
                Collider = obstructionRayHit.collider
            };
        }

        #endregion

        #region Internals

        List<Vector3> arcPoints = new List<Vector3>();
        List<Vector3> obstructedArcPoints = new List<Vector3>();

        bool isObstructed = false;
        RaycastHit obstructionRayHit;
        Dictionary<GameObject, RaycastHit> detectedObjectHits = new Dictionary<GameObject, RaycastHit>();
        List<Signal> workList = new List<Signal>();
        float totalLength = 0f;
        List<GameObject> goWorkList = new List<GameObject>();
        MapToRigidBodyFilter mapToRB = new MapToRigidBodyFilter();

        static ArcSegmentTest arcSegmentTestInstance = new ArcSegmentTest();
        PhysicsNonAlloc<RaycastHit> physics;

        protected override void InitialiseSignalProcessors()
        {
            base.InitialiseSignalProcessors();
            mapToRB.Sensor = this;
            SignalProcessors.Add(mapToRB);
            SignalProcessors.Add(new MapToSignalProxyFilter());
            signalPipeline.Filter = signalFilter;
        }

        protected override List<Collider> GetInputColliders(GameObject InputObject, List<Collider> storeIn)
        {
            RaycastHit hit;
            if (detectedObjectHits.TryGetValue(InputObject, out hit))
            {
                storeIn.Add(hit.collider);
            }

            return storeIn;
        }

        protected override void Awake()
        {
            base.Awake();

            if (onObstruction == null)
            {
                onObstruction = new ObstructionEvent();
            }

            if (onClear == null)
            {
                onClear = new ObstructionEvent();
            }

            if (pulseRoutine == null)
            {
                pulseRoutine = new PulseRoutine();
            }

            pulseRoutine.Awake(this);
        }

        void OnEnable()
        {
            clearDetectedObjects();
            pulseRoutine.OnEnable();
        }

        void OnDisable() { pulseRoutine.OnDisable(); }

        void OnValidate() { pulseRoutine?.OnValidate(); }

        bool isSingleResult()
        {
            var layerMaskIsSubsset = ((DetectsOnLayers | ObstructedByLayers) & (~ObstructedByLayers)) == 0;
            return layerMaskIsSubsset && signalFilter.IsNull();
        }

        List<RaycastHit> hits = new List<RaycastHit>();

        void TestArc()
        {
            clearDetectedObjects();

            SampleArc();

            var accLength = 0f;
            var segmentIsObstructed = false;
            for (var i = 0; i < arcPoints.Count - 1; i++)
            {
                var p1 = arcPoints[i];
                var p2 = arcPoints[i + 1];
                var delta = p2 - p1;
                var length = delta.magnitude;
                var dir = delta / length;

                arcSegmentTestInstance.SegmentRay = new Ray(p1, dir);
                arcSegmentTestInstance.SegmentLength = length;
                var numberOfHits = physics.PerformTest(this, arcSegmentTestInstance);

                hits.Clear();
                for (int j = 0; j < numberOfHits; j++)
                {
                    var hit = physics.Buffer[j];
                    hit.distance += accLength;
                    hits.Add(hit);
                }

                hits.Sort(RaycastHitComparison);

                foreach (var hit in hits)
                {
                    if ((1 << hit.collider.gameObject.layer & DetectsOnLayers) != 0)
                    {
                        if (signalFilter.TestCollider(hit.collider))
                        {
                            addRayHit(hit);
                        }
                    }

                    if ((1 << hit.collider.gameObject.layer & ObstructedByLayers) != 0)
                    {
                        if (signalFilter.TestCollider(hit.collider))
                        {
                            obstructionRayHit = hit;
                            segmentIsObstructed = true;
                            break;
                        }
                    }
                }

                if (segmentIsObstructed)
                {
                    var obspoint = obstructionRayHit.point;
                    obstructedArcPoints.Add(obspoint);
                    for (var j = i + 1; j < arcPoints.Count; j++)
                    {
                        obstructedArcPoints.Add(arcPoints[j]);
                    }

                    arcPoints.RemoveRange(i + 1, obstructedArcPoints.Count - 1);
                    arcPoints.Add(obspoint);
                    break;
                }

                accLength += length;
            }
        }

        void SampleArc()
        {
            obstructedArcPoints.Clear();

            if (Parameterisation == ParameterisationType.Bezier)
            {
                arcPoints = Bezier.Sample(transform.position, WorldSpace ? Quaternion.identity : transform.rotation, arcPoints);
            }
            else if (Parameterisation == ParameterisationType.Ballistic)
            {
                arcPoints = Ballistic.Sample(transform.position, WorldSpace ? Quaternion.identity : transform.rotation, arcPoints);
            }

            totalLength = 0f;
            for (var i = 0; i < arcPoints.Count - 1; i++)
            {
                totalLength += (arcPoints[i + 1] - arcPoints[i]).magnitude;
            }
        }

        void SendObstructionEvents()
        {
            if (isObstructed && obstructionRayHit.collider == null)
            {
                isObstructed = false;
                OnClear.Invoke(this);
            }
            else if (!isObstructed && obstructionRayHit.collider != null)
            {
                isObstructed = true;
                OnObstruction.Invoke(this);
            }
        }

        void addRayHit(RaycastHit hit)
        {
            var go = hit.collider.gameObject;
            if (!detectedObjectHits.ContainsKey(go))
            {
                detectedObjectHits.Add(go, hit);
                workList.Add(new Signal() {Object = go, Strength = 1f, Bounds = new Bounds(hit.point, Vector3.zero)});
            }
        }

        void clearDetectedObjects()
        {
            obstructionRayHit = new RaycastHit();
            detectedObjectHits.Clear();
            workList.Clear();
        }

        static Comparison<RaycastHit> RaycastHitComparison = new Comparison<RaycastHit>(CompareRaycastHits);

        static int CompareRaycastHits(RaycastHit x, RaycastHit y)
        {
            if (x.distance < y.distance)
            {
                return -1;
            }
            else if (x.distance > y.distance)
            {
                return 1;
            }

            return 0;
        }

        protected override void OnDrawGizmosSelected()
        {
            //base.OnDrawGizmosSelected();

            if (!IsRunning())
            {
                SampleArc();
            }

            if (IsObstructed && IsRunning())
            {
                Gizmos.color = Color.red;
            }
            else
            {
                Gizmos.color = Color.cyan;
            }

            for (int i = 0; i < arcPoints.Count - 1; i++)
            {
                var p1 = arcPoints[i];
                var p2 = arcPoints[i + 1];
                Gizmos.DrawLine(p1, p2);
            }

            if (ShowDetectionGizmos)
            {
                foreach (var detection in GetDetections())
                {
                    var hit = GetDetectionRayHit(detection);
                    SensorGizmos.RaycastHitGizmo(hit.Point, hit.Normal, false);
                }

                if (IsObstructed)
                {
                    SensorGizmos.RaycastHitGizmo(GetObstructionRayHit().Point, GetObstructionRayHit().Normal, true);
                }
            }
        }

        bool IsRunning() { return Application.isPlaying || ShowDetectionGizmos; }

        class ArcSegmentTest : ITestNonAlloc<ArcSensor, RaycastHit>
        {
            public Ray SegmentRay;
            public float SegmentLength;

            public int Test(ArcSensor sensor, RaycastHit[] results)
            {
                var queryTriggerInteraction = sensor.IgnoreTriggerColliders ? QueryTriggerInteraction.Ignore : QueryTriggerInteraction.Collide;
                LayerMask combinedLayers = sensor.DetectsOnLayers | sensor.ObstructedByLayers;
                if (sensor.isSingleResult())
                {
                    RaycastHit hit;
                    if (Physics.Raycast(SegmentRay,
                            out hit,
                            SegmentLength,
                            combinedLayers,
                            queryTriggerInteraction))
                    {
                        results[0] = hit;
                        return 1;
                    }

                    return 0;
                }
                else
                {
                    return Physics.RaycastNonAlloc(SegmentRay,
                        results,
                        SegmentLength,
                        combinedLayers,
                        queryTriggerInteraction);
                }
            }
        }

        #endregion
    }
}