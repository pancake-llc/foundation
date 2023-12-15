using System;
using System.Runtime.InteropServices;

namespace Pancake.Greenery
{
    using Unity.Mathematics;
    using UnityEngine;

    [Serializable, StructLayout(LayoutKind.Sequential)]
    public struct SpawnData
    {
        public float3 position;
        public float3 normal;
        public Color surfaceColor;
        public float sizeFactor;
        public Color color;

        public SpawnData(float3 position, float3 normal, Color surfaceColor, float sizeFactor, Color color)
        {
            this.position = position;
            this.normal = normal;
            this.surfaceColor = surfaceColor;
            this.sizeFactor = sizeFactor;
            this.color = color;
        }
    }
}