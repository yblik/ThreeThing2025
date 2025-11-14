Shader "Custom/SimpleWaterOverlay"
{
    Properties
    {
        _MainTex ("Water Texture", 2D) = "white" {}
        _Color ("Tint Color", Color) = (1,1,1,1)
        _SpeedX ("Scroll Speed X", Float) = 0.2
        _SpeedY ("Scroll Speed Y", Float) = 0.1
        _Tiling ("Texture Tiling", Vector) = (1,1,0,0)
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

            sampler2D _MainTex;
            float4 _Color;
            float _SpeedX;
            float _SpeedY;
            float4 _Tiling;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            v2f vert (appdata v)
            {
                v2f o;

                o.vertex = UnityObjectToClipPos(v.vertex);

                // Apply tiling (prevents stretched look)
                float2 uv = v.uv * _Tiling.xy;

                // Add scrolling
                uv += float2(_Time.y * _SpeedX, _Time.y * _SpeedY);

                o.uv = uv;
                return o;
            }

            float4 frag (v2f i) : SV_Target
            {
                float4 tex = tex2D(_MainTex, i.uv);
                return tex * _Color;
            }
            ENDCG
        }
    }
}
