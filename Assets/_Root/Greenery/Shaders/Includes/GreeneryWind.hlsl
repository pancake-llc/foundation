#ifndef GREENERYWIND_INCLUDED
#define GREENERYWIND_INCLUDED

#include "GreeneryUtilities.hlsl"

sampler2D _GLOBAL_GreeneryWindTexture;
float2 _GLOBAL_GreeneryWindTextureScale;
float2 _GLOBAL_GreeneryWindDirection;
float4 _GLOBAL_GreeneryFrequencyFactors;
float _GLOBAL_GreeneryWindStrength;
float _GLOBAL_GreeneryWindSpeed;

float4 SampleWindTexture(float3 positionWS)
{
    float2 uv = positionWS.xz * _GLOBAL_GreeneryWindTextureScale + _Time.y * _GLOBAL_GreeneryWindDirection * _GLOBAL_GreeneryWindSpeed;
    float angle = atan2(_GLOBAL_GreeneryWindDirection.x, _GLOBAL_GreeneryWindDirection.y);
    uv = rotateUV(uv, angle);
    float4 wind = tex2Dlod(_GLOBAL_GreeneryWindTexture, float4(uv, 0, 0));
    wind *= _GLOBAL_GreeneryFrequencyFactors;
    return wind;
}

float4 WindTextureDebug(float3 positionWS)
{
    float2 uv = positionWS.xz * _GLOBAL_GreeneryWindTextureScale + _Time.y * _GLOBAL_GreeneryWindDirection * _GLOBAL_GreeneryWindSpeed;
    float angle = atan2(_GLOBAL_GreeneryWindDirection.x, _GLOBAL_GreeneryWindDirection.y);
    uv = rotateUV(uv, angle);
    float4 wind = tex2Dlod(_GLOBAL_GreeneryWindTexture, float4(uv, 0, 0));
    return wind;
}

float2 CalculateWind(float3 positionWS)
{
    float4 windValue = SampleWindTexture(positionWS);
    float wind = (windValue.x + windValue.y + windValue.z + windValue.a) * 0.25;
    return -_GLOBAL_GreeneryWindDirection * wind * _GLOBAL_GreeneryWindStrength;
}

#endif
