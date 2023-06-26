using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Pancake.Sensor
{
    /*
     * The Range Sensor detects objects that are inside its detection volume. It uses the family of Overlap functions inside Physics or Physics2D. 
     * A detected object will have one or more Collider that overlaps the detection volume.
     */
    [AddComponentMenu("Sensors/Range Sensor")]
    public class RangeSensor : BaseVolumeSensor, IPulseRoutine
    {
        #region Configurations

        public enum Shapes
        {
            Sphere,
            Box,
            Capsule
        }

        // Determines which shape of Physics.Overlap[...] function to use
        public Shapes Shape;

        // Configurations for Sphere shape.
        public SphereShape Sphere = new SphereShape(1f);

        // Configurations for Box shape.
        public BoxShape Box = new BoxShape(Vector3.one * .5f);

        // Configurations for Capsule shape.
        public CapsuleShape Capsule = new CapsuleShape(.5f, 1f);

        [Tooltip("A layer mask specifying which physics layers objects will be detected on.")]
        public LayerMask DetectsOnLayers;

        [Tooltip("The sensor will not detect trigger colliders when this is set to true.")]
        public bool IgnoreTriggerColliders;

        [SerializeField] PulseRoutine pulseRoutine;

        #endregion

        #region Events

        public override event Action OnPulsed;

        #endregion

        #region Public

        // Change the pulse mode at runtime
        public PulseRoutine.Modes PulseMode { get => pulseRoutine.Mode.Value; set => pulseRoutine.Mode.Value = value; }

        // Change the pulse interval at runtime
        public float PulseInterval { get => pulseRoutine.Interval.Value; set => pulseRoutine.Interval.Value = value; }

        // The array size allocated for storing results from Physics.RaycastNonAlloc. Will automatically
        // be doubled in size when more space is needed.
        public int CurrentBufferSize => physics != null ? physics.Buffer.Length : 0;

        public void SetSphereShape(float radius)
        {
            Shape = Shapes.Sphere;
            Sphere.Radius = radius;
        }

        public void SetBoxShape(Vector3 halfExtents)
        {
            Shape = Shapes.Box;
            Box.HalfExtents = halfExtents;
        }

        public void SetCapsuleShape(float radius, float height)
        {
            Shape = Shapes.Capsule;
            Capsule.Radius = radius;
            Capsule.Height = height;
        }

        // Pulses the sensor to update its list of detected objects
        public override void Pulse()
        {
            if (!isActiveAndEnabled)
            {
                return;
            }

            if (physics == null)
            {
                physics = new PhysicsNonAlloc<Collider>();
            }

            int numberDetected = physics.PerformTest(this, physicsTest);

            ClearColliders();

            for (var i = 0; i < numberDetected; i++)
            {
                AddCollider(physics.Buffer[i], false);
            }

            UpdateAllSignals();
            OnPulsed?.Invoke();
        }

        public override void PulseAll() => Pulse();

        #endregion

        #region Internals

        static OverlapSphereTest SphereTestInstance = new OverlapSphereTest();
        static OverlapBoxTest BoxTestInstance = new OverlapBoxTest();
        static OverlapCapsuleTest CapsuleTestInstance = new OverlapCapsuleTest();

        ITestNonAlloc<RangeSensor, Collider> physicsTest
        {
            get
            {
                switch (Shape)
                {
                    case Shapes.Sphere:
                        return SphereTestInstance;
                    case Shapes.Box:
                        return BoxTestInstance;
                    case Shapes.Capsule:
                        return CapsuleTestInstance;
                    default:
                        return SphereTestInstance;
                }
            }
        }

        PhysicsNonAlloc<Collider> physics;


        protected override void Awake()
        {
            base.Awake();

            if (pulseRoutine == null)
            {
                pulseRoutine = new PulseRoutine();
            }

            pulseRoutine.Awake(this);
        }

        protected void OnEnable() { pulseRoutine.OnEnable(); }

        void OnDisable() { pulseRoutine.OnDisable(); }

        void OnValidate() { pulseRoutine?.OnValidate(); }

        protected override void OnDrawGizmosSelected()
        {
            base.OnDrawGizmosSelected();
            switch (Shape)
            {
                case Shapes.Sphere:
                    DrawSphereGizmo();
                    break;
                case Shapes.Box:
                    DrawBoxGizmo();
                    break;
                case Shapes.Capsule:
                    DrawCapsuleGizmo();
                    break;
            }
        }

        class OverlapSphereTest : ITestNonAlloc<RangeSensor, Collider>
        {
            public int Test(RangeSensor sensor, Collider[] results)
            {
                var queryTriggerInteraction = sensor.IgnoreTriggerColliders ? QueryTriggerInteraction.Ignore : QueryTriggerInteraction.Collide;
                return Physics.OverlapSphereNonAlloc(sensor.transform.position,
                    sensor.Sphere.Radius,
                    results,
                    sensor.DetectsOnLayers,
                    queryTriggerInteraction);
            }
        }

        void DrawSphereGizmo()
        {
            SensorGizmos.PushColor(Color.cyan);
            SensorGizmos.SphereGizmo(transform.position, Sphere.Radius);
            SensorGizmos.PopColor();
        }

        class OverlapBoxTest : ITestNonAlloc<RangeSensor, Collider>
        {
            public int Test(RangeSensor sensor, Collider[] results)
            {
                var queryTriggerInteraction = sensor.IgnoreTriggerColliders ? QueryTriggerInteraction.Ignore : QueryTriggerInteraction.Collide;
                return Physics.OverlapBoxNonAlloc(sensor.transform.position,
                    sensor.Box.HalfExtents,
                    results,
                    sensor.transform.rotation,
                    sensor.DetectsOnLayers,
                    queryTriggerInteraction);
            }
        }

        void DrawBoxGizmo()
        {
            Gizmos.color = Color.cyan;
            Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one);
            Gizmos.DrawWireCube(Vector3.zero, Box.HalfExtents * 2f);
            Gizmos.matrix = Matrix4x4.identity;
        }

        class OverlapCapsuleTest : ITestNonAlloc<RangeSensor, Collider>
        {
            public int Test(RangeSensor sensor, Collider[] results)
            {
                var queryTriggerInteraction = sensor.IgnoreTriggerColliders ? QueryTriggerInteraction.Ignore : QueryTriggerInteraction.Collide;
                var pt1 = sensor.transform.position + sensor.transform.up * sensor.Capsule.Height / 2f;
                var pt2 = sensor.transform.position - sensor.transform.up * sensor.Capsule.Height / 2f;
                return Physics.OverlapCapsuleNonAlloc(pt1,
                    pt2,
                    sensor.Capsule.Radius,
                    results,
                    sensor.DetectsOnLayers,
                    queryTriggerInteraction);
            }
        }

        void DrawCapsuleGizmo()
        {
            SensorGizmos.PushColor(Color.cyan);
            SensorGizmos.PushMatrix(Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one));
            SensorGizmos.CapsuleGizmo(Vector3.zero, Capsule.Radius, Capsule.Height);
            SensorGizmos.PopMatrix();
            SensorGizmos.PopColor();
        }

        #endregion
    }
}