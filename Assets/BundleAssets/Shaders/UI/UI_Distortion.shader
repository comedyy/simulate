Shader "Custom/UI/Distortion"
{
    Properties
    {
        _MainTex ("Main Texture", 2D) = "white" {}
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

        [Toggle(_THIRD_D_UI_CLIP)]
        _ThirdDUIClip ("Third D UI Clip", float) = 0
    
        _MaskTex ("Mask Texture", 2D) = "white" {}
        [F4VectorDrawer(UMove_f,10,10,  VMove_f,10,10, DistortFactor_f,0,10,  H,0,0)]
        _Params ("Params", Vector) = (0, 1, 0.04, 0)
        [HideInInspector][Toggle(_USE_LIGHT)]_UseLight("Use Light", float)= 0

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
            
            #pragma shader_feature _ UNITY_UI_CLIP_RECT _THIRD_D_UI_CLIP
            #pragma shader_feature __ _USE_UI_PARTICLE_UV2
            
            //#include "Lib/UIBase.hlsl"
            #include "../CGIncludes/Texture_Rotation.cginc"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "../CGIncludes/URP_Util.hlsl"

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
                
                #if defined(UNITY_UI_CLIP_RECT) || defined(_THIRD_D_UI_CLIP)
                    float2 clipPosition : TEXCOORD5;
                #endif
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            half4 _MainTex_ST;
            TEXTURE2D(_MaskTex);
            SAMPLER(sampler_MaskTex);
            half4 _MaskTex_ST;
            TEXTURE2D(_CameraGrabTexture);
            SAMPLER(sampler_CameraGrabTexture);
            
            half4 _BaseColor;
            half4 _Params;

            float _RotationBase;
            float _RotationMask;


            Distortion_v2f vert (Distortion_appdata input)
            {
                Distortion_v2f o;
                o.vertex = TransformObjectToHClip(input.vertex.xyz);
                
                // calc noise uv
                half2 noiseFlowUV;
                noiseFlowUV.x = _Time.y * _Params.x;
                noiseFlowUV.y = _Time.y * _Params.y;
                
                o.uv.xy = UVRotate(TRANSFORM_TEX(input.uv, _MainTex) + noiseFlowUV, round(_RotationBase));
                o.uv.zw = UVRotate(TRANSFORM_TEX(input.uv, _MaskTex), round(_RotationMask));
                
                o.grabPos = ComputeGrabScreenPos(o.vertex);
                o.color = input.color;
                
                #if defined(UNITY_UI_CLIP_RECT) || defined(_THIRD_D_UI_CLIP)
                    o.clipPosition = GetClipPosition(input.vertex);
                #endif
                return o;
            }

            half4 frag (Distortion_v2f input) : SV_Target
            {
                half4 noiseTex = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv.xy);
                half4 maskTex = SAMPLE_TEXTURE2D(_MaskTex, sampler_MaskTex, input.uv.zw);

                input.grabPos.xy += (noiseTex.xy * _Params.z) * maskTex.r * _BaseColor.a * input.color.a;
                half4 bgcolor = SAMPLE_TEXTURE2D(_CameraGrabTexture, sampler_CameraGrabTexture, input.grabPos.xy / input.grabPos.w);
                
                #if defined(UNITY_UI_CLIP_RECT) || defined(_THIRD_D_UI_CLIP)
                    bgcolor.a *= CalcClipAlpha(input.clipPosition);
                #endif
                
                #if defined(UNITY_UI_CLIP_RECT) || defined(_THIRD_D_UI_CLIP)
                    finalColor.a *= CalcClipAlpha(input.clipPosition);
                #endif
                
                return bgcolor;
            }
            ENDHLSL
        }
    }
    CustomEditor "VFXShaderEditor.DistortionEditor"
}
