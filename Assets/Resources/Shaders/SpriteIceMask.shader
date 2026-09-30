// Shared slow-status texture, clipped to the current body sprite's alpha.
Shader "SurvivorArena/SpriteIceMask"
{
    Properties
    {
        [PerRendererData] _MainTex ("Body Sprite", 2D) = "white" {}
        _IceTex ("Ice Pattern", 2D) = "white" {}
        _Color ("Ice Tint", Color) = (1, 1, 1, 1)
        _IceUvTransform ("Ice UV Transform", Vector) = (1, 1, 0, 0)
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
            sampler2D _IceTex;
            fixed4 _Color;
            float4 _IceUvTransform;

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
                fixed bodyAlpha = tex2D(_MainTex, input.uv).a;
                fixed4 ice = tex2D(_IceTex, input.uv * _IceUvTransform.xy + _IceUvTransform.zw);
                return fixed4(ice.rgb * _Color.rgb, bodyAlpha * ice.a * _Color.a);
            }
            ENDCG
        }
    }
}
