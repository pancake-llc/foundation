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
        public static readonly Color DarkPurple = new Color(.2f, .169f, .255f);
        public static readonly Color Green = new Color(0.345f, 0.737f, 0.51f);
        public static readonly Color Red = new Color(0.922f, 0.318f, 0.376f);
        public static readonly Color Yellow = new Color(0.949f, 0.816f, 0.482f);
        public static readonly Color Blue = new Color(0.757f, 0.816f, 0.933f);
        public static readonly Color Cyan = new Color(0.698f, 0.125f, 0.467f);

        static readonly List<Matrix4x4> matrixStack = new List<Matrix4x4>();
        public static Matrix4x4 Matrix => matrixStack.Count > 0 ? matrixStack[matrixStack.Count - 1] : Matrix4x4.identity;

        public static void PushMatrix(Matrix4x4 m)
        {
            matrixStack.Add(m);
            SetMatrix(m);
        }

        public static void PopMatrix()
        {
            if (matrixStack.Count > 0)
            {
                matrixStack.RemoveAt(matrixStack.Count - 1);
            }

            SetMatrix(Matrix);
        }

        static void SetMatrix(Matrix4x4 m)
        {
            if (matrixStack.Count > 0)
            {
                matrixStack[matrixStack.Count - 1] = m;
            }

            Gizmos.matrix = m;
#if UNITY_EDITOR
            Handles.matrix = m;
#endif
        }

        static readonly List<Color> colorStack = new List<Color>();
        public static Color Color => colorStack.Count > 0 ? colorStack[colorStack.Count - 1] : Color.cyan;

        public static void PushColor(Color c)
        {
            colorStack.Add(c);
            SetColor(c);
        }

        public static void PopColor()
        {
            if (colorStack.Count > 0)
            {
                colorStack.RemoveAt(colorStack.Count - 1);
            }

            SetColor(Color);
        }

        static void SetColor(Color c)
        {
            if (colorStack.Count > 0)
            {
                colorStack[colorStack.Count - 1] = c;
            }

            Gizmos.color = c;
#if UNITY_EDITOR
            Handles.color = c;
#endif
        }

        static Color SetA(Color c, float a) => new Color(c.r, c.g, c.b, a);

        public static Color LerpColour(Color[] pts, float t)
        {
            var i = Mathf.FloorToInt(t * pts.Length);
            var frac = (t * pts.Length) - i;
            if (i < 0)
            {
                return pts[0];
            }

            if (i >= pts.Length - 1)
            {
                return pts[pts.Length - 1];
            }

            return Color.Lerp(pts[i], pts[i + 1], frac);
        }

        public static Color ParseHexColour(string c)
        {
            var col = Color.magenta;
            ColorUtility.TryParseHtmlString(c, out col);
            return col;
        }

        public static Color[] ParseHexColours(string[] colours)
        {
            var result = new Color[colours.Length];
            for (var i = 0; i < colours.Length; i++)
            {
                result[i] = ParseHexColour(colours[i]);
            }

            return result;
        }
    }
}