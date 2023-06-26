using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Pancake.Sensor
{
    public class LOSTest3D : BaseLOSTest
    {
        LOSFieldOfView fov = new LOSFieldOfView();
        List<Triangle> triangles = new List<Triangle>();
        List<Triangle> projectedTriangles = new List<Triangle>();
        ComponentCache losColliderOwnerCache;
        SobolSequence sobol = new SobolSequence(3);

        QueryTriggerInteraction queryTriggerInteraction => Config.IgnoreTriggerColliders ? QueryTriggerInteraction.Ignore : QueryTriggerInteraction.Collide;

        public override void DrawGizmos()
        {
            base.DrawGizmos();
            SensorGizmos.PushColor(Color.blue);
            foreach (var triangle in triangles)
            {
                triangle.DrawGizmos();
            }

            /*foreach (var triangle in projectedTriangles) {
                triangle.DrawGizmos();
            }*/
            SensorGizmos.PopColor();
        }

        protected override void Clear()
        {
            triangles.Clear();
            projectedTriangles.Clear();
        }

        protected override LOSRayResult TestPoint(Vector3 testPoint)
        {
            var delta = testPoint - config.Origin;

            var ray = new Ray(config.Origin, delta.normalized);
            RaycastHit hitInfo;
            var result = new LOSRayResult() {OriginPoint = ray.origin, TargetPoint = testPoint, VisibilityMultiplier = 1f};
            if (Physics.Raycast(ray,
                    out hitInfo,
                    delta.magnitude,
                    config.BlocksLineOfSight,
                    queryTriggerInteraction))
            {
                // Ray hit something, check that it was the target.
                var isTarget = (hitInfo.rigidbody != null && hitInfo.rigidbody.gameObject == config.InputSignal.Object) ||
                               hitInfo.collider.gameObject == config.InputSignal.Object;

                isTarget = isTarget || (config.OwnedColliders?.Contains(hitInfo.collider) ?? false);
                var losColliderOwner = losColliderOwnerCache.GetComponent<LOSColliderOwner>(config.InputSignal.Object);
                if (losColliderOwner != null)
                {
                    isTarget = isTarget || losColliderOwner.IsColliderOwner(hitInfo.collider);
                }

                if (!isTarget)
                {
                    result.RayHit = new RayHit()
                    {
                        IsObstructing = true,
                        Point = hitInfo.point,
                        Normal = hitInfo.normal,
                        Distance = hitInfo.distance,
                        DistanceFraction = hitInfo.distance / delta.magnitude,
                        Collider = hitInfo.collider
                    };
                }
            }

            return result;
        }

        protected override bool IsInsideSignal() => Config.InputSignal.Bounds.Contains(Config.Origin);

        protected override void GenerateTestPoints(List<Vector3> storeIn)
        {
            if (Config.PointSamplingMethod == PointSamplingMethod.Fast)
            {
                FastGenerateTestPoints(storeIn);
            }
            else if (Config.PointSamplingMethod == PointSamplingMethod.Quality)
            {
                QualityGenerateTestPoints(storeIn);
            }
        }

        void FastGenerateTestPoints(List<Vector3> storeIn)
        {
            var bounds = Config.InputSignal.Bounds;
            for (int i = 0; i < Config.NumberOfRays; i++)
            {
                var nextSobol = sobol.Next();
                var random3 = new Vector3(Mathf.Lerp(-1, 1, (float) nextSobol[0]), Mathf.Lerp(-1, 1, (float) nextSobol[1]), Mathf.Lerp(-1, 1, (float) nextSobol[2]));
                random3 *= .9f;
                var randomPoint = bounds.center + Vector3.Scale(bounds.extents, random3);
                storeIn.Add(randomPoint);
            }
        }

        void QualityGenerateTestPoints(List<Vector3> storeIn)
        {
            triangles.Clear();
            projectedTriangles.Clear();

            var bounds = config.InputSignal.Bounds;
            LOSUtils.MapBoundsToTriangles(config.Origin, bounds, triangles);

            if (config.LimitViewAngle)
            {
                fov.Set(config.MaxHorizAngle * 2f, config.MaxVertAngle * 2f, config.Origin, Quaternion.LookRotation(config.Frame.Forward, config.Frame.Up));
                fov.Clip(triangles);
            }

            if (triangles.Count == 0)
            {
                return;
            }

            foreach (var triangle in triangles)
            {
                projectedTriangles.Add(triangle.ProjectSphere(config.Origin));
            }

            for (int i = 0; i < config.NumberOfRays; i++)
            {
                var nextSobol = sobol.Next();
                var randomPoint = LOSUtils.GetRandomPointInTriangles(projectedTriangles, nextSobol);

                float boundsDist;
                var ray = new Ray(config.Origin, (randomPoint - config.Origin).normalized);
                bounds.IntersectRay(ray, out boundsDist);

                var intBoundsInPoint = ray.origin + ray.direction * boundsDist;
                var intBoundsOutPoint = LOSUtils.RaycastBoundsOutPoint(intBoundsInPoint, (intBoundsInPoint - config.Origin).normalized, bounds);

                var midpoint = (intBoundsOutPoint + intBoundsInPoint) / 2f;
                var penetration = midpoint - intBoundsInPoint;

                if (config.LimitDistance)
                {
                    penetration = Vector3.ClampMagnitude(penetration, config.MaxDistance / 100f);
                }

                storeIn.Add(intBoundsInPoint + penetration);
            }
        }

        protected override float GetVisibilityScale()
        {
            var visibilityScale = 1f;
            if (config.LimitDistance)
            {
                float distance = Mathf.Sqrt((config.InputSignal.Bounds.SqrDistance(config.Origin)));
                visibilityScale *= config.VisibilityByDistance.Evaluate(distance / config.MaxDistance);
            }

            if (config.LimitViewAngle)
            {
                var horizAngle = LOSUtils.MinAngleToBounds(config.Origin, config.Frame.Forward, config.Frame.Right, config.InputSignal.Bounds);
                var vertAngle = LOSUtils.MinAngleToBounds(config.Origin, config.Frame.Forward, config.Frame.Up, config.InputSignal.Bounds);
                visibilityScale *= config.VisibilityByHorizAngle.Evaluate(horizAngle / config.MaxHorizAngle) *
                                   config.VisibilityByVertAngle.Evaluate(vertAngle / config.MaxVertAngle);
            }

            return visibilityScale;
        }

        protected override float GetRayVisibilityScale(Vector3 target)
        {
            var visibilityScale = 1f;
            if (config.LimitDistance)
            {
                float distance = (config.Origin - target).magnitude;
                visibilityScale *= config.VisibilityByDistance.Evaluate(distance / config.MaxDistance);
            }

            if (config.LimitViewAngle)
            {
                var horizAngle = LOSUtils.AngleToPoint(config.Origin, config.Frame.Forward, config.Frame.Right, target);
                var vertAngle = LOSUtils.AngleToPoint(config.Origin, config.Frame.Forward, config.Frame.Up, target);
                visibilityScale *= config.VisibilityByHorizAngle.Evaluate(horizAngle / config.MaxHorizAngle) *
                                   config.VisibilityByVertAngle.Evaluate(vertAngle / config.MaxVertAngle);
            }

            return visibilityScale;
        }
    }
}