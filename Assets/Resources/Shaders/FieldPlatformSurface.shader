Shader "SurvivorArena/FieldPlatformSurface"
{
    Properties
    {
        _MainTex ("Approved material reference", 2D) = "white" {}
        _UvBounds ("Material sample rectangle", Vector) = (0,0,1,1)
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
            #include "UnityCG.cginc"
            struct appdata { float4 vertex : POSITION; float2 uv : TEXCOORD0; fixed4 color : COLOR; };
            struct v2f { float4 vertex : SV_POSITION; float2 uv : TEXCOORD0; fixed4 color : COLOR; };
            sampler2D _MainTex;
            float4 _UvBounds;
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
                // Mirror inside the approved region, never sampling paths/characters elsewhere in the source.
                float2 repeat = 1.0 - abs(frac(i.uv * 0.5) * 2.0 - 1.0);
                float2 uv = lerp(_UvBounds.xy, _UvBounds.zw, repeat);
                return tex2D(_MainTex, uv) * i.color;
            }
            ENDCG
        }
    }
}
