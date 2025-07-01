Shader "Hidden/HeatmapOverlay"
{
    Properties
    {
        _BackgroundTex ("Background Texture", 2D) = "white" {}
    }
    
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _BackgroundTex;
            float4 _MapBounds;
            float _SphereRadius;
            int _PointCount;
            StructuredBuffer<float3> _Positions;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // Converte UV para posição mundial
                float worldX = lerp(_MapBounds.x, _MapBounds.z, i.uv.x);
                float worldZ = lerp(_MapBounds.y, _MapBounds.w, i.uv.y);
                
                // Cor de fundo
                fixed4 bgColor = tex2D(_BackgroundTex, i.uv);
                
                // Calcula densidade
                float density = 0;
                for(int idx = 0; idx < _PointCount; idx++)
                {
                    float3 pos = _Positions[idx];
                    float dist = distance(float2(worldX, worldZ), pos.xz);
                    
                    if(dist < _SphereRadius)
                    {
                        density += 1.0 - smoothstep(0, _SphereRadius, dist);
                    }
                }
                
                // Aplica gradiente de cor
                density = saturate(density / 15); // Ajuste conforme necessidade
                fixed4 heatColor = lerp(
                    fixed4(0, 1, 0, 0.7),   // Verde transparente
                    fixed4(1, 0, 0, 0.9),   // Vermelho transparente
                    density
                );
                
                // Combina fundo com heatmap
                return lerp(bgColor, heatColor, heatColor.a);
            }
            ENDCG
        }
    }
}