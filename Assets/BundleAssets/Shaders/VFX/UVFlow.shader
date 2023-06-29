Shader "VFX/UVFlow"
{
    Properties
    {
        _BaseTex ("Base Texture", 2D) = "white" {}
        [HDR]_BaseColor ("Base Color", Color) = (1,1,1,1)
        
        // Render Type
        [BzbeeEnumDrawer(ShaderEditor.RenderingType)] _RenderingType ("__RenderingType", float) = 1
        _SrcBlend ("__SrcBlend", float) = 5
        _DstBlend ("__DstBlend", float) = 6
        
        [Toggle]
        _OpenCustomBlendMode ("__OpenCustomBlendMode", float) = 0
        [BzbeeEnumDrawer(ShaderEditor.CustomBlendMode)] _CustomBlendMode ("__CustomBlendMode", float) = 1
        
        _Cull("__cull", Float) = 2.0
        _ColorMask ("__ColorMask", float) = 15
        [Toggle]_ZWrite ("__ZWrite", float) = 0
        _ZTest ("__ZTest", float) = 4
        
        [Toggle]
        _StencilEnable ("__StencilEnable", float) = 0
        _StencilID ("__StencilID", Range(0, 255)) = 0
        _StencilComp ("__StencilComp", float) = 8
        _StencilOp ("__StencilOp", float) = 0
        _StencilWriteMask ("__StencilWriteMask", Range(0, 255)) = 255
        _StencilReadMask ("__StencilReadMask", Range(0, 255)) = 255
        
        [Toggle(_RECEIVE_SHADOWS_OFF)]
        _ReceiveShadows ("Receive Shadows", float) = 1
        
        
        _MaskTex ("Mask Texture", 2D) = "white" {}
        _MaskTotalTex ("Mask Total Texture", 2D) = "white" {}
        
        [F4VectorDrawer(BaseUMove_f,10,10, BaseVMove_f,10,10, MaskUMove_f,10,10,  MaskVMove_f,10,10)]
        //[F4VectorDrawer(BaseUMove_f,10,10, BaseVMove_f,10,10, H,10,10,  H,10,10)]
        _FlowParams ("Flow Params", Vector) = (0, 1, 0, 0)
        
        [F4VectorDrawer(Bias_f,1,5,  Scale_f,0,10, Power,0,10,  H,0,0)]
        _FresnelParams ("Fresnel Params", Vector) = (0, 1, 0, 0)
        
        [Toggle(_USE_UV2)]_UseUV2("UseUV2", float)= 0
        [Toggle(_LOOP)]_Loop("Loop", float)= 0
        [Toggle(_FRESNEL)]_Fresnel("Fresnel", float) = 0

        [Toggle(_USE_LIGHT)]_UseLight("Use Light", float)= 0

        [Enum(Rotation)]_RotationBase("Rotation Base", float) = 0
        [Enum(Rotation)]_RotationMask("Rotation Mask", float) = 0
        [Enum(Rotation)]_RotationMaskTotle("Rotation Mask Totle", float) = 0
    }
    
    SubShader
    {
        Tags { "Queue"="Transparent"  "RenderType"="Transparent" }
        LOD 100
        
        Pass
        {
            Tags {"LightMode" = "UniversalForward"}

            Blend [_SrcBlend][_DstBlend]
            ZWrite [_ZWrite]
            ZTest [_ZTest]
            Cull [_Cull]
            
            ColorMask [_ColorMask]
            
            Stencil{
                Ref [_StencilID]
                Comp [_StencilComp]
                Pass [_StencilOp]
                ReadMask [_StencilReadMask]
                WriteMask [_StencilWriteMask]
            }
        
            HLSLPROGRAM
            
            #pragma shader_feature _RECEIVE_SHADOWS_OFF
            #pragma shader_feature _MAIN_LIGHT_SHADOWS
            #pragma shader_feature _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
            #pragma shader_feature _ _ADDITIONAL_LIGHT_SHADOWS
            
            #pragma shader_feature _ _BLEND_ALPHA
            #pragma shader_feature _ _USE_UV2
            #pragma shader_feature _ _LOOP
            #pragma multi_compile _ _FRESNEL
            #pragma shader_feature _ _USE_LIGHT
            
            #define _USE_VERTEX_COLOR 1
            
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "../../Shaders/CGIncludes/Texture_Rotation.cginc"
            //#include "../../Shaders/CGIncludes/VFXBase.hlsl"
            #include "../../Shaders/CGIncludes/FuncUtils.cginc"
            #include "../../Shaders/CGIncludes/CommonFunc.cginc"
            
            #pragma vertex vert
            #pragma fragment frag
            
            CBUFFER_START(UnityPerMaterial)
            float4 _MaskTex_ST;            
            float4 _MaskTotalTex_ST;
            float4 _BaseTex_ST;

            half4 _BaseColor;
            
            float4 _FlowParams;

            float4 _SpriteColor;
            float4 _DirectionLightColor;
            
            #ifdef _FRESNEL
                float4 _FresnelParams;
            #endif

            float _RotationBase;
            float _RotationMask;
            float _RotationMaskTotle;
            CBUFFER_END

            TEXTURE2D(_BaseTex);	            SAMPLER(sampler_BaseTex);
            TEXTURE2D(_MaskTex);	            SAMPLER(sampler_MaskTex);
            TEXTURE2D(_MaskTotalTex);	        SAMPLER(sampler_MaskTotalTex);
            
            
            struct UVFlow_a2v{
                float4 vertex : POSITION;
                
            #ifdef _USE_VERTEX_COLOR
                half4 color : COLOR;
            #endif
            
             #ifdef _FRESNEL
                float3 normalOS : NORMAL;
            #endif
                
            #ifdef _LOOP
                float2 uv : TEXCOORD0;
            #else
                float2 uv : TEXCOORD0;
                float4 uv2 : TEXCOORD1;
            #endif
                
            };
            
            
            struct UVFlow_v2f
            {
                float4 vertex : SV_POSITION;                
                
            #ifdef _LOOP
                float4 uv : TEXCOORD0;
            #else
                float4 uv : TEXCOORD0;
                float4 uv2 : TEXCOORD1;
            #endif
                
            #ifdef _USE_VERTEX_COLOR
                half4 color : TEXCOORD2;
            #endif
            
            #ifdef _FRESNEL
                float3 normalWS : TEXCOORD3;
                float3 positionWS : TEXCOORD4;
            #endif

                half4 baseColor : TEXCOORD5;

                float4 uvs : TEXCOORD6;
                float2 uvBase : TEXCOORD7;
                
            };
            
            float2 GetFlowUV(UVFlow_v2f input)
            {
                #ifdef _LOOP
                    return input.uv.xy;
                #elif _USE_UV2
                    return input.uv.xy + input.uv2.xy;
                #else
                    return input.uv.xy;
                #endif
            }
            
            UVFlow_v2f vert(UVFlow_a2v input)
            {
                UVFlow_v2f o;
                
                o.vertex = TransformObjectToHClip(input.vertex);
                
                
            #ifdef _LOOP
                o.uv.xy = input.uv;
            #else
                o.uv.xy = input.uv;
                o.uv2 = input.uv2;
            #endif
            
            #ifdef _FRESNEL
                o.normalWS = TransformObjectToWorldNormal(input.normalOS);
                o.positionWS = TransformObjectToWorld(input.vertex);
            #endif

                o.color = input.color;
                o.baseColor = _BaseColor;

                #ifdef _LOOP
                    o.uvBase = TRANSFORM_TEX(GetFlowUV(o), _BaseTex) + _FlowParams.xy * _Time.y;
                #else
                    o.uvBase = TRANSFORM_TEX(GetFlowUV(o), _BaseTex);
                #endif

                o.uvs.xy = UVRotate(TRANSFORM_TEX(input.uv, _MaskTex) + _FlowParams.zw * _Time.y, round(_RotationMask));                

                o.uvs.zw = UVRotate(TRANSFORM_TEX(input.uv, _MaskTotalTex), round(_RotationMaskTotle));
                
                return o;
            }
            
            half4 frag(UVFlow_v2f input) : SV_Target{
                half4 finalColor = (half4) 0;
                half4 vertexColor = input.color;        

                float2 uvBase = UVRotate(input.uvBase, round(_RotationBase));    
                float2 uvMask = input.uvs.xy;   
                float2 uvMaskTotal = input.uvs.zw;
                
                half4 albedo = SAMPLE_TEXTURE2D(_BaseTex, sampler_BaseTex, uvBase);
                half4 maskTex = SAMPLE_TEXTURE2D(_MaskTex, sampler_BaseTex, input.uvs.xy);                
                half4 maskTotalTex = SAMPLE_TEXTURE2D(_MaskTotalTex, sampler_MaskTotalTex, uvMaskTotal);
                
                #ifdef _BLEND_ALPHA
                    finalColor.rgb = albedo.rgb * vertexColor.rgb * input.baseColor.rgb;
                    finalColor.a = albedo.a * vertexColor.a * input.baseColor.a * maskTex.a; 
                    
                #else
                    finalColor.rgb = albedo.rgb * albedo.a * input.baseColor.rgb * vertexColor.rgb * vertexColor.a;
                    
                    finalColor.rbg *= maskTex.rgb * input.baseColor.a;
                    finalColor.a = albedo.a * vertexColor.a * input.baseColor.a;
                #endif
                
                #ifdef _FRESNEL
                    float3 viewDirWS = TransformObjectToWorldDir(input.positionWS);
                    float3 normalWS = input.normalWS;
                    float NDotV = dot(normalWS, viewDirWS);
                    float fresnel = _FresnelParams.x + (_FresnelParams.y * pow(1 - NDotV, _FresnelParams.z));
                    finalColor.rgb *= fresnel;
                #endif

                #ifdef _USE_LIGHT
                    BLEND_OVERLAY_3(finalColor.rgb)
                    finalColor.rgb = finalCol;
                #endif
                
                finalColor *= maskTotalTex;
                
                return finalColor;
            }

            ENDHLSL
        }
    }
    
    CustomEditor "VFXShaderEditor.UVFlowEditor"
}
