using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Pancake.Sensor
{
    public static partial class SensorGizmos
    {
        public static void SphereGizmo(Vector3 position, float radius)
        {
#if UNITY_EDITOR
            PushMatrix(Matrix4x4.identity);
            BackfaceSphereHandle(position, radius);
            PopMatrix();
#endif
        }

        public static void CircleGizmo(Vector3 position, float radius)
        {
#if UNITY_EDITOR
            PushMatrix(Matrix4x4.identity);
            Handles.DrawWireDisc(position, -Vector3.forward, radius);
            PopMatrix();
#endif
        }

        public static void CircleSector(Vector3 position, Vector3 direction, Vector3 normal, float angle, float radius)
        {
#if UNITY_EDITOR
            PushMatrix(Matrix4x4.identity);

            var r1 = Quaternion.AngleAxis(-angle, normal) * direction * radius;
            var r2 = Quaternion.AngleAxis(angle, normal) * direction * radius;

            Handles.DrawWireArc(position,
                normal,
                r1,
                angle * 2,
                radius);
            //BackfaceArc(position, direction, normal, angle, radius);
            Handles.DrawLine(position, position + r1);
            Handles.DrawLine(position, position + r2);

            PopMatrix();
#endif
        }

        public static void BackfaceArc(Vector3 position, Vector3 direction, Vector3 normal, float angle, float radius)
        {
#if UNITY_EDITOR
            PushMatrix(Matrix4x4.identity);

            if (Camera.current.orthographic)
            {
                Vector3 normalized = Vector3.Cross(normal, Camera.current.transform.forward).normalized;
                DrawTwoShadedWireDiscSector(position,
                    normal,
                    normalized,
                    180f,
                    Vector3.SignedAngle(normalized, Quaternion.AngleAxis(-angle, normal) * direction, normal),
                    Vector3.SignedAngle(normalized, Quaternion.AngleAxis(angle, normal) * direction, normal),
                    radius);
            }
            else
            {
                var cam2pos = Matrix.MultiplyPoint(position) - Camera.current.transform.position;
                float sqrMagnitude = cam2pos.sqrMagnitude;
                float rad2 = radius * radius;
                float f1 = rad2 * rad2 / sqrMagnitude;
                float num3 = f1 / rad2;
                if (num3 < 1.0f)
                {
                    float a = Vector3.Angle(cam2pos, normal);
                    float num4 = Mathf.Tan((90f - Mathf.Min(a, 180f - a)) * (Mathf.PI / 180f));
                    float f2 = Mathf.Sqrt(f1 + num4 * num4 * f1) / radius;
                    if (f2 < 1.0f)
                    {
                        float grazeAngle = Mathf.Asin(f2) * 57.29578f;
                        Vector3 normalized = Vector3.Cross(normal, cam2pos).normalized;
                        Vector3 from = Quaternion.AngleAxis(grazeAngle, normal) * normalized;
                        DrawTwoShadedWireDiscSector(position,
                            normal,
                            from,
                            (90.0f - grazeAngle) * 2.0f,
                            Vector3.SignedAngle(from, Quaternion.AngleAxis(-angle, normal) * direction, normal),
                            Vector3.SignedAngle(from, Quaternion.AngleAxis(angle, normal) * direction, normal),
                            radius);
                    }
                    else
                    {
                        PushColor(SetA(Color, .2f));
                        var r1 = Quaternion.AngleAxis(-angle, normal) * direction * radius;
                        Handles.DrawWireArc(position,
                            normal,
                            r1,
                            angle * 2,
                            radius);
                        PopColor();
                    }
                }
                else
                {
                    PushColor(SetA(Color, .2f));
                    var r1 = Quaternion.AngleAxis(-angle, normal) * direction * radius;
                    Handles.DrawWireArc(position,
                        normal,
                        r1,
                        angle * 2,
                        radius);
                    PopColor();
                }
            }

            PopMatrix();
#endif
        }

        public static void CapsuleGizmo(Vector3 position, float radius, float height)
        {
#if UNITY_EDITOR
            PushMatrix(Matrix * Matrix4x4.Translate(position));

            height = Mathf.Abs(height);
            var extent = Vector3.up * height / 2f;

            HalfSphere(extent, radius);
            PushMatrix(Matrix * Matrix4x4.Rotate(Quaternion.FromToRotation(Vector3.up, Vector3.down)));
            HalfSphere(extent, radius);
            PopMatrix();

            Gizmos.DrawRay(extent + Vector3.right * radius, Vector3.down * height);
            Gizmos.DrawRay(extent - Vector3.right * radius, Vector3.down * height);
            Gizmos.DrawRay(extent + Vector3.forward * radius, Vector3.down * height);
            Gizmos.DrawRay(extent - Vector3.forward * radius, Vector3.down * height);

            PopMatrix();
#endif
        }

        public static void Capsule2DGizmo(Vector3 position, float radius, float height)
        {
#if UNITY_EDITOR
            PushMatrix(Matrix * Matrix4x4.Translate(position));

            height = Mathf.Max(radius * 2, height);

            var pt1 = Vector3.up * (height - 2 * radius) / 2f;
            var pt2 = -pt1;
            Handles.color = Gizmos.color;
            Handles.DrawWireArc(pt1,
                -Vector3.forward,
                Vector3.left,
                180f,
                radius);
            Handles.DrawWireArc(pt2,
                -Vector3.forward,
                Vector3.right,
                180f,
                radius);
            Gizmos.DrawRay(pt1 + Vector3.right * radius, Vector3.down * (height - 2 * radius));
            Gizmos.DrawRay(pt1 - Vector3.right * radius, Vector3.down * (height - 2 * radius));

            PopMatrix();
#endif
        }

        static void CameraFacingWireDisc(Vector3 position, float radius)
        {
#if UNITY_EDITOR
            if (Camera.current.orthographic)
            {
                Handles.DrawWireDisc(position, Matrix.inverse.MultiplyPoint(Camera.current.transform.position), radius);
            }
            else
            {
                var cam2pos = position - Matrix.inverse.MultiplyPoint(Camera.current.transform.position);
                float sqrMagnitude = cam2pos.sqrMagnitude;
                float rad2 = radius * radius;
                float f1 = rad2 * rad2 / sqrMagnitude;
                float num3 = f1 / rad2;
                if (num3 < 1.0f)
                {
                    float num4 = Mathf.Sqrt(rad2 - f1);
                    Handles.DrawWireDisc(position - rad2 * cam2pos / sqrMagnitude, cam2pos, num4);
                }
            }
#endif
        }

        static Vector3[] vector3Array = new Vector3[6] {Vector3.right, Vector3.up, Vector3.forward, -Vector3.right, -Vector3.up, -Vector3.forward};

        static void BackfaceSphereHandle(Vector3 position, float radius)
        {
#if UNITY_EDITOR
            CameraFacingWireDisc(position, radius);

            var localCamPos = Matrix.inverse.MultiplyPoint(Camera.current.transform.position);
            var localCamForward = Matrix.inverse.MultiplyVector(Camera.current.transform.forward);

            if (Camera.current.orthographic)
            {
                for (int index = 0; index < 3; ++index)
                {
                    Vector3 normalized = Vector3.Cross(vector3Array[index], localCamForward).normalized;
                    DrawTwoShadedWireDisc(position,
                        vector3Array[index],
                        normalized,
                        180f,
                        radius);
                }
            }
            else
            {
                var cam2pos = position - localCamPos;
                float sqrMagnitude = cam2pos.sqrMagnitude;
                float rad2 = radius * radius;
                float f1 = rad2 * rad2 / sqrMagnitude;
                float num3 = f1 / rad2;
                for (int index = 0; index < 3; ++index)
                {
                    if (num3 < 1.0f)
                    {
                        float a = Vector3.Angle(cam2pos, vector3Array[index]);
                        float num4 = Mathf.Tan((90f - Mathf.Min(a, 180f - a)) * (Mathf.PI / 180f));
                        float f2 = Mathf.Sqrt(f1 + num4 * num4 * f1) / radius;
                        if (f2 < 1.0f)
                        {
                            float angle = Mathf.Asin(f2) * 57.29578f;
                            Vector3 normalized = Vector3.Cross(vector3Array[index], cam2pos).normalized;
                            Vector3 from = Quaternion.AngleAxis(angle, vector3Array[index]) * normalized;
                            DrawTwoShadedWireDisc(position,
                                vector3Array[index],
                                from,
                                (90.0f - angle) * 2.0f,
                                radius);
                        }
                        else
                        {
                            DrawTwoShadedWireDisc(position, vector3Array[index], radius);
                        }
                    }
                    else
                    {
                        DrawTwoShadedWireDisc(position, vector3Array[index], radius);
                    }
                }
            }
#endif
        }

        static void HalfSphere(Vector3 position, float radius)
        {
#if UNITY_EDITOR
            Handles.DrawWireDisc(position, Vector3.up, radius);
            Handles.DrawWireArc(position,
                Vector3.right,
                Vector3.back,
                180f,
                radius);
            Handles.DrawWireArc(position,
                Vector3.forward,
                Vector3.right,
                180f,
                radius);
#endif
        }

        static void DrawTwoShadedWireDisc(Vector3 position, Vector3 axis, float radius)
        {
#if UNITY_EDITOR
            PushColor(SetA(Color, .2f));
            Handles.DrawWireDisc(position, axis, radius);
            PopColor();
#endif
        }

        static void DrawTwoShadedWireDisc(Vector3 position, Vector3 axis, Vector3 from, float degrees, float radius)
        {
#if UNITY_EDITOR
            Handles.DrawWireArc(position,
                axis,
                from,
                degrees,
                radius);
            PushColor(SetA(Color, .2f));
            Handles.DrawWireArc(position,
                axis,
                from,
                degrees - 360f,
                radius);
            PopColor();
#endif
        }

        static void DrawTwoShadedWireDiscSector(Vector3 position, Vector3 axis, Vector3 from, float degrees, float startDegrees, float endDegrees, float radius)
        {
#if UNITY_EDITOR
            if (endDegrees < startDegrees)
            {
                endDegrees += 360f;
            }

            var currPos = startDegrees;
            var i = 0;
            while (currPos < endDegrees && i < 4)
            {
                var isFront = currPos >= 0 && currPos < degrees;
                var isBack = !isFront;
                if (isBack)
                {
                    PushColor(SetA(Color, .2f));
                }

                var nextPos = currPos;
                if (currPos < 0f)
                {
                    nextPos = Mathf.Min(0, endDegrees);
                }
                else if (currPos < 360f)
                {
                    nextPos = isBack ? Mathf.Min(endDegrees, 360f) : Mathf.Min(endDegrees, degrees);
                }
                else
                {
                    nextPos = endDegrees;
                }

                var delta = (nextPos - currPos);

                Handles.DrawWireArc(position,
                    axis,
                    Quaternion.AngleAxis(currPos, axis) * from,
                    delta,
                    radius);

                currPos = nextPos;
                i += 1;

                if (isBack)
                {
                    PopColor();
                }
            }


            /*Debug.Log($"start: {startDegrees} -- end: {endDegrees}  --  degrees: {degrees}");

            PushColor(Color.green);
            Gizmos.DrawCube(position + from*radius, Vector3.one * .2f);
            PopColor();
            PushColor(Color.red);
            Gizmos.DrawCube(position + Quaternion.AngleAxis(degrees, axis) * from*radius, Vector3.one * .2f);
            PopColor();

            PushColor(Color.blue);
            Gizmos.DrawCube(position + Quaternion.AngleAxis(startDegrees, axis) * from * radius, Vector3.one * .2f);
            PopColor();*/
#endif
        }
    }
}