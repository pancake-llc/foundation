Shader "Greenery/GreeneryInstanceBillboard"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        [Normal]_Normal("Normal", 2D) = "bump" {}
        _NormalStrength("Normal strength", Range(-2,2)) = 1
        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0.0
        _SurfaceColorBlending("Surface color blending", float) = 0
        _WindStrengthFactor("Wind strength factor", Range(0, 1)) = 0
        _QuadSize("Quad size", float) = 0
        _Cutoff("Cutoff", Range(0,1)) = 0.5
    }
    SubShader
    {
        Cull Off
        Tags { "RenderType"="Opaque" }
        LOD 200

        CGPROGRAM
        #include "Includes/GreeneryWind.hlsl"
        #include "Includes/GreeneryUtilities.hlsl"

        #pragma surface surf Standard fullforwardshadows addshadow vertex:vert 
        #pragma multi_compile_instancing
        #pragma instancing_options procedural:setup

        #pragma target 3.0

        sampler2D _MainTex;

        struct SpawnData {
            float3 positionOS;
            float3 normalOS;
            float4 surfaceColor;
            float sizeFactor;
            float4 color;
        };

        struct Input
        {
            float2 uv_MainTex;
            float2 uv_Normal;
            float3 localPosition;
        };

        half _Glossiness;
        half _Metallic;
        fixed4 _Color;
        float4x4 _LocalToWorld;
        float _UseSurfaceNormals;
        float _NormalStrength;
        sampler2D _Normal;
        float _SurfaceColorBlending;
        float _WindStrengthFactor;
        float _QuadSize;
        float _Cutoff;

        float4x4 inverse(float4x4 m)
        {
            float n11 = m[0][0], n12 = m[1][0], n13 = m[2][0], n14 = m[3][0];
            float n21 = m[0][1], n22 = m[1][1], n23 = m[2][1], n24 = m[3][1];
            float n31 = m[0][2], n32 = m[1][2], n33 = m[2][2], n34 = m[3][2];
            float n41 = m[0][3], n42 = m[1][3], n43 = m[2][3], n44 = m[3][3];

            float t11 = n23 * n34 * n42 - n24 * n33 * n42 + n24 * n32 * n43 - n22 * n34 * n43 - n23 * n32 * n44 + n22 * n33 * n44;
            float t12 = n14 * n33 * n42 - n13 * n34 * n42 - n14 * n32 * n43 + n12 * n34 * n43 + n13 * n32 * n44 - n12 * n33 * n44;
            float t13 = n13 * n24 * n42 - n14 * n23 * n42 + n14 * n22 * n43 - n12 * n24 * n43 - n13 * n22 * n44 + n12 * n23 * n44;
            float t14 = n14 * n23 * n32 - n13 * n24 * n32 - n14 * n22 * n33 + n12 * n24 * n33 + n13 * n22 * n34 - n12 * n23 * n34;

            float det = n11 * t11 + n21 * t12 + n31 * t13 + n41 * t14;
            float idet = 1.0f / det;

            float4x4 ret;

            ret[0][0] = t11 * idet;
            ret[0][1] = (n24 * n33 * n41 - n23 * n34 * n41 - n24 * n31 * n43 + n21 * n34 * n43 + n23 * n31 * n44 - n21 * n33 * n44) * idet;
            ret[0][2] = (n22 * n34 * n41 - n24 * n32 * n41 + n24 * n31 * n42 - n21 * n34 * n42 - n22 * n31 * n44 + n21 * n32 * n44) * idet;
            ret[0][3] = (n23 * n32 * n41 - n22 * n33 * n41 - n23 * n31 * n42 + n21 * n33 * n42 + n22 * n31 * n43 - n21 * n32 * n43) * idet;

            ret[1][0] = t12 * idet;
            ret[1][1] = (n13 * n34 * n41 - n14 * n33 * n41 + n14 * n31 * n43 - n11 * n34 * n43 - n13 * n31 * n44 + n11 * n33 * n44) * idet;
            ret[1][2] = (n14 * n32 * n41 - n12 * n34 * n41 - n14 * n31 * n42 + n11 * n34 * n42 + n12 * n31 * n44 - n11 * n32 * n44) * idet;
            ret[1][3] = (n12 * n33 * n41 - n13 * n32 * n41 + n13 * n31 * n42 - n11 * n33 * n42 - n12 * n31 * n43 + n11 * n32 * n43) * idet;

            ret[2][0] = t13 * idet;
            ret[2][1] = (n14 * n23 * n41 - n13 * n24 * n41 - n14 * n21 * n43 + n11 * n24 * n43 + n13 * n21 * n44 - n11 * n23 * n44) * idet;
            ret[2][2] = (n12 * n24 * n41 - n14 * n22 * n41 + n14 * n21 * n42 - n11 * n24 * n42 - n12 * n21 * n44 + n11 * n22 * n44) * idet;
            ret[2][3] = (n13 * n22 * n41 - n12 * n23 * n41 - n13 * n21 * n42 + n11 * n23 * n42 + n12 * n21 * n43 - n11 * n22 * n43) * idet;

            ret[3][0] = t14 * idet;
            ret[3][1] = (n13 * n24 * n31 - n14 * n23 * n31 + n14 * n21 * n33 - n11 * n24 * n33 - n13 * n21 * n34 + n11 * n23 * n34) * idet;
            ret[3][2] = (n14 * n22 * n31 - n12 * n24 * n31 - n14 * n21 * n32 + n11 * n24 * n32 + n12 * n21 * n34 - n11 * n22 * n34) * idet;
            ret[3][3] = (n12 * n23 * n31 - n13 * n22 * n31 + n13 * n21 * n32 - n11 * n23 * n32 - n12 * n21 * n33 + n11 * n22 * n33) * idet;

            return ret;
        }

        #ifdef UNITY_PROCEDURAL_INSTANCING_ENABLED
            StructuredBuffer<float4x4> _TransformBuffer;
            StructuredBuffer<float3> _NormalBuffer;
            StructuredBuffer<float3> _SurfaceColorBuffer;
            StructuredBuffer<float3> _ColorBuffer;
            StructuredBuffer<uint> _VisibleInstanceIDBuffer;
        #endif

        void setup() {
            #ifdef UNITY_PROCEDURAL_INSTANCING_ENABLED
                float4x4 transform = mul(_LocalToWorld, _TransformBuffer[_VisibleInstanceIDBuffer[unity_InstanceID]]);
                unity_ObjectToWorld = transform;
                unity_WorldToObject = inverse(transform);
            #endif
        }

        float3 random3( float3 p ) {
            return frac(sin(float3(dot(p,float3(127.1,311.7, 185.3)),dot(p,float3(269.5,183.3, 198.2)), dot(p,float3(215.3,163.4, 305.2))))*43758.5453);
        }

        float random (float2 st) {
            return frac(sin(dot(st.xy,float2(12.9898,78.233)))*43758.5453123);
        }

        float3 rotateAboutAxis(float3 In, float3 Axis, float Rotation)
        {
            float s = sin(Rotation);
            float c = cos(Rotation);
            float one_minus_c = 1.0 - c;

            Axis = normalize(Axis);
            float3x3 rot_mat = 
            {   one_minus_c * Axis.x * Axis.x + c, one_minus_c * Axis.x * Axis.y - Axis.z * s, one_minus_c * Axis.z * Axis.x + Axis.y * s,
                one_minus_c * Axis.x * Axis.y + Axis.z * s, one_minus_c * Axis.y * Axis.y + c, one_minus_c * Axis.y * Axis.z - Axis.x * s,
                one_minus_c * Axis.z * Axis.x - Axis.y * s, one_minus_c * Axis.y * Axis.z + Axis.x * s, one_minus_c * Axis.z * Axis.z + c
            };
            return mul(rot_mat,  In);
        }

        void vert(inout appdata_full v, out Input o) {
            float rand = random(v.texcoord1);
            float2 uv = rotateUV(v.texcoord, rand * UNITY_TWO_PI);
            float3 offset = float3((uv * 2.0 - 1.0), 0.0);
            offset = mul(unity_ObjectToWorld, offset);
            offset = mul(UNITY_MATRIX_V, offset);
            offset = normalize(offset);
            v.vertex.xyz += offset * _QuadSize * rand;
            #ifdef UNITY_PROCEDURAL_INSTANCING_ENABLED
                float3 position = _TransformBuffer[_VisibleInstanceIDBuffer[unity_InstanceID]]._m03_m13_m23;
                float3 normal = _NormalBuffer[_VisibleInstanceIDBuffer[unity_InstanceID]];
                v.normal = _UseSurfaceNormals ? normal : v.normal;
                float2 wind = CalculateWind(mul(unity_ObjectToWorld, position));
                v.vertex.xz += wind * _WindStrengthFactor * v.vertex.y * rand;
                UNITY_INITIALIZE_OUTPUT(Input, o)
                o.localPosition = v.vertex;
            #else
                UNITY_INITIALIZE_OUTPUT(Input, o)
            #endif
        }

        // Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
        // See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
        // #pragma instancing_options assumeuniformscaling
        UNITY_INSTANCING_BUFFER_START(Props)
        // put more per-instance properties here
        UNITY_INSTANCING_BUFFER_END(Props)

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            // Albedo comes from a texture tinted by color
            fixed4 c = float4(_Color.rgb, tex2D (_MainTex, IN.uv_MainTex).a);
            clip(c.a - _Cutoff);
            #ifdef UNITY_PROCEDURAL_INSTANCING_ENABLED
                float3 color = _ColorBuffer[_VisibleInstanceIDBuffer[unity_InstanceID]];
                o.Albedo = lerp(_SurfaceColorBuffer[_VisibleInstanceIDBuffer[unity_InstanceID]], c.rgb, saturate(IN.localPosition.y - _SurfaceColorBlending)) * color;
            #else
                o.Albedo = c.rgb;
            #endif
            o.Normal = UnpackNormalWithScale(tex2D(_Normal, IN.uv_Normal), _NormalStrength);
            // Metallic and smoothness come from slider variables
            o.Metallic = _Metallic;
            o.Smoothness = _Glossiness;
            o.Alpha = c.a;
        }
        ENDCG
    }
    FallBack "Diffuse"
}