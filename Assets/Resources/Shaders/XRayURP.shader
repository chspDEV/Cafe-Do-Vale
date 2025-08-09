Shader "Custom/XRayURP"
{
    Properties
    {
        _BaseColor ("Base Color", Color) = (1,1,1,1)
        _BaseMap ("Base Map (RGB)", 2D) = "white" {}
        _NormalMap ("Normal Map", 2D) = "bump" {}
        _MetallicGlossMap ("MetallicGlossMap", 2D) = "white" {}
        _OcclusionMap ("Occlusion Map", 2D) = "white" {}
        _EmissionMap ("Emission Map", 2D) = "black" {}
        _EmissionColor ("Emission Color", Color) = (0,0,0,1)

        _Metallic ("Metallic", Range(0,1)) = 0.0
        _Smoothness ("Smoothness", Range(0,1)) = 0.5

        _XRayColor ("X-Ray Color", Color) = (0.2,1,1,0.8)
        _FresnelPower ("Fresnel Power", Range(0.1,8)) = 2.0
        _Thickness ("Thickness Glow", Range(0,1)) = 0.6
        _EdgeSoftness ("Edge Softness", Range(0.01,1)) = 0.15
        _DepthBias ("Depth Bias (meters)", Range(0.0,0.1)) = 0.001
        _XRayIntensity ("X-Ray Intensity", Range(0,2)) = 1.0
    }

    SubShader
    {
        Tags 
        { 
            "RenderType" = "Transparent" 
            "Queue" = "Transparent" 
            "RenderPipeline" = "UniversalPipeline"
        }
        LOD 200

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }
            
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            Cull Back

            HLSLPROGRAM
            #pragma target 4.5
            #pragma vertex vert
            #pragma fragment frag

            // Unity URP includes
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"

            // Properties
            TEXTURE2D(_BaseMap);
            SAMPLER(sampler_BaseMap);
            TEXTURE2D(_NormalMap);
            SAMPLER(sampler_NormalMap);
            TEXTURE2D(_MetallicGlossMap);
            SAMPLER(sampler_MetallicGlossMap);
            TEXTURE2D(_OcclusionMap);
            SAMPLER(sampler_OcclusionMap);
            TEXTURE2D(_EmissionMap);
            SAMPLER(sampler_EmissionMap);

            CBUFFER_START(UnityPerMaterial)
                float4 _BaseColor;
                float4 _BaseMap_ST;
                float4 _EmissionColor;
                float _Metallic;
                float _Smoothness;
                float4 _XRayColor;
                float _FresnelPower;
                float _Thickness;
                float _EdgeSoftness;
                float _DepthBias;
                float _XRayIntensity;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
                float3 normalOS : NORMAL;
                float4 tangentOS : TANGENT;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 positionWS : TEXCOORD1;
                float3 normalWS : TEXCOORD2;
                float4 positionNDC : TEXCOORD3;
                float3 viewDirWS : TEXCOORD4;
            };

            Varyings vert(Attributes input)
            {
                Varyings output;
                
                VertexPositionInputs vertexInput = GetVertexPositionInputs(input.positionOS.xyz);
                VertexNormalInputs normalInput = GetVertexNormalInputs(input.normalOS, input.tangentOS);
                
                output.positionCS = vertexInput.positionCS;
                output.positionWS = vertexInput.positionWS;
                output.normalWS = normalInput.normalWS;
                output.uv = TRANSFORM_TEX(input.uv, _BaseMap);
                output.positionNDC = vertexInput.positionNDC;
                output.viewDirWS = GetWorldSpaceViewDir(vertexInput.positionWS);
                
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                // Sample textures
                half4 baseMap = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, input.uv);
                half4 baseColor = baseMap * _BaseColor;
                
                half3 normalTS = UnpackNormal(SAMPLE_TEXTURE2D(_NormalMap, sampler_NormalMap, input.uv));
                half4 emissionMap = SAMPLE_TEXTURE2D(_EmissionMap, sampler_EmissionMap, input.uv);
                
                // World space normal (simplified for X-ray effect)
                half3 normalWS = normalize(input.normalWS);
                half3 viewDirWS = SafeNormalize(input.viewDirWS);
                
                // Fresnel calculation
                half NdotV = saturate(dot(normalWS, viewDirWS));
                half fresnel = pow(1.0 - NdotV, _FresnelPower);
                
                // Depth comparison for X-ray effect
                float2 screenUV = input.positionNDC.xy / input.positionNDC.w;
                float rawDepth = SampleSceneDepth(screenUV);
                float sceneDepth = LinearEyeDepth(rawDepth, _ZBufferParams);
                float pixelDepth = LinearEyeDepth(input.positionCS.z, _ZBufferParams);
                
                // Check if object is behind something
                float depthDiff = sceneDepth - pixelDepth;
                float isOccluded = step(_DepthBias, depthDiff);
                
                // Calculate X-ray intensity
                float depthFactor = saturate(depthDiff / _Thickness);
                float xrayIntensity = fresnel * isOccluded * smoothstep(0.0, _EdgeSoftness, depthFactor);
                xrayIntensity *= _XRayIntensity;
                
                // Blend between base color and X-ray color
                half3 xrayEffect = _XRayColor.rgb * xrayIntensity;
                half3 finalColor = lerp(baseColor.rgb, xrayEffect, xrayIntensity * _XRayColor.a);
                
                // Add emission
                finalColor += emissionMap.rgb * _EmissionColor.rgb * xrayIntensity;
                
                // Calculate final alpha
                half finalAlpha = lerp(baseColor.a, _XRayColor.a, xrayIntensity);
                finalAlpha = saturate(finalAlpha * (0.5 + xrayIntensity * 0.5));
                
                return half4(finalColor, finalAlpha);
            }
            ENDHLSL
        }
        
        // Depth pass for proper sorting
        Pass
        {
            Name "DepthOnly"
            Tags { "LightMode" = "DepthOnly" }
            
            ZWrite On
            ColorMask 0
            Cull Back
            
            HLSLPROGRAM
            #pragma target 4.5
            #pragma vertex DepthOnlyVertex
            #pragma fragment DepthOnlyFragment
            
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/Shaders/LitInput.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/Shaders/DepthOnlyPass.hlsl"
            ENDHLSL
        }
    }

    FallBack "Hidden/Universal Render Pipeline/FallbackError"
}