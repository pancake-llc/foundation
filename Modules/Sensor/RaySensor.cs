using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;
using System;

namespace Pancake.Sensor
{
    /*
     * The Ray Sensor detects objects intersected by a ray. It uses the family of raycast functions inside Physics or Physics2D. The sensor detects 
     * all objects along its path up until it hits an Obstructing Collider. It will not detect objects beyond the obstruction.
     */
    [AddComponentMenu("Sensors/Ray Sensor")]
    public class RaySensor : Sensor, IRayCastingSensor, IPulseRoutine
    {
        #region Configurations

        public enum CastShapeType
        {
            Ray,
            Sphere,
            Box,
            Capsule
        }

        [Tooltip("The shape of the ray. Determines which Physics.Raycast function is called.")]
        public CastShapeType Shape;

        // Configurations for the Sphere shape
        public SphereShape Sphere = new SphereShape(1f);

        // Configurations for the Box shape
        public BoxShape Box = new BoxShape(Vector3.one * .5f);

        // Configurations for the Capsule shape
        public CapsuleShape Capsule = new CapsuleShape(.5f, 1f);

        [SerializeField] SignalFilter signalFilter = new SignalFilter();

        [Tooltip("The detection range in world units.")] public float Length = 5f;

        [Tooltip("The vector direction that the ray is cast in.")] public Vector3 Direction = Vector3.forward;

        [Tooltip("Is the Direction parameter in world space or local space.")]
        public bool WorldSpace = false;

        [Tooltip("A layer mask specifying which physics layers objects will be detected on.")]
        public LayerMask DetectsOnLayers;

        [Tooltip("A layer mask specifying which physics layers objects will obstruct the ray on.")]
        public LayerMask ObstructedByLayers;

        [Tooltip("In Collider mode the sensor detects GameObjects attached to colliders. In RigidBody mode it detects the Collider.AttachedRigidbody.")]
        public DetectionModes DetectionMode;

        [Tooltip("Ignores all trigger colliders. Will not detect them or be obstructed by them.")]
        public bool IgnoreTriggerColliders;

        [Range(0f, 90f)] [Tooltip("Calculated slope angle must be greater for the intersection to be a detection or an obstruction.")]
        public float MinimumSlopeAngle = 0f;

        [Tooltip("Measure slope angle between this direction and the Normal of the RayCastHit. Interpreted in world-space.")]
        public Vector3 SlopeUpDirection = Vector3.up;

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

        public void SetRayShape() { Shape = CastShapeType.Ray; }

        public void SetSphereShape(float radius)
        {
            Sphere.Radius = radius;
            Shape = CastShapeType.Sphere;
        }

        public void SetBoxShape(Vector3 halfExtents)
        {
            Box.HalfExtents = halfExtents;
            Shape = CastShapeType.Box;
        }

        public void SetCapsuleShape(float radius, float height)
        {
            Capsule.Radius = radius;
            Capsule.Height = height;
            Shape = CastShapeType.Capsule;
        }

        // Pulse the ray sensor
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

            TestRay();

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
                            DistanceFraction = hit.distance / Length,
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

            return new RayHit
            {
                IsObstructing = true,
                Point = obstructionRayHit.point,
                Normal = obstructionRayHit.normal,
                Distance = obstructionRayHit.distance,
                DistanceFraction = obstructionRayHit.distance / Length,
                Collider = obstructionRayHit.collider
            };
        }

        #endregion

        #region Internals

        Vector3 direction => WorldSpace ? Direction.normalized : transform.rotation * Direction.normalized;

        QueryTriggerInteraction queryTriggerInteraction => IgnoreTriggerColliders ? QueryTriggerInteraction.Ignore : QueryTriggerInteraction.Collide;

        bool isObstructed = false;
        RaycastHit obstructionRayHit;
        Dictionary<GameObject, RaycastHit> detectedObjectHits = new Dictionary<GameObject, RaycastHit>();
        List<Signal> workList = new List<Signal>();
        List<GameObject> goWorkList = new List<GameObject>();
        MapToRigidBodyFilter mapToRB = new MapToRigidBodyFilter();

        static SphereCastTest SphereCastTestInstance = new SphereCastTest();
        static BoxCastTest BoxCastTestInstance = new BoxCastTest();
        static CapsuleCastTest CapsuleCastTestInstance = new CapsuleCastTest();
        static RayCastTest RayCastTestInstance = new RayCastTest();

        ITestNonAlloc<RaySensor, RaycastHit> physicsTest
        {
            get
            {
                if (Shape == CastShapeType.Sphere)
                {
                    return SphereCastTestInstance;
                }
                else if (Shape == CastShapeType.Box)
                {
                    return BoxCastTestInstance;
                }
                else if (Shape == CastShapeType.Capsule)
                {
                    return CapsuleCastTestInstance;
                }

                return RayCastTestInstance;
            }
        }

        PhysicsNonAlloc<RaycastHit> physics;

        protected override void InitialiseSignalProcessors()
        {
            base.InitialiseSignalProcessors();
            mapToRB.Sensor = this;
            SignalProcessors.Add(mapToRB);
            SignalProcessors.Add(new MapToSignalProxyFilter());
            signalPipeline.Filter = signalFilter;
        }

        protected override List<Collider> GetInputColliders(GameObject inputObject, List<Collider> storeIn)
        {
            RaycastHit hit;
            if (detectedObjectHits.TryGetValue(inputObject, out hit))
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
            return layerMaskIsSubsset && MinimumSlopeAngle == 0f && signalFilter.IsNull();
        }

        List<RaycastHit> hits = new List<RaycastHit>();

        void TestRay()
        {
            clearDetectedObjects();

            var numberOfHits = physics.PerformTest(this, physicsTest);

            hits.Clear();
            for (int i = 0; i < numberOfHits; i++)
            {
                var hit = physics.Buffer[i];
                if (hit.distance == 0)
                {
                    continue;
                }

                hits.Add(physics.Buffer[i]);
            }

            hits.Sort(RaycastHitComparison);

            foreach (var hit in hits)
            {
                if (MinimumSlopeAngle > 0f)
                {
                    var slope = Vector3.Angle(SlopeUpDirection, hit.normal);
                    if (slope < MinimumSlopeAngle)
                    {
                        continue;
                    }
                }

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
                        break;
                    }
                }
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

            switch (Shape)
            {
                case CastShapeType.Ray:
                    DrawRayGizmo();
                    break;
                case CastShapeType.Sphere:
                    DrawSphereGizmo();
                    break;
                case CastShapeType.Box:
                    DrawBoxGizmo();
                    break;
                case CastShapeType.Capsule:
                    DrawCapsuleGizmo();
                    break;
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

        class RayCastTest : ITestNonAlloc<RaySensor, RaycastHit>
        {
            public int Test(RaySensor sensor, RaycastHit[] results)
            {
                var queryTriggerInteraction = sensor.IgnoreTriggerColliders ? QueryTriggerInteraction.Ignore : QueryTriggerInteraction.Collide;
                Ray ray = new Ray(sensor.transform.position, sensor.direction);
                LayerMask combinedLayers = sensor.DetectsOnLayers | sensor.ObstructedByLayers;
                if (sensor.isSingleResult())
                {
                    if (Physics.Raycast(ray,
                            out var hit,
                            sensor.Length,
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
                    return Physics.RaycastNonAlloc(ray,
                        results,
                        sensor.Length,
                        combinedLayers,
                        queryTriggerInteraction);
                }
            }
        }

        void DrawRayGizmo()
        {
            Vector3 endPosition;
            if (IsObstructed && IsRunning())
            {
                Gizmos.color = Color.red;
                endPosition = transform.position + direction * obstructionRayHit.distance;
            }
            else
            {
                Gizmos.color = Color.cyan;
                endPosition = transform.position + direction * Length;
            }

            Gizmos.DrawLine(transform.position, endPosition);
        }

        class SphereCastTest : ITestNonAlloc<RaySensor, RaycastHit>
        {
            public int Test(RaySensor sensor, RaycastHit[] results)
            {
                var queryTriggerInteraction = sensor.IgnoreTriggerColliders ? QueryTriggerInteraction.Ignore : QueryTriggerInteraction.Collide;
                Ray ray = new Ray(sensor.transform.position, sensor.direction);
                LayerMask combinedLayers = sensor.DetectsOnLayers | sensor.ObstructedByLayers;
                if (sensor.isSingleResult())
                {
                    if (Physics.SphereCast(ray,
                            sensor.Sphere.Radius,
                            out var hit,
                            sensor.Length,
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
                    return Physics.SphereCastNonAlloc(ray,
                        sensor.Sphere.Radius,
                        results,
                        sensor.Length,
                        combinedLayers,
                        queryTriggerInteraction);
                }
            }
        }

        void DrawSphereGizmo()
        {
            var showObstruction = (isObstructed && IsRunning());
            var length = showObstruction ? obstructionRayHit.distance : Length;
            var direction = WorldSpace ? transform.InverseTransformDirection(Direction) : Direction;
            SensorGizmos.SpherecastGizmo(new Ray(transform.position, direction),
                length,
                transform.rotation,
                Sphere.Radius,
                showObstruction);
        }

        class BoxCastTest : ITestNonAlloc<RaySensor, RaycastHit>
        {
            public int Test(RaySensor sensor, RaycastHit[] results)
            {
                var queryTriggerInteraction = sensor.IgnoreTriggerColliders ? QueryTriggerInteraction.Ignore : QueryTriggerInteraction.Collide;
                LayerMask combinedLayers = sensor.DetectsOnLayers | sensor.ObstructedByLayers;
                if (sensor.isSingleResult())
                {
                    if (Physics.BoxCast(sensor.transform.position,
                            sensor.Box.HalfExtents,
                            sensor.direction,
                            out var hit,
                            sensor.transform.rotation,
                            sensor.Length,
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
                    return Physics.BoxCastNonAlloc(sensor.transform.position,
                        sensor.Box.HalfExtents,
                        sensor.direction,
                        results,
                        sensor.transform.rotation,
                        sensor.Length,
                        combinedLayers,
                        queryTriggerInteraction);
                }
            }
        }

        void DrawBoxGizmo()
        {
            var showObstruction = (isObstructed && IsRunning());
            var length = showObstruction ? obstructionRayHit.distance : Length;
            var direction = WorldSpace ? transform.InverseTransformDirection(Direction) : Direction;
            SensorGizmos.BoxcastGizmo(new Ray(transform.position, direction),
                length,
                transform.rotation,
                Box.HalfExtents,
                showObstruction);
        }

        class CapsuleCastTest : ITestNonAlloc<RaySensor, RaycastHit>
        {
            public int Test(RaySensor sensor, RaycastHit[] results)
            {
                var queryTriggerInteraction = sensor.IgnoreTriggerColliders ? QueryTriggerInteraction.Ignore : QueryTriggerInteraction.Collide;
                LayerMask combinedLayers = sensor.DetectsOnLayers | sensor.ObstructedByLayers;

                var center = sensor.transform.position;
                var up = sensor.transform.up;
                var pt1 = center + up * sensor.Capsule.Height / 2f;
                var pt2 = center - up * sensor.Capsule.Height / 2f;

                if (sensor.isSingleResult())
                {
                    if (Physics.CapsuleCast(pt1,
                            pt2,
                            sensor.Capsule.Radius,
                            sensor.direction,
                            out var hit,
                            sensor.Length,
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
                    return Physics.CapsuleCastNonAlloc(pt1,
                        pt2,
                        sensor.Capsule.Radius,
                        sensor.direction,
                        results,
                        sensor.Length,
                        combinedLayers,
                        queryTriggerInteraction);
                }
            }
        }

        void DrawCapsuleGizmo()
        {
            var showObstruction = (isObstructed && IsRunning());
            var length = showObstruction ? obstructionRayHit.distance : Length;
            var direction = WorldSpace ? transform.InverseTransformDirection(Direction) : Direction;
            SensorGizmos.CapsulecastGizmo(new Ray(transform.position, direction),
                length,
                transform.rotation,
                Capsule.Radius,
                Capsule.Height,
                showObstruction);
        }

        #endregion
    }
}