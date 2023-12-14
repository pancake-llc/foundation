#ifndef COMMONSTRUCTS_INCLUDED
#define COMMONSTRUCTS_INCLUDED

struct DrawVertex
{
    float3 positionWS;
    float2 uv;
    float4 surfaceColor;
    float3 surfaceNormal;
    float3 normal;
    float4 color;
};

struct DrawTriangle
{
    DrawVertex vertices[3];
};

#endif
