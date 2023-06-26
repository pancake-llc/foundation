using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Pancake.Sensor
{
    public interface ILOSResult
    {
        Signal OutputSignal { get; }
        float Visibility { get; }
        bool IsVisible { get; }
        List<LOSRayResult> Rays { get; }
    }

    public struct LOSRayResult
    {
        public Vector3 OriginPoint;
        public Vector3 TargetPoint;
        public Transform TargetTransform;
        public RayHit RayHit;
        public float VisibilityMultiplier;
        public bool IsObstructed => RayHit.IsObstructing;
        public float Visibility => IsObstructed ? 0f : VisibilityMultiplier;
    }

    public enum ScalingMode
    {
        Step,
        LinearDecay,
        Curve
    }

    [System.Serializable]
    public struct ScalingFunction
    {
        public ScalingMode Mode;
        public AnimationCurve Curve;

        public float Evaluate(float t)
        {
            if (Mode == ScalingMode.Step)
            {
                return t < 1f ? 1f : 0f;
            }

            if (Mode == ScalingMode.LinearDecay)
            {
                return 1f - Mathf.Clamp01(t);
            }
            else
            {
                return Curve.Evaluate(Mathf.Clamp01(t));
            }
        }

        public static ScalingFunction Default() =>
            new ScalingFunction() {Mode = ScalingMode.Step, Curve = new AnimationCurve(new Keyframe(0, 1), new Keyframe(.5f, 1), new Keyframe(1f, 0))};
    }

    public enum FOVConstraintMethod
    {
        BoundingBox,
        PerRay
    }

    public enum PointSamplingMethod
    {
        Fast,
        Quality
    }

    public abstract class BaseLOSTest : ILOSResult
    {
        public class ConfigParams
        {
            public Signal InputSignal;
            public List<Collider> OwnedColliders;
            public List<Collider2D> OwnedCollider2Ds;

            public Vector3 Origin;
            public ReferenceFrame Frame;

            public float MinimumVisibility;

            public LayerMask BlocksLineOfSight;
            public bool IgnoreTriggerColliders;
            public PointSamplingMethod PointSamplingMethod;
            public bool TestLOSTargetsOnly;
            public int NumberOfRays;

            public bool MovingAverageEnabled;
            public int MovingAverageWindowSize;

            public bool LimitDistance;
            public float MaxDistance;
            public ScalingFunction VisibilityByDistance;

            public bool LimitViewAngle;
            public float MaxHorizAngle;
            public ScalingFunction VisibilityByHorizAngle;
            public float MaxVertAngle;
            public ScalingFunction VisibilityByVertAngle;

            public FOVConstraintMethod FOVConstraintMethod;
        }

        public ConfigParams Config => config;
        protected ConfigParams config = new ConfigParams();

        public Signal OutputSignal { get; private set; }
        public float Visibility { get; private set; }
        public bool IsVisible { get; private set; } // Possible to have Visibility=0 and still be visible
        public List<LOSRayResult> Rays { get; } = new List<LOSRayResult>();

        Signal prevInputSignal;
        ComponentCache losTargetsCache;
        List<Vector3> generatedPoints = new List<Vector3>();
        MovingAverageFilter avgFilter = new MovingAverageFilter(1);

        public void Reset()
        {
            avgFilter.Clear();
            Rays.Clear();
            generatedPoints.Clear();
            Visibility = 0f;
            Clear();
        }

        public bool PerformTest()
        {
            Rays.Clear();
            generatedPoints.Clear();
            Clear();

            // Need to initialize output signal so .Object is populated
            OutputSignal = new Signal() {Object = config.InputSignal.Object, Shape = config.InputSignal.Shape, Strength = 0f};
            IsVisible = false;
            Visibility = 0f;
            if (!ReferenceEquals(config.InputSignal.Object, prevInputSignal.Object))
            {
                avgFilter.Clear();
            }

            prevInputSignal = config.InputSignal;

            var visibilityScale = Config.FOVConstraintMethod == FOVConstraintMethod.BoundingBox ? GetVisibilityScale() : 1f;
            if (visibilityScale <= 0f)
            {
                return false;
            }

            var isUsingGeneratedPoints = false;

            var losTargets = losTargetsCache.GetComponent<LOSTargets>(config.InputSignal.Object);
            if (losTargets == null || losTargets.Targets == null || losTargets.Targets.Count == 0)
            {
                if (config.TestLOSTargetsOnly)
                {
                    return IsVisible;
                }

                if (IsInsideSignal())
                {
                    // If I'm inside the bounds of the target signal then I can see it.
                    Visibility = 1f;
                    IsVisible = true;
                    OutputSignal = new Signal {Object = config.InputSignal.Object, Shape = config.InputSignal.Shape, Strength = config.InputSignal.Strength};
                    return IsVisible;
                }

                GenerateTestPoints(generatedPoints);
                isUsingGeneratedPoints = true;
                if (generatedPoints.Count == 0)
                {
                    return false; // Couldn't generate any testpoints, therefore not visible.
                }
            }

            if (isUsingGeneratedPoints)
            {
                foreach (var pt in generatedPoints)
                {
                    var trans = config.InputSignal.Object.transform;
                    var result = TestPoint(pt);
                    result.VisibilityMultiplier = visibilityScale;
                    if (Config.FOVConstraintMethod == FOVConstraintMethod.PerRay)
                    {
                        result.VisibilityMultiplier *= GetRayVisibilityScale(pt);
                    }

                    Rays.Add(result);
                }
            }
            else
            {
                foreach (var target in losTargets.Targets)
                {
                    var result = TestTransform(target);
                    result.VisibilityMultiplier = visibilityScale;
                    if (Config.FOVConstraintMethod == FOVConstraintMethod.PerRay)
                    {
                        result.VisibilityMultiplier *= GetRayVisibilityScale(target.position);
                    }

                    Rays.Add(result);
                }
            }

            var rayVisibilitySum = 0f;
            foreach (var ray in Rays)
            {
                rayVisibilitySum += ray.Visibility;
            }

            Visibility = (rayVisibilitySum / Rays.Count);

            if (config.MovingAverageEnabled)
            {
                avgFilter.Size = config.MovingAverageWindowSize;
                avgFilter.AddSample(Visibility);
                Visibility = avgFilter.Value;
            }
            else
            {
                avgFilter.Clear();
            }

            IsVisible = Visibility >= config.MinimumVisibility;

            OutputSignal = new Signal()
            {
                Object = config.InputSignal.Object, Shape = config.InputSignal.Shape, Strength = IsVisible ? config.InputSignal.Strength * Visibility : 0f
            };

            return IsVisible;
        }

        private static Color[] defaultVisibilityGradientColours =
        {
            new(.2f, 1f, 1f), new(.21f, 1f, .74f), new(.21f, 1f, .47f), new(.22f, 1f, .22f), new(.48f, 1f, .23f), new(.75f, 1f, .23f), new(1f, 1f, .24f),
            new(1f, .75f, .25f), new(1f, .5f, .25f), new(1f, .26f, .26f)
        };

        public virtual void DrawGizmos()
        {
            foreach (var result in Rays)
            {
                Gizmos.color = SensorGizmos.LerpColour(defaultVisibilityGradientColours, 1f - result.VisibilityMultiplier);
                if (result.IsObstructed)
                {
                    Gizmos.DrawLine(result.OriginPoint, result.RayHit.Point);
                    Gizmos.color = Color.red;
                    Gizmos.DrawLine(result.RayHit.Point, result.TargetPoint);
                    Gizmos.DrawCube(result.TargetPoint, Vector3.one * 0.02f);
                }
                else
                {
                    Gizmos.DrawLine(result.OriginPoint, result.TargetPoint);
                    Gizmos.DrawCube(result.TargetPoint, Vector3.one * 0.02f);
                }
            }
        }

        LOSRayResult TestTransform(Transform testTransform)
        {
            var result = TestPoint(testTransform.position);
            result.TargetTransform = testTransform;
            return result;
        }

        protected abstract LOSRayResult TestPoint(Vector3 testPoint);

        protected abstract bool IsInsideSignal();

        protected abstract void GenerateTestPoints(List<Vector3> storeIn);

        protected abstract float GetVisibilityScale();

        protected abstract float GetRayVisibilityScale(Vector3 target);

        protected abstract void Clear();
    }
}