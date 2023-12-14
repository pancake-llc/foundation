Shader "Hidden/TerrainColorShader"
{
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _SplatMap;
            sampler2D _Diffuse0, _Diffuse1, _Diffuse2, _Diffuse3;
            float4 _ST0, _ST1, _ST2, _ST3;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float4 control = tex2D(_SplatMap, i.uv);
                float4 col = control.r * tex2D(_Diffuse0, i.uv * _ST0.xy + _ST0.zw);
                col += control.g * tex2D(_Diffuse1, i.uv * _ST1.xy + _ST1.zw);
                col += control.b * tex2D(_Diffuse2, i.uv * _ST2.xy + _ST2.zw);
                col += control.a * tex2D(_Diffuse3, i.uv * _ST3.xy + _ST3.zw);
                return col;
            }
            ENDCG
        }
    }
}
