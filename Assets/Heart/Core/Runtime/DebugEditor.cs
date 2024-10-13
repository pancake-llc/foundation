using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using System.Diagnostics;
using Debug = UnityEngine.Debug;
using Object = UnityEngine.Object;

namespace Pancake
{
    /// <summary>
    /// Class to log only in Unity editor, double-clicking console logs produced by this class still open the calling source file
    /// NOTE: Implement your own version of this is supported. Just implement a class named "DebugEditor" and any method inside this class starting with "Log" will, when double-clicked, open the file of the calling method. Use [Conditional] attributes to control when any of these methods should be included.
    /// </summary>
    public static class DebugEditor
    {
        [Conditional("UNITY_EDITOR")]
        public static void Log(object message, Object context = null) { Debug.Log(message, context); }

        [Conditional("UNITY_EDITOR")]
        public static void LogWarning(object message, Object context = null) { Debug.LogWarning(message, context); }

        [Conditional("UNITY_EDITOR")]
        public static void LogError(object message, Object context = null) { Debug.LogError(message, context); }

        [Conditional("UNITY_EDITOR")]
        public static void Toast(string message, float duration = 1f)
        {
#if UNITY_EDITOR
            foreach (SceneView scene in SceneView.sceneViews)
            {
                scene.ShowNotification(new GUIContent(message), duration);
                scene.Repaint();
            }
#endif
        }

#if UNITY_EDITOR

        [Conditional("UNITY_EDITOR")]
        public static void VisualizeCircle(Vector3 origin, float radius, bool isDotted = false)
        {
            var delta = 0.1f;
            if (isDotted && radius > 0.2f) delta /= radius;

            float x = radius * Mathf.Cos(0f);
            float y = radius * Mathf.Sin(0f);
            var beginPosition = origin + new Vector3(x, y, 0);
            var lastPosition = beginPosition;
            var z = 1;

            for (var deltaAngle = 0f; deltaAngle < Mathf.PI * 2; deltaAngle += delta)
            {
                z++;
                x = radius * Mathf.Cos(deltaAngle);
                y = radius * Mathf.Sin(deltaAngle);
                var endPosition = origin + new Vector3(x, y, 0);
                if (isDotted && z % 2 == 0) Gizmos.DrawLine(beginPosition, endPosition);
                else if (isDotted == false) Gizmos.DrawLine(beginPosition, endPosition);

                beginPosition = endPosition;
            }

            Gizmos.DrawLine(beginPosition, lastPosition);
        }

        [Conditional("UNITY_EDITOR")]
        public static void DrawCircleCast(Vector3 origin, float radius, Vector3 direction, float distance)
        {
            direction = new Vector3(direction.x, direction.y);
            distance = float.IsPositiveInfinity(distance) ? 1000000f : distance;
            var endPositionOfCircle = origin + direction.normalized * distance;
            var tangent = Vector3.zero;
            Vector3.OrthoNormalize(ref direction, ref tangent);
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(origin + tangent * radius, endPositionOfCircle + tangent * radius);
            Gizmos.DrawLine(origin - tangent * radius, endPositionOfCircle - tangent * radius);

            Gizmos.color = Color.green;
            VisualizeCircle(origin, radius);
            VisualizeCircle(endPositionOfCircle, radius);

            Gizmos.matrix = Matrix4x4.identity;
        }

        [Conditional("UNITY_EDITOR")]
        public static void DrawBoxCast2D(Vector3 origin, Vector3 size, float angle, Vector3 direction, float distance)
        {
            direction = new Vector3(direction.x, direction.y);
            size = new Vector3(size.x, size.y);
            distance = float.IsPositiveInfinity(distance) ? 1000000f : distance;

            var endPositionOfBox = origin + direction.normalized * distance;

            GetInfoAboutDiagonalsOfBox(size, out float angleOfDiagonal, out float halfOfDiagonalLenght);
            GetCornersPositionInLsOfBox(angle,
                angleOfDiagonal,
                halfOfDiagonalLenght,
                out var cornerPosition1,
                out var cornerPosition2,
                out var cornerPosition3,
                out var cornerPosition4);

            Gizmos.matrix = Matrix4x4.identity;
            //We're getting lenght from start to point projected on direction to delete not important lines
            float dlA = Vector3.Project(cornerPosition1, direction).magnitude;
            float dlB = Vector3.Project(cornerPosition2, direction).magnitude;
            float dlC = Vector3.Project(cornerPosition3, direction).magnitude;
            float dlD = Vector3.Project(cornerPosition4, direction).magnitude;

            Gizmos.color = Color.yellow;
            if (dlA + dlC < dlB + dlD)
            {
                Gizmos.DrawLine(origin + cornerPosition1, endPositionOfBox + cornerPosition1);
                Gizmos.DrawLine(origin + cornerPosition3, endPositionOfBox + cornerPosition3);
            }
            else
            {
                Gizmos.DrawLine(origin + cornerPosition2, endPositionOfBox + cornerPosition2);
                Gizmos.DrawLine(origin + cornerPosition4, endPositionOfBox + cornerPosition4);
            }


            Gizmos.color = Color.green;
            Gizmos.matrix = Matrix4x4.TRS(origin, Quaternion.AngleAxis(angle, Vector3.forward), Vector3.one);
            Gizmos.DrawWireCube(Vector3.zero, size);
            Gizmos.matrix = Matrix4x4.TRS(endPositionOfBox, Quaternion.AngleAxis(angle, Vector3.forward), Vector3.one);
            Gizmos.DrawWireCube(Vector3.zero, size);
            Gizmos.matrix = Matrix4x4.identity;
        }

        [Conditional("UNITY_EDITOR")]
        public static void DrawSphereCast3D(Vector3 origin, float radius, Vector3 direction, float maxDistance)
        {
            direction = direction.normalized;
            direction = direction.normalized;
            var endPositionOfSphere = direction.normalized * maxDistance + origin;
            var tangentToDirection = Vector3.zero;
            Vector3.OrthoNormalize(ref direction, ref tangentToDirection);
            var tangendOffset = tangentToDirection * radius;


            Gizmos.color = Color.green;
            Gizmos.matrix = Matrix4x4.identity;
            Gizmos.matrix = Matrix4x4.TRS(endPositionOfSphere, Quaternion.identity, new Vector3(radius, radius, radius));
            Gizmos.DrawWireSphere(Vector3.zero, 1);
            Gizmos.color = Color.green;
            Gizmos.matrix = Matrix4x4.TRS(origin, Quaternion.identity, new Vector3(radius, radius, radius));
            Gizmos.DrawWireSphere(Vector3.zero, 1);
            Gizmos.matrix = Matrix4x4.identity;


            if (radius == 0)
            {
                Gizmos.DrawSphere(origin, 0.05f);
            }

            #region Draw sphere connecting lines

            Gizmos.matrix = Matrix4x4.TRS(origin, Quaternion.identity, Vector3.one);
            var basePositionOfLine = tangendOffset;
            var endPositionOfLine = direction.normalized * maxDistance + tangendOffset;
            var connectingLinesAngularOffset = Quaternion.AngleAxis(18, direction);
            Gizmos.color = Color.yellow;
            for (var i = 0; i < 20; i++)
            {
                Gizmos.DrawLine(basePositionOfLine, endPositionOfLine);
                basePositionOfLine = connectingLinesAngularOffset * basePositionOfLine;
                endPositionOfLine = connectingLinesAngularOffset * endPositionOfLine;
            }

            #endregion

            Gizmos.matrix = Matrix4x4.identity;
        }

        [Conditional("UNITY_EDITOR")]
        public static void DrawBoxCast3D(Vector3 center, Vector3 halfExtents, Vector3 direction, Quaternion orientation, float maxDistance)
        {
            orientation = orientation.Equals(default) ? Quaternion.identity : orientation;
            if (maxDistance < 0)
            {
                Debug.LogWarning(
                    "<b><size=13><color=#0392CF> In method </color><color=#CD1426FF> DrawBoxCast3D </color><color=#0392CF> - </color> <color=#CD1426FF>maxdistance</color>  <color=#0392CF>must be greater then 0! </color> </size></b>");
                return;
            }

            if (halfExtents.x < 0 || halfExtents.y < 0 || halfExtents.z < 0)
            {
                Debug.LogWarning(
                    "<b><size=13> <color=#0392CF>In method</color> <color=#CD1426FF>DrawBoxCast3D</color> <color=#0392CF> components of</color><color=#CD1426FF> halfExtends</color><color=#0392CF> should't be negative! </color> </size></b>");
            }

            direction = direction.normalized;
            var endPositionOfCube = direction.normalized * maxDistance + center;
            var vertexes = new Vector3[8];
            var a1 = new Vector3(1f * halfExtents.x, 1f * halfExtents.y, 1f * halfExtents.z);
            var b1 = new Vector3(-1f * halfExtents.x, 1f * halfExtents.y, 1f * halfExtents.z);
            var c1 = new Vector3(-1f * halfExtents.x, -1f * halfExtents.y, 1f * halfExtents.z);
            var d1 = new Vector3(1f * halfExtents.x, -1f * halfExtents.y, 1f * halfExtents.z);
            var a = new Vector3(1f * halfExtents.x, 1f * halfExtents.y, -1f * halfExtents.z);
            var b = new Vector3(-1f * halfExtents.x, 1f * halfExtents.y, -1f * halfExtents.z);
            var c = new Vector3(-1f * halfExtents.x, -1f * halfExtents.y, -1f * halfExtents.z);
            var d = new Vector3(1f * halfExtents.x, -1f * halfExtents.y, -1f * halfExtents.z);
            vertexes[0] = a1;
            vertexes[1] = c;
            vertexes[2] = b1;
            vertexes[3] = d1;
            vertexes[4] = a;
            vertexes[5] = c1;
            vertexes[6] = d;
            vertexes[7] = b;

            #region Drawing BoxCast3D

            var lenghtOProjectedVertexesOnDirection = new List<float>(8);

            for (var i = 0; i < 8; i++)
            {
                float lenght = Vector3.Project(orientation * vertexes[i], direction).magnitude;
                lenghtOProjectedVertexesOnDirection.Add(lenght);
            }

            float min = Mathf.Max(lenghtOProjectedVertexesOnDirection[0],
                lenghtOProjectedVertexesOnDirection[1],
                lenghtOProjectedVertexesOnDirection[2],
                lenghtOProjectedVertexesOnDirection[3],
                lenghtOProjectedVertexesOnDirection[4],
                lenghtOProjectedVertexesOnDirection[5],
                lenghtOProjectedVertexesOnDirection[6],
                lenghtOProjectedVertexesOnDirection[7]);
            Gizmos.color = new Color(r: 0.129f, g: 0.108f, b: 0.922f, a: 0.25f);
            Gizmos.color = Color.green;
            for (var i = 0; i < 8; i++)
            {
                if (!Mathf.Approximately(lenghtOProjectedVertexesOnDirection[i], min))
                {
                    Gizmos.DrawLine(center + orientation * vertexes[i], endPositionOfCube + orientation * vertexes[i]);
                }
                else if (direction == Vector3.right || direction == Vector3.left || direction == Vector3.up || direction == Vector3.down || direction == Vector3.back ||
                         direction == Vector3.forward)
                {
                    Gizmos.DrawLine(center + orientation * vertexes[i], endPositionOfCube + orientation * vertexes[i]); //orientation == Quaternion.identity &&
                }
            }

            Gizmos.color = Color.yellow;
            Gizmos.matrix = Matrix4x4.TRS(endPositionOfCube, orientation, Vector3.one);
            Gizmos.DrawWireCube(Vector3.zero, halfExtents * 2);
            Gizmos.color = Color.yellow;
            Gizmos.matrix = Matrix4x4.TRS(center, orientation, Vector3.one);
            Gizmos.DrawWireCube(Vector3.zero, halfExtents * 2);

            #endregion

            if (halfExtents == Vector3.zero)
            {
                Gizmos.DrawSphere(Vector3.zero, 0.05f);
            }

            Gizmos.matrix = Matrix4x4.identity;
        }

        /// <summary> /// Gets angle between horizontal line and diagonals and half lenght of diagonal. /// </summary>
        private static void GetInfoAboutDiagonalsOfBox(Vector2 size, out float angleOfDiagonal, out float halfOfDiagonalLenght)
        {
            angleOfDiagonal = Mathf.Atan(size.x / size.y) * Mathf.Rad2Deg;
            halfOfDiagonalLenght = Mathf.Sqrt(Mathf.Pow(size.x, 2) + Mathf.Pow(size.y, 2)) * 0.5f;
        }

        private static void GetCornersPositionInLsOfBox(
            float angle,
            float angleOfDiagonal,
            float halfOfDiagonalLenght,
            out Vector3 cornerPosition1,
            out Vector3 cornerPosition2,
            out Vector3 cornerPosition3,
            out Vector3 cornerPosition4)
        {
            cornerPosition1 = new Vector3(-Mathf.Sin(Mathf.Deg2Rad * (angle - angleOfDiagonal)), Mathf.Cos(Mathf.Deg2Rad * (angle - angleOfDiagonal))).normalized *
                              halfOfDiagonalLenght;
            cornerPosition2 =
                new Vector3(-Mathf.Sin(Mathf.Deg2Rad * (angle - 180 + angleOfDiagonal)), Mathf.Cos(Mathf.Deg2Rad * (angle - 180 + angleOfDiagonal))).normalized *
                halfOfDiagonalLenght;
            cornerPosition3 =
                new Vector3(-Mathf.Sin(Mathf.Deg2Rad * (angle - 180 - angleOfDiagonal)), Mathf.Cos(Mathf.Deg2Rad * (angle - 180 - angleOfDiagonal))).normalized *
                halfOfDiagonalLenght;
            cornerPosition4 =
                new Vector3(-Mathf.Sin(Mathf.Deg2Rad * (angle - 360 + angleOfDiagonal)), Mathf.Cos(Mathf.Deg2Rad * (angle - 360 + angleOfDiagonal))).normalized *
                halfOfDiagonalLenght;
        }

#endif
    }
}