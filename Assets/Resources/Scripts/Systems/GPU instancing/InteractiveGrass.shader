Shader "Custom/InteractiveGrass"
{
    Properties
    {
        _MainTex("Grass Texture", 2D) = "white" {}
        _Cutoff("Alpha Cutoff", Range(0,1)) = 0.2

        _TopColor("Top Color", Color) = (0.6, 0.8, 0.3, 1)
        _BottomColor("Bottom Color", Color) = (0.2, 0.4, 0.1, 1)
        _ShadowIntensity("Shadow Intensity", Range(0,1)) = 0.3

        _WindSpeed("Wind Speed", Float) = 2
        _WindStrength("Wind Strength", Float) = 0.5
        _WindFrequency("Wind Frequency", Float) = 1
        _WindNoiseScale("Wind Noise Scale", Float) = 1

        _PlayerInfluenceRadius("Player Influence Radius", Float) = 5
        _PlayerInfluenceStrength("Player Influence Strength", Float) = 1
        _RecoverySpeed("Recovery Speed", Float) = 1
        _InnerRadius("Inner Zone Radius", Float) = 2

        _GrassHeight("Grass Height", Float) = 1
        _BendStiffness("Bend Stiffness", Float) = 1
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="AlphaTest" }
        LOD 200
        Cull Off

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode"="UniversalForward" }

            ZWrite On
            Blend One Zero

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
            #pragma multi_compile _ _SHADOWS_SOFT
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
                float3 normalOS : NORMAL;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 normalWS : TEXCOORD1;
                float grassFactor : TEXCOORD2;
                float4 shadowCoord : TEXCOORD3;
                UNITY_VERTEX_INPUT_INSTANCE_ID
                UNITY_VERTEX_OUTPUT_STEREO
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
                float _Cutoff;
                float4 _TopColor;
                float4 _BottomColor;
                float _ShadowIntensity;
                float _WindSpeed;
                float _WindStrength;
                float _WindFrequency;
                float _WindNoiseScale;
                float _PlayerInfluenceRadius;
                float _PlayerInfluenceStrength;
                float _RecoverySpeed;
                float _InnerRadius;
                float _GrassHeight;
                float _BendStiffness;
            CBUFFER_END

            Varyings vert(Attributes v)
            {
                Varyings o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

                float3 posOS = v.positionOS.xyz;
                o.grassFactor = saturate(posOS.y / _GrassHeight);

                // Wind sway
                float t = _Time.y * _WindSpeed;
                float sway = sin(posOS.x * _WindFrequency + t) * _WindStrength;
                posOS.x += sway * (1.0 - o.grassFactor);

                float3 posWS = TransformObjectToWorld(posOS);
                o.positionHCS = TransformWorldToHClip(posWS);

                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.normalWS = TransformObjectToWorldNormal(v.normalOS);
                o.shadowCoord = TransformWorldToShadowCoord(posWS);
                return o;
            }

            float4 frag(Varyings i) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(i);
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);

                float4 albedo = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv);
                float alpha = (albedo.a > 0.001) ? albedo.a : 1.0;
                clip(alpha - max(0.1, _Cutoff));

                float heightGradient = saturate(i.grassFactor);
                float4 grassColor = lerp(_BottomColor, _TopColor, heightGradient);
                float3 finalColor = albedo.rgb * grassColor.rgb;

                Light mainLight = GetMainLight(i.shadowCoord);
                float3 normalWS = normalize(i.normalWS);
                float NdotL = saturate(dot(normalWS, mainLight.direction) * 0.5 + 0.5);

                float3 lighting = mainLight.color * mainLight.distanceAttenuation * NdotL;
                lighting += unity_AmbientSky.rgb * 0.4;

                float shadowFactor = 1.0 - (1.0 - heightGradient) * _ShadowIntensity * 0.5;
                finalColor *= lighting * shadowFactor;
                finalColor = max(finalColor, finalColor * 0.3);

                return float4(finalColor, alpha);
            }
            ENDHLSL
        }

        // === ShadowCaster corrigido (sem LerpWhiteTo) ===
        Pass
        {
            Name "ShadowCaster"
            Tags { "LightMode"="ShadowCaster" }

            ZWrite On
            ZTest LEqual
            ColorMask 0
            Cull Off

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing
            #pragma multi_compile _ _CASTING_PUNCTUAL_LIGHT_SHADOW
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
                float _Cutoff;
            CBUFFER_END

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            Varyings vert (Attributes v)
            {
                Varyings o;
                UNITY_SETUP_INSTANCE_ID(v);
                float3 posWS = TransformObjectToWorld(v.positionOS.xyz);
                o.positionHCS = TransformWorldToHClip(posWS);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            half4 frag(Varyings i) : SV_Target
            {
                float alpha = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv).a;
                clip(alpha - _Cutoff);
                return 0;
            }
            ENDHLSL
        }
    }

    FallBack Off
}
