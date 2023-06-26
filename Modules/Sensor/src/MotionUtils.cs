using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Pancake.Sensor
{
    [System.Serializable]
    public struct ReferenceFrame
    {
        public Vector3 Forward, Right, Up;

        public ReferenceFrame(Vector3 forward, Vector3 right, Vector3 up)
        {
            Forward = forward;
            Right = right;
            Up = up;
        }

        public ReferenceFrame(Transform transform)
        {
            Forward = transform.forward;
            Right = transform.right;
            Up = transform.up;
        }
    }

    public static class MotionUtils
    {
        public static float SeekAccel(float cSpring, float cDamp, float delta, float velocity) => -(cDamp * velocity) - (cSpring * delta);

        public static Vector2 SeekAccel(float cSpring, float cDamp, Vector2 delta, Vector2 velocity) =>
            new Vector2(SeekAccel(cSpring, cDamp, delta.x, velocity.x), SeekAccel(cSpring, cDamp, delta.y, velocity.y));

        public static Vector3 SeekAccel(float cSpring, float cDamp, Vector3 delta, Vector3 velocity) =>
            new Vector3(SeekAccel(cSpring, cDamp, delta.x, velocity.x), SeekAccel(cSpring, cDamp, delta.y, velocity.y), SeekAccel(cSpring, cDamp, delta.z, velocity.z));

        // Scale the delta-angles and causes a stronger acceleration then there would be otherwise.
        // Without this characters may turn sluggishly towards their target.
        static float angleDeltaScale(float maxAccel) => Mathf.Max((maxAccel / 360f) * 3f, 1f);

        public static Vector3 SeekAngularAccel(float cSpring, float cDamp, float maxAccel, ReferenceFrame frame, Vector3 angularVelocity, Vector3 tdir, Vector3 tup)
        {
            var dPitch = ProjectedAngle(tdir, frame.Forward, frame.Right);
            var dYaw = ProjectedAngle(tdir, frame.Forward, frame.Up);
            var dRoll = ProjectedAngle(tup, frame.Up, frame.Forward);

            var vPitch = Vector3.Dot(angularVelocity, frame.Right);
            var vYaw = Vector3.Dot(angularVelocity, frame.Up);
            var vRoll = Vector3.Dot(angularVelocity, frame.Forward);

            return SeekAccel(cSpring, cDamp, new Vector3(dPitch, dYaw, dRoll) * angleDeltaScale(maxAccel), new Vector3(vPitch, vYaw, vRoll));
        }

        public static Vector3 SeekPlanarAngularAccel(
            float cSpring,
            float cDamp,
            float maxAccel,
            ReferenceFrame frame,
            Vector3 angularVelocity,
            Vector3 tdir,
            Vector3 planeNormal)
        {
            var projTDir = Vector3.ProjectOnPlane(tdir, planeNormal).normalized;
            var dPitch = ProjectedAngle(Vector3.ProjectOnPlane(frame.Forward, planeNormal).normalized, frame.Forward, frame.Right);
            var dYaw = ProjectedAngle(projTDir, frame.Forward, planeNormal);
            var dRoll = ProjectedAngle(planeNormal, frame.Up, frame.Forward);

            var vPitch = Vector3.Dot(angularVelocity, frame.Right);
            var vYaw = Vector3.Dot(angularVelocity, frame.Up);
            var vRoll = Vector3.Dot(angularVelocity, frame.Forward);

            return SeekAccel(cSpring, cDamp, new Vector3(dPitch, dYaw, dRoll) * angleDeltaScale(maxAccel), new Vector3(vPitch, vYaw, vRoll));
        }

        public static float SeekAngularAccel2D(float cSpring, float cDamp, float maxAccel, Vector2 cdir, float angularVelocity, Vector2 tdir)
        {
            var dAngle = Vector2.SignedAngle(tdir, cdir);
            return SeekAccel(cSpring, cDamp, dAngle * angleDeltaScale(maxAccel), angularVelocity);
        }

        public static float ProjectedAngle(Vector3 from, Vector3 to, Vector3 axis, bool clockwise = false)
        {
            Vector3 right;
            if (clockwise)
            {
                right = Vector3.Cross(from, axis);
                from = Vector3.Cross(axis, right);
            }
            else
            {
                right = Vector3.Cross(axis, from);
                from = Vector3.Cross(right, axis);
            }

            return Mathf.Atan2(Vector3.Dot(to, right), Vector3.Dot(to, from)) * Mathf.Rad2Deg;
        }
    }
}