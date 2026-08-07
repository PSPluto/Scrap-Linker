Shader "Custom/URPToonPostProcess"
{
    Properties
    {
        _ToonSteps("Toon Steps", Range(2, 10)) = 4
        _OutlineColor("Outline Color", Color) = (0.7, 0.7, 0.7, 1)
        _OutlineThickness("Outline Thickness", Float) = 1.0
        _DepthThreshold("Depth Threshold", Float) = 0.05
    }
    SubShader
    {
        Tags { "RenderPipeline" = "UniversalPipeline" }
        Cull Off ZWrite Off ZTest Always

        Pass
        {
            Name "ToonPostProcess"

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"

            struct Attributes
            {
                uint vertexID : SV_VertexID;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            TEXTURE2D(_BlitTexture);
            SAMPLER(sampler_BlitTexture);

            CBUFFER_START(UnityPerMaterial)
                float _ToonSteps;
                float4 _OutlineColor;
                float _OutlineThickness;
                float _DepthThreshold;
                float4 _BlitTexture_TexelSize;
            CBUFFER_END

            Varyings Vert(Attributes input)
            {
                Varyings output;
                output.positionCS = GetFullScreenTriangleVertexPosition(input.vertexID);
                output.uv = GetFullScreenTriangleTexCoord(input.vertexID);
                return output;
            }

            half4 Frag(Varyings input) : SV_Target
            {
                half4 col = SAMPLE_TEXTURE2D(_BlitTexture, sampler_BlitTexture, input.uv);
                
                // 階調化 (トゥーン処理)
                float stepSize = 1.0 / _ToonSteps;
                float3 toonColor = floor(col.rgb / stepSize) * stepSize;

                // 深度差によるアウトライン抽出
                float2 delta = _OutlineThickness * _BlitTexture_TexelSize.xy;
                float depthC = SampleSceneDepth(input.uv);
                float depthR = SampleSceneDepth(input.uv + float2(delta.x, 0));
                float depthU = SampleSceneDepth(input.uv + float2(0, delta.y));

                float depthDiff = abs(depthC - depthR) + abs(depthC - depthU);
                float outline = step(_DepthThreshold, depthDiff);

                float3 finalColor = lerp(toonColor, _OutlineColor.rgb, outline * _OutlineColor.a);
                return half4(finalColor, col.a);
            }
            ENDHLSL
        }
    }
}