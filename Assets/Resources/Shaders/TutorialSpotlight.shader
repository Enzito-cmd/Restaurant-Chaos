Shader "Restaurant/UI/TutorialSpotlight"
{
    Properties
    {
        [PerRendererData] _MainTex ("Texture", 2D) = "white" {}
        _Color ("Dim Color", Color) = (0, 0, 0, 1)
        _DimAlpha ("Dim Alpha", Range(0, 1)) = 0.75
        _Fade ("Fade", Range(0, 1)) = 1
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
            float _DimAlpha;
            float _Fade;
            float _RimWidth;
            float _Corner;
            float _Softness;
            float _Pulse;
            float _HoleCount;
            float4 _RectSize;
            float4 _Holes[8];

            v2f vert(appdata_t v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.texcoord;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float2 pixel = i.uv * _RectSize.xy;
                float distanceToHole = 100000.0;

                for (int k = 0; k < 8; k++)
                {
                    if (k >= (int)_HoleCount)
                    {
                        break;
                    }

                    float2 holeCenter = _Holes[k].xy * _RectSize.xy;
                    float2 holeHalf = _Holes[k].zw * _RectSize.xy;
                    float2 q = abs(pixel - holeCenter) - holeHalf + _Corner;
                    float d = length(max(q, 0.0)) + min(max(q.x, q.y), 0.0) - _Corner;

                    distanceToHole = min(distanceToHole, d);
                }

                float dimMask = smoothstep(0.0, max(_Softness, 0.001), distanceToHole);
                float rimMask = 1.0 - smoothstep(0.0, max(_RimWidth, 0.001), abs(distanceToHole));

                float dimAlpha = _DimAlpha * _Fade * dimMask;
                float rimAlpha = _RimColor.a * _Fade * _Pulse * rimMask;

                float outAlpha = rimAlpha + dimAlpha * (1.0 - rimAlpha);
                float3 outColor = (_RimColor.rgb * rimAlpha + _Color.rgb * dimAlpha * (1.0 - rimAlpha)) / max(outAlpha, 0.0001);

                return fixed4(outColor, outAlpha);
            }
            ENDCG
        }
    }
}
