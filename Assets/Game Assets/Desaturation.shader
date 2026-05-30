// URP ile uyumlu Desaturation (Renk Solma) Shader'ı
// Built-in Pipeline'daki OnRenderImage'ın URP karşılığı: DesaturationFeature.cs üzerinden çalışır.

Shader "Custom/Desaturation"
{
    Properties
    {
        _MainTex ("Source Texture", 2D) = "white" {}
        _Amount  ("Desaturation Amount", Range(0.0, 1.0)) = 0.0
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalPipeline" }

        ZWrite Off
        ZTest  Always
        Cull   Off
        Blend  Off

        Pass
        {
            Name "Desaturation"

            HLSLPROGRAM
            #pragma vertex   Vert
            #pragma fragment Frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
                float  _Amount;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv         : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv         : TEXCOORD0;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            Varyings Vert(Attributes IN)
            {
                Varyings OUT;
                UNITY_SETUP_INSTANCE_ID(IN);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);
                OUT.positionCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.uv = IN.uv;
                return OUT;
            }

            half4 Frag(Varyings IN) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(IN);

                half4 col = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.uv);

                // Luminance (parlaklık) hesabı — BT.601 katsayıları
                float lum = dot(col.rgb, half3(0.299h, 0.587h, 0.114h));

                // _Amount: 0 = tam renkli, 1 = tamamen gri
                col.rgb = lerp(col.rgb, half3(lum, lum, lum), _Amount);

                return col;
            }
            ENDHLSL
        }
    }

    FallBack Off
}
