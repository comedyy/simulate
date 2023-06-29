Shader "Custom/TSceneItem"
{
    Properties
    {
        [MainTexture] _BaseMap("Albedo", 2D) = "white" {}
        [MainColor] _BaseColor("Color", Color) = (1,1,1,1)

        _BumpScale("Scale", Float) = 1.0
        _BumpMap("Normal Map", 2D) = "bump" {}

        _Cutoff("Alpha Cutoff", Range(0.0, 1.0)) = 0.5
        _Metallic("Metallic", Range(0.0, 1.0)) = 0.0
        _Smoothness("Smoothness", Range(0.0, 1.0)) = 0.0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue" = "Geometry+100"}
        LOD 100

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            #define kDieletricSpec half4(0.04, 0.04, 0.04, 1.0 - 0.04)

            CBUFFER_START(UnityPerMaterial)
            float4 _BaseMap_ST;
            float4 _BumpMap_ST;
            half4 _BaseColor;
            float _BumpScale;
            float _Cutoff;
            float _Metallic;
            float _Smoothness;
            CBUFFER_END

			TEXTURE2D(_BaseMap);	SAMPLER(sampler_BaseMap);
			TEXTURE2D(_BumpMap);	SAMPLER(sampler_BumpMap);

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 texcoord : TEXCOORD0;
                float3 normalOS     : NORMAL;
                float4 tangentOS : TANGENT;
                float2 lightmapUV   : TEXCOORD1;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                DECLARE_LIGHTMAP_OR_SH(lightmapUV, vertexSH, 1);
                float3 normalWS : TEXCOORD2;
                float4 tangentWS : TEXCOORD3;
                //loat3 bitangentWS : TEXCOORD3;
                float3 positionWS : TEXCOORD4;
                float3 viewDirWS                : TEXCOORD5;
            };

            
            half4 SampleMetallicSpecGloss(float2 uv, half albedoAlpha)
            {
                half4 specGloss;
                specGloss.rgb = _Metallic.rrr;
                specGloss.a = albedoAlpha * _Smoothness;

                return specGloss;
            }

            half3 SampleNormal(float2 uv, TEXTURE2D_PARAM(bumpMap, sampler_bumpMap), half scale = 1.0h)
            {
                half4 n = SAMPLE_TEXTURE2D(bumpMap, sampler_bumpMap, uv);
                #if BUMP_SCALE_NOT_SUPPORTED
                    return UnpackNormal(n);
                #else
                    return UnpackNormalScale(n, scale);
                #endif
            }

            Varyings vert (Attributes input)
            {
                Varyings output;
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uv = TRANSFORM_TEX(input.texcoord, _BaseMap);
                output.positionWS = TransformObjectToWorld(input.positionOS.xyz);

                VertexNormalInputs normalInput = GetVertexNormalInputs(input.normalOS, input.tangentOS);

                real sign = input.tangentOS.w * GetOddNegativeScale();
                output.normalWS = normalInput.normalWS;
                output.tangentWS = half4(normalInput.tangentWS.xyz, sign);
                //output.bitangentWS = cross(output.normalWS, output.tangentWS) * sign;
                output.viewDirWS = GetCameraPositionWS() - output.positionWS;

                OUTPUT_LIGHTMAP_UV(input.lightmapUV, unity_LightmapST, output.lightmapUV);
                OUTPUT_SH(output.normalWS.xyz, output.vertexSH);

                return output;
            }

            half4 frag (Varyings input) : SV_Target
            {
                half4 albedoAlpha = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, input.uv);
                half alpha = albedoAlpha.a * _BaseColor.a;
                clip(alpha - _Cutoff);
                half4 specGloss = SampleMetallicSpecGloss(input.uv, albedoAlpha.a);
                half3 albedo = albedoAlpha.rgb * _BaseColor.rgb;

                half metallic = specGloss.r;
                half3 specular = half3(0.0h, 0.0h, 0.0h);
                half smoothness = specGloss.a;
                float3 normalTS = SampleNormal(input.uv, TEXTURE2D_ARGS(_BumpMap, sampler_BumpMap), _BumpScale);

                half3 viewDirWS = SafeNormalize(input.viewDirWS);
                float sgn = input.tangentWS.w;
                float3 bitangent = sgn * cross(input.normalWS.xyz, input.tangentWS.xyz);
                half3 normalWS = TransformTangentToWorld(normalTS, half3x3(input.tangentWS.xyz, bitangent.xyz, input.normalWS.xyz));
                normalWS = NormalizeNormalPerPixel(normalWS);
                half3 bakedGI = SAMPLE_GI(input.lightmapUV, input.vertexSH, normalWS);


                half oneMinusReflectivity = OneMinusReflectivityMetallic(metallic);
                half reflectivity = 1.0 - oneMinusReflectivity;

                half3 diffuse = albedo * oneMinusReflectivity;
                specular = lerp(kDieletricSpec.rgb, albedo, metallic);

                float grazingTerm = saturate(smoothness + reflectivity);
                float perceptualRoughness = PerceptualSmoothnessToPerceptualRoughness(smoothness);
                float roughness = max(PerceptualRoughnessToRoughness(perceptualRoughness), HALF_MIN);
                float roughness2 = roughness * roughness;
                
                Light mainLight = GetMainLight();
                MixRealtimeAndBakedGI(mainLight, normalWS, bakedGI, half4(0, 0, 0, 0));

                half3 reflectVector = reflect(-input.viewDirWS, normalWS);
                half fresnelTerm = Pow4(1.0 - saturate(dot(normalWS, input.viewDirWS)));

                half occlusion = 1.0;
                half3 indirectDiffuse = bakedGI * occlusion;
                half3 indirectSpecular = GlossyEnvironmentReflection(reflectVector, perceptualRoughness, occlusion);
                half3 finalColor = indirectDiffuse * diffuse;
                float surfaceReduction = 1.0 / (roughness2 + 1.0);
                finalColor += surfaceReduction * indirectSpecular * lerp(specular, grazingTerm, fresnelTerm);

                half NdotL = saturate(dot(normalWS, mainLight.direction));
                half3 lightColor = mainLight.color * mainLight.distanceAttenuation;
                half3 radiance = lightColor * (mainLight.distanceAttenuation * mainLight.shadowAttenuation * NdotL);
                finalColor += diffuse * radiance;
                return half4(finalColor, alpha);
            }
            ENDHLSL
        }
    }
}
