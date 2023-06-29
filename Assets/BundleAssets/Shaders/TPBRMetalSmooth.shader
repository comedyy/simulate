Shader "Custom/TPBRMetalSmooth"
{
    Properties
    {
        _Albedo ("_Albedo", 2D) = "white" {}
        _NormalEmission("_Normal Emisstion", 2D) = "white" {}
        _MSAB("_MSAB", 2D) = "white" {}
        _TexEnvironment("_TexEnvironment", 2D) = "white" {}

        _SmoothnessScale("_SmoothnessScale", float) = 1.0
        _MetalnessScale("_MetalnessScale", float) = 1.0
        [HDR]_EmissionColor("_EmissionColor", Color) = (1.0, 1.0, 1.0, 1.0)
        _EmissionIntensity("_EmissionIntensity", float) = 1.0
        _OffsetColor("_OffsetColor", Color) = (0, 0, 0, 0)
        _LerpColor("_LerpColor", Color) = (0, 0, 0, 0)
        [HDR]_TransformColor("_TransformColor", Color) = (1.0, 1.0, 1.0, 1.0)

        _HitColor("HitColor", Color) = (1, 1, 1, 1)

        //_SpecColor("_SpecColor", Color) = (1.0, 1.0, 1.0, 1.0)
        _SpecIntensity("_SpecIntensity", float) = 1.0
        _ShIntensity("_ShIntensity", float) = 1.0
        _Cull("__cull", Float) = 2.0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Geometry+200"}
        LOD 100

        Pass
        {
            Cull[_Cull]

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #pragma shader_feature _ ENABLE_SECONDARY_LIGHT

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float4 tangentOS : TANGENT;
                float2 texcoord : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float4 viewDirWS : TEXCOORD1;
                float4 TtoW0 : TEXCOORD2;
                float4 TtoW1 : TEXCOORD3;
                float4 TtoW2 : TEXCOORD4;
            };

#define GAMMA 2.2
#define ColorSpaceDielectricSpec 0.04
#define SMALL_FLOAT   0.0001

            CBUFFER_START(UnityPerMaterial)
            float4 _Albedo_ST;
            float4 _NormalEmission_ST;
            float4 _MSAB_ST;
            float4 _TexEnvironment_ST;

            float _SmoothnessScale;
            float _MetalnessScale;
            half4 _EmissionColor;
            float _EmissionIntensity;

            half4 _HitColor;

            //half4 _SpecColor;
            half4 _OffsetColor;
            half4 _LerpColor;
            half4 _TransformColor;
            float _SpecIntensity;
            float _ShIntensity;
            CBUFFER_END

            TEXTURE2D(_Albedo);	                        SAMPLER(sampler_Albedo);
            TEXTURE2D(_NormalEmission);	                SAMPLER(sampler_NormalEmission);
            TEXTURE2D(_MSAB);	                        SAMPLER(sampler_MSAB);
            TEXTURE2D(_TexEnvironment);	                SAMPLER(sampler_TexEnvironment);


            float3 fresnelTerm(float3 F0 , float cosA)
            {
                float t =pow(1.0-cosA, 5.0);
                return F0 + (1.0-F0) * t;
            }

            float3 fresnelLerp(float3 F0 , float3 F90 , float cosA)
            {
                float t = pow(1.0-cosA, 5.0);
                return lerp(F0, F90, t);
            }
            float disneyDiffuse(float NdotV, float NdotL, float LdotH, float perceptualRoughness)
            {
                float fd90 = 0.5 + 2.0 * LdotH * LdotH * perceptualRoughness;
                float lightScatter   = 1.0 + (fd90 - 1.0) * pow((1.0 - NdotL), 5.0);
                float viewScatter    = 1.0 + (fd90 - 1.0) * pow((1.0 - NdotV), 5.0);
                return lightScatter * viewScatter;
            }
            float smithJointGGXVisibilityTerm(float NdotL, float NdotV, float roughness)
            {
                float a = roughness;
                float lambdaV = NdotL * (NdotV * (1.0 - a) + a);
                float lambdaL = NdotV * (NdotL * (1.0 - a) + a);
                float totalLambda = lambdaV + lambdaL;
                return ( totalLambda * 10000.0 > SMALL_FLOAT ) ? 0.5 / totalLambda : 10000.0;
            }
            float GGXterm (float NdotH, float roughness)
            {
                float a2 = roughness * roughness;
                float d = (NdotH * a2 - NdotH) * NdotH + 1.0;
                float d2 = d*d;
                
                return (d2 * 10000.0) > SMALL_FLOAT ? (INV_PI * a2 / d2) : 10000.0;
            }
            float3 brdf_PBS(	float3 diffColor, float3 specColor, float oneMinusReflectivity, float smoothness,
                                float3 normal, float3 viewDir,
                                float3 brdf_lightDir, float3 brdf_lightColor, float3 gi_diffuse, float3 gi_specular)
            {
                float perceptualRoughness = 1.0 - smoothness;
                float3 halfDir = normalize(brdf_lightDir + viewDir);
                float nv = abs(dot(normal, viewDir));

                float nl = clamp(dot(normal, brdf_lightDir), 0.0, 1.0);
                float nh = clamp(dot(normal, halfDir), 0.0, 1.0);

                float lv = clamp(dot(brdf_lightDir, viewDir), 0.0, 1.0);
                float lh = clamp(dot(brdf_lightDir, halfDir), 0.0, 1.0);

                float diffuseTerm = disneyDiffuse(nv, nl, lh, perceptualRoughness) * nl;
                float roughness = perceptualRoughness * perceptualRoughness;

                roughness = max(roughness, 0.002);
                float V = smithJointGGXVisibilityTerm (nl, nv, roughness);
                float D = GGXterm (nh, roughness);

                float specularTerm = V * D * PI;

                specularTerm = max(0.0, specularTerm * nl);

                float surfaceReduction = 1.0 / (roughness*roughness + 1.0);

                float grazingTerm = clamp(smoothness + (1.0-oneMinusReflectivity), 0.0, 1.0);
           
                float f = 1.0;

                float3 color =	(diffColor + fresnelTerm (specColor, lh) * float3(specularTerm, specularTerm, specularTerm)) * 
                                float3(diffuseTerm, diffuseTerm, diffuseTerm) * brdf_lightColor * f
                                    + diffColor * gi_diffuse
                                    + float3(surfaceReduction, surfaceReduction, surfaceReduction) * gi_specular * 
                                    fresnelLerp (specColor, float3(grazingTerm, grazingTerm, grazingTerm), nv);
                return color;
            }

            float3 forwardAdd(float3 diffColor, float3 specColor, float oneMinusReflectivity, float smoothness,
                                float3 normal, float3 viewDir, float3 brdf_lightDir, float3 brdf_lightColor)
            {
                float perceptualRoughness = 1.0 - smoothness;
                float3 halfDir = normalize(brdf_lightDir + viewDir);
                float nv = abs(dot(normal, viewDir));


                float nl = clamp(dot(normal, brdf_lightDir), 0.0, 1.0);
                float nh = clamp(dot(normal, halfDir), 0.0, 1.0);

                float lv = clamp(dot(brdf_lightDir, viewDir), 0.0, 1.0);
                float lh = clamp(dot(brdf_lightDir, halfDir), 0.0, 1.0);

                float diffuseTerm = disneyDiffuse(nv, nl, lh, perceptualRoughness) * nl;
                float roughness = max(perceptualRoughness * perceptualRoughness, 0.002);
                float V = smithJointGGXVisibilityTerm (nl, nv, roughness);
                float D = GGXterm (nh, roughness);
                float specularTerm = max(V * D * PI * nl, 0.0);
                float surfaceReduction = 1.0 / (roughness*roughness + 1.0);
                float grazingTerm = clamp(smoothness + (1.0-oneMinusReflectivity), 0.0, 1.0);
                
                float3 color = brdf_lightColor *	(float3(diffuseTerm, diffuseTerm, diffuseTerm) * diffColor
                                    + float3(specularTerm, specularTerm, specularTerm) * fresnelTerm(specColor, lh));
                return color;
            }

            Varyings vert (Attributes input)
            {
                Varyings output = (Varyings)0;
                float3 positionWS = TransformObjectToWorld(input.positionOS.xyz);
                output.positionCS = TransformWorldToHClip(positionWS);
                output.uv = TRANSFORM_TEX(input.texcoord, _Albedo);
                output.viewDirWS.xyz = normalize(GetCameraPositionWS().xyz - positionWS);

                VertexNormalInputs normalInput = GetVertexNormalInputs(input.normalOS, input.tangentOS);
                output.TtoW0 = float4(normalInput.tangentWS.x, normalInput.bitangentWS.x, normalInput.normalWS.x, positionWS.x);
                output.TtoW1 = float4(normalInput.tangentWS.y, normalInput.bitangentWS.y, normalInput.normalWS.y, positionWS.y);
                output.TtoW2 = float4(normalInput.tangentWS.z, normalInput.bitangentWS.z, normalInput.normalWS.z, positionWS.z);

                output.viewDirWS.w = max(dot(output.viewDirWS.xyz, normalInput.normalWS), 0.0); // 菲涅尔分量

                return output;
            }

            half4 frag (Varyings input) : SV_Target
            {
                half4 albedo = SAMPLE_TEXTURE2D(_Albedo, sampler_Albedo, input.uv);
                half4 normalEmission = SAMPLE_TEXTURE2D(_NormalEmission, sampler_NormalEmission, input.uv);
                half4 msab = SAMPLE_TEXTURE2D(_MSAB, sampler_MSAB, input.uv);
                albedo.xyz = pow(albedo.xyz, GAMMA);
                clip(albedo.a - 0.0001);

                float3 positionWS = float3(input.TtoW0.w, input.TtoW1.w, input.TtoW2.w);

                //albedo.xyz = pow(albedo.xyz, float3(2.2));
                float3 normal = normalEmission.xyz * 2.0 - float3(1.0, 1.0, 1.0);
                normal = normalize(float3(dot(input.TtoW0.xyz, normal), dot(input.TtoW1.xyz, normal), dot(input.TtoW2.xyz, normal)));

                float roughness = 1.0 - msab.y * _SmoothnessScale;
                float metalness = msab.x * _MetalnessScale;

                float alpha = albedo.w;
                float3 emissionColor = pow(_EmissionColor.xyz, GAMMA);
                float3 emission = albedo.xyz * normalEmission.w * emissionColor * _EmissionIntensity;

                float3 viewDirWS = normalize(input.viewDirWS.xyz);

                Light mainLight = GetMainLight();
                float3 lightDir = mainLight.direction;
                half3 lightColor = mainLight.color;

                //float ColorSpaceDielectricSpec = 0.04f;
                float3 specularTint = lerp(float3(ColorSpaceDielectricSpec, ColorSpaceDielectricSpec, ColorSpaceDielectricSpec), albedo.rgb, metalness);
                float oneMinusReflectivity = lerp(1.0 - ColorSpaceDielectricSpec, 0.0, metalness);
                albedo *= oneMinusReflectivity;

                float3 reflectionDir = normalize(reflect(-viewDirWS, normal));
                float3 reflection = SAMPLE_TEXTURECUBE(_TexEnvironment, sampler_TexEnvironment, reflectionDir).rgb;
                float3 gi_specular = lerp(reflection.rgb, float3(0.078,0.078,0.078), roughness) * float3(_SpecIntensity,_SpecIntensity,_SpecIntensity);
                float shIntensity = _ShIntensity * 2.0;
                float3 gi_diffuse = SampleSH(normal) * float3(shIntensity, shIntensity, shIntensity);

                float3 final = brdf_PBS(albedo.xyz, specularTint, oneMinusReflectivity, 
                                            1.0-roughness, normal, viewDirWS, 
                                            lightDir, lightColor, 
                                            gi_diffuse * msab.zzz, gi_specular * msab.zzz) /* * msab.zzz */ 
                                + emission + _OffsetColor.xyz;

                // #if ENABLE_SECONDARY_LIGHT
				// final += forwardAdd(albedo.xyz, specularTint, oneMinusReflectivity,
				// 							1.0-roughness, normal, viewDir,
				// 							-u_secondLightDir, u_secondLightColor);
				// final += forwardAdd(albedo.xyz, specularTint, oneMinusReflectivity,
				// 							1.0-roughness, normal, viewDir,
				// 							-u_thirdLightDir, u_thirdLightColor);
		        // #endif

                #ifdef ENABLE_SECONDARY_LIGHT
                uint pixelLightCount = GetAdditionalLightsCount();
                for (uint lightIndex = 0u; lightIndex < pixelLightCount; ++lightIndex)
                {
                    Light light = GetAdditionalLight(lightIndex, positionWS);
                    final += forwardAdd(albedo.xyz, specularTint, oneMinusReflectivity,
                                                1.0-roughness, normal, viewDirWS,
                                                -light.direction, light.color);
                }
                #endif

                final = lerp(final, _LerpColor.xyz, _LerpColor.w);
                final.rgb = pow(final.rgb, 1/GAMMA);

                float4 col = float4(final, alpha) * _TransformColor;
                col *= _HitColor;

                return col;
            }
            ENDHLSL
        }
    }
}
