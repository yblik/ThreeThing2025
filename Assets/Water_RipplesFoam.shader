Shader "Custom/Water_RipplesFoam"
{
    Properties
    {
        _WaterTex ("Water Texture", 2D) = "white" {}
        _FoamTex ("Foam Texture", 2D) = "white" {}

        _WaterColor ("Water Tint", Color) = (0.2, 0.6, 0.8, 1)
        _FoamColor ("Foam Tint", Color) = (1,1,1,1)

        _WaterSpeed ("Water Scroll Speed", Vector) = (0.1, 0.05, 0, 0)
        _FoamSpeed ("Foam Scroll Speed", Vector) = (-0.05, 0.03, 0, 0)

        _WaterTiling ("Water Tiling", Vector) = (1,1,0,0)
        _FoamTiling ("Foam Tiling", Vector) = (2,2,0,0)

        _RippleStrength ("Ripple Distortion Strength", Float) = 0.03
        _RippleSpeed ("Ripple Speed", Float) = 1.5
    }

    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _WaterTex;
            sampler2D _FoamTex;

            float4 _WaterColor;
            float4 _FoamColor;

            float4 _WaterSpeed;
            float4 _FoamSpeed;

            float4 _WaterTiling;
            float4 _FoamTiling;

            float _RippleStrength;
            float _RippleSpeed;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 uvWater : TEXCOORD0;
                float2 uvFoam : TEXCOORD1;
            };

            v2f vert (appdata v)
            {
                v2f o;

                o.vertex = UnityObjectToClipPos(v.vertex);

                // Base UVs
                float2 baseUV = v.uv;

                // Water UV (tiling + scroll)
                o.uvWater = baseUV * _WaterTiling.xy;
                o.uvWater += _Time.y * _WaterSpeed.xy;

                // Foam UV (tiling + scroll)
                o.uvFoam = baseUV * _FoamTiling.xy;
                o.uvFoam += _Time.y * _FoamSpeed.xy;

                return o;
            }

            float4 frag (v2f i) : SV_Target
            {
                // Ripple distortion (fake normalmap)
                float ripple =
                    sin(i.uvWater.x * 12 + _Time.y * _RippleSpeed) +
                    cos(i.uvWater.y * 12 + _Time.y * _RippleSpeed * 1.2);

                ripple *= _RippleStrength;

                float2 rippleUV = i.uvWater + ripple;

                // Water layer
                float4 waterTex = tex2D(_WaterTex, rippleUV);
                float4 waterFinal = waterTex * _WaterColor;

                // Foam layer (no distortion for clarity)
                float4 foamTex = tex2D(_FoamTex, i.uvFoam);
                float4 foamFinal = foamTex * _FoamColor;

                // Combine
                float4 final = waterFinal + foamFinal * foamTex.a;

                // Transparency
                final.a = waterFinal.a;

                return final;
            }
            ENDCG
        }
    }
}
