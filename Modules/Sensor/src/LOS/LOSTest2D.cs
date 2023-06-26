using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Pancake.Sensor
{
    public class LOSTest2D : BaseLOSTest
    {
        LOSFieldOfView2D fov = new LOSFieldOfView2D();
        List<Edge2D> edges = new List<Edge2D>();
        List<Edge2D> projectedEdges = new List<Edge2D>();
        ComponentCache losColliderOwnerCache;
        SobolSequence sobol = new SobolSequence(3);

        public override void DrawGizmos()
        {
            base.DrawGizmos();
            SensorGizmos.PushColor(Color.blue);
            foreach (var edge in edges)
            {
                edge.DrawGizmos(config.Origin.z);
            }

            /*foreach (var edge in projectedEdges) {
                edge.DrawGizmos(Config.Origin.z);
            }*/
            SensorGizmos.PopColor();
        }

        protected override void Clear()
        {
            edges.Clear();
            projectedEdges.Clear();
        }

        protected override LOSRayResult TestPoint(Vector3 testPoint)
        {
            var saveQHT = Physics2D.queriesHitTriggers;
            Physics2D.queriesHitTriggers = !config.IgnoreTriggerColliders;
            var result = DoTest(testPoint);
            Physics2D.queriesHitTriggers = saveQHT;
            return result;
        }

        LOSRayResult DoTest(Vector3 testPoint)
        {
            var delta = (Vector2) testPoint - (Vector2) config.Origin;

            var ray = new Ray(config.Origin, delta.normalized);
            var result = new LOSRayResult() {OriginPoint = ray.origin, TargetPoint = testPoint, VisibilityMultiplier = 1f};
            var hitInfo = Physics2D.Raycast(ray.origin, ray.direction, delta.magnitude, config.BlocksLineOfSight);
            if (hitInfo.collider != null)
            {
                // Ray hit something, check that it was the target.
                var isTarget = (hitInfo.rigidbody != null && hitInfo.rigidbody.gameObject == config.InputSignal.Object) ||
                               hitInfo.collider.gameObject == config.InputSignal.Object;

                isTarget = isTarget || config.OwnedCollider2Ds.Contains(hitInfo.collider);
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
                        Collider2D = hitInfo.collider
                    };
                }
            }

            return result;
        }

        protected override bool IsInsideSignal()
        {
            var origin = Config.Origin;
            var bounds = Config.InputSignal.Bounds;
            origin.Set(origin.x, origin.y, bounds.center.z);
            return bounds.Contains(origin);
        }

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
            edges.Clear();
            projectedEdges.Clear();

            var bounds = config.InputSignal.Bounds;
            bounds.center = (Vector2) bounds.center;
            LOSUtils.MapBoundsToEdges(config.Origin, bounds, edges);

            if (config.LimitViewAngle)
            {
                fov.Set(config.MaxHorizAngle * 2f, config.Origin, Quaternion.LookRotation(config.Frame.Forward, config.Frame.Up));
                fov.Clip(edges);
            }

            if (edges.Count == 0)
            {
                return;
            }

            foreach (var edge in edges)
            {
                projectedEdges.Add(edge.ProjectCircle(config.Origin));
            }

            for (int i = 0; i < config.NumberOfRays; i++)
            {
                var nextSobol = sobol.Next();
                var randomPoint = LOSUtils.GetRandomPointOnEdges(projectedEdges, nextSobol);

                float boundsDist;
                var ray = new Ray((Vector2) config.Origin, ((Vector2) (randomPoint - config.Origin)).normalized);
                bounds.IntersectRay(ray, out boundsDist);

                var intBoundsInPoint = ray.origin + ray.direction * boundsDist + new Vector3(0f, 0f, config.Origin.z);
                var intBoundsOutPoint = LOSUtils.RaycastBoundsOutPoint(intBoundsInPoint, (intBoundsInPoint - Config.Origin).normalized, bounds);

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
                var bounds = config.InputSignal.Bounds;
                bounds.center.Set(bounds.center.x, bounds.center.y, config.Origin.z);
                float distance = Mathf.Sqrt((bounds.SqrDistance(config.Origin)));
                visibilityScale *= config.VisibilityByDistance.Evaluate(distance / config.MaxDistance);
            }

            if (config.LimitViewAngle)
            {
                var horizAngle = LOSUtils.MinAngleToBounds(config.Origin, config.Frame.Forward, config.Frame.Right, Config.InputSignal.Bounds);
                visibilityScale *= config.VisibilityByHorizAngle.Evaluate(horizAngle / config.MaxHorizAngle);
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
                visibilityScale *= config.VisibilityByHorizAngle.Evaluate(horizAngle / config.MaxHorizAngle);
            }

            return visibilityScale;
        }
    }
}