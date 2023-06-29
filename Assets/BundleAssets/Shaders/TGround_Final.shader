Shader "Custom/TGround_Final"
{
    Properties
    {
        _Texture1("Texture1", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Geometry+300"}
        LOD 100

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 texcoord : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            CBUFFER_START(UnityPerMaterial)
            float4 _Texture1_ST;
            CBUFFER_END

			TEXTURE2D(_Texture1);	SAMPLER(sampler_Texture1);

            Varyings vert (Attributes input)
            {
                Varyings output;
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);	
                output.uv = TRANSFORM_TEX(input.texcoord, _Texture1);

                return output;
            }

            half4 frag (Varyings input) : SV_Target
            {
                half4 col1 = SAMPLE_TEXTURE2D(_Texture1, sampler_Texture1, input.uv);
                return col1;
            }
            ENDHLSL
        }
    }
}
