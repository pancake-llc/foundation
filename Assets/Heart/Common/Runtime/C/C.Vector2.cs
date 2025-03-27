using UnityEngine;

namespace Pancake.Common
{
    public static partial class C
    {
        public static Vector2 WithX(this Vector2 vector, float x) => new(x, vector.y);
        public static Vector2 WithY(this Vector2 vector, float y) => new(vector.x, y);

        public static Vector2Int WithX(this Vector2Int vector, int x) => new(x, vector.y);
        public static Vector2Int WithY(this Vector2Int vector, int y) => new(vector.x, y);
    }
}