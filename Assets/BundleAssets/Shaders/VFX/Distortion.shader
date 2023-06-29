/*
*  场景所有不透明物体渲染完后,  开始截屏做扰动
*/
Shader "VFX/Distortion"
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
        [F4VectorDrawer(UMove_f,10,10,  VMove_f,10,10, DistortFactor_f,0,10,  H,0,0)]
        _Params ("Params", Vector) = (0, 1, 0.04, 0)

        [Toggle(_USE_LIGHT)]_UseLight("Use Light", float)= 0
        [Enum(Rotation)]_RotationBase("Rotation Base", float) = 0
        [Enum(Rotation)]_RotationMask("Rotation Mask", float) = 0
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" "PreviewType"="Plane" "RenderPipeline" = "UniversalPipeline" }
        LOD 100
        
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
        
        // GrabPass
        //{
        //    "_BackgroundTexture"
        //}
        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            
            #pragma shader_feature _RECEIVE_SHADOWS_OFF
            #pragma shader_feature _MAIN_LIGHT_SHADOWS
            #pragma shader_feature _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
            #pragma shader_feature _ _ADDITIONAL_LIGHT_SHADOWS
			#pragma multi_compile _ LINEAR_TO_GAMMA_SPACE
            
            //#include "../../Shaders/CGIncludes/VFXBase.hlsl"
            #include "../../Shaders/CGIncludes/Texture_Rotation.cginc"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #pragma shader_feature _ _USE_LIGHT
			#include "../../Shaders/CGIncludes/FuncUtils.cginc"
            #include "../../Shaders/CGIncludes/URP_Util.hlsl"
            #include "../../Shaders/CGIncludes/CommonFunc.cginc"

            struct Distortion_appdata
            {
                float4 vertex : POSITION;
                half4 color : COLOR;
                half4 uv : TEXCOORD0;

            };

            struct Distortion_v2f
            {
                float4 vertex : SV_POSITION;
                half4 uv : TEXCOORD0;
                half4 grabPos :TEXCOORD1;
                half4 color :TEXCOORD2;
                half4 baseColor : TEXCOORD3;
            };

            TEXTURE2D(_MaskTex);
            SAMPLER(sampler_MaskTex);
            half4 _MaskTex_ST;
            TEXTURE2D(_BaseTex);
            SAMPLER(sampler_BaseTex);
            half4 _BaseTex_ST;
            TEXTURE2D(_CameraGrabTexture);
            SAMPLER(sampler_CameraGrabTexture);

            half4 _BaseColor;
            float4 _SpriteColor;
            float4 _DirectionLightColor;

            float _RotationBase;
            float _RotationMask;
            
            half4 _Params;


            Distortion_v2f vert (Distortion_appdata input)
            {
                Distortion_v2f o;
                o.vertex = TransformObjectToHClip(input.vertex.xyz);
                
                // calc noise uv
                half2 noiseFlowUV;
                noiseFlowUV.xy = _Time.y * _Params.xy;
                
                o.uv.xy = UVRotate(TRANSFORM_TEX(input.uv, _BaseTex) + noiseFlowUV, round(_RotationBase));
                o.uv.zw = UVRotate(TRANSFORM_TEX(input.uv, _MaskTex), round(_RotationMask));
                
                //o.grabPos = ComputeGrabScreenPos(o.vertex);
                o.grabPos = ComputeScreenPos(o.vertex);
                o.color = TransformColorSpace(input.color);
                o.baseColor = TransformColorSpace(_BaseColor);
                return o;
            }

            half4 frag (Distortion_v2f input) : SV_Target
            {
                half4 noiseTex = SAMPLE_TEXTURE2D(_BaseTex, sampler_BaseTex, input.uv.xy);
                half4 maskTex = SAMPLE_TEXTURE2D(_MaskTex, sampler_MaskTex, input.uv.zw);

                input.grabPos.xy += (noiseTex.xy * _Params.z) * maskTex.r * input.baseColor.a * input.color.a;
                half4 bgcolor = SAMPLE_TEXTURE2D(_CameraGrabTexture, sampler_CameraGrabTexture, input.grabPos.xy / input.grabPos.w);
                #ifdef _USE_LIGHT
                    BLEND_OVERLAY_3(bgcolor.rgb)
                    bgcolor.rgb = finalCol;
                #endif
                return bgcolor;
            }
            ENDHLSL
        }
    }
    CustomEditor "VFXShaderEditor.DistortionEditor"
}
