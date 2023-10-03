using System.Collections.Generic;
using UnityEngine;

namespace Pancake.ExLib.Maths
{
    public static class Math2D
    {
        public static float PseudoDistanceFromPointToLine(Vector2 a, Vector2 b, Vector2 c) { return Mathf.Abs((c.x - a.x) * (-b.y + a.y) + (c.y - a.y) * (b.x - a.x)); }

        public static int SideOfLine(Vector2 a, Vector2 b, Vector2 c) { return (int) Mathf.Sign((c.x - a.x) * (-b.y + a.y) + (c.y - a.y) * (b.x - a.x)); }

        public static int SideOfLine(float ax, float ay, float bx, float by, float cx, float cy)
        {
            return (int) Mathf.Sign((cx - ax) * (-by + ay) + (cy - ay) * (bx - ax));
        }

        public static bool PointInTriangle(Vector2 a, Vector2 b, Vector2 c, Vector2 p)
        {
            float area = 0.5f * (-b.y * c.x + a.y * (-b.x + c.x) + a.x * (b.y - c.y) + b.x * c.y);
            float s = 1 / (2 * area) * (a.y * c.x - a.x * c.y + (c.y - a.y) * p.x + (a.x - c.x) * p.y);
            float t = 1 / (2 * area) * (a.x * b.y - a.y * b.x + (a.y - b.y) * p.x + (b.x - a.x) * p.y);
            return s >= 0 && t >= 0 && (s + t) <= 1;
        }

        public static bool LineSegmentsIntersect(Vector2 a, Vector2 b, Vector2 c, Vector2 d)
        {
            float denominator = ((b.x - a.x) * (d.y - c.y)) - ((b.y - a.y) * (d.x - c.x));
            if (Mathf.Approximately(denominator, 0))
            {
                return false;
            }

            float numerator1 = ((a.y - c.y) * (d.x - c.x)) - ((a.x - c.x) * (d.y - c.y));
            float numerator2 = ((a.y - c.y) * (b.x - a.x)) - ((a.x - c.x) * (b.y - a.y));

            if (Mathf.Approximately(numerator1, 0) || Mathf.Approximately(numerator2, 0))
            {
                return false;
            }

            float r = numerator1 / denominator;
            float s = numerator2 / denominator;

            return (r > 0 && r < 1) && (s > 0 && s < 1);
        }

        public static bool PointInPolygon(Vector2 point, List<Vector2> points)
        {
            bool result = false;
            int j = points.Count - 1;
            for (int i = 0; i < points.Count; i++)
            {
                if ((points[i].y < point.y && points[j].y >= point.y || points[j].y < point.y && points[i].y >= point.y) &&
                    (points[i].x + (point.y - points[i].y) / (points[j].y - points[i].y) * (points[j].x - points[i].x) < point.x))
                    result = !result;
                j = i;
            }

            return result;
        }
    }
}