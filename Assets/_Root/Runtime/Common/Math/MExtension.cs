using System;
using System.Runtime.CompilerServices;
using UnityEngine;


namespace Pancake
{
    /// <summary>Various extensions for floats, vectors and colors</summary>
    public static class MExtension
    {
        const MethodImplOptions INLINE = MethodImplOptions.AggressiveInlining;

        #region Vector rotation and angles

        /// <summary>Returns the angle of this vector, in radians</summary>
        /// <param name="v">The vector to get the angle of. It does not have to be normalized</param>
        /// <seealso cref="M.DirToAng"/>
        [MethodImpl(INLINE)]
        public static float Angle(this Vector2 v) => Mathf.Atan2(v.y, v.x);

        /// <summary>Get the angle in degrees off the forward defined by x.</summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <seealso cref="M.DirToAng"/>
        [MethodImpl(INLINE)]
        public static float Angle(float x, float y) => Mathf.Atan2(y, x);

        /// <summary>
        /// The angle between 2 vectors in radian
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static float AngleBetween(Vector2 a, Vector2 b)
        {
            // // Due to float error the dot / mag can sometimes be ever so slightly over 1, which can cause NaN in acos.
            //return Mathf.Acos(Vector2.Dot(a, b) / (a.magnitude * b.magnitude)) * MathUtil.RAD_TO_DEG;
            double d = (double) Vector2.Dot(a, b) / ((double) a.magnitude * (double) b.magnitude);
            if (d >= 1d) return 0f;
            else if (d <= -1d) return 180f;
            return (float) System.Math.Acos(d);
        }

        /// <summary>
        /// The angle between 2 lines, regardless of heading (+/- versions of vector are normalized so that opposite vectors are treated as an angle of 0). 
        /// This means a value near 0 means the vectors are parrallel, a value near 90 means they're perpendicular.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns>Returns an angle in degrees from 0 -> 90</returns>
        public static float AngleAgainst(Vector2 a, Vector2 b)
        {
            // // Due to float error the dot / mag can sometimes be ever so slightly over 1, which can cause NaN in acos.
            //return Mathf.Acos(Vector2.Dot(a, b) / (a.magnitude * b.magnitude)) * MathUtil.RAD_TO_DEG;
            double d = System.Math.Abs((double) Vector2.Dot(a, b) / ((double) a.magnitude * (double) b.magnitude));
            if (d >= 1d) return 0f;
            else if (d <= 0d) return 90f;
            return (float) System.Math.Acos(d) * M.RAD_TO_DEG;
        }

        /// <summary>
        /// The cosine between 2 vectors regardless of magnitude. Acos this value to get the angle between.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static float CosBetween(Vector2 a, Vector2 b)
        {
            // // Due to float error the dot / mag can sometimes be ever so slightly over 1, which can cause NaN in acos.
            double d = System.Math.Abs((double) Vector2.Dot(a, b) / ((double) a.magnitude * (double) b.magnitude));
            if (d >= 1d) return 1f;
            else if (d <= 0d) return 0f;
            return (float) d;
        }

        /// <summary>
        /// Angle in degrees off some axis in the counter-clockwise direction. Think of like 'Angle' or 'Atan2' where you get to control 
        /// which axis as opposed to only measuring off of <1,0>. 
        /// </summary>
        /// <param name="v"></param>
        /// <param name="axis"></param>
        /// <returns></returns>
        public static float AngleOff(Vector2 v, Vector2 axis)
        {
            if (axis.sqrMagnitude < 0.0001f) return float.NaN;
            axis.Normalize();
            var tang = new Vector2(-axis.y, axis.x);
            return AngleBetween(v, axis) * Mathf.Sign(Vector2.Dot(v, tang));
        }

        public static void Reflect(ref Vector2 v, Vector2 normal)
        {
            var dp = 2f * Vector2.Dot(v, normal);
            var ix = v.x - normal.x * dp;
            var iy = v.y - normal.y * dp;
            v.x = ix;
            v.y = iy;
        }

        public static Vector2 Reflect(Vector2 v, Vector2 normal)
        {
            var dp = 2 * Vector2.Dot(v, normal);
            return new Vector2(v.x - normal.x * dp, v.y - normal.y * dp);
        }

        public static void Mirror(ref Vector2 v, Vector2 axis) { v = (2 * (Vector2.Dot(v, axis) / Vector2.Dot(axis, axis)) * axis) - v; }

        public static Vector2 Mirror(Vector2 v, Vector2 axis) { return (2 * (Vector2.Dot(v, axis) / Vector2.Dot(axis, axis)) * axis) - v; }

        /// <summary>
        /// Rotate Vector2 counter-clockwise by 'a'
        /// </summary>
        /// <param name="v"></param>
        /// <param name="aRad">angle in rad</param>
        /// <returns></returns>
        public static Vector2 RotateBy(Vector2 v, float aRad)
        {
            var ca = System.Math.Cos(aRad);
            var sa = System.Math.Sin(aRad);
            var rx = v.x * ca - v.y * sa;

            return new Vector2((float) rx, (float) (v.x * sa + v.y * ca));
        }


        /// <summary>Rotates the vector 90 degrees clockwise (negative Z axis rotation)</summary>
        [MethodImpl(INLINE)]
        public static Vector2 Rotate90CW(this Vector2 v) => new Vector2(v.y, -v.x);

        /// <summary>Rotates the vector 90 degrees counter-clockwise (positive Z axis rotation)</summary>
        [MethodImpl(INLINE)]
        public static Vector2 Rotate90CCW(this Vector2 v) => new Vector2(-v.y, v.x);

        /// <summary>Rotates the vector around <c>pivot</c> with the given angle (in radians)</summary>
        /// <param name="v">The vector to rotate</param>
        /// <param name="pivot">The point to rotate around</param>
        /// <param name="angRad">The angle to rotate by, in radians</param>
        [MethodImpl(INLINE)]
        public static Vector2 RotateAround(this Vector2 v, Vector2 pivot, float angRad) => Rotate(v - pivot, angRad) + pivot;

        /// <summary>Rotates the vector around <c>(0,0)</c> with the given angle (in radians)</summary>
        /// <param name="v">The vector to rotate</param>
        /// <param name="angRad">The angle to rotate by, in radians</param>
        public static Vector2 Rotate(this Vector2 v, float angRad)
        {
            float ca = Mathf.Cos(angRad);
            float sa = Mathf.Sin(angRad);
            return new Vector2(ca * v.x - sa * v.y, sa * v.x + ca * v.y);
        }

        /// <summary>Converts an angle in degrees to radians</summary>
        /// <param name="angDegrees">The angle, in degrees, to convert to radians</param>
        [MethodImpl(INLINE)]
        public static float DegToRad(this float angDegrees) => angDegrees * M.DEG_TO_RAD;

        /// <summary>Converts an angle in radians to degrees</summary>
        /// <param name="angRadians">The angle, in radians, to convert to degrees</param>
        [MethodImpl(INLINE)]
        public static float RadToDeg(this float angRadians) => angRadians * M.RAD_TO_DEG;

        /// <summary>Extracts the quaternion components into a Vector4</summary>
        /// <param name="q">The quaternion to get the components of</param>
        [MethodImpl(INLINE)]
        public static Vector4 ToVector4(this Quaternion q) => new Vector4(q.x, q.y, q.z, q.w);

        #endregion

        #region Swizzling

        /// <summary>Returns X and Y as a Vector2, equivalent to <c>new Vector2(v.x,v.y)</c></summary>
        [MethodImpl(INLINE)]
        public static Vector2 XY(this Vector2 v) => new Vector2(v.x, v.y);

        /// <summary>Returns Y and X as a Vector2, equivalent to <c>new Vector2(v.y,v.x)</c></summary>
        [MethodImpl(INLINE)]
        public static Vector2 YX(this Vector2 v) => new Vector2(v.y, v.x);

        /// <summary>Returns X and Z as a Vector2, equivalent to <c>new Vector2(v.x,v.z)</c></summary>
        [MethodImpl(INLINE)]
        public static Vector2 XZ(this Vector3 v) => new Vector2(v.x, v.z);

        /// <summary>Returns this vector as a Vector3, slotting X into X, and Y into Z, and the input value y into Y.
        /// Equivalent to <c>new Vector3(v.x,y,v.y)</c></summary>
        [MethodImpl(INLINE)]
        public static Vector3 XZtoXYZ(this Vector2 v, float y = 0) => new Vector3(v.x, y, v.y);

        /// <summary>Returns this vector as a Vector3, slotting X into X, and Y into Y, and the input value z into Z.
        /// Equivalent to <c>new Vector3(v.x,v.y,z)</c></summary>
        [MethodImpl(INLINE)]
        public static Vector3 XYtoXYZ(this Vector2 v, float z = 0) => new Vector3(v.x, v.y, z);

        /// <summary>Sets X to 0</summary>
        [MethodImpl(INLINE)]
        public static Vector2 FlattenX(this Vector2 v) => new Vector2(0f, v.y);

        /// <summary>Sets Y to 0</summary>
        [MethodImpl(INLINE)]
        public static Vector2 FlattenY(this Vector2 v) => new Vector2(v.x, 0f);

        /// <summary>Sets X to 0</summary>
        [MethodImpl(INLINE)]
        public static Vector3 FlattenX(this Vector3 v) => new Vector3(0f, v.y, v.z);

        /// <summary>Sets Y to 0</summary>
        [MethodImpl(INLINE)]
        public static Vector3 FlattenY(this Vector3 v) => new Vector3(v.x, 0f, v.z);

        /// <summary>Sets Z to 0</summary>
        [MethodImpl(INLINE)]
        public static Vector3 FlattenZ(this Vector3 v) => new Vector3(v.x, v.y, 0f);

        #endregion

        #region Vector directions & magnitudes

        /// <summary>Returns a vector with the same direction, but with the given magnitude.
        /// Equivalent to <c>v.normalized*mag</c></summary>
        [MethodImpl(INLINE)]
        public static Vector2 WithMagnitude(this Vector2 v, float mag) => v.normalized * mag;

        /// <summary>Returns a vector with the same direction, but with the given magnitude.
        /// Equivalent to <c>v.normalized*mag</c></summary>
        [MethodImpl(INLINE)]
        public static Vector3 WithMagnitude(this Vector3 v, float mag) => v.normalized * mag;

        /// <summary>Returns the vector going from one position to another, also known as the displacement.
        /// Equivalent to <c>target-v</c></summary>
        [MethodImpl(INLINE)]
        public static Vector2 To(this Vector2 v, Vector2 target) => target - v;

        /// <summary>Returns the vector going from one position to another, also known as the displacement.
        /// Equivalent to <c>target-v</c></summary>
        [MethodImpl(INLINE)]
        public static Vector3 To(this Vector3 v, Vector3 target) => target - v;

        /// <summary>Returns the normalized direction from this vector to the target.
        /// Equivalent to <c>(target-v).normalized</c> or <c>v.To(target).normalized</c></summary>
        [MethodImpl(INLINE)]
        public static Vector2 DirTo(this Vector2 v, Vector2 target) => (target - v).normalized;

        /// <summary>Returns the normalized direction from this vector to the target.
        /// Equivalent to <c>(target-v).normalized</c> or <c>v.To(target).normalized</c></summary>
        [MethodImpl(INLINE)]
        public static Vector3 DirTo(this Vector3 v, Vector3 target) => (target - v).normalized;

        /// <summary>Mirrors this vector around another point. Equivalent to rotating the vector 180° around the point</summary>
        /// <param name="p">The point to mirror</param>
        /// <param name="pivot">The point to mirror around</param>
        [MethodImpl(INLINE)]
        public static Vector2 MirrorAround(this Vector2 p, Vector2 pivot) => new(2 * pivot.x - p.x, 2 * pivot.y - p.y);

        /// <summary>Mirrors this vector around an x coordinate</summary>
        /// <param name="p">The point to mirror</param>
        /// <param name="xPivot">The x coordinate to mirror around</param>
        [MethodImpl(INLINE)]
        public static Vector2 MirrorAroundX(this Vector2 p, float xPivot) => new(2 * xPivot - p.x, p.y);

        /// <summary>Mirrors this vector around a y coordinate</summary>
        /// <param name="p">The point to mirror</param>
        /// <param name="yPivot">The y coordinate to mirror around</param>
        [MethodImpl(INLINE)]
        public static Vector2 MirrorAroundY(this Vector2 p, float yPivot) => new(p.x, 2 * yPivot - p.y);

        /// <inheritdoc cref="MirrorAroundX(Vector2,float)"/>
        [MethodImpl(INLINE)]
        public static Vector3 MirrorAroundX(this Vector3 p, float xPivot) => new(2 * xPivot - p.x, p.y, p.z);

        /// <inheritdoc cref="MirrorAroundY(Vector2,float)"/>
        [MethodImpl(INLINE)]
        public static Vector3 MirrorAroundY(this Vector3 p, float yPivot) => new(p.x, 2 * yPivot - p.y, p.z);

        /// <summary>Mirrors this vector around a y coordinate</summary>
        /// <param name="p">The point to mirror</param>
        /// <param name="zPivot">The z coordinate to mirror around</param>
        [MethodImpl(INLINE)]
        public static Vector3 MirrorAroundZ(this Vector3 p, float zPivot) => new(p.x, p.y, 2 * zPivot - p.z);

        /// <inheritdoc cref="MirrorAround(Vector2,Vector2)"/>
        [MethodImpl(INLINE)]
        public static Vector3 MirrorAround(this Vector3 p, Vector3 pivot) => new(2 * pivot.x - p.x, 2 * pivot.y - p.y, 2 * pivot.z - p.z);

        #endregion

        #region Color manipulation

        /// <summary>Returns the same color, but with the specified alpha value</summary>
        /// <param name="c">The source color</param>
        /// <param name="a">The new alpha value</param>
        [MethodImpl(INLINE)]
        public static Color WithAlpha(this Color c, float a) => new Color(c.r, c.g, c.b, a);

        /// <summary>Returns the same color and alpha, but with RGB multiplied by the given value</summary>
        /// <param name="c">The source color</param>
        /// <param name="m">The multiplier for the RGB channels</param>
        [MethodImpl(INLINE)]
        public static Color MultiplyRGB(this Color c, float m) => new Color(c.r * m, c.g * m, c.b * m, c.a);

        /// <summary>Returns the same color and alpha, but with the RGB values multiplief by another color</summary>
        /// <param name="c">The source color</param>
        /// <param name="m">The color to multiply RGB by</param>
        [MethodImpl(INLINE)]
        public static Color MultiplyRGB(this Color c, Color m) => new Color(c.r * m.r, c.g * m.g, c.b * m.b, c.a);

        /// <summary>Returns the same color, but with the alpha channel multiplied by the given value</summary>
        /// <param name="c">The source color</param>
        /// <param name="m">The multiplier for the alpha</param>
        [MethodImpl(INLINE)]
        public static Color MultiplyA(this Color c, float m) => new Color(c.r, c.g, c.b, c.a * m);

        #endregion

        #region Rect

        /// <summary>Expands the rectangle to encapsulate the point <c>p</c></summary>
        /// <param name="r">The rectangle to expand</param>
        /// <param name="p">The point to encapsulate</param>
        public static Rect Encapsulate(this Rect r, Vector2 p)
        {
            r.xMax = Mathf.Max(r.xMax, p.x);
            r.xMin = Mathf.Min(r.xMin, p.x);
            r.yMax = Mathf.Max(r.yMax, p.y);
            r.yMin = Mathf.Min(r.yMin, p.y);
            return r;
        }

        /// <summary>Interpolates a position within this rectangle, given a normalized position</summary>
        /// <param name="r">The rectangle to get a position within</param>
        /// <param name="tPos">The normalized position within this rectangle</param>
        public static Vector2 Lerp(this Rect r, Vector2 tPos) => new(M.Lerp(r.xMin, r.xMax, tPos.x), M.Lerp(r.yMin, r.yMax, tPos.y));

        /// <summary>The x axis range of this rectangle</summary>
        /// <param name="rect">The rectangle to get the x range of</param>
        public static FloatRange RangeX(this Rect rect) => (rect.xMin, rect.xMax);

        /// <summary>The y axis range of this rectangle</summary>
        /// <param name="rect">The rectangle to get the y range of</param>
        public static FloatRange RangeY(this Rect rect) => (rect.yMin, rect.yMax);

        #endregion

        #region Simple float and int operations

        /// <summary>Returns true if v is between or equal to <c>min</c> &amp; <c>max</c></summary>
        /// <seealso cref="Between(float,float,float)"/>
        [MethodImpl(INLINE)]
        public static bool Within(this float v, float min, float max) => v >= min && v <= max;

        /// <summary>Returns true if v is between or equal to <c>min</c> &amp; <c>max</c></summary>
        /// <seealso cref="Between(int,int,int)"/>
        [MethodImpl(INLINE)]
        public static bool Within(this int v, int min, int max) => v >= min && v <= max;

        /// <summary>Returns true if v is between, but not equal to, <c>min</c> &amp; <c>max</c></summary>
        /// <seealso cref="Within(float,float,float)"/>
        [MethodImpl(INLINE)]
        public static bool Between(this float v, float min, float max) => v > min && v < max;

        /// <summary>Returns true if v is between, but not equal to, <c>min</c> &amp; <c>max</c></summary>
        /// <seealso cref="Within(int,int,int)"/>
        [MethodImpl(INLINE)]
        public static bool Between(this int v, int min, int max) => v > min && v < max;

        /// <summary>Clamps the value to be at least <c>min</c></summary>
        [MethodImpl(INLINE)]
        public static float AtLeast(this float v, float min) => v < min ? min : v;

        /// <summary>Clamps the value to be at least <c>min</c></summary>
        [MethodImpl(INLINE)]
        public static int AtLeast(this int v, int min) => v < min ? min : v;

        /// <summary>Clamps the value to be at most <c>max</c></summary>
        [MethodImpl(INLINE)]
        public static float AtMost(this float v, float max) => v > max ? max : v;

        /// <summary>Clamps the value to be at most <c>max</c></summary>
        [MethodImpl(INLINE)]
        public static int AtMost(this int v, int max) => v > max ? max : v;

        /// <summary>Squares the value. Equivalent to <c>v*v</c></summary>
        [MethodImpl(INLINE)]
        public static float Square(this float v) => v * v;

        /// <summary>Cubes the value. Equivalent to <c>v*v*v</c></summary>
        [MethodImpl(INLINE)]
        public static float Cube(this float v) => v * v * v;

        /// <summary>Squares the value. Equivalent to <c>v*v</c></summary>
        [MethodImpl(INLINE)]
        public static int Square(this int v) => v * v;

        /// <summary>The next integer, modulo <c>length</c>. Behaves the way you want with negative values for stuff like array index access etc</summary>
        [MethodImpl(INLINE)]
        public static int NextMod(this int value, int length) => (value + 1).Mod(length);

        /// <summary>The previous integer, modulo <c>length</c>. Behaves the way you want with negative values for stuff like array index access etc</summary>
        [MethodImpl(INLINE)]
        public static int PrevMod(this int value, int length) => (value - 1).Mod(length);

        #endregion

        #region String extensions

        public static string ToValueTableString(this string[,] m)
        {
            int rowCount = m.GetLength(0);
            int colCount = m.GetLength(1);
            string[] r = new string[rowCount];
            for (int i = 0; i < rowCount; i++)
                r[i] = "";

            for (int c = 0; c < colCount; c++)
            {
                string endBit = c == colCount - 1 ? "" : ", ";

                int colWidth = 4; // min width
                string[] columnEntries = new string[rowCount];
                for (int row = 0; row < rowCount; row++)
                {
                    string s = m[row, c].StartsWith('-') ? "" : " ";
                    columnEntries[row] = $"{s}{m[row, c]}{endBit}";
                    colWidth = M.Max(colWidth, columnEntries[row].Length);
                }

                for (int row = 0; row < rowCount; row++)
                {
                    r[row] += columnEntries[row].PadRight(colWidth, ' ');
                }
            }

            return string.Join('\n', r);
        }

        #endregion

        #region Extension method counterparts of the static M functions - lots of boilerplate in here

        #region Math operations

        /// <inheritdoc cref="M.Sqrt(float)"/>
        [MethodImpl(INLINE)]
        public static float Sqrt(this float value) => M.Sqrt(value);

        /// <inheritdoc cref="M.Sqrt(Vector2)"/>
        [MethodImpl(INLINE)]
        public static Vector2 Sqrt(this Vector2 value) => M.Sqrt(value);

        /// <inheritdoc cref="M.Sqrt(Vector3)"/>
        [MethodImpl(INLINE)]
        public static Vector3 Sqrt(this Vector3 value) => M.Sqrt(value);

        /// <inheritdoc cref="M.Sqrt(Vector4)"/>
        [MethodImpl(INLINE)]
        public static Vector4 Sqrt(this Vector4 value) => M.Sqrt(value);

        /// <inheritdoc cref="M.Cbrt(float)"/>
        [MethodImpl(INLINE)]
        public static float Cbrt(this float value) => M.Cbrt(value);

        /// <inheritdoc cref="M.Pow(float, float)"/>
        [MethodImpl(INLINE)]
        public static float Pow(this float value, float exponent) => M.Pow(value, exponent);

        /// <summary>Calculates exact positive integer powers</summary>
        /// <param name="value"></param>
        /// <param name="pow">A positive integer power</param>
        [MethodImpl(INLINE)]
        public static int Pow(this int value, int pow)
        {
            if (pow < 0)
                throw new ArithmeticException("int.Pow(int) doesn't support negative powers");
            checked
            {
                switch (pow)
                {
                    case 0: return 1;
                    case 1: return value;
                    case 2: return value * value;
                    case 3: return value * value * value;
                    default:
                        if (value == 2)
                            return 1 << pow;
                        // from: https://stackoverflow.com/questions/383587/how-do-you-do-integer-exponentiation-in-c
                        int ret = 1;
                        while (pow != 0)
                        {
                            if ((pow & 1) == 1)
                                ret *= value;
                            value *= value;
                            pow >>= 1;
                        }

                        return ret;
                }
            }
        }

        #endregion

        #region Absolute Values

        /// <inheritdoc cref="M.Abs(float)"/>
        [MethodImpl(INLINE)]
        public static float Abs(this float value) => M.Abs(value);

        /// <inheritdoc cref="M.Abs(int)"/>
        [MethodImpl(INLINE)]
        public static int Abs(this int value) => M.Abs(value);

        /// <inheritdoc cref="M.Abs(Vector2)"/>
        [MethodImpl(INLINE)]
        public static Vector2 Abs(this Vector2 v) => M.Abs(v);

        /// <inheritdoc cref="M.Abs(Vector3)"/>
        [MethodImpl(INLINE)]
        public static Vector3 Abs(this Vector3 v) => M.Abs(v);

        /// <inheritdoc cref="M.Abs(Vector4)"/>
        [MethodImpl(INLINE)]
        public static Vector4 Abs(this Vector4 v) => M.Abs(v);

        #endregion

        #region Clamping

        /// <inheritdoc cref="M.Clamp(float,float,float)"/>
        [MethodImpl(INLINE)]
        public static float Clamp(this float value, float min, float max) => M.Clamp(value, min, max);

        /// <inheritdoc cref="M.Clamp(Vector2,Vector2,Vector2)"/>
        [MethodImpl(INLINE)]
        public static Vector2 Clamp(this Vector2 v, Vector2 min, Vector2 max) => M.Clamp(v, min, max);

        /// <inheritdoc cref="M.Clamp(Vector3,Vector3,Vector3)"/>
        [MethodImpl(INLINE)]
        public static Vector3 Clamp(this Vector3 v, Vector3 min, Vector3 max) => M.Clamp(v, min, max);

        /// <inheritdoc cref="M.Clamp(Vector4,Vector4,Vector4)"/>
        [MethodImpl(INLINE)]
        public static Vector4 Clamp(this Vector4 v, Vector4 min, Vector4 max) => M.Clamp(v, min, max);

        /// <inheritdoc cref="M.Clamp(int,int,int)"/>
        [MethodImpl(INLINE)]
        public static int Clamp(this int value, int min, int max) => M.Clamp(value, min, max);

        /// <inheritdoc cref="M.Clamp01(float)"/>
        [MethodImpl(INLINE)]
        public static float Clamp01(this float value) => M.Clamp01(value);

        /// <inheritdoc cref="M.Clamp01(Vector2)"/>
        [MethodImpl(INLINE)]
        public static Vector2 Clamp01(this Vector2 v) => M.Clamp01(v);

        /// <inheritdoc cref="M.Clamp01(Vector3)"/>
        [MethodImpl(INLINE)]
        public static Vector3 Clamp01(this Vector3 v) => M.Clamp01(v);

        /// <inheritdoc cref="M.Clamp01(Vector4)"/>
        [MethodImpl(INLINE)]
        public static Vector4 Clamp01(this Vector4 v) => M.Clamp01(v);

        /// <inheritdoc cref="M.ClampNeg1to1(float)"/>
        [MethodImpl(INLINE)]
        public static float ClampNeg1to1(this float value) => M.ClampNeg1to1(value);

        /// <inheritdoc cref="M.ClampNeg1to1(Vector2)"/>
        [MethodImpl(INLINE)]
        public static Vector2 ClampNeg1to1(this Vector2 v) => M.ClampNeg1to1(v);

        /// <inheritdoc cref="M.ClampNeg1to1(Vector3)"/>
        [MethodImpl(INLINE)]
        public static Vector3 ClampNeg1to1(this Vector3 v) => M.ClampNeg1to1(v);

        /// <inheritdoc cref="M.ClampNeg1to1(Vector4)"/>
        [MethodImpl(INLINE)]
        public static Vector4 ClampNeg1to1(this Vector4 v) => M.ClampNeg1to1(v);

        #endregion

        #region Min & Max

        /// <inheritdoc cref="M.Min(Vector2)"/>
        [MethodImpl(INLINE)]
        public static float Min(this Vector2 v) => M.Min(v);

        /// <inheritdoc cref="M.Min(Vector3)"/>
        [MethodImpl(INLINE)]
        public static float Min(this Vector3 v) => M.Min(v);

        /// <inheritdoc cref="M.Min(Vector4)"/>
        [MethodImpl(INLINE)]
        public static float Min(this Vector4 v) => M.Min(v);

        /// <inheritdoc cref="M.Max(Vector2)"/>
        [MethodImpl(INLINE)]
        public static float Max(this Vector2 v) => M.Max(v);

        /// <inheritdoc cref="M.Max(Vector3)"/>
        [MethodImpl(INLINE)]
        public static float Max(this Vector3 v) => M.Max(v);

        /// <inheritdoc cref="M.Max(Vector4)"/>
        [MethodImpl(INLINE)]
        public static float Max(this Vector4 v) => M.Max(v);

        #endregion

        #region Signs & Rounding

        /// <inheritdoc cref="M.Sign(float)"/>
        [MethodImpl(INLINE)]
        public static float Sign(this float value) => M.Sign(value);

        /// <inheritdoc cref="M.Sign(Vector2)"/>
        [MethodImpl(INLINE)]
        public static Vector2 Sign(this Vector2 value) => M.Sign(value);

        /// <inheritdoc cref="M.Sign(Vector3)"/>
        [MethodImpl(INLINE)]
        public static Vector3 Sign(this Vector3 value) => M.Sign(value);

        /// <inheritdoc cref="M.Sign(Vector4)"/>
        [MethodImpl(INLINE)]
        public static Vector4 Sign(this Vector4 value) => M.Sign(value);

        /// <inheritdoc cref="M.Sign(int)"/>
        [MethodImpl(INLINE)]
        public static int Sign(this int value) => M.Sign(value);

        /// <inheritdoc cref="M.SignAsInt(float)"/>
        [MethodImpl(INLINE)]
        public static int SignAsInt(this float value) => M.SignAsInt(value);

        /// <inheritdoc cref="M.SignWithZero(float,float)"/>
        [MethodImpl(INLINE)]
        public static float SignWithZero(this float value, float zeroThreshold = 0.000001f) => M.SignWithZero(value, zeroThreshold);

        /// <inheritdoc cref="M.SignWithZero(Vector2,float)"/>
        [MethodImpl(INLINE)]
        public static Vector2 SignWithZero(this Vector2 value, float zeroThreshold = 0.000001f) => M.SignWithZero(value, zeroThreshold);

        /// <inheritdoc cref="M.SignWithZero(Vector3,float)"/>
        [MethodImpl(INLINE)]
        public static Vector3 SignWithZero(this Vector3 value, float zeroThreshold = 0.000001f) => M.SignWithZero(value, zeroThreshold);

        /// <inheritdoc cref="M.SignWithZero(Vector4,float)"/>
        [MethodImpl(INLINE)]
        public static Vector4 SignWithZero(this Vector4 value, float zeroThreshold = 0.000001f) => M.SignWithZero(value, zeroThreshold);

        /// <inheritdoc cref="M.SignWithZero(int)"/>
        [MethodImpl(INLINE)]
        public static int SignWithZero(this int value) => M.SignWithZero(value);

        /// <inheritdoc cref="M.SignWithZeroAsInt(float,float)"/>
        [MethodImpl(INLINE)]
        public static int SignWithZeroAsInt(this float value, float zeroThreshold = 0.000001f) => M.SignWithZeroAsInt(value, zeroThreshold);

        /// <inheritdoc cref="M.Floor(float)"/>
        [MethodImpl(INLINE)]
        public static float Floor(this float value) => M.Floor(value);

        /// <inheritdoc cref="M.Floor(Vector2)"/>
        [MethodImpl(INLINE)]
        public static Vector2 Floor(this Vector2 value) => M.Floor(value);

        /// <inheritdoc cref="M.Floor(Vector2)"/>
        [MethodImpl(INLINE)]
        public static Vector3 Floor(this Vector3 value) => M.Floor(value);

        /// <inheritdoc cref="M.Floor(Vector2)"/>
        [MethodImpl(INLINE)]
        public static Vector4 Floor(this Vector4 value) => M.Floor(value);

        /// <inheritdoc cref="M.FloorToInt(float)"/>
        [MethodImpl(INLINE)]
        public static int FloorToInt(this float value) => M.FloorToInt(value);

        /// <inheritdoc cref="M.FloorToInt(Vector2)"/>
        [MethodImpl(INLINE)]
        public static Vector2Int FloorToInt(this Vector2 value) => M.FloorToInt(value);

        /// <inheritdoc cref="M.FloorToInt(Vector2)"/>
        [MethodImpl(INLINE)]
        public static Vector3Int FloorToInt(this Vector3 value) => M.FloorToInt(value);

        /// <inheritdoc cref="M.Ceil(float)"/>
        [MethodImpl(INLINE)]
        public static float Ceil(this float value) => M.Ceil(value);

        /// <inheritdoc cref="M.Ceil(Vector2)"/>
        [MethodImpl(INLINE)]
        public static Vector2 Ceil(this Vector2 value) => M.Ceil(value);

        /// <inheritdoc cref="M.Ceil(Vector2)"/>
        [MethodImpl(INLINE)]
        public static Vector3 Ceil(this Vector3 value) => M.Ceil(value);

        /// <inheritdoc cref="M.Ceil(Vector2)"/>
        [MethodImpl(INLINE)]
        public static Vector4 Ceil(this Vector4 value) => M.Ceil(value);

        /// <inheritdoc cref="M.CeilToInt(float)"/>
        [MethodImpl(INLINE)]
        public static int CeilToInt(this float value) => M.CeilToInt(value);

        /// <inheritdoc cref="M.CeilToInt(Vector2)"/>
        [MethodImpl(INLINE)]
        public static Vector2Int CeilToInt(this Vector2 value) => M.CeilToInt(value);

        /// <inheritdoc cref="M.CeilToInt(Vector2)"/>
        [MethodImpl(INLINE)]
        public static Vector3Int CeilToInt(this Vector3 value) => M.CeilToInt(value);

        /// <inheritdoc cref="M.Round(float)"/>
        [MethodImpl(INLINE)]
        public static float Round(this float value) => M.Round(value);

        /// <inheritdoc cref="M.Round(Vector2)"/>
        [MethodImpl(INLINE)]
        public static Vector2 Round(this Vector2 value) => M.Round(value);

        /// <inheritdoc cref="M.Round(Vector2)"/>
        [MethodImpl(INLINE)]
        public static Vector3 Round(this Vector3 value) => M.Round(value);

        /// <inheritdoc cref="M.Round(Vector2)"/>
        [MethodImpl(INLINE)]
        public static Vector4 Round(this Vector4 value) => M.Round(value);

        /// <inheritdoc cref="M.Round(float)"/>
        [MethodImpl(INLINE)]
        public static float Round(this float value, float snapInterval) => M.Round(value, snapInterval);

        /// <inheritdoc cref="M.Round(Vector2)"/>
        [MethodImpl(INLINE)]
        public static Vector2 Round(this Vector2 value, float snapInterval) => M.Round(value, snapInterval);

        /// <inheritdoc cref="M.Round(Vector2,float)"/>
        [MethodImpl(INLINE)]
        public static Vector3 Round(this Vector3 value, float snapInterval) => M.Round(value, snapInterval);

        /// <inheritdoc cref="M.Round(Vector2,float)"/>
        [MethodImpl(INLINE)]
        public static Vector4 Round(this Vector4 value, float snapInterval) => M.Round(value, snapInterval);

        /// <inheritdoc cref="M.RoundToInt(float)"/>
        [MethodImpl(INLINE)]
        public static int RoundToInt(this float value) => M.RoundToInt(value);

        /// <inheritdoc cref="M.RoundToInt(Vector2)"/>
        [MethodImpl(INLINE)]
        public static Vector2Int RoundToInt(this Vector2 value) => M.RoundToInt(value);

        /// <inheritdoc cref="M.RoundToInt(Vector2)"/>
        [MethodImpl(INLINE)]
        public static Vector3Int RoundToInt(this Vector3 value) => M.RoundToInt(value);

        #endregion

        #region Range Repeating

        /// <inheritdoc cref="M.Frac(float)"/>
        [MethodImpl(INLINE)]
        public static float Frac(this float x) => M.Frac(x);

        /// <inheritdoc cref="M.Frac(Vector2)"/>
        [MethodImpl(INLINE)]
        public static Vector2 Frac(this Vector2 v) => M.Frac(v);

        /// <inheritdoc cref="M.Frac(Vector3)"/>
        [MethodImpl(INLINE)]
        public static Vector3 Frac(this Vector3 v) => M.Frac(v);

        /// <inheritdoc cref="M.Frac(Vector4)"/>
        [MethodImpl(INLINE)]
        public static Vector4 Frac(this Vector4 v) => M.Frac(v);

        /// <inheritdoc cref="M.Repeat(float,float)"/>
        [MethodImpl(INLINE)]
        public static float Repeat(this float value, float length) => M.Repeat(value, length);

        /// <inheritdoc cref="M.Mod(int,int)"/>
        [MethodImpl(INLINE)]
        public static int Mod(this int value, int length) => M.Mod(value, length);

        #endregion

        #region Smoothing & Easing Curves

        /// <inheritdoc cref="M.Smooth01(float)"/>
        [MethodImpl(INLINE)]
        public static float Smooth01(this float x) => M.Smooth01(x);

        /// <inheritdoc cref="M.Smoother01(float)"/>
        [MethodImpl(INLINE)]
        public static float Smoother01(this float x) => M.Smoother01(x);

        /// <inheritdoc cref="M.SmoothCos01(float)"/>
        [MethodImpl(INLINE)]
        public static float SmoothCos01(this float x) => M.SmoothCos01(x);

        #endregion

        #region Value & Vector interpolation

        /// <inheritdoc cref="M.Remap(float,float,float,float,float)"/>
        [MethodImpl(INLINE)]
        public static float Remap(this float value, float iMin, float iMax, float oMin, float oMax) =>
            M.Remap(iMin,
                iMax,
                oMin,
                oMax,
                value);

        /// <inheritdoc cref="M.RemapClamped(float,float,float,float,float)"/>
        [MethodImpl(INLINE)]
        public static float RemapClamped(this float value, float iMin, float iMax, float oMin, float oMax) =>
            M.RemapClamped(iMin,
                iMax,
                oMin,
                oMax,
                value);

        /// <inheritdoc cref="M.Remap(float,float,float,float,int)"/>
        [MethodImpl(INLINE)]
        public static float Remap(this int value, float iMin, float iMax, float oMin, float oMax) =>
            M.Remap(iMin,
                iMax,
                oMin,
                oMax,
                value);

        /// <inheritdoc cref="M.RemapClamped(float,float,float,float,float)"/>
        [MethodImpl(INLINE)]
        public static float RemapClamped(this int value, float iMin, float iMax, float oMin, float oMax) =>
            M.RemapClamped(iMin,
                iMax,
                oMin,
                oMax,
                value);

        /// <inheritdoc cref="M.Remap(float,FloatRange,FloatRange)"/>
        [MethodImpl(INLINE)]
        public static float Remap(this float value, FloatRange inRange, FloatRange outRange) =>
            M.Remap(inRange.a,
                inRange.b,
                outRange.a,
                outRange.b,
                value);

        /// <inheritdoc cref="M.RemapClamped(float,FloatRange,FloatRange)"/>
        [MethodImpl(INLINE)]
        public static float RemapClamped(this float value, FloatRange inRange, FloatRange outRange) =>
            M.RemapClamped(inRange.a,
                inRange.b,
                outRange.a,
                outRange.b,
                value);

        /// <inheritdoc cref="M.Lerp(float,float,float)"/>
        [MethodImpl(INLINE)]
        public static float Lerp(this float t, float a, float b) => M.Lerp(a, b, t);

        /// <inheritdoc cref="M.InverseLerp(float,float,float)"/>
        [MethodImpl(INLINE)]
        public static float InverseLerp(this float value, float a, float b) => M.InverseLerp(a, b, value);

        /// <inheritdoc cref="M.LerpClamped(float,float,float)"/>
        [MethodImpl(INLINE)]
        public static float LerpClamped(this float t, float a, float b) => M.LerpClamped(a, b, t);

        /// <inheritdoc cref="M.InverseLerpClamped(float,float,float)"/>
        [MethodImpl(INLINE)]
        public static float InverseLerpClamped(this float value, float a, float b) => M.InverseLerpClamped(a, b, value);

        /// <inheritdoc cref="M.Remap(Vector2,Vector2,Vector2,Vector2,Vector2)"/>
        [MethodImpl(INLINE)]
        public static Vector2 Remap(this Vector2 v, Vector2 iMin, Vector2 iMax, Vector2 oMin, Vector2 oMax) =>
            M.Remap(iMin,
                iMax,
                oMin,
                oMax,
                v);

        /// <inheritdoc cref="M.Remap(Vector3,Vector3,Vector3,Vector3,Vector3)"/>
        [MethodImpl(INLINE)]
        public static Vector3 Remap(this Vector3 v, Vector3 iMin, Vector3 iMax, Vector3 oMin, Vector3 oMax) =>
            M.Remap(iMin,
                iMax,
                oMin,
                oMax,
                v);

        /// <inheritdoc cref="M.Remap(Vector4,Vector4,Vector4,Vector4,Vector4)"/>
        [MethodImpl(INLINE)]
        public static Vector4 Remap(this Vector4 v, Vector4 iMin, Vector4 iMax, Vector4 oMin, Vector4 oMax) =>
            M.Remap(iMin,
                iMax,
                oMin,
                oMax,
                v);

        /// <inheritdoc cref="M.Remap(Rect,Rect,Vector2)"/>
        [MethodImpl(INLINE)]
        public static Vector2 Remap(this Vector2 iPos, Rect iRect, Rect oRect) =>
            M.Remap(iRect.min,
                iRect.max,
                oRect.min,
                oRect.max,
                iPos);

        /// <inheritdoc cref="M.Remap(Bounds,Bounds,Vector3)"/>
        [MethodImpl(INLINE)]
        public static Vector3 Remap(this Vector3 iPos, Bounds iBounds, Bounds oBounds) =>
            M.Remap(iBounds.min,
                iBounds.max,
                oBounds.min,
                oBounds.max,
                iPos);

        /// <inheritdoc cref="M.Eerp(float,float,float)"/>
        [MethodImpl(INLINE)]
        public static float Eerp(this float t, float a, float b) => M.Eerp(a, b, t);

        /// <inheritdoc cref="M.InverseEerp(float,float,float)"/>
        [MethodImpl(INLINE)]
        public static float InverseEerp(this float v, float a, float b) => M.InverseEerp(a, b, v);

        #endregion

        #region Vector Math

        /// <inheritdoc cref="M.GetDirAndMagnitude(Vector2)"/>
        [MethodImpl(INLINE)]
        public static (Vector2 dir, float magnitude ) GetDirAndMagnitude(this Vector2 v) => M.GetDirAndMagnitude(v);

        /// <inheritdoc cref="M.GetDirAndMagnitude(Vector3)"/>
        [MethodImpl(INLINE)]
        public static (Vector3 dir, float magnitude ) GetDirAndMagnitude(this Vector3 v) => M.GetDirAndMagnitude(v);

        /// <inheritdoc cref="M.ClampMagnitude(Vector2,float,float)"/>
        [MethodImpl(INLINE)]
        public static Vector2 ClampMagnitude(this Vector2 v, float min, float max) => M.ClampMagnitude(v, min, max);

        /// <inheritdoc cref="M.ClampMagnitude(Vector3,float,float)"/>
        [MethodImpl(INLINE)]
        public static Vector3 ClampMagnitude(this Vector3 v, float min, float max) => M.ClampMagnitude(v, min, max);

        #endregion

        #endregion

        #region Wrap

        public enum WrapMode
        {
            Oblivion = 0,
            Clamp = 1,
            Loop = 2,
            PingPong = 3
        }
        
          /// <summary>
        /// Wraps a value around some significant range.
        /// 
        /// Similar to C# modulo, but works like a true module, in a unary direction over any range (including negative values).
        /// 
        /// ex:
        /// Wrap(8,6,2) == 4
        /// Wrap(4,2,0) == 0
        /// Wrap(4,2,-2) == 0
        /// </summary>
        /// <param name="value">value to wrap</param>
        /// <param name="max">max in range</param>
        /// <param name="min">min in range</param>
        /// <returns>A value wrapped around min to max</returns>
        /// <remarks></remarks>
        public static int Wrap(int value, int max, int min)
        {
            max -= min;
            if (max == 0)
                return min;

            var result = (value - min) % max;
            return result < 0 ? result + max + min : result + min;
        }
        public static int Wrap(int value, int max)
        {
            var result = value % max;
            return result < 0 ? result + max : result;
        }

        public static long Wrap(long value, long max, long min)
        {
            max -= min;
            if (max == 0)
                return min;

            var result = (value - min) % max;
            return result < 0 ? result + max + min : result + min;
        }
        public static long Wrap(long value, long max)
        {
            var result = value % max;
            return result < 0 ? result + max : result;
        }

        public static float Wrap(float value, float max, float min)
        {
            max -= min;
            if (max == 0)
                return min;

            var result = (value - min) % max;
            return result < 0 ? result + max + min : result + min;
        }
        public static float Wrap(float value, float max)
        {
            var result = value % max;
            return result < 0 ? result + max : result;
        }

        public static double Wrap(double value, double max, double min)
        {
            max -= min;
            if (max == 0)
                return min;

            var result = (value - min) % max;
            return result < 0 ? result + max + min : result + min;
        }
        public static double Wrap(double value, double max)
        {
            var result = value % max;
            return result < 0 ? result + max : result;
        }

        /// <summary>
        /// Wrap an index by some mode
        /// </summary>
        /// <param name="mode"></param>
        /// <param name="value"></param>
        /// <param name="max"></param>
        /// <param name="min"></param>
        /// <returns></returns>
        public static int WrapIndex(WrapMode mode, int value, int max)
        {
            switch (mode)
            {
                case WrapMode.Clamp:
                    return Mathf.Clamp(value, 0, max - 1);
                case WrapMode.Loop:
                    return MExtension.Wrap(value, max);
                case WrapMode.PingPong:
                    return (int)Mathf.PingPong(value, max - 1);
                default:
                    return value;
            }
        }

        public static float Wrap(WrapMode mode, int value, int max, int min = 0)
        {
            switch (mode)
            {
                case WrapMode.Clamp:
                    return Mathf.Clamp(value, min, max);
                case WrapMode.Loop:
                    return MExtension.Wrap(value, max, min);
                case WrapMode.PingPong:
                    return (int)Mathf.PingPong(value - min, max - min) + min;
                default:
                    return value;
            }
        }
        public static float Wrap(WrapMode mode, float value, float max, float min = 0)
        {
            switch (mode)
            {
                case WrapMode.Clamp:
                    return Mathf.Clamp(value, min, max - 1);
                case WrapMode.Loop:
                    return MExtension.Wrap(value, max, min);
                case WrapMode.PingPong:
                    return (int)Mathf.PingPong(value - min, max - min - 1) + min;
                default:
                    return value;
            }
        }

        #endregion
    }
}