using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Pancake.Sensor
{
    [Serializable]
    public struct RayShape
    {
        public float Length;
        public Vector3 Direction;
        public bool WorldSpace;

        public RayShape(float length, Vector3 direction, bool worldSpace)
        {
            Length = length;
            Direction = direction;
            WorldSpace = worldSpace;
        }
    }

    [Serializable]
    public struct SphereShape
    {
        public float Radius;
        public SphereShape(float radius) { Radius = radius; }
    }

    [Serializable]
    public struct BoxShape
    {
        public Vector3 HalfExtents;
        public BoxShape(Vector3 halfExtents) { HalfExtents = halfExtents; }
    }

    [Serializable]
    public struct Box2DShape
    {
        public Vector2 HalfExtents;
        public Box2DShape(Vector2 halfExtents) { HalfExtents = halfExtents; }
    }

    [Serializable]
    public struct CapsuleShape
    {
        public float Radius;
        public float Height;

        public CapsuleShape(float radius, float height)
        {
            Radius = radius;
            Height = height;
        }
    }

    [Serializable]
    public struct BallisticCurve
    {
        public Vector3 Velocity;
        public Vector3 Gravity;
        public float Time;
        [Min(2)] public int Segments;

        public BallisticCurve(Vector3 velocity, Vector3 gravity, float time, int segments)
        {
            Velocity = velocity;
            Gravity = gravity;
            Time = time;
            Segments = segments;
        }

        public List<Vector3> Sample(Vector3 origin, Quaternion rotation, List<Vector3> storeIn = null)
        {
            var v = rotation * Velocity;

            var p0 = new Vector3(0, 0, 0);
            var p1 = p0 + .5f * v * Time;
            var p2 = v * Time + Gravity * Time * Time * .5f;
            var bezier = new BezierCurve(p0, p1, p2, Segments);

            return bezier.Sample(origin, Quaternion.identity, storeIn);
        }
    }

    [Serializable]
    public struct BezierCurve
    {
        public Vector3 P1, P2, P3;
        [Min(2)] public int Segments;

        public BezierCurve(Vector3 p1, Vector3 p2, Vector3 p3, int segments)
        {
            P1 = p1;
            P2 = p2;
            P3 = p3;
            Segments = segments;
        }

        public Vector3 Eval(float t) { return Vector3.Lerp(Vector3.Lerp(P1, P2, t), Vector3.Lerp(P2, P3, t), t); }

        public List<Vector3> Sample(Vector3 origin, Quaternion rotation, List<Vector3> storeIn = null)
        {
            storeIn = storeIn ?? new List<Vector3>();
            if (storeIn.Count != Segments)
            {
                storeIn.Clear();
                for (var i = 0; i < Segments; i++)
                {
                    storeIn.Add(origin);
                }
            }

            for (var i = 0; i < Segments; i++)
            {
                var t = (float) i / (Segments - 1);
                storeIn[i] = (rotation * Eval(t)) + origin;
            }

            return storeIn;
        }
    }

    [Serializable]
    public struct BallisticCurve2D
    {
        public Vector2 Velocity;
        public Vector2 Gravity;
        public float Time;
        [Min(2)] public int Segments;

        public BallisticCurve2D(Vector2 velocity, Vector2 gravity, float time, int segments)
        {
            Velocity = velocity;
            Gravity = gravity;
            Time = time;
            Segments = segments;
        }

        public List<Vector2> Sample(Vector2 origin, Quaternion rotation, List<Vector2> storeIn = null)
        {
            var v = (Vector2) (rotation * Velocity);

            var p0 = new Vector2(0, 0);
            var p1 = p0 + .5f * v * Time;
            var p2 = v * Time + Gravity * Time * Time * .5f;
            var bezier = new BezierCurve2D(p0, p1, p2, Segments);

            return bezier.Sample(origin, Quaternion.identity, storeIn);
        }
    }

    [Serializable]
    public struct BezierCurve2D
    {
        public Vector2 P1, P2, P3;
        [Min(2)] public int Segments;

        public BezierCurve2D(Vector2 p1, Vector2 p2, Vector2 p3, int segments)
        {
            P1 = p1;
            P2 = p2;
            P3 = p3;
            Segments = segments;
        }

        public Vector2 Eval(float t) { return Vector2.Lerp(Vector2.Lerp(P1, P2, t), Vector2.Lerp(P2, P3, t), t); }

        public List<Vector2> Sample(Vector2 origin, Quaternion rotation, List<Vector2> storeIn = null)
        {
            storeIn = storeIn ?? new List<Vector2>();
            if (storeIn.Count != Segments)
            {
                storeIn.Clear();
                for (var i = 0; i < Segments; i++)
                {
                    storeIn.Add(origin);
                }
            }

            for (var i = 0; i < Segments; i++)
            {
                var t = (float) i / (Segments - 1);
                storeIn[i] = (Vector2) (rotation * Eval(t)) + origin;
            }

            return storeIn;
        }
    }
}