Shader "Greenery/GreeneryInstanceShader"
{
    Properties
    {
        _ColorTop("Color top", Color) = (1,1,1,1)
        _ColorBottom("Color bottom", Color) = (0,0,0,1)
        _MainTex ("Texture", 2D) = "white" {}
        
        _WindGuide("Wind guide", 2D) = "gray" {}
        _WindStrength("Wind strength", float) = 0
    }
    SubShader
    {
        Cull Off
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog

            #include "UnityCG.cginc"
            #include "Includes/CommonStructs.hlsl"

            StructuredBuffer<DrawTriangle> _DrawTrianglesBuffer;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
                float3 color : COLOR;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            fixed4 _ColorTop;
            fixed4 _ColorBottom;

            float _WindStrength;
            sampler2D _WindGuide;
            float4 _WindGuide_ST;

            v2f vert (uint vertexID : SV_VertexID)
            {
                v2f o;

                DrawTriangle tri = _DrawTrianglesBuffer[vertexID / 3];
                DrawVertex v = tri.vertices[vertexID % 3];
                
                float2 wind = tex2Dlod(_WindGuide, float4(v.positionWS.xz * _WindGuide_ST.xy + _Time.y * _WindGuide_ST.zw, 0, 0)).xy * 2.0 - 1.0;
                v.positionWS.xz += wind * _WindStrength * v.uv.y;

                o.vertex = UnityObjectToClipPos(v.positionWS);
                o.uv = v.uv;
                o.color = v.color;
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float tex = tex2D(_MainTex, TRANSFORM_TEX(i.uv, _MainTex)).a;
                clip(tex - 0.5);
                
                fixed4 col = lerp(_ColorBottom, _ColorTop, i.uv.y);
                UNITY_APPLY_FOG(i.fogCoord, col);
                return col;
            }
            ENDCG
        }
    }
}
