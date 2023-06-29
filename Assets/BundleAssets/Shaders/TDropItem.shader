Shader "Custom/TDropItem"
{
    Properties
    {
        _MainTex ("Main Texture", 2D) = "white" {}
        [HDR]_BaseColor ("Base Color", Color) = (1,1,1,1)
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Geometry+200"}
        LOD 300

        Pass
        {
            Tags {"LightMode" = "UniversalForward"}

            HLSLPROGRAM
            #pragma target 3.0
            #pragma exclude_renderers nomrt

            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing

            #pragma instancing_options procedural:setup

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            CBUFFER_START(UnityPerMaterial)
            float4 _MainTex_ST;
            half4 _BaseColor;
            CBUFFER_END

			TEXTURE2D(_MainTex);	SAMPLER(sampler_MainTex);

            StructuredBuffer<float3> objectPositionBuffer;
            StructuredBuffer<float4> objectRotationBuffer;
            //StructuredBuffer<float3> objectScaleBuffer;

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 texcoord : TEXCOORD0;

                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;

                UNITY_VERTEX_INPUT_INSTANCE_ID
                UNITY_VERTEX_OUTPUT_STEREO
            };            

            void setup()
			{
	#ifdef UNITY_PROCEDURAL_INSTANCING_ENABLED
				unity_ObjectToWorld = float4x4(1, 0, 0, 0,
					0, 1, 0, 0,
					0, 0, 1, 0,
					0, 0, 0, 1);
				unity_WorldToObject = unity_ObjectToWorld;

				unity_WorldToObject._14_24_34 *= -1;
				unity_WorldToObject._11_22_33 = 1.0f / unity_WorldToObject._11_22_33;
	#endif
			}
            
            inline float4 QuaternionMul(float4 v, float4 q)
			{
				v = float4(v.xyz + 2 * cross(q.xyz, cross(q.xyz, v.xyz) + q.w * v.xyz), v.w);
				return v;
			}

            Varyings vert(Attributes input)
            {
                Varyings output = (Varyings)0;
				UNITY_SETUP_INSTANCE_ID(input);
				UNITY_TRANSFER_INSTANCE_ID(input, output);

#ifdef UNITY_PROCEDURAL_INSTANCING_ENABLED
                float4 positionOS = QuaternionMul(input.positionOS, objectRotationBuffer[unity_InstanceID]);
                //positionOS = positionOS * float4(objectScaleBuffer[unity_InstanceID], 1.0);
                float3 positionWS = TransformObjectToWorld(positionOS.xyz);
                positionWS = positionWS + objectPositionBuffer[unity_InstanceID].xyz;
                output.positionCS = TransformWorldToHClip(positionWS);
#else
				float3 positionWS = TransformObjectToWorld(input.positionOS.xyz);
				output.positionCS = TransformWorldToHClip(positionWS);
#endif
                output.uv = TRANSFORM_TEX(input.texcoord, _MainTex);

				return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(input);

                half4 color = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv);
                color.rgb - color.rgb * _BaseColor.rgb;
                color.a = color.a * _BaseColor.a;

                return color;
            }

            ENDHLSL
        }
    }
}
