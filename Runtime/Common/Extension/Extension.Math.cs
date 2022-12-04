namespace Pancake
{
    using System;
    using System.Collections.Generic;
    using UnityEngine;

    public static partial class C
    {
        public static void ShiftEnum<T>(ref T val) where T : struct, IConvertible { val = GetNextEnum(val); }

        public static T GetNextEnum<T>(T val) where T : struct, IConvertible
        {
            var type = typeof(T);
            if (!type.IsEnum) return val;

            var enums = Enum.GetValues(type) as T[];
            if (enums != null && enums.Length > 0)
            {
                int index = Array.IndexOf(enums, val);
                if (index >= 0)
                {
                    index++;
                    if (index >= enums.Length) index = 0;
                    return enums[index];
                }

                return val;
            }

            return default;
        }

        /// <summary>
        /// return snapped value in a range. e.g. from 1 to 3, value 2.2547f, step 10 => 2.2
        /// </summary>
        public static float StepSnap(float from, float to, float value, int steps)
        {
            var ratio = Mathf.InverseLerp(from, to, value);
            ratio = Mathf.Round(ratio * (steps - 1)) / (steps - 1);
            return Mathf.Lerp(from, to, ratio);
        }

        // ReSharper disable once CompareOfFloatsByEqualityOperator
        public static float InverseLerpUnclamped(float a, float b, float x) { return b != a ? (x - a) / (b - a) : float.PositiveInfinity * (x - a); }

        /// <summary>
        /// count must be >=0, return from 0->count-1
        /// </summary>
        public static int Repeat(int value, int count)
        {
            if (count <= 0) return 0;
            while (value < 0) value += count;
            if (value < count) return value;
            return value % count;
        }

        /// <summary>
        /// pingpong with the ends happening half time, max must be >=0, return steps ...,2,1,0,1,2,...,max-1,max,max-1,...,2,1,0,1,2,...
        /// </summary>
        public static float PingPong(float value, int max)
        {
            if (max <= 0) return 0;
            var group = Mathf.CeilToInt(value / max);
            if (group % 2 == 0)
            {
                return max * group - value;
            }

            return value - max * (@group - 1);
        }

        /// <summary>
        /// Think of a simple jump trajectory / bell parabola. Highest point has height of <paramref name="topHeight"/>, jump is normalized from x=0->x=1.
        /// Evaluate the height at any given normalized <paramref name="x"/>
        /// </summary>
        public static float GetNormalizedBellHeight(float topHeight, float x) { return -topHeight * 4 * Mathf.Pow(x - 0.5f, 2) + topHeight; }

        public static bool CalculateIdealCount(float availableSpace, float minSize, float maxSize, int defaultCount, out int count, out float size)
        {
            int minCount = Mathf.FloorToInt(availableSpace / maxSize);
            int maxCount = Mathf.FloorToInt(availableSpace / minSize);
            bool goodness = defaultCount >= minCount && defaultCount <= maxCount;
            count = Mathf.Clamp(defaultCount, minCount, maxCount);
            size = availableSpace / count;
            return goodness;
        }

        #region Matrix

        public static Vector3 Position(this Matrix4x4 m) { return m.GetColumn(3); }

        public static Vector3 Scale(this Matrix4x4 m) { return new Vector3(m.GetColumn(0).magnitude, m.GetColumn(1).magnitude, m.GetColumn(2).magnitude); }

        #endregion

        #region Vector3

        /// <summary>
        /// is vector equal zero?
        /// </summary>
        /// <param name="v"></param>
        /// <returns></returns>
        public static bool IsZero(this Vector3 v) => v.x == 0 && v.y == 0 && v.z == 0;

        public static Vector3 Flat(this Vector3 v) { return new Vector3(v.x, 0, v.z); }

        public static Vector3 Multiply(this Vector3 v, float x, float y, float z) { return v.Multiply(new Vector3(x, y, z)); }

        public static Vector3 Multiply(this Vector3 v, Vector3 other) { return Vector3.Scale(v, other); }

        public static Vector3 Divide(this Vector3 v, Vector3 other) { return Vector3.Scale(v, new Vector3(1 / other.x, 1 / other.y, 1 / other.z)); }

        public static Vector3Int Rounded(this Vector3 v) { return new Vector3Int(Mathf.RoundToInt(v.x), Mathf.RoundToInt(v.y), Mathf.RoundToInt(v.z)); }

        public static Vector3Int Floored(this Vector3 v) { return new Vector3Int(Mathf.FloorToInt(v.x), Mathf.FloorToInt(v.y), Mathf.FloorToInt(v.z)); }

        public static Vector3Int Ceiled(this Vector3 v) { return new Vector3Int(Mathf.CeilToInt(v.x), Mathf.CeilToInt(v.y), Mathf.CeilToInt(v.z)); }

        #endregion

        #region Vector2

        public static Vector2 Multiply(this Vector2 v, float x, float y) { return v.Multiply(new Vector2(x, y)); }

        public static Vector2 Multiply(this Vector2 v, Vector2 other) { return Vector2.Scale(v, other); }

        public static Vector2 Divide(this Vector2 v, Vector2 other) { return Vector2.Scale(v, new Vector2(1 / other.x, 1 / other.y)); }

        public static Vector2Int Rounded(this Vector2 v) { return new Vector2Int(Mathf.RoundToInt(v.x), Mathf.RoundToInt(v.y)); }

        public static Vector2Int Floored(this Vector2 v) { return new Vector2Int(Mathf.FloorToInt(v.x), Mathf.FloorToInt(v.y)); }

        public static Vector2Int Ceiled(this Vector2 v) { return new Vector2Int(Mathf.CeilToInt(v.x), Mathf.CeilToInt(v.y)); }

        #endregion

        #region Rect

        public static Vector2[] GetCorners(this Rect r)
        {
            var c1 = new Vector2(r.xMin, r.yMax);
            var c2 = new Vector2(r.xMax, r.yMin);
            return new[] {r.min, c1, r.max, c2};
        }

        public static Rect AsRect(this Bounds bound) { return new Rect(bound.min, bound.size); }

        public static bool Contains(this Rect r, Rect smaller) { return r.Contains(smaller.min) && r.Contains(smaller.max); }

        public static Vector2 Clamp(this Rect r, Vector2 v)
        {
            v.x = Mathf.Clamp(v.x, r.xMin, r.xMax);
            v.y = Mathf.Clamp(v.y, r.yMin, r.yMax);
            return v;
        }

        public static Vector2 InnerLerp(this Rect r, float x, float y) { return new Vector2(Mathf.Lerp(r.xMin, r.xMax, x), Mathf.Lerp(r.yMin, r.yMax, y)); }

        public static Rect InnerLerp(this Rect r, float xMin, float yMin, float xMax, float yMax)
        {
            var min = r.InnerLerp(xMin, yMin);
            var max = r.InnerLerp(xMax, yMax);
            return new Rect(min, max - min);
        }

        public static Vector2 RandomPosition(this Rect r) { return r.InnerLerp(UnityEngine.Random.value, UnityEngine.Random.value); }

        public static Rect FromCenter(this Rect r, Vector2 center, Vector2 size) { return new Rect(center - size / 2f, size); }

        public static Rect Scale(this Rect r, float f) { return r.Scale(f, f); }

        public static Rect Scale(this Rect r, float fx, float fy) { return r.FromCenter(r.center, Vector2.Scale(r.size, new Vector2(fx, fy))); }

        public static Rect Normalized(this Rect r, float fx, float fy) { return new Rect(r.x / fx, r.y / fy, r.width / fx, r.height / fy); }

        public static Rect Grown(this Rect r, float f) { return r.Grown(Vector2.one * f); }

        public static Rect Grown(this Rect r, Vector2 half) { return new Rect(r.position - half, r.size + half * 2); }

        // left to right, bottom to top
        public static Rect[] Split(this Rect rect, int cols = 1, int rows = 1)
        {
            rows = Mathf.Max(1, rows);
            cols = Mathf.Max(1, cols);

            Vector2 size = new Vector2(rect.width / cols, rect.height / rows);
            Rect[] rs = new Rect[rows * cols];

            for (int y = 0; y < rows; y++)
            {
                for (int x = 0; x < cols; x++)
                {
                    float cx = rect.position.x + (x + 0.5f) * size.x;
                    float cy = rect.position.y + (y + 0.5f) * size.y;

                    int index = y * cols + x;
                    rs[index] = rect.FromCenter(new Vector2(cx, cy), size);
                }
            }

            return rs;
        }

        // left to right, bottom to top
        public static Rect[,] Split2D(this Rect rect, int cols = 1, int rows = 1)
        {
            rows = Mathf.Max(1, rows);
            cols = Mathf.Max(1, cols);

            Vector2 size = new Vector2(rect.width / cols, rect.height / rows);
            Rect[,] rs = new Rect[cols, rows];

            for (int y = 0; y < rows; y++)
            {
                for (int x = 0; x < cols; x++)
                {
                    float cx = rect.position.x + (x + 0.5f) * size.x;
                    float cy = rect.position.y + (y + 0.5f) * size.y;

                    //int index = y * cols + x;
                    rs[x, y] = rect.FromCenter(new Vector2(cx, cy), size);
                }
            }

            return rs;
        }

        #endregion

        #region Bounds

        /// <summary>
        /// Interpolate a position inside a bounds from 3d normalized coordinates. (x,y,z) 0->1
        /// </summary>
        public static Vector3 InnerLerp(this Bounds r, float x, float y, float z)
        {
            Vector3 min = r.min;
            Vector3 max = r.max;
            return new Vector3(Mathf.Lerp(min.x, max.x, x), Mathf.Lerp(min.y, max.y, y), Mathf.Lerp(min.z, max.z, z));
        }

        /// <summary>
        /// Return the normalized & localized position of "pos" inside bounds
        /// </summary>
        public static Vector3 InnerInverseLerp(this Bounds r, Vector3 pos, bool clamped = false)
        {
            Vector3 outvec = Vector3.zero;
            Vector3 min = r.min;
            Vector3 max = r.max;
            outvec.x = clamped ? Mathf.InverseLerp(min.x, max.x, pos.x) : InverseLerpUnclamped(min.x, max.x, pos.x);
            outvec.y = clamped ? Mathf.InverseLerp(min.y, max.y, pos.y) : InverseLerpUnclamped(min.y, max.y, pos.y);
            outvec.z = clamped ? Mathf.InverseLerp(min.z, max.z, pos.z) : InverseLerpUnclamped(min.z, max.z, pos.z);
            return outvec;
        }

        /// <summary>
        /// Get a random position inside a bounds
        /// </summary>
        public static Vector3 RandomPosition(this Bounds r) { return r.InnerLerp(UnityEngine.Random.value, UnityEngine.Random.value, UnityEngine.Random.value); }

        /// <summary>
        /// Calculate the 6 planes making up the bounds. With normals pointing out or in.
        /// </summary>
        public static Plane[] GetPlanes(this Bounds b, bool normalsInside = false)
        {
            float f = normalsInside ? -1 : 1;

            Plane[] pls = new Plane[6];
            pls[0] = new Plane(Vector3.left * f, b.min);
            pls[1] = new Plane(Vector3.back * f, b.min);
            pls[2] = new Plane(Vector3.down * f, b.min);
            pls[3] = new Plane(Vector3.right * f, b.max);
            pls[4] = new Plane(Vector3.forward * f, b.max);
            pls[5] = new Plane(Vector3.up * f, b.max);
            return pls;
        }

        /// <summary>
        /// Calculate the intersection of a ray from inside the bounds, if possible.
        /// </summary>
        /// <returns>The multiplier or the ray direction</returns>
        public static bool IntersectRayFromInside(this Bounds b, Ray ray, out float dist)
        {
            // use planes.raycast
            dist = 0;
            if (!b.Contains(ray.origin)) return false;

            var planes = b.GetPlanes(true);
            float d = -1;
            foreach (var plane in planes)
            {
                if (plane.Raycast(ray, out float dp))
                {
                    if (d < 0 || d > dp) d = dp;
                }
            }

            if (d >= 0) dist = d;

            return true;
        }

        /// <summary>
        /// Expand bounds so that it contains vector
        /// </summary>
        public static void Encapsulate(this ref BoundsInt b, Vector3Int v)
        {
            var min = Vector3Int.Min(v, b.min);
            var max = Vector3Int.Min(v, b.max);
            b.SetMinMax(min, max);
        }

        /// <summary>
        /// Expand the bounds equally on 3 sides
        /// </summary>
        public static void Expand(this ref BoundsInt b, int i)
        {
            b.max += Vector3Int.one * i;
            b.min -= Vector3Int.one * i;
        }

        /// <summary>
        /// Expand the bounds each min and max by vector
        /// </summary>
        public static void Expand(this ref BoundsInt b, Vector3Int i)
        {
            b.max += i;
            b.min -= i;
        }

        /// <summary>
        /// Get the corners of the bounds
        /// </summary>
        public static List<Vector3> GetCorners(this Bounds b)
        {
            var min = b.min;
            var max = b.max;
            var ps = new[] {min.x, max.x, min.y, max.y, min.z, max.z};
            List<Vector3> l = new List<Vector3>();
            for (int ix = 0; ix <= 1; ix++)
            for (int iy = 2; iy <= 3; iy++)
            for (int iz = 4; iz <= 5; iz++)
            {
                l.Add(new Vector3(ps[ix], ps[iy], ps[iz]));
            }

            return l;
        }

        /// <summary>
        /// Get the corners of the bounds
        /// </summary>
        public static List<Vector3Int> GetCorners(this BoundsInt b)
        {
            var min = b.min;
            var max = b.max;
            var ps = new[] {min.x, max.x, min.y, max.y, min.z, max.z};
            List<Vector3Int> l = new List<Vector3Int>();
            for (int ix = 0; ix <= 1; ix++)
            for (int iy = 2; iy <= 3; iy++)
            for (int iz = 4; iz <= 5; iz++)
            {
                l.Add(new Vector3Int(ps[ix], ps[iy], ps[iz]));
            }

            return l;
        }


        /// <summary>
        /// Check if bounds B fully contains Another
        /// </summary>
        public static bool Contains(this Bounds b, Bounds another) { return b.Contains(another.min) && b.Contains(another.max); }

        #endregion

        #region M

        /// <summary>
        /// Internal method used to compute the spring velocity
        /// </summary>
        /// <param name="currentValue"></param>
        /// <param name="targetValue"></param>
        /// <param name="velocity"></param>
        /// <param name="damping"></param>
        /// <param name="frequency"></param>
        /// <param name="deltaTime"></param>
        /// <returns></returns>
        private static float SpringVelocity(float currentValue, float targetValue, float velocity, float damping, float frequency, float deltaTime)
        {
            frequency = frequency * 2f * Mathf.PI;
            return velocity + deltaTime * frequency * frequency * (targetValue - currentValue) + -2.0f * deltaTime * frequency * damping * velocity;
        }

        /// <summary>
        /// Springs a float towards a target value 
        /// </summary>
        /// <param name="currentValue">the current value to spring, passed as a ref</param>
        /// <param name="targetValue">the target value we're aiming for</param>
        /// <param name="velocity">a velocity value, passed as ref, used to compute the current speed of the springed value</param>
        /// <param name="damping">the damping, between 0.01f and 1f, the higher the daming, the less springy it'll be</param>
        /// <param name="frequency">the frequency, in Hz, so the amount of periods the spring should go over in 1 second</param>
        /// <param name="speed">the speed (between 0 and 1) at which the spring should operate</param>
        /// <param name="deltaTime">the delta time (usually Time.deltaTime or Time.unscaledDeltaTime)</param>
        public static void Spring(ref float currentValue, float targetValue, ref float velocity, float damping, float frequency, float speed, float deltaTime)
        {
            float initialVelocity = velocity;
            velocity = SpringVelocity(currentValue,
                targetValue,
                velocity,
                damping,
                frequency,
                deltaTime);
            velocity = Lerp(initialVelocity, velocity, speed, Time.deltaTime);
            currentValue += deltaTime * velocity;
        }

        /// <summary>
        /// Springs a Vector2 towards a target value 
        /// </summary>
        /// <param name="currentValue">the current value to spring, passed as a ref</param>
        /// <param name="targetValue">the target value we're aiming for</param>
        /// <param name="velocity">a velocity value, passed as ref, used to compute the current speed of the springed value</param>
        /// <param name="damping">the damping, between 0.01f and 1f, the higher the daming, the less springy it'll be</param>
        /// <param name="frequency">the frequency, in Hz, so the amount of periods the spring should go over in 1 second</param>
        /// <param name="speed">the speed (between 0 and 1) at which the spring should operate</param>
        /// <param name="deltaTime">the delta time (usually Time.deltaTime or Time.unscaledDeltaTime)</param>
        public static void Spring(ref Vector2 currentValue, Vector2 targetValue, ref Vector2 velocity, float damping, float frequency, float speed, float deltaTime)
        {
            Vector2 initialVelocity = velocity;
            velocity.x = SpringVelocity(currentValue.x,
                targetValue.x,
                velocity.x,
                damping,
                frequency,
                deltaTime);
            velocity.y = SpringVelocity(currentValue.y,
                targetValue.y,
                velocity.y,
                damping,
                frequency,
                deltaTime);
            velocity.x = Lerp(initialVelocity.x, velocity.x, speed, Time.deltaTime);
            velocity.y = Lerp(initialVelocity.y, velocity.y, speed, Time.deltaTime);
            currentValue += deltaTime * velocity;
        }

        /// <summary>
        /// Springs a Vector3 towards a target value 
        /// </summary>
        /// <param name="currentValue">the current value to spring, passed as a ref</param>
        /// <param name="targetValue">the target value we're aiming for</param>
        /// <param name="velocity">a velocity value, passed as ref, used to compute the current speed of the springed value</param>
        /// <param name="damping">the damping, between 0.01f and 1f, the higher the daming, the less springy it'll be</param>
        /// <param name="frequency">the frequency, in Hz, so the amount of periods the spring should go over in 1 second</param>
        /// <param name="speed">the speed (between 0 and 1) at which the spring should operate</param>
        /// <param name="deltaTime">the delta time (usually Time.deltaTime or Time.unscaledDeltaTime)</param>
        public static void Spring(ref Vector3 currentValue, Vector3 targetValue, ref Vector3 velocity, float damping, float frequency, float speed, float deltaTime)
        {
            Vector3 initialVelocity = velocity;
            velocity.x = SpringVelocity(currentValue.x,
                targetValue.x,
                velocity.x,
                damping,
                frequency,
                deltaTime);
            velocity.y = SpringVelocity(currentValue.y,
                targetValue.y,
                velocity.y,
                damping,
                frequency,
                deltaTime);
            velocity.z = SpringVelocity(currentValue.z,
                targetValue.z,
                velocity.z,
                damping,
                frequency,
                deltaTime);
            velocity.x = Lerp(initialVelocity.x, velocity.x, speed, Time.deltaTime);
            velocity.y = Lerp(initialVelocity.y, velocity.y, speed, Time.deltaTime);
            velocity.z = Lerp(initialVelocity.z, velocity.z, speed, Time.deltaTime);
            currentValue += deltaTime * velocity;
        }

        /// <summary>
        /// Springs a Vector4 towards a target value 
        /// </summary>
        /// <param name="currentValue">the current value to spring, passed as a ref</param>
        /// <param name="targetValue">the target value we're aiming for</param>
        /// <param name="velocity">a velocity value, passed as ref, used to compute the current speed of the springed value</param>
        /// <param name="damping">the damping, between 0.01f and 1f, the higher the daming, the less springy it'll be</param>
        /// <param name="frequency">the frequency, in Hz, so the amount of periods the spring should go over in 1 second</param>
        /// <param name="speed">the speed (between 0 and 1) at which the spring should operate</param>
        /// <param name="deltaTime">the delta time (usually Time.deltaTime or Time.unscaledDeltaTime)</param>
        public static void Spring(ref Vector4 currentValue, Vector4 targetValue, ref Vector4 velocity, float damping, float frequency, float speed, float deltaTime)
        {
            Vector4 initialVelocity = velocity;
            velocity.x = SpringVelocity(currentValue.x,
                targetValue.x,
                velocity.x,
                damping,
                frequency,
                deltaTime);
            velocity.y = SpringVelocity(currentValue.y,
                targetValue.y,
                velocity.y,
                damping,
                frequency,
                deltaTime);
            velocity.z = SpringVelocity(currentValue.z,
                targetValue.z,
                velocity.z,
                damping,
                frequency,
                deltaTime);
            velocity.w = SpringVelocity(currentValue.w,
                targetValue.w,
                velocity.w,
                damping,
                frequency,
                deltaTime);
            velocity.x = Lerp(initialVelocity.x, velocity.x, speed, Time.deltaTime);
            velocity.y = Lerp(initialVelocity.y, velocity.y, speed, Time.deltaTime);
            velocity.z = Lerp(initialVelocity.z, velocity.z, speed, Time.deltaTime);
            velocity.w = Lerp(initialVelocity.w, velocity.w, speed, Time.deltaTime);
            currentValue += deltaTime * velocity;
        }

        /// <summary>
        /// internal method used to determine the lerp rate
        /// </summary>
        /// <param name="rate"></param>
        /// <param name="deltaTime"></param>
        /// <returns></returns>
        private static float LerpRate(float rate, float deltaTime)
        {
            rate = Mathf.Clamp01(rate);
            float invRate = -Mathf.Log(1.0f - rate, 2.0f) * 60f;
            return Mathf.Pow(2.0f, -invRate * deltaTime);
        }

        /// <summary>
        /// Lerps a float towards a target at the specified rate
        /// </summary>
        /// <param name="value"></param>
        /// <param name="target"></param>
        /// <param name="rate"></param>
        /// <param name="deltaTime"></param>
        /// <returns></returns>
        public static float Lerp(float value, float target, float rate, float deltaTime)
        {
            if (deltaTime == 0f)
            {
                return value;
            }

            return Mathf.Lerp(target, value, LerpRate(rate, deltaTime));
        }

        /// <summary>
        /// Lerps a Vector2 towards a target at the specified rate
        /// </summary>
        /// <param name="value"></param>
        /// <param name="target"></param>
        /// <param name="rate"></param>
        /// <param name="deltaTime"></param>
        /// <returns></returns>
        public static Vector2 Lerp(Vector2 value, Vector2 target, float rate, float deltaTime)
        {
            if (deltaTime == 0f)
            {
                return value;
            }

            return Vector2.Lerp(target, value, LerpRate(rate, deltaTime));
        }

        /// <summary>
        /// Lerps a Vector3 towards a target at the specified rate
        /// </summary>
        /// <param name="value"></param>
        /// <param name="target"></param>
        /// <param name="rate"></param>
        /// <param name="deltaTime"></param>
        /// <returns></returns>
        public static Vector3 Lerp(Vector3 value, Vector3 target, float rate, float deltaTime)
        {
            if (deltaTime == 0f)
            {
                return value;
            }

            return Vector3.Lerp(target, value, LerpRate(rate, deltaTime));
        }

        /// <summary>
        /// Lerps a Vector4 towards a target at the specified rate
        /// </summary>
        /// <param name="value"></param>
        /// <param name="target"></param>
        /// <param name="rate"></param>
        /// <param name="deltaTime"></param>
        /// <returns></returns>
        public static Vector4 Lerp(Vector4 value, Vector4 target, float rate, float deltaTime)
        {
            if (deltaTime == 0f)
            {
                return value;
            }

            return Vector4.Lerp(target, value, LerpRate(rate, deltaTime));
        }

        /// <summary>
        /// Lerps a Quaternion towards a target at the specified rate
        /// </summary>
        /// <param name="value"></param>
        /// <param name="target"></param>
        /// <param name="rate"></param>
        /// <param name="deltaTime"></param>
        /// <returns></returns>
        public static Quaternion Lerp(Quaternion value, Quaternion target, float rate, float deltaTime)
        {
            if (deltaTime == 0f)
            {
                return value;
            }

            return Quaternion.Lerp(target, value, LerpRate(rate, deltaTime));
        }

        /// <summary>
        /// Lerps a Color towards a target at the specified rate
        /// </summary>
        /// <param name="value"></param>
        /// <param name="target"></param>
        /// <param name="rate"></param>
        /// <param name="deltaTime"></param>
        /// <returns></returns>
        public static Color Lerp(Color value, Color target, float rate, float deltaTime)
        {
            if (deltaTime == 0f)
            {
                return value;
            }

            return Color.Lerp(target, value, LerpRate(rate, deltaTime));
        }

        /// <summary>
        /// Lerps a Color32 towards a target at the specified rate
        /// </summary>
        /// <param name="value"></param>
        /// <param name="target"></param>
        /// <param name="rate"></param>
        /// <param name="deltaTime"></param>
        /// <returns></returns>
        public static Color32 Lerp(Color32 value, Color32 target, float rate, float deltaTime)
        {
            if (deltaTime == 0f)
            {
                return value;
            }

            return Color32.Lerp(target, value, LerpRate(rate, deltaTime));
        }

        /// <summary>
        /// Clamps a float between min and max, both bounds being optional and driven by clampMin and clampMax respectively
        /// </summary>
        /// <param name="value"></param>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <param name="clampMin"></param>
        /// <param name="clampMax"></param>
        /// <returns></returns>
        public static float Clamp(float value, float min, float max, bool clampMin, bool clampMax)
        {
            float returnValue = value;
            if (clampMin && returnValue < min)
            {
                returnValue = min;
            }

            if (clampMax && returnValue > max)
            {
                returnValue = max;
            }

            return returnValue;
        }

        /// <summary>
        /// Rounds a float to the nearest half value : 1, 1.5, 2, 2.5 etc
        /// </summary>
        /// <param name="a"></param>
        /// <returns></returns>
        public static float RoundToNearestHalf(float a) { return a - a % 0.5f; }

        /// <summary>
        /// Takes a Vector3 and turns it into a Vector2
        /// </summary>
        /// <returns>The vector2.</returns>
        /// <param name="target">The Vector3 to turn into a Vector2.</param>
        public static Vector2 Vector3ToVector2(Vector3 target) { return new Vector2(target.x, target.y); }

        /// <summary>
        /// Takes a Vector2 and turns it into a Vector3 with a null z value
        /// </summary>
        /// <returns>The vector3.</returns>
        /// <param name="target">The Vector2 to turn into a Vector3.</param>
        public static Vector3 Vector2ToVector3(Vector2 target) { return new Vector3(target.x, target.y, 0); }

        /// <summary>
        /// Takes a Vector2 and turns it into a Vector3 with the specified z value 
        /// </summary>
        /// <returns>The vector3.</returns>
        /// <param name="target">The Vector2 to turn into a Vector3.</param>
        /// <param name="newZValue">New Z value.</param>
        public static Vector3 Vector2ToVector3(Vector2 target, float newZValue) { return new Vector3(target.x, target.y, newZValue); }

        /// <summary>
        /// Rounds all components of a Vector3.
        /// </summary>
        /// <returns>The vector3.</returns>
        /// <param name="vector">Vector.</param>
        public static Vector3 RoundVector3(Vector3 vector) { return new Vector3(Mathf.Round(vector.x), Mathf.Round(vector.y), Mathf.Round(vector.z)); }

        /// <summary>
        /// Returns a random Vector2 from 2 defined Vector2.
        /// </summary>
        /// <returns>The random Vector2.</returns>
        /// <param name="minimum">Minimum.</param>
        /// <param name="maximum">Maximum.</param>
        public static Vector2 RandomVector2(Vector2 minimum, Vector2 maximum)
        {
            return new Vector2(UnityEngine.Random.Range(minimum.x, maximum.x), UnityEngine.Random.Range(minimum.y, maximum.y));
        }

        /// <summary>
        /// Returns a random Vector3 from 2 defined Vector3.
        /// </summary>
        /// <returns>The random Vector3.</returns>
        /// <param name="minimum">Minimum.</param>
        /// <param name="maximum">Maximum.</param>
        public static Vector3 RandomVector3(Vector3 minimum, Vector3 maximum)
        {
            return new Vector3(UnityEngine.Random.Range(minimum.x, maximum.x),
                UnityEngine.Random.Range(minimum.y, maximum.y),
                UnityEngine.Random.Range(minimum.z, maximum.z));
        }

        /// <summary>
        /// Returns a random point on the circle of the specified radius
        /// </summary>
        /// <param name="circleRadius"></param>
        /// <returns></returns>
        public static Vector2 RandomPointOnCircle(float circleRadius) { return UnityEngine.Random.insideUnitCircle.normalized * circleRadius; }

        /// <summary>
        /// Returns a random point on the sphere of the specified radius
        /// </summary>
        /// <param name="sphereRadius"></param>
        /// <returns></returns>
        public static Vector3 RandomPointOnSphere(float sphereRadius) { return UnityEngine.Random.onUnitSphere * sphereRadius; }

        /// <summary>
        /// Rotates a point around the given pivot.
        /// </summary>
        /// <returns>The new point position.</returns>
        /// <param name="point">The point to rotate.</param>
        /// <param name="pivot">The pivot's position.</param>
        /// <param name="angle">The angle we want to rotate our point.</param>
        public static Vector3 RotatePointAroundPivot(Vector3 point, Vector3 pivot, float angle)
        {
            angle = angle * (Mathf.PI / 180f);
            float rotatedX = Mathf.Cos(angle) * (point.x - pivot.x) - Mathf.Sin(angle) * (point.y - pivot.y) + pivot.x;
            float rotatedY = Mathf.Sin(angle) * (point.x - pivot.x) + Mathf.Cos(angle) * (point.y - pivot.y) + pivot.y;
            return new Vector3(rotatedX, rotatedY, 0);
        }

        /// <summary>
        /// Rotates a point around the given pivot.
        /// </summary>
        /// <returns>The new point position.</returns>
        /// <param name="point">The point to rotate.</param>
        /// <param name="pivot">The pivot's position.</param>
        /// <param name="angle">The angle as a Vector3.</param>
        public static Vector3 RotatePointAroundPivot(Vector3 point, Vector3 pivot, Vector3 angle)
        {
            // we get point direction from the point to the pivot
            Vector3 direction = point - pivot;
            // we rotate the direction
            direction = Quaternion.Euler(angle) * direction;
            // we determine the rotated point's position
            point = direction + pivot;
            return point;
        }

        /// <summary>
        /// Rotates a point around the given pivot.
        /// </summary>
        /// <returns>The new point position.</returns>
        /// <param name="point">The point to rotate.</param>
        /// <param name="pivot">The pivot's position.</param>
        /// <param name="quaternion">The angle as a Vector3.</param>
        public static Vector3 RotatePointAroundPivot(Vector3 point, Vector3 pivot, Quaternion quaternion)
        {
            // we get point direction from the point to the pivot
            Vector3 direction = point - pivot;
            // we rotate the direction
            direction = quaternion * direction;
            // we determine the rotated point's position
            point = direction + pivot;
            return point;
        }

        /// <summary>
        /// Rotates a vector2 by the angle (in degrees) specified and returns it
        /// </summary>
        /// <returns>The rotated Vector2.</returns>
        /// <param name="vector">The vector to rotate.</param>
        /// <param name="angle">Degrees.</param>
        public static Vector2 RotateVector2(Vector2 vector, float angle)
        {
            if (angle == 0)
            {
                return vector;
            }

            float sinus = Mathf.Sin(angle * Mathf.Deg2Rad);
            float cosinus = Mathf.Cos(angle * Mathf.Deg2Rad);

            float oldX = vector.x;
            float oldY = vector.y;
            vector.x = cosinus * oldX - sinus * oldY;
            vector.y = sinus * oldX + cosinus * oldY;
            return vector;
        }

        /// <summary>
        /// Computes and returns the angle between two vectors, on a 360° scale
        /// </summary>
        /// <returns>The <see cref="System.Single"/>.</returns>
        /// <param name="vectorA">Vector a.</param>
        /// <param name="vectorB">Vector b.</param>
        public static float AngleBetween(Vector2 vectorA, Vector2 vectorB)
        {
            float angle = Vector2.Angle(vectorA, vectorB);
            Vector3 cross = Vector3.Cross(vectorA, vectorB);

            if (cross.z > 0)
            {
                angle = 360 - angle;
            }

            return angle;
        }

        /// <summary>
        /// Returns the distance between a point and a line.
        /// </summary>
        /// <returns>The between point and line.</returns>
        /// <param name="point">Point.</param>
        /// <param name="lineStart">Line start.</param>
        /// <param name="lineEnd">Line end.</param>
        public static float DistanceBetweenPointAndLine(Vector3 point, Vector3 lineStart, Vector3 lineEnd)
        {
            return Vector3.Magnitude(ProjectPointOnLine(point, lineStart, lineEnd) - point);
        }

        /// <summary>
        /// Projects a point on a line (perpendicularly) and returns the projected point.
        /// </summary>
        /// <returns>The point on line.</returns>
        /// <param name="point">Point.</param>
        /// <param name="lineStart">Line start.</param>
        /// <param name="lineEnd">Line end.</param>
        public static Vector3 ProjectPointOnLine(Vector3 point, Vector3 lineStart, Vector3 lineEnd)
        {
            Vector3 rhs = point - lineStart;
            Vector3 vector2 = lineEnd - lineStart;
            float magnitude = vector2.magnitude;
            Vector3 lhs = vector2;
            if (magnitude > 1E-06f)
            {
                lhs /= magnitude;
            }

            float num2 = Mathf.Clamp(Vector3.Dot(lhs, rhs), 0f, magnitude);
            return lineStart + lhs * num2;
        }

        /// <summary>
        /// Returns a random success based on X% of chance.
        /// Example : I have 20% of chance to do X, Chance(20) > true, yay!
        /// </summary>
        /// <param name="percent">Percent of chance.</param>
        public static bool Chance(int percent) { return UnityEngine.Random.Range(0, 100) <= percent; }

        /// <summary>
        /// Moves from "from" to "to" by the specified amount and returns the corresponding value
        /// </summary>
        /// <param name="from">From.</param>
        /// <param name="to">To.</param>
        /// <param name="amount">Amount.</param>
        public static float Approach(float from, float to, float amount)
        {
            if (from < to)
            {
                from += amount;
                if (from > to)
                {
                    return to;
                }
            }
            else
            {
                from -= amount;
                if (from < to)
                {
                    return to;
                }
            }

            return from;
        }

        /// <summary>
        /// Clamps the angle in parameters between a minimum and maximum angle (all angles expressed in degrees)
        /// </summary>
        /// <param name="angle"></param>
        /// <param name="minimumAngle"></param>
        /// <param name="maximumAngle"></param>
        /// <returns></returns>
        public static float ClampAngle(float angle, float minimumAngle, float maximumAngle)
        {
            if (angle < -360)
            {
                angle += 360;
            }

            if (angle > 360)
            {
                angle -= 360;
            }

            return Mathf.Clamp(angle, minimumAngle, maximumAngle);
        }

        public static float RoundToDecimal(float value, int numberOfDecimals) { return Mathf.Round(value * 10f * numberOfDecimals) / (10f * numberOfDecimals); }

        /// <summary>
        /// Rounds the value passed in parameters to the closest value in the parameter array
        /// </summary>
        /// <param name="value"></param>
        /// <param name="possibleValues"></param>
        /// <param name="pickSmallestDistance"></param>
        /// <returns></returns>
        public static float RoundToClosest(float value, float[] possibleValues, bool pickSmallestDistance = false)
        {
            if (possibleValues.Length == 0)
            {
                return 0f;
            }

            float closestValue = possibleValues[0];

            foreach (float possibleValue in possibleValues)
            {
                float closestDistance = Mathf.Abs(closestValue - value);
                float possibleDistance = Mathf.Abs(possibleValue - value);

                if (closestDistance > possibleDistance)
                {
                    closestValue = possibleValue;
                }
                // ReSharper disable once CompareOfFloatsByEqualityOperator
                else if (closestDistance == possibleDistance)
                {
                    if (pickSmallestDistance && closestValue > possibleValue || !pickSmallestDistance && closestValue < possibleValue)
                    {
                        closestValue = value < 0 ? closestValue : possibleValue;
                    }
                }
            }

            return closestValue;
        }

        /// <summary>
        /// Returns a vector3 based on the angle in parameters
        /// </summary>
        /// <param name="angle"></param>
        /// <param name="additionalAngle"></param>
        /// <returns></returns>
        public static Vector3 DirectionFromAngle(float angle, float additionalAngle)
        {
            angle += additionalAngle;

            Vector3 direction = Vector3.zero;
            direction.x = Mathf.Sin(angle * Mathf.Deg2Rad);
            direction.y = 0f;
            direction.z = Mathf.Cos(angle * Mathf.Deg2Rad);
            return direction;
        }

        /// <summary>
        /// Returns a vector3 based on the angle in parameters
        /// </summary>
        /// <param name="angle"></param>
        /// <param name="additionalAngle"></param>
        /// <returns></returns>
        public static Vector3 DirectionFromAngle2D(float angle, float additionalAngle)
        {
            angle += additionalAngle;

            Vector3 direction = Vector3.zero;
            direction.x = Mathf.Cos(angle * Mathf.Deg2Rad);
            direction.y = Mathf.Sin(angle * Mathf.Deg2Rad);
            direction.z = 0f;
            return direction;
        }

        #endregion
    }
}