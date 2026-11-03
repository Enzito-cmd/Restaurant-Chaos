Shader "Restaurant/UI/BurnDissolve"
{
    Properties
    {
        [PerRendererData] _MainTex ("Texture", 2D) = "white" {}
        _Progress ("Progress", Range(0, 1)) = 0
        _NoiseScale ("Noise Scale", Float) = 16.97
        _Distortion ("Distortion", Float) = 0.38
        _EdgeWidth_1 ("Edge Width 1", Float) = 0.06
        _EdgeWidth_2 ("Edge Width 2", Float) = 0.03
        _EdgeWidth_3 ("Edge Width 3", Float) = 0.015
        _Color_1 ("Edge Color 1", Color) = (0.651, 0.058, 0.058, 1)
        _Color_2 ("Edge Color 2", Color) = (0.821, 0.370, 0.166, 1)
        _Color_3 ("Edge Color 3", Color) = (0.915, 0.860, 0.505, 1)
        _Color_Emission ("Emission Color", Color) = (0.415, 0.020, 0, 1)
        _EmmisionStrength ("Emission Strength", Float) = 3
    }

    SubShader
    {
        Tags
        {
            "Queue" = "Transparent"
            "IgnoreProjector" = "True"
            "RenderType" = "Transparent"
            "PreviewType" = "Plane"
            "CanUseSpriteAtlas" = "True"
        }

        Cull Off
        ZWrite Off
        ZTest Always
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.5
            #include "UnityCG.cginc"

            struct appdata_t
            {
                float4 vertex : POSITION;
                float4 color : COLOR;
                float2 texcoord : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float4 color : COLOR;
                float2 uv : TEXCOORD0;
            };

            sampler2D _MainTex;
            float _Progress;
            float _NoiseScale;
            float _Distortion;
            float _EdgeWidth_1;
            float _EdgeWidth_2;
            float _EdgeWidth_3;
            float4 _Color_1;
            float4 _Color_2;
            float4 _Color_3;
            float4 _Color_Emission;
            float _EmmisionStrength;

            float BurnHash(float2 p)
            {
                uint2 v = (uint2)(int2)round(p);
                v.y ^= 1103515245U;
                v.x += v.y;
                v.x *= v.y;
                v.x ^= v.x >> 5u;
                v.x *= 0x27d4eb2du;
                return (v.x >> 8) * (1.0 / float(0x00ffffff));
            }

            float BurnValueNoise(float2 p)
            {
                float2 i = floor(p);
                float2 f = frac(p);
                f = f * f * (3.0 - 2.0 * f);

                float bottom = lerp(BurnHash(i), BurnHash(i + float2(1.0, 0.0)), f.x);
                float top = lerp(BurnHash(i + float2(0.0, 1.0)), BurnHash(i + float2(1.0, 1.0)), f.x);

                return lerp(bottom, top, f.y);
            }

            float BurnNoise(float2 uv, float scale)
            {
                float n = 0.0;

                for (int k = 0; k < 3; k++)
                {
                    n += BurnValueNoise(uv * (scale / pow(2.0, k))) * pow(0.5, 3 - k);
                }

                return n;
            }

            float Band(float front, float width, float m)
            {
                return step(front - width, m) - step(front + width, m);
            }

            v2f vert(appdata_t v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.color = v.color;
                o.uv = v.texcoord;
                return o;
            }

            float4 frag(v2f i) : SV_Target
            {
                float m = i.uv.y + (BurnNoise(i.uv, i.uv.x * _NoiseScale) - 0.5) * _Distortion;
                float front = lerp(-(_Distortion * 0.5 + _EdgeWidth_1), 1.0 + _Distortion * 0.375 + 0.01, _Progress);

                clip(m - front);

                float4 col = tex2D(_MainTex, i.uv) * i.color;
                float band1 = Band(front, _EdgeWidth_1, m);

                col.rgb = lerp(col.rgb, _Color_1.rgb, band1);
                col.rgb = lerp(col.rgb, _Color_2.rgb, Band(front, _EdgeWidth_2, m));
                col.rgb = lerp(col.rgb, _Color_3.rgb, Band(front, _EdgeWidth_3, m));
                col.rgb += band1 * _Color_Emission.rgb * _EmmisionStrength;

                return col;
            }
            ENDCG
        }
    }
}
