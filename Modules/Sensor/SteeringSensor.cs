using UnityEngine;
using UnityEngine.Serialization;
using System.Collections;
using System.Collections.Generic;

namespace Pancake.Sensor
{
    /**
     * The Steering Sensor is a unique sensor for implementing steering behaviour. It's an implementation of 
     * Context Based Steering as described here. The sensor can operate in a spherical mode suitable for flying 
     * agents, or a planar mode for ground-based agents.
     */
    [AddComponentMenu("Sensors/Steering Sensor")]
    public class SteeringSensor : BasePulsableSensor, IPulseRoutine, ISteeringSensor
    {
        #region Configurations

        [SerializeField] [Tooltip("Steering Vectors are 3D when this is true and they are planar when this is false.")]
        ObservableBool isSpherical;

        [SerializeField] [Tooltip("Determines the number of discrete buckets that directions around the sensor are boken up into.")]
        ObservableInt resolution = new ObservableInt() {Value = 3};

        [Tooltip("Speed that the sensor interpolates it's state.")] public float InterpolationSpeed = 8f;

        // Configurations struct for the Seek behaviour.
        [SerializeField, FormerlySerializedAs("Seek")] SteerSeek seek = new SteerSeek();

        // Configurations struct for the Avoid behaviour.
        [SerializeField, FormerlySerializedAs("Avoid")] SteerAvoid avoid = new SteerAvoid();

        [SerializeField] PulseRoutine pulseRoutine;

        [Tooltip("Enables the built-in locomotion if this is any value other then None.")]
        public LocomotionMode LocomotionMode;

        [Tooltip("The RigidBody to control with built-in locomotion.")]
        public Rigidbody RigidBody;

        [Tooltip("The CharacterController to control with built-in locomotion.")]
        public CharacterController CharacterController;

        // Configurations struct for the built-in locomotion behaviours.
        [SerializeField, FormerlySerializedAs("Locomotion")] LocomotionSystem locomotion;

        #endregion

        #region Events

        public override event System.Action OnPulsed;

        #endregion

        #region Public

        // Change IsSpherical value at runtime
        public bool IsSpherical { get => isSpherical.Value; set => isSpherical.Value = value; }

        // Change Resolution at runtime
        public int Resolution { get => Mathf.Abs(resolution.Value); set => resolution.Value = value; }

        // Change the pulse mode at runtime
        public PulseRoutine.Modes PulseMode { get => pulseRoutine.Mode.Value; set => pulseRoutine.Mode.Value = value; }

        // Change the pulse interval at runtime
        public float PulseInterval { get => pulseRoutine.Interval.Value; set => pulseRoutine.Interval.Value = value; }

        public SteerSeek Seek => seek;

        public SteerAvoid Avoid => avoid;

        public LocomotionSystem Locomotion => locomotion;

        // The Vector3 position we're currently seeking. Maps to Seek.Destination.
        public Vector3 Destination { get => seek.Destination; set => seek.Destination = value; }

        // The Transform that we're currently seeking. Maps to Seek.DestinationTransform.
        public Transform DestinationTransform { get => seek.DestinationTransform; set => seek.DestinationTransform = value; }

        // Is true when we are within the desired range from the target seek position.
        public bool IsDestinationReached => seek.GetIsDestinationReached(gameObject);

        // Is true when we have not yet reached the destination.
        public bool IsSeeking => !IsDestinationReached;

        // Returns a vector that the agent should move towards. It's length will be roughly the distance to the target position.
        public Vector3 GetSteeringVector() => interpolatedMap?.GetMaxContinuous() ?? Vector3.zero;

        // Calculates a new steering vector.
        public override void Pulse()
        {
            if (!isActiveAndEnabled)
            {
                return;
            }

            CalculateMaps();

            OnPulsed?.Invoke();
        }

        public override void PulseAll()
        {
            if (!Application.isPlaying)
            {
                GridConfigChangeHandler();
            }

            avoid.PulseSensors();
            Pulse();
        }

        public override void Clear()
        {
            interestMap.Fill(0);
            avoidMap.Fill(0);
            mergedMap.Fill(0);
            interpolatedMap.Fill(0);
        }

        #endregion

        #region Internals

        bool isControlling => LocomotionMode != LocomotionMode.None;

        ObservableEffect gridConfigEffect;

        DirectionalGrid interestMap;
        DirectionalGrid avoidMap;
        DirectionalGrid mergedMap;
        DirectionalGrid interpolatedMap;

        void CalculateMaps()
        {
            seek.SetInterest(gameObject, interestMap);
            avoid.SetAvoid(avoidMap);

            mergedMap.Copy(interestMap);

            var nCells = mergedMap.CellCount;
            for (int i = 0; i < nCells; i++)
            {
                var seekDistance = interestMap.GetCell(i).Value;
                var avoid = avoidMap.GetCell(i).Value;
                if (avoid == 0)
                {
                    continue;
                }

                var obstacleDistance = (this.avoid.DesiredDistance - avoid);
                if ((seekDistance - seek.AcceptedDistanceRange) < obstacleDistance)
                {
                    avoidMap.GetCell(i).SetValue(0);
                }
            }

            var minAvoid = avoidMap.GetMinCell().VectorValue;
            mergedMap.MaskUnder(avoidMap, minAvoid.magnitude);

            if (Application.isPlaying && PulseMode != PulseRoutine.Modes.Manual)
            {
                interpolatedMap.InterpolateTo(mergedMap, pulseRoutine.dt * InterpolationSpeed);
            }
            else
            {
                interpolatedMap.Copy(mergedMap);
            }
        }

        void Awake()
        {
            if (isSpherical == null)
            {
                isSpherical = new ObservableBool();
            }

            if (resolution == null)
            {
                resolution = new ObservableInt() {Value = 3};
            }

            gridConfigEffect = ObservableEffect.Create(GridConfigChangeHandler, new Observable[] {isSpherical, resolution});

            if (pulseRoutine == null)
            {
                pulseRoutine = new PulseRoutine();
            }

            pulseRoutine.Awake(this);
        }

        void OnEnable() { pulseRoutine.OnEnable(); }

        void OnDisable() { pulseRoutine.OnDisable(); }

        void OnDestroy() { gridConfigEffect.Dispose(); }

        void OnValidate()
        {
            isSpherical?.OnValidate();
            resolution?.OnValidate();
            pulseRoutine?.OnValidate();
        }

        void Update()
        {
            if (LocomotionMode == LocomotionMode.UnityCharacterController)
            {
                locomotion.CharacterSeek(CharacterController, transform.position + GetSteeringVector(), Vector3.up);
            }
        }

        void FixedUpdate()
        {
            if (LocomotionMode == LocomotionMode.RigidBodyFlying)
            {
                locomotion.FlyableSeek(RigidBody, transform.position + GetSteeringVector());
            }
            else if (LocomotionMode == LocomotionMode.RigidBodyCharacter)
            {
                locomotion.CharacterSeek(RigidBody, transform.position + GetSteeringVector(), Vector3.up);
            }
        }

        void GridConfigChangeHandler()
        {
            if (IsSpherical)
            {
                interestMap = new SphereGrid(Resolution);
                avoidMap = new SphereGrid(Resolution);
                mergedMap = new SphereGrid(Resolution);
                interpolatedMap = new SphereGrid(Resolution);
            }
            else
            {
                interestMap = new CircleGrid(Vector3.up, Resolution * 4);
                avoidMap = new CircleGrid(Vector3.up, Resolution * 4);
                mergedMap = new CircleGrid(Vector3.up, Resolution * 4);
                interpolatedMap = new CircleGrid(Vector3.up, Resolution * 4);
            }
        }

        void OnDrawGizmosSelected()
        {
            if (!ShowDetectionGizmos)
            {
                return;
            }

            Gizmos.color = Color.yellow;
            interpolatedMap?.DrawGizmos(transform.position, 1, 1f / (transform.position - seek.Destination).magnitude);

            Gizmos.color = Color.red;
            avoidMap?.DrawGizmos(transform.position, 2, 1f / avoid.DesiredDistance);

            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position, transform.position + GetSteeringVector());

            avoid.DrawGizmos();
        }

        #endregion
    }
}