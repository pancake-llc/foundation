using Pancake.Common;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Pancake.Component
{
    /// <summary>
    /// Add this component to an object, and it'll get moved towards the target at update, with or without interpolation based on your settings
    /// </summary>
    public class FollowTargetComponet : GameUnit
    {
        /// the possible follow modes
        public enum FollowModes
        {
            RegularLerp,

            // ReSharper disable once InconsistentNaming
            MMLerp,
            Spring
        }

        /// whether to operate in world or local space
        public enum PositionSpaces
        {
            World,
            Local
        }

        [Header("Follow Position")] public bool followPosition = true;

        [ShowIf(nameof(followPosition)), Indent, LabelText("Follow X")]
        public bool followPositionX = true;

        [ShowIf(nameof(followPosition)), Indent, LabelText("Follow Y")]
        public bool followPositionY = true;

        [ShowIf(nameof(followPosition)), Indent, LabelText("Follow Z")]
        public bool followPositionZ = true;

        [ShowIf(nameof(followPosition)), Indent, LabelText("Space")] public PositionSpaces positionSpace = PositionSpaces.World;

        [Header("Follow Rotation")] public bool followRotation = true;

        [Header("Follow Scale")] public bool followScale = true;
        [ShowIf(nameof(followScale)), Indent] public float followScaleFactor = 1f;

        [Header("Target")] public Transform target;

        [ShowIf(nameof(followPosition))] public Vector3 offset;
        [ShowIf(nameof(followPosition))] public bool addInitialDistanceXToXOffset;
        [ShowIf(nameof(followPosition))] public bool addInitialDistanceYToYOffset;
        [ShowIf(nameof(followPosition))] public bool addInitialDistanceZToZOffset;

        [Header("Position Interpolation")] public bool interpolatePosition = true;

        [ShowIf(nameof(interpolatePosition)), Indent] public FollowModes followPositionMode = FollowModes.MMLerp;
        [ShowIf(nameof(interpolatePosition)), Indent] public float followPositionSpeed = 10f;

        /// higher values mean more damping, less spring, low values mean less damping, more spring
        [ShowIf(nameof(followPositionMode), FollowModes.Spring), Indent, LabelText("Spring Damping")] [Range(0.01f, 1.0f)]
        public float positionSpringDamping = 0.3f;

        /// the frequency at which the spring should "vibrate", in Hz (1 : the spring will do one full period in one second)
        [ShowIf(nameof(followPositionMode), FollowModes.Spring), Indent, LabelText("Spring Frequency")]
        public float positionSpringFrequency = 3f;

        [Header("Rotation Interpolation")] public bool interpolateRotation = true;

        /// the follow mode to use when interpolating the rotation
        [ShowIf(nameof(interpolateRotation))] public FollowModes followRotationMode = FollowModes.MMLerp;

        /// the speed at which to interpolate the follower's rotation
        [ShowIf(nameof(interpolateRotation))] public float followRotationSpeed = 10f;

        /// higher values mean more damping, less spring, low values mean less damping, more spring
        [ShowIf(nameof(followRotationMode), FollowModes.Spring)] [Range(0.01f, 1.0f)]
        public float rotationSpringDamping = 0.3f;

        /// the frequency at which the spring should "vibrate", in Hz (1 : the spring will do one full period in one second)
        [ShowIf(nameof(followRotationMode), FollowModes.Spring)] public float rotationSpringFrequency = 3f;

        [Header("Scale Interpolation")] public bool interpolateScale = true;

        /// the follow mode to use when interpolating the scale
        [ShowIf(nameof(interpolateScale))] public FollowModes followScaleMode = FollowModes.MMLerp;

        /// the speed at which to interpolate the follower's scale
        [ShowIf(nameof(interpolateScale))] public float followScaleSpeed = 10f;

        /// higher values mean more damping, less spring, low values mean less damping, more spring
        [ShowIf(nameof(followScaleMode), FollowModes.Spring)] [Range(0.01f, 1.0f)]
        public float scaleSpringDamping = 0.3f;

        /// the frequency at which the spring should "vibrate", in Hz (1 : the spring will do one full period in one second)
        [ShowIf(nameof(followScaleMode), FollowModes.Spring)] public float scaleSpringFrequency = 3f;

        [Header("Mode")] public bool disableSelfOnSetActiveFalse;

        [Header("Distances")] public bool useMinimumDistanceBeforeFollow;

        /// the minimum distance to keep between the object and its target
        public float minimumDistanceBeforeFollow = 1f;

        /// whether we want to make sure the object is never too far away from its target
        public bool useMaximumDistance;

        /// the maximum distance at which the object can be away from its target
        public float maximumDistance = 1f;

        [Header("Anchor")] public bool anchorToInitialPosition;

        /// the maximum distance around the initial position at which the transform can move
        [ShowIf(nameof(anchorToInitialPosition))] public float maxDistanceToAnchor = 1f;

        protected bool LocalSpace => positionSpace == PositionSpaces.Local;

        protected Vector3 positionVelocity = Vector3.zero;
        protected Vector3 scaleVelocity = Vector3.zero;
        protected Vector3 rotationVelocity = Vector3.zero;

        protected Vector3 initialPosition;
        protected Vector3 direction;

        protected Vector3 newPosition;
        protected Vector3 newRotation;
        protected Vector3 newScale;

        protected Vector3 newTargetPosition;
        protected Quaternion newTargetRotation;
        protected Vector3 newTargetRotationEulerAngles;
        protected Vector3 newTargetRotationEulerAnglesLastFrame;
        protected Vector3 newTargetScale;

        protected float rotationFloatVelocity;
        protected float rotationFloatCurrent;
        protected float rotationFloatTarget;

        protected Vector3 currentRotationEulerAngles;
        protected Quaternion rotationBeforeSpring;

        protected Quaternion initialRotation;
        protected Vector3 lastTargetPosition;

        protected virtual void Start() { Initialization(); }

        public virtual void Initialization()
        {
            SetInitialPosition();
            SetOffset();
        }

        /// <summary>
        /// Prevents the object from following the target anymore
        /// </summary>
        public virtual void StopFollowing() { followPosition = false; }

        /// <summary>
        /// Makes the object follow the target
        /// </summary>
        public virtual void StartFollowing()
        {
            followPosition = true;
            SetInitialPosition();
        }

        /// <summary>
        /// Stores the initial position
        /// </summary>
        protected virtual void SetInitialPosition()
        {
            initialPosition = LocalSpace ? transform.localPosition : transform.position;
            initialRotation = transform.rotation;
            lastTargetPosition = LocalSpace ? transform.localPosition : transform.position;
        }

        /// <summary>
        /// Adds initial offset to the offset if needed
        /// </summary>
        protected virtual void SetOffset()
        {
            if (target == null)
            {
                return;
            }

            Vector3 difference = transform.position - target.transform.position;
            offset.x = addInitialDistanceXToXOffset ? difference.x : offset.x;
            offset.y = addInitialDistanceYToYOffset ? difference.y : offset.y;
            offset.z = addInitialDistanceZToZOffset ? difference.z : offset.z;
        }


        public override void OnUpdate()
        {
            if (target == null) return;

            FollowTargetRotation();
            FollowTargetScale();
            FollowTargetPosition();
        }

        public override void OnFixedUpdate()
        {
            FollowTargetRotation();
            FollowTargetScale();
            FollowTargetPosition();
        }

        public override void OnLateUpdate()
        {
            FollowTargetRotation();
            FollowTargetScale();
            FollowTargetPosition();
        }

        /// <summary>
        /// Follows the target, lerping the position or not based on what's been defined in the inspector
        /// </summary>
        protected virtual void FollowTargetPosition()
        {
            if (target == null) return;

            if (!followPosition) return;

            newTargetPosition = target.position + offset;
            if (!followPositionX) newTargetPosition.x = initialPosition.x;

            if (!followPositionY) newTargetPosition.y = initialPosition.y;

            if (!followPositionZ) newTargetPosition.z = initialPosition.z;

            direction = (newTargetPosition - transform.position).normalized;
            float trueDistance = Vector3.Distance(transform.position, newTargetPosition);

            float interpolatedDistance = trueDistance;
            if (interpolatePosition)
            {
                switch (followPositionMode)
                {
                    case FollowModes.MMLerp:
                        interpolatedDistance = Math.MMLerp(0f, trueDistance, followPositionSpeed, Time.deltaTime);
                        interpolatedDistance = ApplyMinMaxDistancing(trueDistance, interpolatedDistance);
                        transform.Translate(direction * interpolatedDistance, Space.World);
                        break;
                    case FollowModes.RegularLerp:
                        interpolatedDistance = Math.Lerp(0f, trueDistance, Time.deltaTime * followPositionSpeed);
                        interpolatedDistance = ApplyMinMaxDistancing(trueDistance, interpolatedDistance);
                        transform.Translate(direction * interpolatedDistance, Space.World);
                        break;
                    case FollowModes.Spring:
                        newPosition = transform.position;
                        Math.Spring(ref newPosition,
                            newTargetPosition,
                            ref positionVelocity,
                            positionSpringDamping,
                            positionSpringFrequency,
                            Time.deltaTime);
                        if (LocalSpace)
                        {
                            transform.localPosition = newPosition;
                        }
                        else
                        {
                            transform.position = newPosition;
                        }

                        break;
                }
            }
            else
            {
                interpolatedDistance = ApplyMinMaxDistancing(trueDistance, interpolatedDistance);
                transform.Translate(direction * interpolatedDistance, Space.World);
            }

            if (anchorToInitialPosition)
            {
                if (Vector3.Distance(transform.position, initialPosition) > maxDistanceToAnchor)
                {
                    if (LocalSpace)
                    {
                        transform.localPosition = initialPosition + Vector3.ClampMagnitude(transform.localPosition - initialPosition, maxDistanceToAnchor);
                    }
                    else
                    {
                        transform.position = initialPosition + Vector3.ClampMagnitude(transform.position - initialPosition, maxDistanceToAnchor);
                    }
                }
            }
        }

        /// <summary>
        /// Applies minimal and maximal distance rules to the interpolated distance
        /// </summary>
        /// <param name="trueDistance"></param>
        /// <param name="interpolatedDistance"></param>
        /// <returns></returns>
        protected virtual float ApplyMinMaxDistancing(float trueDistance, float interpolatedDistance)
        {
            if (useMinimumDistanceBeforeFollow && (trueDistance - interpolatedDistance < minimumDistanceBeforeFollow))
            {
                interpolatedDistance = 0f;
            }

            if (useMaximumDistance && (trueDistance - interpolatedDistance >= maximumDistance))
            {
                interpolatedDistance = trueDistance - maximumDistance;
            }

            return interpolatedDistance;
        }

        /// <summary>
        /// Makes the object follow its target's rotation
        /// </summary>
        protected virtual void FollowTargetRotation()
        {
            if (target == null)
            {
                return;
            }

            if (!followRotation)
            {
                return;
            }

            newTargetRotation = target.rotation;

            newTargetRotationEulerAngles = target.rotation.eulerAngles;
            currentRotationEulerAngles = transform.rotation.eulerAngles;

            if (followRotationMode == FollowModes.Spring && (newTargetRotationEulerAnglesLastFrame != newTargetRotationEulerAngles))
            {
                rotationBeforeSpring = transform.rotation;
                rotationFloatCurrent = 0f;
                rotationFloatTarget = (Mathf.Abs(newTargetRotation.eulerAngles.x) + Mathf.Abs(newTargetRotation.eulerAngles.y) + Mathf.Abs(newTargetRotation.z)) -
                                      (Mathf.Abs(currentRotationEulerAngles.x) + Mathf.Abs(currentRotationEulerAngles.y) + Mathf.Abs(currentRotationEulerAngles.z));

                rotationFloatTarget = Mathf.Abs(rotationFloatTarget);
            }

            if (interpolateRotation)
            {
                switch (followRotationMode)
                {
                    case FollowModes.MMLerp:
                        transform.rotation = Math.MMLerp(transform.rotation, newTargetRotation, followRotationSpeed, Time.deltaTime);
                        break;
                    case FollowModes.RegularLerp:
                        transform.rotation = Quaternion.Lerp(transform.rotation, newTargetRotation, Time.deltaTime * followRotationSpeed);
                        break;
                    case FollowModes.Spring:
                        if (rotationFloatCurrent.Approximately(rotationFloatTarget)) break;

                        Math.Spring(ref rotationFloatCurrent,
                            rotationFloatTarget,
                            ref rotationFloatVelocity,
                            rotationSpringDamping,
                            rotationSpringFrequency,
                            Time.deltaTime);
                        float lerpValue = Math.Remap(rotationFloatCurrent,
                            0f,
                            rotationFloatTarget,
                            0f,
                            1f);
                        transform.rotation = Quaternion.LerpUnclamped(rotationBeforeSpring, newTargetRotation, lerpValue);
                        break;
                }
            }
            else
            {
                transform.rotation = newTargetRotation;
            }

            newTargetRotationEulerAnglesLastFrame = newTargetRotationEulerAngles;
        }

        /// <summary>
        /// Makes the object follow its target's scale
        /// </summary>
        protected virtual void FollowTargetScale()
        {
            if (target == null) return;

            if (!followScale) return;

            newTargetScale = target.localScale * followScaleFactor;

            if (interpolateScale)
            {
                switch (followScaleMode)
                {
                    case FollowModes.MMLerp:
                        transform.localScale = Math.MMLerp(transform.localScale, newTargetScale, followScaleSpeed, Time.deltaTime);
                        break;
                    case FollowModes.RegularLerp:
                        transform.localScale = Vector3.Lerp(transform.localScale, newTargetScale, Time.deltaTime * followScaleSpeed);
                        break;
                    case FollowModes.Spring:
                        newScale = transform.localScale;
                        Math.Spring(ref newScale,
                            newTargetScale,
                            ref scaleVelocity,
                            scaleSpringDamping,
                            scaleSpringFrequency,
                            Time.deltaTime);
                        transform.localScale = newScale;
                        break;
                }
            }
            else
            {
                transform.localScale = newTargetScale;
            }
        }

        public virtual void ChangeFollowTarget(Transform newTarget) => target = newTarget;

        protected virtual void OnDisable()
        {
            if (disableSelfOnSetActiveFalse)
            {
                enabled = false;
            }
        }
    }
}