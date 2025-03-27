using UnityEngine;

namespace Pancake.Common
{
    public static partial class C
    {
        public static Vector3 WithX(this Vector3 v, float x) => new(x, v.y, v.z);
        public static Vector3 WithX(this Vector3 v, Transform target) => new(target.position.x, v.y, v.z);
        public static Vector3 WithX(this Vector3 v, Vector3 target) => new(target.x, v.y, v.z);

        public static Vector3 SetX(this ref Vector3 v, float x)
        {
            v = new Vector3(x, v.y, v.z);
            return v;
        }

        public static Vector3 SetX(this ref Vector3 v, Vector3 target)
        {
            v = new Vector3(target.x, v.y, v.z);
            return v;
        }

        public static Vector3 WithY(this Vector3 v, float y) => new(v.x, y, v.z);
        public static Vector3 WithY(this Vector3 v, Transform target) => new(v.x, target.position.y, v.z);
        public static Vector3 WithY(this Vector3 v, Vector3 target) => new(v.x, target.y, v.z);

        public static Vector3 SetY(this ref Vector3 v, float y)
        {
            v = new Vector3(v.x, y, v.z);
            return v;
        }

        public static Vector3 SetY(this ref Vector3 v, Vector3 target)
        {
            v = new Vector3(v.x, target.y, v.z);
            return v;
        }

        public static Vector3 WithZ(this Vector3 v, float z) => new(v.x, v.y, z);
        public static Vector3 WithZ(this Vector3 v, Transform target) => new(v.x, v.y, target.position.z);
        public static Vector3 WithZ(this Vector3 v, Vector3 target) => new(v.x, v.y, target.z);

        public static Vector3 SetZ(this ref Vector3 v, float z)
        {
            v = new Vector3(v.x, v.y, z);
            return v;
        }

        public static Vector3 SetZ(this ref Vector3 v, Vector3 target)
        {
            v = new Vector3(v.x, v.y, target.z);
            return v;
        }

        public static Vector3 WithXY(this Vector3 v, float x, float y) => new(x, y, v.z);
        public static Vector3 WithXZ(this Vector3 v, float x, float z) => new(x, v.y, z);
        public static Vector3 WithYZ(this Vector3 v, float y, float z) => new(v.x, y, z);

        public static Vector3 SetXY(this ref Vector3 v, float x, float y)
        {
            v = new Vector3(x, y, v.z);
            return v;
        }

        public static Vector3 SetXZ(this ref Vector3 v, float x, float z)
        {
            v = new Vector3(x, v.y, z);
            return v;
        }

        public static Vector3 SetYZ(this ref Vector3 v, float y, float z)
        {
            v = new Vector3(v.x, y, z);
            return v;
        }

        public static Vector3Int WithX(this Vector3Int v, int x) => new(x, v.y, v.z);
        public static Vector3Int WithX(this Vector3Int v, Vector3Int target) => new(target.x, v.y, v.z);

        public static Vector3Int SetX(this ref Vector3Int v, int x)
        {
            v = new Vector3Int(x, v.y, v.z);
            return v;
        }

        public static Vector3Int SetX(this ref Vector3Int v, Vector3Int target)
        {
            v = new Vector3Int(target.x, v.y, v.z);
            return v;
        }

        public static Vector3Int WithY(this Vector3Int v, int y) => new(v.x, y, v.z);
        public static Vector3Int WithY(this Vector3Int v, Vector3Int target) => new(v.x, target.y, v.z);

        public static Vector3Int SetY(this ref Vector3Int v, int y)
        {
            v = new Vector3Int(v.x, y, v.z);
            return v;
        }

        public static Vector3Int SetY(this ref Vector3Int v, Vector3Int target)
        {
            v = new Vector3Int(v.x, target.y, v.z);
            return v;
        }

        public static Vector3Int WithZ(this Vector3Int v, int z) => new(v.x, v.y, z);
        public static Vector3Int WithZ(this Vector3Int v, Vector3Int target) => new(v.x, v.y, target.z);

        public static Vector3Int SetZ(this ref Vector3Int v, int z)
        {
            v = new Vector3Int(v.x, v.y, z);
            return v;
        }

        public static Vector3Int SetZ(this ref Vector3Int v, Vector3Int target)
        {
            v = new Vector3Int(v.x, v.y, target.z);
            return v;
        }
    }
}