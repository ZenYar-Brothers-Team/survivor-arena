Shader "SurvivorArena/FieldPlatformSurface"
{
    Properties
    {
        _MainTex ("Approved material reference", 2D) = "white" {}
        _UvBounds ("Material sample rectangle", Vector) = (0,0,1,1)
        _BridgeVeil ("Procedural bridge veil", Float) = 0
        _VeilColor ("Transparent cloth", Color) = (1,1,1,0)
        _ThreadColor ("Woven threads", Color) = (1,1,1,0)
        _EdgeColor ("Safe boundary threads", Color) = (1,1,1,0)
        _Weave ("Diamond length, thread width, edge width, half bridge width", Vector) = (1,0,0,1)
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" "IgnoreProjector"="True" }
        Cull Off
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.0
            #include "UnityCG.cginc"
            struct appdata { float4 vertex : POSITION; float2 uv : TEXCOORD0; fixed4 color : COLOR; };
            struct v2f { float4 vertex : SV_POSITION; float2 uv : TEXCOORD0; fixed4 color : COLOR; };
            sampler2D _MainTex;
            float4 _UvBounds;
            float _BridgeVeil;
            float4 _Weave;
            fixed4 _VeilColor, _ThreadColor, _EdgeColor;
            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.color = v.color;
                return o;
            }
            fixed4 frag(v2f i) : SV_Target
            {
                if (_BridgeVeil > 0.5)
                {
                    // Coordinates follow each bridge: x is distance along it, y is signed across it.
                    // Large diamonds span the whole width; holes are visual transparency, not damage gaps.
                    float slope = _Weave.x / (2.0 * _Weave.w);
                    float2 phase = float2(i.uv.x + i.uv.y * slope, i.uv.x - i.uv.y * slope) / _Weave.x;
                    float2 dist = abs(frac(phase + 0.5) - 0.5) * _Weave.x / sqrt(1.0 + slope * slope);
                    float distanceToThread = min(dist.x, dist.y);
                    float aa = max(fwidth(distanceToThread), 0.0001);
                    float thread = 1.0 - smoothstep(_Weave.y * 0.5 - aa, _Weave.y * 0.5 + aa, distanceToThread);
                    float edgeDistance = _Weave.w - abs(i.uv.y);
                    float edgeAa = max(fwidth(edgeDistance), 0.0001);
                    float edge = 1.0 - smoothstep(_Weave.z - edgeAa, _Weave.z + edgeAa, edgeDistance);
                    fixed4 cloth = lerp(_VeilColor, _ThreadColor, thread);
                    return lerp(cloth, _EdgeColor, edge) * i.color;
                }
                // Mirror inside the approved region, never sampling paths/characters elsewhere in the source.
                float2 repeat = 1.0 - abs(frac(i.uv * 0.5) * 2.0 - 1.0);
                float2 uv = lerp(_UvBounds.xy, _UvBounds.zw, repeat);
                return tex2D(_MainTex, uv) * i.color;
            }
            ENDCG
        }
    }
}
