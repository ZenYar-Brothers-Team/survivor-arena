// Draws a thin outline hugging a sprite's silhouette in the material _Color: the alpha of the texture is dilated by
// _WidthPixels screen pixels. The width is measured on the screen (through the UV derivatives), so it stays the same thickness
// and stays round whatever the window size, zoom or rotation of the sprite (a width measured in texture space turned into
// star-shaped spikes when the sprite was drawn larger). Drawn behind the sprite itself it shows as a rim. The texture
// contributes alpha only; unlit, like SpriteSolidColor. Used for the red danger outline of trap projectiles (DECISION-0156).
Shader "SurvivorArena/SpriteDilatedOutline"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Color", Color) = (1, 0.05, 0.05, 1)
        _WidthPixels ("Outline width (screen pixels)", Float) = 2.5
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" "PreviewType"="Plane" "CanUseSpriteAtlas"="True" }
        Cull Off
        Lighting Off
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            fixed4 _Color;
            float _WidthPixels;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 position : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            v2f vert(appdata input)
            {
                v2f output;
                output.position = UnityObjectToClipPos(input.vertex);
                output.uv = input.uv;
                return output;
            }

            fixed4 frag(v2f input) : SV_Target
            {
                // Taps on three rings of eight directions, laid out in screen space: one screen pixel to the right / up moves the uv by
                // ddx / ddy, so the offsets form a true circle on the screen whatever the sprite's rotation or scale.
                float2 stepX = ddx(input.uv);
                float2 stepY = ddy(input.uv);
                float alpha = tex2D(_MainTex, input.uv).a;
                [unroll]
                for (int ring = 1; ring <= 3; ring++)
                {
                    float radius = _WidthPixels * ring / 3.0;
                    [unroll]
                    for (int tap = 0; tap < 8; tap++)
                    {
                        float angle = tap * 0.78539816 + ring * 0.2618;
                        float2 offset = (cos(angle) * stepX + sin(angle) * stepY) * radius;
                        alpha = max(alpha, tex2D(_MainTex, input.uv + offset).a);
                    }
                }
                return fixed4(_Color.rgb, alpha * _Color.a);
            }
            ENDCG
        }
    }
}
