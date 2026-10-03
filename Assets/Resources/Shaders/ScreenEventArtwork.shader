// Approved FIELD-010 ink; exact hazard geometry comes from the event, never from the PNG silhouette.
Shader "SurvivorArena/ScreenEventArtwork"
{
    Properties
    {
        _MainTex ("Approved artwork", 2D) = "white" {}
        _Color ("Artwork tint", Color) = (1,1,1,1)
        _FillColor ("Danger fill", Color) = (0,0,0,0)
        _UvRect ("Authored crop", Vector) = (0,0,1,1)
        _BorderWidth ("Border world width", Float) = .12
        _RepeatLength ("Repeat world length", Float) = 3
        _ExteriorRibbonWidth ("Exterior ribbon world width", Float) = .7
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" }
        Cull Off
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            sampler2D _MainTex;
            float4 _UvRect, _Size, _Origin, _Direction, _Ring, _Lane;
            float _Shape, _Sliced, _BorderWidth, _RepeatLength, _ExteriorRibbonWidth;
            fixed4 _Color, _FillColor;
            struct appdata { float4 vertex : POSITION; float2 uv : TEXCOORD0; };
            struct v2f { float4 position : SV_POSITION; float2 uv : TEXCOORD0; float2 world : TEXCOORD1; };
            v2f vert(appdata input)
            {
                v2f output;
                output.position = UnityObjectToClipPos(input.vertex);
                output.world = mul(unity_ObjectToWorld, input.vertex).xy;
                output.uv = input.uv;
                return output;
            }
            // Keep the painted border/corners at a fixed world width. Repeat the interior instead of stretching its chevrons.
            float sliced(float u, float length, float sourceBorder, float repeatInterior)
            {
                float border = min(_BorderWidth / max(length, .001), .25);
                if (u < border) return u / border * sourceBorder;
                if (u > 1 - border) return 1 - (1 - u) / border * sourceBorder;
                float middle = (u - border) / (1 - 2 * border);
                if (repeatInterior > .5)
                    middle = frac(middle * max(1, round((length - 2 * _BorderWidth) / _RepeatLength)));
                return lerp(sourceBorder, 1 - sourceBorder, middle);
            }
            fixed4 frag(v2f input) : SV_Target
            {
                float2 relative = input.world - _Origin.xy;
                float distance = length(relative);
                float along = dot(relative, _Direction.xy);
                float across = abs(_Direction.x * relative.y - _Direction.y * relative.x);
                if (_Lane.y > .5)
                {
                    clip(along);
                    clip(_Lane.x - along);
                    if (_Lane.y > 1.5) clip(along - _Lane.z);
                }
                float2 uv = input.uv;
                float artworkMask = 1;
                if (_Shape > 2.5 && _Shape < 3.5)
                {
                    // The constant-width safe corridor is cut analytically, including at very small radii.
                    clip(_Ring.y * .5 - abs(distance - _Ring.x));
                    if (along > 0 && across < _Ring.z * .5) discard;
                    float angle = atan2(relative.y, relative.x);
                    float repeat = max(1, round(6.2831853 * _Ring.x / _RepeatLength));
                    uv.x = lerp(.08, .92, frac((angle / 6.2831853 + .5) * repeat));
                    uv.y = saturate((distance - _Ring.x) / _Ring.y + .5);
                }
                else if (_Shape > 1.5 && _Shape < 2.5)
                {
                    clip(distance - _Ring.x);
                    // Sparse warning/strike ribbons across the hazardous exterior; no texture inside the safe hole.
                    float band = frac((input.world.y + input.world.x * .25) / _RepeatLength);
                    float bandWidth = _ExteriorRibbonWidth / _RepeatLength;
                    artworkMask = 1 - step(bandWidth, band);
                    uv = float2(lerp(.08,.92,frac(input.world.x / _RepeatLength)), saturate(band / bandWidth));
                }
                else if (_Shape > .5 && _Shape < 1.5)
                {
                    clip(_Ring.x - distance);
                }
                else if (_Shape > 3.5)
                {
                    // Calm corridor: two straight teal edges, rather than stretching a circular painting into a rectangle.
                    float edge = min(input.uv.y, 1 - input.uv.y) * _Size.y;
                    float edgeAlpha = 1 - step(_BorderWidth, edge);
                    return fixed4(_FillColor.rgb, max(_FillColor.a, edgeAlpha * _Color.a));
                }
                else if (_Sliced > .5)
                    uv = float2(sliced(uv.x, _Size.x, .04, 1),
                        sliced(uv.y, _Size.y, .16, step(_RepeatLength, _Size.y)));
                fixed4 art = tex2D(_MainTex, _UvRect.xy + uv * _UvRect.zw) * _Color;
                art.a *= artworkMask;
                // Straight-alpha source-over; the faint fill says the whole area is a hazard even when art is hollow.
                fixed alpha = art.a + _FillColor.a * (1 - art.a);
                fixed3 rgb = (art.rgb * art.a + _FillColor.rgb * _FillColor.a * (1 - art.a)) / max(alpha, .0001);
                return fixed4(rgb, alpha);
            }
            ENDCG
        }
    }
}
