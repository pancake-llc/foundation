#ifndef MESHGENERATIONCOMMON_INCLUDED
#define MESHGENERATIONCOMMON_INCLUDED

#define PI            3.14159265359f
#define TWO_PI        6.28318530718f

#include "CommonStructs.hlsl"

struct SpawnData
{
    float3 positionOS;
    float3 normalOS;
    float4 surfaceColor;
    float sizeFactor;
    float4 color;
};

StructuredBuffer<SpawnData> _SpawnDataBuffer;
AppendStructuredBuffer<DrawTriangle> _DrawTrianglesBuffer;
float _ApplyCulling = 0.0;

struct IndirectArgs
{
    uint numVerticesPerInstance;
    uint numInstances;
    uint startVertexIndex;
    uint startInstanceIndex;
};

RWStructuredBuffer<IndirectArgs> _IndirectArgsBuffer;

int _NumSpawnData;
float4x4 _LocalToWorld;
float4x4 _VPMatrix;
float _MaxDistance;
float _MinFadeDistance;

float random(float2 st, float2 dotDir = float2(12.9898, 78.233))
{
    return frac(sin(dot(st.xy, dotDir)) * 43758.5453123);
}

float2 random2d(float2 value)
{
    return float2(
        random(value, float2(12.989, 78.233)),
        random(value, float2(39.346, 11.135))
    );
}

float invLerp(float a, float b, float t)
{
    return (t - a) / (b - a);
}

float3 RotateAboutAxis(float3 In, float3 Axis, float Rotation)
{
    float s = sin(Rotation);
    float c = cos(Rotation);
    float one_minus_c = 1.0 - c;

    Axis = normalize(Axis);
    float3x3 rot_mat =
    {
        one_minus_c * Axis.x * Axis.x + c, one_minus_c * Axis.x * Axis.y - Axis.z * s, one_minus_c * Axis.z * Axis.x + Axis.y * s,
        one_minus_c * Axis.x * Axis.y + Axis.z * s, one_minus_c * Axis.y * Axis.y + c, one_minus_c * Axis.y * Axis.z - Axis.x * s,
        one_minus_c * Axis.z * Axis.x - Axis.y * s, one_minus_c * Axis.y * Axis.z + Axis.x * s, one_minus_c * Axis.z * Axis.z + c
    };
    return mul(rot_mat, In);
}

float4x4 rotX(float a)
{
    return transpose(float4x4(
        1, 0, 0, 0,
        0, cos(a), -sin(a), 0,
        0, sin(a), cos(a), 0,
        0, 0, 0, 1
    ));
}

float4x4 rotY(float a)
{
    return transpose(float4x4(
        cos(a), 0, sin(a), 0,
        0, 1, 0, 0,
        -sin(a), 0, cos(a), 0,
        0, 0, 0, 1
    ));
}


float4x4 rotZ(float a)
{
    return transpose(float4x4(
        cos(a), -sin(a), 0, 0,
        sin(a), cos(a), 0, 0,
        0, 0, 1, 0,
        0, 0, 0, 1
    ));
}


DrawVertex GetVertex(float3 positionOS, float2 uv, float4 surfaceColor, float3 surfaceNormal, float3 normal, float4 color)
{
    DrawVertex output = (DrawVertex)0;
    output.positionWS = mul(_LocalToWorld, float4(positionOS, 1)).xyz;
    output.uv = uv;
    output.surfaceColor = surfaceColor;
    output.normal = mul(_LocalToWorld, float4(normal, 0.0)).xyz;
    output.surfaceNormal = mul(_LocalToWorld, float4(surfaceNormal, 0.0)).xyz;
    output.color = color;
    return output;
}


bool FrustumCulling(float3 worldPos, float sizeFactor, float maxHeight, float maxWidth)
{
    //if (_ApplyCulling <= 0) return false;
    float4 absPosCS = abs(mul(_VPMatrix, float4(worldPos, 1.0)));
    return (absPosCS.z > absPosCS.w || absPosCS.y > absPosCS.w * 1.5 + maxHeight * sizeFactor || absPosCS.x > absPosCS.w * 1.5 + maxWidth * sizeFactor || absPosCS.w >
        _MaxDistance);
}

#endif
