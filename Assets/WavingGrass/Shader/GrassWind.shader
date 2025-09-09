Shader "Custom/GrassWindBillboard"
{
    Properties
    {
        _MainTex("Grass Texture", 2D) = "white" {}
        _Color("Color Tint", Color) = (1,1,1,1)
        _AlphaCutoff("Alpha Cutoff", Range(0,1)) = 0.5

        _RampTex("Ramp", 2D) = "white" {}
        _WaveSpeed("Wave Speed", float) = 7.0
        _WaveAmp("Wave Amp", float) = 1.2
        _HeightFactor("Height Factor", float) = 1.5
        _HeightCutoff("Height Cutoff", float) = 0.1
        _WindTex("Wind Texture", 2D) = "white" {}
        _WorldSize("World Size", vector) = (40, 40, 0, 0)
        _WindSpeed("Wind Speed", vector) = (1.5, 1.5, 0, 0)
        _YOffset("Y offset", float) = 0.0
        _MaxWidth("Max Displacement Width", Range(0, 2)) = 0.1
        _Radius("Radius", Range(0,5)) = 1
        _Brightness("Brightness", Range(0,20)) = 1.8
        _Emission("Emission", Color) = (0,0,0,0)
    }

    SubShader
    {
        Tags { "Queue"="AlphaTest" "RenderType"="TransparentCutout" }
        Cull Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing
            #include "UnityCG.cginc"
            #include "Lighting.cginc"

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _Color;
            float _AlphaCutoff;

            sampler2D _RampTex;
            sampler2D _WindTex;
            float4 _WindTex_ST;

            float4 _WorldSize;
            float _WaveSpeed;
            float _WaveAmp;
            float _HeightFactor;
            float _HeightCutoff;
            float2 _WindSpeed;

            float _MaxWidth;
            float _Radius;
            float _YOffset;

            float _Brightness;
            float4 _Emission;

            uniform float3 _Positions[100];
            uniform float _PositionArray;

            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float3 normal : NORMAL;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            v2f vert(appdata v)
            {
                v2f o;
                
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_TRANSFER_INSTANCE_ID(v, o);
                
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);

                float4 worldPos = mul(unity_ObjectToWorld, v.vertex);

                float2 samplePos = worldPos.xz / _WorldSize.xy;
                samplePos += _Time.x * _WindSpeed.xy;
                samplePos = frac(samplePos);

                float windSample = tex2Dlod(_WindTex, float4(samplePos, 0, 0));

                float heightFactor = v.vertex.y > _HeightCutoff;
                heightFactor = heightFactor * pow(abs(v.vertex.y), _HeightFactor);

                float interactionFactor;

                v.vertex.x += cos(_Time.y * _WaveSpeed + worldPos.x) * windSample * _WaveAmp * heightFactor;

                for (int i = 0; i < _PositionArray; i++) {
                    float3 dis = distance(_Positions[i], worldPos);
                    float radius = 1 - saturate(dis / _Radius);
                    float3 sphereDisp = normalize(worldPos - _Positions[i]) * radius * _MaxWidth;

                    v.vertex.xz += sphereDisp.xz * heightFactor;
                }

                o.pos = UnityObjectToClipPos(v.vertex);
                float4 normal4 = float4(v.normal, 0.0);
                o.normal = normalize(mul(normal4, unity_WorldToObject).xyz);

                return o;
            }

            float4 frag(v2f i) : COLOR
            {
                UNITY_SETUP_INSTANCE_ID(i);
                
                float4 tex = tex2D(_MainTex, i.uv);
                clip(tex.a - _AlphaCutoff);

                float3 lightDir = normalize(_WorldSpaceLightPos0.xyz);
                float ramp = clamp(dot(i.normal, lightDir), 0.001, 1.0);
                float3 lighting = tex2D(_RampTex, float2(ramp, 0.5)).rgb;

                float3 rgb = tex.rgb * _LightColor0.rgb * _Brightness * lighting * _Color.rgb + _Emission.xyz;
                return float4(rgb, 1);
            }

            ENDCG
        }
    }
}