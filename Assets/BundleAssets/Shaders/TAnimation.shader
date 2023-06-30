﻿Shader "Custom/TAnimation"
{
    Properties
    {
        _MainTex("Texture", 2D) = "white" {}
        _Color("Color", Color) = (1.0,1.0,1.0,1.0)

        _AnimationTexture0("Animation Texture0", 2D) = "white" {}
        _AnimationTexture1("Animation Texture0", 2D) = "white" {}
        _AnimationTexture2("Animation Texture0", 2D) = "white" {}

        _Brightness("Brightness (明度)", Range(0, 10)) = 1.0
        _Saturation("Saturation (饱和度)", Range(0, 2)) = 1.0

        _DissolveTex("DissolveTex", 2D) = "white" {}
        [HDR]_DissolveEdgeColor("Dissolve Edge Color", Color) = (0.0, 0.0, 0.0, 0.0)
        _Smoothness("Smoothness", Range(0, 1)) = 1.0
        _Clip("Clip", Range(0,1)) = 0
        _ColorHit("ColorHit", Color) = (0.0,0.0,0.0,0.0)

        // _DissolveTime("Dissolve Time (死亡溶解持续时间)", Range(0.1, 10)) = 1
        // _DissolveScale("Dissolve Scale (死亡模型缩放比例)", Range(0, 10)) = 1
        
        _DissolveAppearTex("DissolveAppearTex (溶解出现mask)", 2D) = "white" {}
        _DissolveAppearOffset("Dissolve Appear Offset (溶解出现)", Range(-1, 10)) = 0
        _DissolveAppearWidth("Dissolve Appear Control 1 (溶解出现)", Range(0, 0.15)) = 0
        _DissolveAppearControl("Dissolve Appear Control 2 (溶解出现)", Range(0, 0.35)) = 0.1
        [HDR]_DissolveAppearEdgeColor("Dissolve Appear Edge Color (溶解出现 颜色)", Color) = (1.0, 1.0, 1.0, 1.0)

    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Geometry+200"}
        LOD 100

        Pass
        {
            Tags {"LightMode" = "UniversalForward"}

            HLSLPROGRAM
            #pragma target 3.0
            #pragma exclude_renderers nomrt

            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing

            #pragma multi_compile _ DISSOLVE 
            #pragma multi_compile _ DISSOLVE_APPEAR

            #pragma instancing_options procedural:setup

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            CBUFFER_START(UnityPerMaterial)
            float4 _MainTex_ST;
            float4 _DissolveTex_ST;
            float4 _DissolveAppearTex_ST;
            float _Brightness;
            float _Saturation;
            half4 _DissolveEdgeColor;
            float _Smoothness;
            float _Clip;
            float4 _ColorHit;

            float _DissolveAppearOffset;
            float _DissolveAppearWidth;
            float _DissolveAppearControl;
            half4 _DissolveAppearEdgeColor;
            CBUFFER_END

			TEXTURE2D(_MainTex);	        SAMPLER(sampler_MainTex);
            TEXTURE2D(_DissolveTex);        SAMPLER(sampler_DissolveTex);
            TEXTURE2D(_DissolveAppearTex);  SAMPLER(sampler_DissolveAppearTex);
			TEXTURE2D(_AnimationTexture0);	SAMPLER(sampler_AnimationTexture0);
			TEXTURE2D(_AnimationTexture1);	SAMPLER(sampler_AnimationTexture1);
			TEXTURE2D(_AnimationTexture2);	SAMPLER(sampler_AnimationTexture2);

            StructuredBuffer<float4> objectPositionBuffer;
            StructuredBuffer<float4> objectRotationBuffer;
            StructuredBuffer<float3> textureCoordinateBuffer;
            StructuredBuffer<float4> emissionColorBuffer;
            StructuredBuffer<float3> scaleBuffer;

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
                float2 boneIDs : TEXCOORD1;
                float2 boneInfluences : TEXCOORD2;

                //float2 boneIDAndInfluences : TEXCOORD3;

                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float4 uv : TEXCOORD0;
                float2 offset : TEXCOORD1;
                float2 uv1 : TEXCOORD2;

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

            inline float4x4 CreateMatrix(float texturePosition, float boneID)
            {
                float2 uv = float2(texturePosition, boneID);
                float4 row0 = SAMPLE_TEXTURE2D_LOD(_AnimationTexture0, sampler_AnimationTexture0, uv, 0);
                float4 row1 = SAMPLE_TEXTURE2D_LOD(_AnimationTexture1, sampler_AnimationTexture0, uv, 0);
                float4 row2 = SAMPLE_TEXTURE2D_LOD(_AnimationTexture2, sampler_AnimationTexture0, uv, 0);

                float4x4 reconstructedMatrix = float4x4(row0, row1, row2, float4(0, 0, 0, 1));

                return reconstructedMatrix;
            }

            Varyings vert(Attributes input)
            {
				Varyings output = (Varyings)0;
				UNITY_SETUP_INSTANCE_ID(input);
				UNITY_TRANSFER_INSTANCE_ID(input, output);

#ifdef UNITY_PROCEDURAL_INSTANCING_ENABLED
                float3 animationTextureCoord = textureCoordinateBuffer[unity_InstanceID];

                float4x4 firstBoneMatrix0 = CreateMatrix(animationTextureCoord.x, input.boneIDs.x);
                float4x4 firstBoneMatrix1 = CreateMatrix(animationTextureCoord.y, input.boneIDs.x);
                float4x4 firstBoneMatrix = firstBoneMatrix0 * (1 - animationTextureCoord.z) + firstBoneMatrix1 * animationTextureCoord.z;

                float4x4 secondBoneMatrix0 = CreateMatrix(animationTextureCoord.x, input.boneIDs.y);
                float4x4 secondBoneMatrix1 = CreateMatrix(animationTextureCoord.y, input.boneIDs.y);
                float4x4 secondBoneMatrix = secondBoneMatrix0 * (1 - animationTextureCoord.z) + secondBoneMatrix1 * animationTextureCoord.z;
                
                // float4x4 thirdBoneMatrix0 = CreateMatrix(animationTextureCoord.x, input.boneIDAndInfluences.x);
                // float4x4 thirdBoneMatrix1 = CreateMatrix(animationTextureCoord.y, input.boneIDAndInfluences.x);
                // float4x4 thirdBoneMatrix = thirdBoneMatrix0 * (1 - animationTextureCoord.z) + thirdBoneMatrix1 * animationTextureCoord.z;

                float4x4 combinedMatrix = firstBoneMatrix * input.boneInfluences.x + secondBoneMatrix * input.boneInfluences.y;// + thirdBoneMatrix * input.boneIDAndInfluences.y;
                float4 skinnedVertex = mul(combinedMatrix, input.positionOS);

                //skinnedVertex *= objectPositionBuffer[unity_InstanceID].w;
                float4 objectPosition = objectPositionBuffer[unity_InstanceID];

    #ifdef DISSOLVE
                skinnedVertex = (skinnedVertex) * float4(scaleBuffer[unity_InstanceID], 1.0);
    #endif

    #ifdef DISSOLVE_APPEAR
                output.offset.x = objectPosition.w - skinnedVertex.y;
                output.offset.y = skinnedVertex.y - objectPosition.w;
    #endif

                float4 positionWS = QuaternionMul(skinnedVertex, objectRotationBuffer[unity_InstanceID]);
				positionWS.xyz = positionWS + objectPosition.xyz;
				output.positionCS = TransformWorldToHClip(positionWS.xyz);
#else
				float3 positionWS = TransformObjectToWorld(input.positionOS.xyz);
				output.positionCS = TransformWorldToHClip(positionWS);

                float3 normalizePosition = input.positionOS.xyz;
                output.offset.x = _DissolveAppearOffset + normalizePosition.x;
                output.offset.y =  -normalizePosition.x - _DissolveAppearOffset;
#endif
                output.uv.xy = TRANSFORM_TEX(input.uv, _MainTex);
                output.uv.zw = TRANSFORM_TEX(input.uv, _DissolveTex);
                output.uv1 = TRANSFORM_TEX(input.uv, _DissolveAppearTex);

				return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(input);

                half4 color = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv);
#ifdef UNITY_PROCEDURAL_INSTANCING_ENABLED
                half4 emissionColor = emissionColorBuffer[unity_InstanceID];

                #ifdef DISSOLVE
                    color.rgb = color.rgb * _Brightness;
                    half gray = 0.2125 * color.r + 0.7154 * color.g + 0.0721 * color.b;
                    half3 grayColor = half3(gray, gray, gray);
                    color.rgb = lerp(grayColor, color.rgb, _Saturation);

                    half dissove = SAMPLE_TEXTURE2D(_DissolveTex, sampler_DissolveTex, input.uv.zw).r;
                    half dissolve_alpha = step(emissionColor.w, dissove);
                    clip(dissolve_alpha - 0.5);
                    float edge_area = saturate(1 - saturate((dissove - emissionColor.w) / _Smoothness));
                    edge_area *= step(0.0001, emissionColor.w);
                    edge_area *= step(dissove, emissionColor.w + 0.2);
                    color.rgb = color.rgb * (1 - edge_area) + _DissolveEdgeColor.rgb * edge_area;

                    //color = half4(0,0,0,1.0);

                #endif

                #ifdef DISSOLVE_APPEAR
                    //clip(input.offset.x);
                    half dissolve = SAMPLE_TEXTURE2D(_DissolveAppearTex, sampler_DissolveAppearTex, input.uv1).r;
                    dissolve += (input.offset.x - _DissolveAppearWidth);
                    half dissolveAppear_alpha = step(0, dissolve);
                    clip(dissolveAppear_alpha - 0.5);
                    float edge_areaAppear = saturate(1 - saturate((dissolve - _DissolveAppearControl) / _Smoothness));
                    edge_areaAppear *= step(0.0001, _DissolveAppearControl);
                    edge_areaAppear *= step(dissolve, _DissolveAppearControl + 0.05);
                    color.rgb = color.rgb * (1 - edge_areaAppear) + _DissolveAppearEdgeColor.rgb * edge_areaAppear;
                #endif
#else
                half3 emissionColor = half3(0.0f, 0.0f, 0.0f);

                #ifdef DISSOLVE
                    color.rgb = color.rgb * _Brightness;
                    half gray = 0.2125 * color.r + 0.7154 * color.g + 0.0721 * color.b;
                    half3 grayColor = half3(gray, gray, gray);
                    color.rgb = lerp(grayColor, color.rgb, _Saturation);

                    half dissove = SAMPLE_TEXTURE2D(_DissolveTex, sampler_DissolveTex, input.uv.zw).r;
                    half dissolve_alpha = step(_Clip, dissove);
                    clip(dissolve_alpha - 0.5);
                    //clip(dissolve_alpha);
                    float edge_area = saturate(1 - saturate((dissove - _Clip) / _Smoothness));
                    edge_area *= step(0.0001, _Clip);
                    edge_area *= step(dissove, _Clip + 0.05);
                    color.rgb = color.rgb * (1 - edge_area) + _DissolveEdgeColor.rgb * edge_area;
                #endif


                #ifdef DISSOLVE_APPEAR
                    //clip(input.offset.x);
                    half dissolve = SAMPLE_TEXTURE2D(_DissolveAppearTex, sampler_DissolveAppearTex, input.uv1).r;
                    dissolve += (input.offset.x - _DissolveAppearWidth);
                    half dissolveAppear_alpha = step(0, dissolve);
                    clip(dissolveAppear_alpha - 0.5);
                    float edgeAppear_area = saturate(1 - saturate((dissolve - _DissolveAppearControl) / _Smoothness));
                    edgeAppear_area *= step(0.0001, _DissolveAppearControl);
                    edgeAppear_area *= step(dissolve, _DissolveAppearControl + 0.05);
                    color.rgb = color.rgb * (1 - edgeAppear_area) + _DissolveAppearEdgeColor.rgb * edgeAppear_area;
                #endif

                //color = half4(1.0, 0.0, 0.0, 1.0);
#endif
                color.rgb = color.rgb + emissionColor + _ColorHit;
                return color;
                //return half4(1.0, 0.0, 0.0, 1.0);
            }


            ENDHLSL
        }
    }
}
