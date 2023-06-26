using UnityEngine;
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
    [AddComponentMenu("Sensors/2D Ray Sensor")]
    public class RaySensor2D : Sensor, IRayCastingSensor, IPulseRoutine
    {
        #region Configurations

        public enum CastShapeType
        {
            Ray,
            Circle,
            Box,
            Capsule
        }

        [Tooltip("The shape of the ray. Determines which Physics.Raycast function is called.")]
        public CastShapeType Shape;

        // Configurations for the Circle shape
        public SphereShape Circle = new SphereShape(1f);

        // Configurations for the Box shape
        public Box2DShape Box = new Box2DShape(Vector2.one * .5f);

        // Configurations for the Capsule shape
        public CapsuleShape Capsule = new CapsuleShape(.5f, 1f);

        [SerializeField] SignalFilter signalFilter = new SignalFilter();

        [Tooltip("The detection range in world units.")] public float Length = 5f;

        [Tooltip("The vector direction that the ray is cast in.")] public Vector2 Direction = Vector2.up;

        [Tooltip("Is the Direction parameter in world space or local space.")]
        public bool WorldSpace = false;

        [Tooltip("A layer mask specifying which physics layers objects will be detected on.")]
        public LayerMask DetectsOnLayers;

        [Tooltip("A layer mask specifying which physics layers objects will obstruct the ray on.")]
        public LayerMask ObstructedByLayers;

        [Tooltip("In Collider mode the sensor detects GameObjects attached to colliders. In RigidBody mode it detects the RigidBody GameObject attached to colliders.")]
        public DetectionModes DetectionMode;

        [Tooltip("Ignores all trigger colliders. Will not detect them or be obstructed by them.")]
        public bool IgnoreTriggerColliders;

        [Range(0f, 90f)] [Tooltip("Calculated slope angle must be greater for the intersection to be a detection or an obstruction.")]
        public float MinimumSlopeAngle = 0f;

        [Tooltip("Measure slope angle between this direction and the Normal of the RayCastHit. Interpreted in world-space.")]
        public Vector2 SlopeUpDirection = Vector3.up;

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

        public void SetCircleShape(float radius)
        {
            Circle.Radius = radius;
            Shape = CastShapeType.Circle;
        }

        public void SetBoxShape(Vector2 halfExtents)
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
                physics = new PhysicsNonAlloc<RaycastHit2D>();
            }

            mapToRB.IsRigidBodyMode = DetectionMode == DetectionModes.RigidBodies;
            mapToRB.Is2D = true;

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
                RaycastHit2D hit;
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
                            Collider2D = hit.collider
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
                DistanceFraction = obstructionRayHit.distance / Length,
                Collider2D = obstructionRayHit.collider
            };
        }

        #endregion

        #region Internals

        Vector2 direction =>
            WorldSpace ? Direction.normalized : (Vector2) (Quaternion.AngleAxis(transform.rotation.eulerAngles.z, Vector3.forward) * Direction.normalized);

        QueryTriggerInteraction queryTriggerInteraction { get { return IgnoreTriggerColliders ? QueryTriggerInteraction.Ignore : QueryTriggerInteraction.Collide; } }

        bool isObstructed = false;
        RaycastHit2D obstructionRayHit;
        Dictionary<GameObject, RaycastHit2D> detectedObjectHits = new Dictionary<GameObject, RaycastHit2D>();
        List<Signal> workList = new List<Signal>();
        List<GameObject> goWorkList = new List<GameObject>();
        MapToRigidBodyFilter mapToRB = new MapToRigidBodyFilter();

        static CircleCastTest SphereCastTestInstance = new CircleCastTest();
        static BoxCastTest BoxCastTestInstance = new BoxCastTest();
        static CapsuleCastTest CapsuleCastTestInstance = new CapsuleCastTest();
        static RayCastTest RayCastTestInstance = new RayCastTest();

        ITestNonAlloc<RaySensor2D, RaycastHit2D> physicsTest
        {
            get
            {
                if (Shape == CastShapeType.Circle)
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

        PhysicsNonAlloc<RaycastHit2D> physics;

        protected override void InitialiseSignalProcessors()
        {
            base.InitialiseSignalProcessors();
            mapToRB.Sensor = this;
            SignalProcessors.Add(mapToRB);
            SignalProcessors.Add(new MapToSignalProxyFilter());
            signalPipeline.Filter = signalFilter;
        }

        protected override List<Collider2D> GetInputColliders(GameObject InputObject, List<Collider2D> storeIn)
        {
            RaycastHit2D hit;
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
            return layerMaskIsSubsset && MinimumSlopeAngle == 0f && signalFilter.IsNull();
        }

        List<RaycastHit2D> hits = new List<RaycastHit2D>();

        void TestRay()
        {
            clearDetectedObjects();

            var saveQHT = Physics2D.queriesHitTriggers;
            Physics2D.queriesHitTriggers = !IgnoreTriggerColliders;
            var numberOfHits = physics.PerformTest(this, physicsTest);
            Physics2D.queriesHitTriggers = saveQHT;

            hits.Clear();
            for (int i = 0; i < numberOfHits; i++)
            {
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

        void addRayHit(RaycastHit2D hit)
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
            obstructionRayHit = new RaycastHit2D();
            detectedObjectHits.Clear();
            workList.Clear();
        }

        static Comparison<RaycastHit2D> RaycastHitComparison = new Comparison<RaycastHit2D>(CompareRaycastHits);

        static int CompareRaycastHits(RaycastHit2D x, RaycastHit2D y)
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
                case CastShapeType.Circle:
                    DrawCircleGizmo();
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
                var depth = Vector3.forward * transform.position.z;
                foreach (var detection in GetDetections())
                {
                    var hit = GetDetectionRayHit(detection);
                    SensorGizmos.RaycastHitGizmo(hit.Point + depth, hit.Normal, false);
                }

                if (IsObstructed)
                {
                    SensorGizmos.RaycastHitGizmo(GetObstructionRayHit().Point + depth, GetObstructionRayHit().Normal, true);
                }
            }
        }

        bool IsRunning() { return Application.isPlaying || ShowDetectionGizmos; }

        class RayCastTest : ITestNonAlloc<RaySensor2D, RaycastHit2D>
        {
            public int Test(RaySensor2D sensor, RaycastHit2D[] results)
            {
                Ray ray = new Ray(sensor.transform.position, sensor.direction);
                LayerMask combinedLayers = sensor.DetectsOnLayers | sensor.ObstructedByLayers;
                if (sensor.isSingleResult())
                {
                    var hit = Physics2D.Raycast(ray.origin, ray.direction, sensor.Length, combinedLayers);
                    if (hit.collider != null)
                    {
                        results[0] = hit;
                        return 1;
                    }

                    return 0;
                }
                else
                {
                    return Physics2D.RaycastNonAlloc(ray.origin,
                        ray.direction,
                        results,
                        sensor.Length,
                        combinedLayers);
                }
            }
        }

        void DrawRayGizmo()
        {
            Vector3 endPosition;
            if (IsObstructed && IsRunning())
            {
                SensorGizmos.PushColor(Color.red);
                endPosition = transform.position + (Vector3) direction * obstructionRayHit.distance;
            }
            else
            {
                SensorGizmos.PushColor(Color.cyan);
                endPosition = transform.position + (Vector3) direction * Length;
            }

            Gizmos.DrawLine(transform.position, endPosition);
            SensorGizmos.PopColor();
        }

        class CircleCastTest : ITestNonAlloc<RaySensor2D, RaycastHit2D>
        {
            public int Test(RaySensor2D sensor, RaycastHit2D[] results)
            {
                Ray ray = new Ray(sensor.transform.position, sensor.direction);
                LayerMask combinedLayers = sensor.DetectsOnLayers | sensor.ObstructedByLayers;
                if (sensor.isSingleResult())
                {
                    var hit = Physics2D.CircleCast(ray.origin,
                        sensor.Circle.Radius,
                        ray.direction,
                        sensor.Length,
                        combinedLayers);
                    if (hit.collider != null)
                    {
                        results[0] = hit;
                        return 1;
                    }

                    return 0;
                }
                else
                {
                    return Physics2D.CircleCastNonAlloc(ray.origin,
                        sensor.Circle.Radius,
                        ray.direction,
                        results,
                        sensor.Length,
                        combinedLayers);
                }
            }
        }

        void DrawCircleGizmo()
        {
            var showObstruction = (isObstructed && IsRunning());
            var length = showObstruction ? obstructionRayHit.distance : Length;
            var euler = Quaternion.Euler(0, 0, transform.rotation.eulerAngles.z);
            var direction = WorldSpace ? Quaternion.Inverse(euler) * Direction : (Vector3) Direction;
            SensorGizmos.CirclecastGizmo(new Ray(transform.position, direction),
                length,
                euler,
                Circle.Radius,
                showObstruction);
        }

        class BoxCastTest : ITestNonAlloc<RaySensor2D, RaycastHit2D>
        {
            public int Test(RaySensor2D sensor, RaycastHit2D[] results)
            {
                LayerMask combinedLayers = sensor.DetectsOnLayers | sensor.ObstructedByLayers;
                if (sensor.isSingleResult())
                {
                    var hit = Physics2D.BoxCast(sensor.transform.position,
                        sensor.Box.HalfExtents,
                        sensor.transform.eulerAngles.z,
                        sensor.direction,
                        sensor.Length,
                        combinedLayers);
                    if (hit.collider != null)
                    {
                        results[0] = hit;
                        return 1;
                    }

                    return 0;
                }
                else
                {
                    return Physics2D.BoxCastNonAlloc(sensor.transform.position,
                        sensor.Box.HalfExtents,
                        sensor.transform.eulerAngles.z,
                        sensor.direction,
                        results,
                        sensor.Length,
                        combinedLayers);
                }
            }
        }

        void DrawBoxGizmo()
        {
            var showObstruction = (isObstructed && IsRunning());
            var length = showObstruction ? obstructionRayHit.distance : Length;
            var euler = Quaternion.Euler(0, 0, transform.rotation.eulerAngles.z);
            var direction = WorldSpace ? Quaternion.Inverse(euler) * Direction : (Vector3) Direction;
            SensorGizmos.BoxcastGizmo(new Ray(transform.position, direction),
                length,
                euler,
                Box.HalfExtents,
                showObstruction);
        }

        class CapsuleCastTest : ITestNonAlloc<RaySensor2D, RaycastHit2D>
        {
            public int Test(RaySensor2D sensor, RaycastHit2D[] results)
            {
                LayerMask combinedLayers = sensor.DetectsOnLayers | sensor.ObstructedByLayers;

                var pos = sensor.transform.position;
                var size = new Vector2(sensor.Capsule.Radius * 2f, sensor.Capsule.Height);
                var dir = CapsuleDirection2D.Vertical;
                var angle = sensor.transform.eulerAngles.z;

                if (sensor.isSingleResult())
                {
                    var hit = Physics2D.CapsuleCast(pos,
                        size,
                        dir,
                        angle,
                        sensor.direction,
                        sensor.Length,
                        combinedLayers);
                    if (hit.collider != null)
                    {
                        results[0] = hit;
                        return 1;
                    }

                    return 0;
                }
                else
                {
                    return Physics2D.CapsuleCastNonAlloc(pos,
                        size,
                        dir,
                        angle,
                        sensor.direction,
                        results,
                        sensor.Length,
                        combinedLayers);
                }
            }
        }

        void DrawCapsuleGizmo()
        {
            var showObstruction = (isObstructed && IsRunning());
            var length = showObstruction ? obstructionRayHit.distance : Length;
            var euler = Quaternion.Euler(0, 0, transform.rotation.eulerAngles.z);
            var direction = WorldSpace ? Quaternion.Inverse(euler) * Direction : (Vector3) Direction;
            SensorGizmos.Capsule2DcastGizmo(new Ray(transform.position, direction),
                length,
                euler,
                Capsule.Radius,
                Capsule.Height,
                showObstruction);
        }

        #endregion
    }
}