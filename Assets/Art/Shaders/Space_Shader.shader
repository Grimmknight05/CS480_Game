Shader "Custom/Space_Shader"
{
    Properties
    {
        _StarDensity ("Star Density", Range(0.8, 0.999)) = 0.97
        _StarIntensity ("Star Intensity", Range(1, 50)) = 20

        _EmissionColor ("Emission Color", Color) = (1,1,1,1)
        _EmissionStrength ("Emission Strength", Range(0, 5)) = 1.5

        _StarScale ("Star Scale", Range(10, 10000)) = 200
    }

    SubShader
    {
        Tags { "Queue"="Background" "RenderType"="Background" }
        Cull Off ZWrite Off Fog { Mode Off }

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            float _StarDensity;
            float _StarIntensity;
            float _StarScale;

            float4 _EmissionColor;
            float _EmissionStrength;

            struct appdata
            {
                float4 vertex : POSITION;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float3 dir : TEXCOORD0;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);

                // stable world direction
                float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                o.dir = worldPos;

                return o;
            }

            // ⭐ stable hash (less precision-sensitive)
            float hash31(float3 p)
            {
                p = frac(p * 0.1031);
                p += dot(p, p.yzx + 33.33);
                return frac((p.x + p.y) * p.z);
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float3 dir = normalize(i.dir);

                // scale sky into discrete stable space
                float3 p = dir * _StarScale;

                // IMPORTANT: quantize BEFORE hashing (prevents flicker)
                float3 cell = floor(p);

                float n = hash31(cell);

                // star mask
                float star = step(_StarDensity, n);

                // sharpen stars
                star *= _StarIntensity * n;

                float3 col = star;

                // emission
                float3 emission = col * _EmissionColor.rgb * _EmissionStrength;

                return float4(col + emission, 1);
            }
            ENDCG
        }
    }
}