Shader "Restaurant/UI/TutorialSpotlight"
{
    Properties
    {
        [PerRendererData] _MainTex ("Texture", 2D) = "white" {}
        _Color ("Dim Color", Color) = (0, 0, 0, 1)
        _DimAlpha ("Dim Alpha", Range(0, 1)) = 0.75
        _Fade ("Fade", Range(0, 1)) = 1
        _HoleScale ("Hole Scale", Range(0, 1)) = 1
        _HoleTint ("Hole Tint", Color) = (1, 0.85, 0.1, 0)
        _RimColor ("Rim Color", Color) = (1, 0.92, 0.6, 0.9)
        _RimWidth ("Rim Width (px)", Float) = 3
        _Corner ("Corner Radius (px)", Float) = 28
        _Softness ("Edge Softness (px)", Float) = 22
        _Pulse ("Rim Pulse", Range(0, 1)) = 1
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
            #include "UnityCG.cginc"

            struct appdata_t
            {
                float4 vertex : POSITION;
                float2 texcoord : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            fixed4 _Color;
            fixed4 _RimColor;
            fixed4 _HoleTint;
            float _DimAlpha;
            float _Fade;
            float _HoleScale;
            float _RimWidth;
            float _Corner;
            float _Softness;
            float _Pulse;
            float _HoleCount;
            float4 _RectSize;
            float4 _Holes[16];

            v2f vert(appdata_t v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.texcoord;
                return o;
            }

            float4 Over(float4 top, float4 bottom)
            {
                float a = top.a + bottom.a * (1.0 - top.a);
                float3 rgb = (top.rgb * top.a + bottom.rgb * bottom.a * (1.0 - top.a)) / max(a, 0.0001);
                return float4(rgb, a);
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float2 pixel = i.uv * _RectSize.xy;
                float distanceToHole = 100000.0;

                for (int k = 0; k < 16; k++)
                {
                    if (k >= (int)_HoleCount)
                    {
                        break;
                    }

                    float2 holeCenter = _Holes[k].xy * _RectSize.xy;
                    float2 holeHalf = _Holes[k].zw * _RectSize.xy * _HoleScale;
                    float2 q = abs(pixel - holeCenter) - holeHalf + _Corner;
                    float d = length(max(q, 0.0)) + min(max(q.x, q.y), 0.0) - _Corner;

                    distanceToHole = min(distanceToHole, d);
                }

                float visible = saturate(_HoleScale * 6.0);
                float dimMask = lerp(1.0, smoothstep(0.0, max(_Softness, 0.001), distanceToHole), visible);
                float holeMask = (1.0 - dimMask);
                float rimMask = (1.0 - smoothstep(0.0, max(_RimWidth, 0.001), abs(distanceToHole))) * visible;

                float4 dim = float4(_Color.rgb, _DimAlpha * _Fade * dimMask);
                float4 tint = float4(_HoleTint.rgb, _HoleTint.a * _Fade * holeMask);
                float4 rim = float4(_RimColor.rgb, _RimColor.a * _Fade * _Pulse * rimMask);

                float4 result = Over(tint, dim);
                result = Over(rim, result);

                return fixed4(result);
            }
            ENDCG
        }
    }
}
