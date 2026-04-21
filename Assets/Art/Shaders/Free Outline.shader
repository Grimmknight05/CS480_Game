Shader "Hidden/Outlines/Free Outline/Outline"
{
    Properties
    {
        _OutlineColor ("Outline Color", Color) = (0,1,0,1)
        _OutlineOccludedColor ("Outline Occluded Color", Color) = (1,0,0,1)

        _OutlineWidth ("Outline Width", Range(0,1)) = 0.5
        _MinimumOutlineWidth ("Minimum Outline Width", Range(0,1)) = 0.5

        _ReferenceResolution ("Reference Resolution", Float) = 1080

        _SrcBlend ("Src Blend", Int) = 5
        _DstBlend ("Dst Blend", Int) = 10

        [Enum(UnityEngine.Rendering.CullMode)] _Cull ("Cull", Float) = 0
        [Enum(UnityEngine.Rendering.CompareFunction)] _ZTest ("ZTest", Float) = 0

        // 🔥 STRIPE ZONE CONTROLS
        _MaskTex ("Mask Texture", 2D) = "white" {}
        _MaskScale ("Mask Scale", Float) = 50
        _MaskStrength ("Mask Strength", Range(0,1)) = 1

        _MaskSpeed ("Mask Speed", Float) = 1
        _StripeDirection ("Stripe Direction", Vector) = (1,1,0,0)

        _StripeSoftness ("Stripe Softness", Range(0.001,1)) = 0.2
        _Alpha ("Outline Alpha", Range(0,1)) = 1
    }

    SubShader
    {
        Tags
        {
            "RenderType"="Transparent"
            "Queue"="Transparent"
            "RenderPipeline"="UniversalPipeline"
        }

        ZWrite Off
        Cull [_Cull]
        ZTest [_ZTest]
        Blend [_SrcBlend] [_DstBlend]

        HLSLINCLUDE
        #pragma fragment frag
        #pragma multi_compile _ OCCLUSION

        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

        #if defined(OCCLUSION)
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"
        #endif

        CBUFFER_START(UnityPerMaterial)
            half4 _OutlineColor;
            half4 _OutlineOccludedColor;

            half _OutlineWidth;
            half _MinimumOutlineWidth;

            float _MaskScale;
            half _MaskStrength;
            float _MaskSpeed;
            float4 _StripeDirection;

            half _StripeSoftness;
            half _Alpha;
        CBUFFER_END

        TEXTURE2D(_MaskTex);
        SAMPLER(sampler_MaskTex);

        struct Attributes
        {
            float4 positionOS : POSITION;
            half3 normalOS : NORMAL;
            UNITY_VERTEX_INPUT_INSTANCE_ID
        };

        struct Varyings
        {
            float4 positionHCS : SV_POSITION;
            float3 positionOS : TEXCOORD0;
            half3 normalOS : TEXCOORD1;
        };

        // ---------------- MASK ----------------
        float SampleStripe(float3 posOS, float3 normalOS)
        {
            float3 p = posOS * _MaskScale;

            float2 dir = normalize(_StripeDirection.xy + 1e-5);
            float angle = atan2(dir.y, dir.x);
            float2x2 rot = float2x2(cos(angle), -sin(angle), sin(angle), cos(angle));

            // --- 3 projections ---
            float2 uvX = mul(p.yz, rot);
            float2 uvY = mul(p.xz, rot);
            float2 uvZ = mul(p.xy, rot);

            uvX.x += _Time.y * _MaskSpeed;
            uvY.x += _Time.y * _MaskSpeed;
            uvZ.x += _Time.y * _MaskSpeed;

            float x = SAMPLE_TEXTURE2D(_MaskTex, sampler_MaskTex, uvX).r;
            float y = SAMPLE_TEXTURE2D(_MaskTex, sampler_MaskTex, uvY).r;
            float z = SAMPLE_TEXTURE2D(_MaskTex, sampler_MaskTex, uvZ).r;

            float3 n = abs(normalOS);
            n /= (n.x + n.y + n.z + 1e-5);

            float mask = x * n.x + y * n.y + z * n.z;

            return mask * _MaskStrength;
        }

        // ---------------- FRAG ----------------
        half4 frag(Varyings IN) : SV_Target
        {
            float mask = SampleStripe(IN.positionOS, IN.normalOS);
            mask = saturate(mask);

            #if defined(OCCLUSION)
            float2 screenUV = IN.positionHCS.xy / _ScaledScreenParams.xy;

            #if UNITY_REVERSED_Z
            real depth = SampleSceneDepth(screenUV);
            #else
            real depth = lerp(UNITY_NEAR_CLIP_VALUE, 1, SampleSceneDepth(screenUV));
            #endif

            half4 col = IN.positionHCS.z < depth ? _OutlineOccludedColor : _OutlineColor;

            col.a *= mask * _Alpha;
            col.rgb *= mask;

            return col;

            #else
            half4 col = _OutlineColor;

            col.a *= mask * _Alpha;
            col.rgb *= mask;

            return col;
            #endif
        }

        ENDHLSL

        Pass
        {
            Name "OUTLINE"

            HLSLPROGRAM
            #pragma vertex vert

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                UNITY_SETUP_INSTANCE_ID(IN);

                half width = _OutlineWidth;

                IN.positionOS.xyz += IN.normalOS * width;

                OUT.positionOS = IN.positionOS.xyz;
                OUT.normalOS = IN.normalOS;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);

                return OUT;
            }
            ENDHLSL
        }
    }
}