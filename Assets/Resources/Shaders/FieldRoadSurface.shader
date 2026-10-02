Shader "SurvivorArena/FieldRoadSurface"
{
    Properties
    {
        _MainTex ("Cover", 2D) = "white" {}
        _MirrorRepeat ("Mirror world UV", Float) = 0
        _EdgeBand ("Dirt feather", Float) = 0
        _EdgeOpacity ("Dirt opacity", Float) = 0
        _EdgeNoiseScale ("Noise world scale", Float) = 1
        _EdgeNoiseStrength ("Edge irregularity", Float) = 0
        _EdgeSoilColor ("Soil", Color) = (1,1,1,1)
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
            struct appdata { float4 vertex : POSITION; float2 uv : TEXCOORD0; float2 band : TEXCOORD1; fixed4 color : COLOR; };
            struct v2f { float4 vertex : SV_POSITION; float2 uv : TEXCOORD0; float2 band : TEXCOORD1; float2 world : TEXCOORD2; fixed4 color : COLOR; };
            sampler2D _MainTex;
            float _MirrorRepeat;
            float _EdgeBand, _EdgeOpacity, _EdgeNoiseScale, _EdgeNoiseStrength;
            fixed4 _EdgeSoilColor;
            float hash(float2 p) { return frac(sin(dot(p,float2(127.1,311.7))) * 43758.5453); }
            float noise(float2 p)
            {
                float2 cell = floor(p), t = frac(p);
                t = t*t*(3.0-2.0*t);
                return lerp(lerp(hash(cell),hash(cell+float2(1,0)),t.x),
                    lerp(hash(cell+float2(0,1)),hash(cell+float2(1,1)),t.x),t.y);
            }
            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.color = v.color;
                o.band = v.band;
                o.world = mul(unity_ObjectToWorld,v.vertex).xy;
                return o;
            }
            fixed4 frag(v2f i) : SV_Target
            {
                float2 uv = i.uv;
                if (_MirrorRepeat > 0.5) uv = 1.0 - abs(frac(uv * 0.5) * 2.0 - 1.0);
                fixed4 cover = tex2D(_MainTex, uv) * i.color;
                if (_EdgeBand > 0.5)
                {
                    float grain = noise(i.world / _EdgeNoiseScale);
                    float irregularity = (grain-0.5)*_EdgeNoiseStrength;
                    // The exterior reaches full transparency; the real existing grass shows through it.
                    float feather = 1.0-smoothstep(0.0,1.0,saturate(i.band.x+irregularity));
                    feather *= 1.0-smoothstep(0.85,1.0,i.band.x);
                    float textureValue = dot(cover.rgb,float3(0.299,0.587,0.114));
                    cover.rgb = _EdgeSoilColor.rgb*(0.8+textureValue*0.35);
                    cover.a *= feather*_EdgeOpacity;
                }
                return cover;
            }
            ENDCG
        }
    }
}
