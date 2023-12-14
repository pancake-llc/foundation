Shader "Greenery/GreeneryGrassShader"
{
    Properties
    {
        [Header(Color settings)]
        _Color("Color", Color) = (1,1,1,1)
        _MainTex ("Main texture", 2D) = "white" {}
        _AlphaTex("Alpha texture", 2D) = "white" {}
        _ClipThreshold("Clip threshold", Range(0,1)) = 0.5
        _Glossiness ("Glossiness", Range(0,1)) = 0.5
        _SpecularColor("Specular color", Color) = (0.05,0.05,0.05,1)
        [Toggle()]
        _ApplyTextureColor("Apply texture color", float) = 0
        [Toggle()]
        _UseSurfaceNormal("Use surface normal", float) = 0
        _SurfaceBlendingHeight("Surface blending height", Range(-1, 1)) = 0
        _SurfaceBlendingFalloff("Surface blending falloff", float) = 1
        [Header(Ground shadow)]
        _GroundShadowHeight("Ground shadow height", Range(-1,1)) = 0
        _GroundShadowFalloff("Ground shadow falloff", float) = 1
        _GroundShadowStrength("Ground shadow strength", Range(0,1)) = 0.5
        
        [Header(Wind coloring)]
        _WindColoringStrength("Wind coloring strength", Range(0,1)) = 1
        _WindColoringThreshold("Wind coloring threshold", Range(0,1)) = 0
        _WindColoringSmoothness("Wind coloring smoothness", Range(0,1)) = 1

        [Header(Subsurface scattering)]
        [HDR]_SSSColor("SSS Color", Color) = (0,0,0,1)
        _SSSPower("SSS Power", float) = 1
        _SSSNormalOffset("SSS Normal Offset", float) = 0

        [Header(Misc)]
        _CameraAngleCompensation("Camera angle compensation", float) = 1
//        _InteractivityStrength("Interactivity strength", float) = 1
    }
    SubShader
    {
        Cull Off
        AlphaToMask on
        Tags { "RenderType"="Opaque" }
        LOD 200

        CGPROGRAM
        #pragma surface surf Grass fullforwardshadows vertex:vert addshadow
        #include "UnityPBSLighting.cginc"
        #include "Includes/CommonStructs.hlsl"
        #include "Includes/GreeneryWind.hlsl"

        // Use shader model 3.0 target, to get nicer looking lighting
        #pragma target 4.0

        sampler2D _MainTex;
        sampler2D _AlphaTex;

        struct appdata {
            float4 vertex : POSITION;
            float2 texcoord : TEXCOORD0;
            float2 texcoord1 : TEXCOORD1;
            float2 texcoord2 : TEXCOORD2;
            float3 normal : NORMAL;
            float4 color : COLOR;
            
            uint id : SV_VertexID;
        };

        struct Input
        {
            float2 uv_MainTex;
            float4 color : COLOR;
            float3 worldPos;
            float4 surfaceColor;
            float vface : VFACE;
            float3 viewDir;
            float3 objNormals;
            float3 surfaceNormal;
        };

        float _ClipThreshold;
        half _Glossiness;
        fixed4 _Color;
        fixed4 _SpecularColor;
        float _CameraAngleCompensation;

        float _InteractivityStrength;
        float _SurfaceBlendingHeight;
        float _SurfaceBlendingFalloff;

        float _GroundShadowStrength;
        float _GroundShadowFalloff;
        float _GroundShadowHeight;


        float _WindColoringStrength;
        float _WindColoringThreshold;
        float _WindColoringSmoothness;
        float _ApplyTextureColor;
        float _UseSurfaceNormal;

        float _SSSNormalOffset;
        float _SSSPower;
        float4 _SSSColor;

        #ifdef SHADER_API_D3D11
            StructuredBuffer<DrawTriangle> _DrawTrianglesBuffer;
        #endif

        struct SurfaceOutputGrass
        {
            fixed3 Albedo;
            fixed3 Normal;
            half3 Emission;
            half Metallic;
            half Smoothness;
            half Occlusion;
            fixed Alpha;
            float Vface;
            float UVY;
            float3 SurfaceNormal;
        };

        SurfaceOutputStandard GetStandardOutput(SurfaceOutputGrass s) {
            SurfaceOutputStandard r;
            r.Albedo = s.Albedo;
            r.Normal = _UseSurfaceNormal ? s.Normal : (s.Vface > 0 ? s.Normal : -s.Normal);
            r.Emission = s.Emission;
            r.Metallic = s.Metallic;
            r.Smoothness = s.Smoothness;
            r.Occlusion = s.Occlusion;
            r.Alpha = s.Alpha;
            return r;
        }

        fixed4 LightingGrass(SurfaceOutputGrass s, half3 viewDir, UnityGI gi) {
            SurfaceOutputStandard r = GetStandardOutput(s);
            return LightingStandard(r, viewDir, gi);
        }

        fixed4 LightingGrass_Deferred(SurfaceOutputGrass s, half3 viewDir, UnityGI gi, out half4 outDiffuseOcclusion, out half4 outSpecSmoothness, out half4 outNormal) {
            SurfaceOutputStandard r = GetStandardOutput(s);
            r.Normal = normalize(lerp(s.SurfaceNormal, r.Normal, s.UVY));
            outDiffuseOcclusion = half4(s.Albedo, s.Occlusion);
            outSpecSmoothness = half4(_SpecularColor.rgb, s.Smoothness);
            outNormal = half4(r.Normal * 0.5 + 0.5, 0);
            return LightingStandard(r, viewDir, gi) + float4(s.Emission, 1.0);
        }

        inline void LightingGrass_GI(SurfaceOutputGrass s, UnityGIInput data, inout UnityGI gi )
        {
            UNITY_GI(gi, s, data);
        }

        // Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
        // See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
        // #pragma instancing_options assumeuniformscaling
        UNITY_INSTANCING_BUFFER_START(Props)
        // put more per-instance properties here
        UNITY_INSTANCING_BUFFER_END(Props)

        void vert (inout appdata output, out Input o) {
            #ifdef SHADER_API_D3D11
                DrawTriangle tri = _DrawTrianglesBuffer[output.id / 3];
                DrawVertex v = tri.vertices[output.id % 3];

                float unscaledWind = 0;
                float2 wind = CalculateWind(v.positionWS);
                // float3 interactivityRT = SampleInteractivityRT(v.positionWS);
                // v.positionWS.y *= v.positionWS.y / (sqrt(pow(length(wind), 2) + pow(v.positionWS.y, 2))) * v.uv.y;
                v.positionWS.xz += wind * v.uv.y;
                float3 camForward = unity_CameraToWorld._m02_m12_m22;
                v.positionWS.xz += camForward.xz * saturate(dot(v.normal, normalize(-camForward))) * _CameraAngleCompensation * v.uv.y;
                // v.positionWS.xz += interactivityRT.xy * _InteractivityStrength * v.uv.y;
                // v.positionWS.y -= interactivityRT.b * _InteractivityStrength;

                float blendingFactor = pow(saturate(v.uv.y - _SurfaceBlendingHeight), _SurfaceBlendingFalloff);
                output.vertex = float4(v.positionWS, 1);
                output.normal = _UseSurfaceNormal ? v.surfaceNormal : lerp(v.surfaceNormal, v.normal, blendingFactor);
                output.texcoord = v.uv; 
                output.texcoord1 = v.uv;
                output.texcoord2 = v.uv;
                output.color = v.color;
                UNITY_INITIALIZE_OUTPUT(Input, o);

                o.surfaceColor = v.surfaceColor;
                o.objNormals = UnityObjectToWorldNormal(v.normal);
                o.surfaceNormal = v.surfaceNormal;

            #endif
        }

        void surf (Input IN, inout SurfaceOutputGrass o)
        {
            float4 tex = tex2D(_MainTex, IN.uv_MainTex);
            float alphaTex = tex2D(_AlphaTex, IN.uv_MainTex);
            fixed4 c = _Color * IN.color;
            if (_ApplyTextureColor > 0) c *= tex;
            clip((tex.a * alphaTex) - _ClipThreshold);
            float4 wind = SampleWindTexture(IN.worldPos);
            o.Albedo = c.rgb;
            float blendingFactor = pow(saturate(IN.uv_MainTex.y - _SurfaceBlendingHeight), _SurfaceBlendingFalloff);
            o.Albedo = IN.surfaceColor.a > 0 ? lerp(IN.surfaceColor.rgb, o.Albedo, blendingFactor) : o.Albedo;
            o.Albedo += smoothstep(_WindColoringThreshold, _WindColoringThreshold + _WindColoringSmoothness, length(abs(wind))) * _WindColoringStrength * o.Albedo;
            float groundShadow = pow(saturate(IN.uv_MainTex.y - _GroundShadowHeight), _GroundShadowFalloff);
            o.Albedo *= lerp(1.0 - _GroundShadowStrength, 1, groundShadow);
            o.Metallic = 0;
            o.Smoothness = _Glossiness;
            o.Alpha = c.a;
            o.Emission = pow(saturate(dot(normalize(-_WorldSpaceLightPos0), normalize(IN.viewDir) + IN.objNormals * _SSSNormalOffset)), _SSSPower) * _LightColor0 * _SSSColor.rgb * IN.uv_MainTex.y;
            o.Vface = IN.vface;
            o.UVY = blendingFactor;
            o.SurfaceNormal = IN.surfaceNormal;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
