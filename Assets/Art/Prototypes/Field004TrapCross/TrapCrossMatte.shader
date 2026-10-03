Shader "ArtPrototypes/TrapCrossMatte"
{
    Properties
    {
        _Color("Matte color", Color) = (1,1,1,1)
        _Expansion("Outline expansion", Float) = 0
        _Cull("Cull", Float) = 2
        _Unlit("Unlit", Float) = 0
        _MaterialKind("Painted material", Float) = 0
    }
    SubShader
    {
        Tags { "RenderPipeline"="UniversalPipeline" "RenderType"="Opaque" "Queue"="Geometry" }
        Pass
        {
            Tags { "LightMode"="SRPDefaultUnlit" }
            Cull [_Cull]
            ZWrite On
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            CBUFFER_START(UnityPerMaterial)
                float4 _Color;
                float _Expansion;
                float _Cull;
                float _Unlit;
                float _MaterialKind;
            CBUFFER_END
            struct Attributes { float4 positionOS:POSITION; float3 normalOS:NORMAL; };
            struct Varyings { float4 positionCS:SV_POSITION; float3 normalWS:TEXCOORD0; float3 positionOS:TEXCOORD1; };
            Varyings Vert(Attributes input)
            {
                Varyings output;
                output.normalWS = TransformObjectToWorldNormal(input.normalOS);
                output.positionOS = input.positionOS.xyz;
                float3 position = TransformObjectToWorld(input.positionOS.xyz);
                position += normalize(output.normalWS) * _Expansion;
                output.positionCS = TransformWorldToHClip(position);
                return output;
            }
            half4 Frag(Varyings input):SV_Target
            {
                float3 p = input.positionOS;
                float3 color = _Color.rgb;
                // Object-space pigment stays attached to geometry; no directional lighting or shadows.
                float patches = sin(p.x*13.0 + sin(p.z*11.0)*2.0) * sin(p.y*17.0+p.z*9.0);
                if (_MaterialKind > 0.5 && _MaterialKind < 1.5)
                {
                    float grain = sin(p.x*29.0 + sin(p.y*5.0+p.z*3.0)*1.3);
                    float streak = smoothstep(0.87,0.99,grain);
                    color *= 1.0 - 0.19*streak;
                    color += float3(0.10,0.065,0.025) * step(0.35,patches);
                }
                else if (_MaterialKind > 1.5)
                {
                    color += float3(0.045,0.04,0.035)*step(0.43,patches);
                    color *= 1.0 - 0.06*step(0.60,-patches);
                }
                return half4(saturate(color), _Color.a);
            }
            ENDHLSL
        }
    }
}
