Shader "UI/CircularHighlightMask"
{
    Properties
    {
        _OverlayAlpha ("Overlay Alpha", Range(0, 1)) = 0.7
        _CutoutCenter ("Cutout Center", Vector) = (0.5, 0.5, 0, 0)
        _CutoutRadius ("Cutout Radius", Float) = 0.1
    }

    SubShader
    {
        Tags { "Queue"="Overlay" "RenderType"="Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            float _OverlayAlpha;
            float4 _CutoutCenter;
            float _CutoutRadius;

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

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float aspect = _ScreenParams.x / _ScreenParams.y;
                float2 delta = float2((i.uv.x - _CutoutCenter.x) * aspect, i.uv.y - _CutoutCenter.y);
                float dist = length(delta);

                if (dist < _CutoutRadius)
                    return fixed4(0, 0, 0, 0); // Fully transparent inside

                return fixed4(0, 0, 0, _OverlayAlpha); // Outside circle fades based on _OverlayAlpha
            }
            ENDCG
        }
    }
}
