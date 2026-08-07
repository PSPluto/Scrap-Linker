Shader "Custom/URP/Water"
{
    Properties
    {
        [Header(Base Color)]
        _ShallowColor ("Shallow Color", Color) = (0.20, 0.55, 0.60, 0.6)
        _DeepColor ("Deep Color", Color) = (0.02, 0.10, 0.20, 0.95)
        _DepthFadeDistance ("Depth Fade Distance", Float) = 3.0

        [Header(Normal)]
        _NormalMap ("Normal Map", 2D) = "bump" {}
        _NormalTiling ("Normal Tiling", Vector) = (1, 1, 0, 0)
        _NormalSpeed1 ("Normal Speed A (XY)", Vector) = (0.05, 0.04, 0, 0)
        _NormalSpeed2 ("Normal Speed B (XY)", Vector) = (-0.03, 0.06, 0, 0)
        _NormalStrength ("Normal Strength", Range(0,2)) = 1.0

        [Header(Vertex Waves)]
        _WaveAmplitude ("Wave Amplitude", Float) = 0.15
        _WaveFrequency ("Wave Frequency", Float) = 1.0
        _WaveSpeed ("Wave Speed", Float) = 1.0

        [Header(Foam)]
        _FoamColor ("Foam Color", Color) = (1,1,1,1)
        _FoamDistance ("Foam Distance", Float) = 0.4
        _FoamNoise ("Foam Noise Tex", 2D) = "white" {}
        _FoamTiling ("Foam Tiling", Float) = 4.0

        [Header(Reflection Specular)]
        _Smoothness ("Smoothness", Range(0,1)) = 0.9
        _Metallic ("Metallic", Range(0,1)) = 0.0
        _FresnelPower ("Fresnel Power", Range(0.1,8)) = 3.0

        [Header(Refraction)]
        _RefractionStrength ("Refraction Strength", Range(0,0.2)) = 0.03
    }

    SubShader
    {
        Tags
        {
            "RenderType" = "Transparent"
            "Queue" = "Transparent"
            "RenderPipeline" = "UniversalPipeline"
        }

        // 背景を屈折させるためにCameraOpacityTextureを使うのでGrabではなく_CameraOpaqueTextureを利用
        // Project SettingsのURP AssetでOpaque TextureとDepth Textureを有効にしてください

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }

            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            Cull Back

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile_fragment _ _SHADOWS_SOFT
            #pragma multi_compile_fog

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareOpaqueTexture.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"

            TEXTURE2D(_NormalMap); SAMPLER(sampler_NormalMap);
            TEXTURE2D(_FoamNoise); SAMPLER(sampler_FoamNoise);

            CBUFFER_START(UnityPerMaterial)
                float4 _ShallowColor;
                float4 _DeepColor;
                float _DepthFadeDistance;

                float4 _NormalTiling;
                float4 _NormalSpeed1;
                float4 _NormalSpeed2;
                float _NormalStrength;

                float _WaveAmplitude;
                float _WaveFrequency;
                float _WaveSpeed;

                float4 _FoamColor;
                float _FoamDistance;
                float _FoamTiling;

                float _Smoothness;
                float _Metallic;
                float _FresnelPower;

                float _RefractionStrength;
            CBUFFER_END

            struct Attributes
            {
                float3 positionOS : POSITION;
                float3 normalOS   : NORMAL;
                float2 uv         : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float3 positionWS  : TEXCOORD0;
                float2 uv          : TEXCOORD1;
                float3 normalWS    : TEXCOORD2;
                float4 screenPos   : TEXCOORD3;
            };

            // シンプルな正弦波によるジオメトリの揺れ
            float3 GerstnerLikeOffset(float3 posWS, float time)
            {
                float wave = sin((posWS.x + posWS.z) * _WaveFrequency + time * _WaveSpeed);
                float wave2 = sin((posWS.x - posWS.z) * _WaveFrequency * 1.7 + time * _WaveSpeed * 1.3);
                float y = (wave + wave2 * 0.5) * _WaveAmplitude;
                return float3(0, y, 0);
            }

            Varyings vert (Attributes IN)
            {
                Varyings OUT;
                float3 positionWS = TransformObjectToWorld(IN.positionOS);
                positionWS += GerstnerLikeOffset(positionWS, _Time.y);

                OUT.positionWS = positionWS;
                OUT.positionHCS = TransformWorldToHClip(positionWS);
                OUT.normalWS = TransformObjectToWorldNormal(IN.normalOS);
                OUT.uv = IN.uv;
                OUT.screenPos = ComputeScreenPos(OUT.positionHCS);
                return OUT;
            }

            half4 frag (Varyings IN) : SV_Target
            {
                float2 screenUV = IN.screenPos.xy / IN.screenPos.w;

                // ---- 深度差分によるフェード & 泡 ----
                float sceneRawDepth = SampleSceneDepth(screenUV);
                float sceneEyeDepth = LinearEyeDepth(sceneRawDepth, _ZBufferParams);
                float waterEyeDepth = IN.screenPos.w;
                float depthDiff = max(sceneEyeDepth - waterEyeDepth, 0);

                float depthFade = saturate(depthDiff / _DepthFadeDistance);
                float foamMask = 1 - saturate(depthDiff / _FoamDistance);

                // ---- 法線マップ(2方向スクロールで自然な揺らぎ) ----
                float2 uvA = IN.uv * _NormalTiling.xy + _NormalSpeed1.xy * _Time.y;
                float2 uvB = IN.uv * _NormalTiling.xy * 1.3 + _NormalSpeed2.xy * _Time.y;
                half3 n1 = UnpackNormalScale(SAMPLE_TEXTURE2D(_NormalMap, sampler_NormalMap, uvA), _NormalStrength);
                half3 n2 = UnpackNormalScale(SAMPLE_TEXTURE2D(_NormalMap, sampler_NormalMap, uvB), _NormalStrength);
                half3 normalTS = normalize(n1 + n2);

                float3 normalWS = normalize(IN.normalWS);
                // 簡易的にワールドZ軸をタンジェント近似(平面水面向け)
                float3 tangentWS = normalize(cross(float3(0,1,0), normalWS + float3(0.0001,0,0)));
                float3 bitangentWS = normalize(cross(normalWS, tangentWS));
                float3x3 TBN = float3x3(tangentWS, bitangentWS, normalWS);
                float3 finalNormalWS = normalize(mul(normalTS, TBN));

                // ---- 屈折(不透明テクスチャをずらしてサンプリング) ----
                float2 refractOffset = normalTS.xy * _RefractionStrength;
                float2 refractedUV = screenUV + refractOffset;
                half3 sceneColor = SampleSceneColor(refractedUV);

                // ---- 水色(浅瀬〜深場のグラデーション) ----
                half4 waterColor = lerp(_ShallowColor, _DeepColor, depthFade);
                half3 baseColor = lerp(sceneColor, waterColor.rgb, waterColor.a);

                // ---- 泡 ----
                float2 foamUV = IN.uv * _FoamTiling + _Time.y * 0.05;
                half foamNoise = SAMPLE_TEXTURE2D(_FoamNoise, sampler_FoamNoise, foamUV).r;
                float foam = saturate(foamMask * 2.0) * step(0.4, foamNoise + foamMask * 0.5);
                baseColor = lerp(baseColor, _FoamColor.rgb, foam);

                // ---- ライティング(フレネル + スペキュラ) ----
                Light mainLight = GetMainLight();
                float3 viewDirWS = normalize(GetWorldSpaceViewDir(IN.positionWS));

                float NdotV = saturate(dot(finalNormalWS, viewDirWS));
                float fresnel = pow(1.0 - NdotV, _FresnelPower);

                float3 halfDir = normalize(mainLight.direction + viewDirWS);
                float spec = pow(saturate(dot(finalNormalWS, halfDir)), lerp(8, 256, _Smoothness)) * _Smoothness;

                float NdotL = saturate(dot(finalNormalWS, mainLight.direction));
                half3 lighting = baseColor * (mainLight.color * NdotL * 0.6 + 0.4) + mainLight.color * spec;

                half3 finalColor = lerp(lighting, half3(1,1,1), fresnel * 0.3);

                float alpha = lerp(waterColor.a, 1.0, foam);
                alpha = saturate(alpha + fresnel * 0.2);

                return half4(finalColor, alpha);
            }
            ENDHLSL
        }
    }
    FallBack "Universal Render Pipeline/Lit"
}
