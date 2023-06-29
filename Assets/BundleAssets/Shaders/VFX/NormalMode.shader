Shader "VFX/NormalMode"
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
        [Toggle(_BLEND_ALPHA)]_BlendAlpha("Blend Alpha", float)= 0

        [Toggle(_USE_LIGHT)]_UseLight("Use Light", float)= 0

        [Enum(Rotation)]_RotationBase("Rotation Base", float) = 0
        [Enum(Rotation)]_RotationMask("Rotation Mask", float) = 0
    }
    
    SubShader
    {
        Tags { "Queue"="Transparent"  "RenderType"="Transparent" }
        LOD 100

        Pass
        {
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
        
            CGPROGRAM
            
            #pragma shader_feature _RECEIVE_SHADOWS_OFF
            #pragma shader_feature _MAIN_LIGHT_SHADOWS
            #pragma shader_feature _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
            #pragma shader_feature _ _ADDITIONAL_LIGHT_SHADOWS
            
            #pragma shader_feature _ _BLEND_ALPHA
            #pragma shader_feature _ _USE_LIGHT
			#pragma multi_compile _ LINEAR_TO_GAMMA_SPACE
            
            #define _USE_VERTEX_COLOR 1
            
            #pragma shader_feature _USE_EDITOR_LIGHT
            #include "../../Shaders/CGIncludes/Texture_Rotation.cginc"
            #include "../../Shaders/CGIncludes/VFXBase.hlsl"
            #include "../../Shaders/CGIncludes/FuncUtils.cginc"
            #include "../../Shaders/CGIncludes/CommonFunc.cginc"
            
            #pragma vertex vert
            #pragma fragment frag


            sampler2D _MaskTex;
            half4 _MaskTex_ST;
            float4 _SpriteColor;
            float4 _DirectionLightColor;

            float _RotationBase;
            float _RotationMask;


            v2f vert(a2v v) {
                v2f o;

                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv.xy = UVRotate(TRANSFORM_TEX(v.uv, _BaseTex), round(_RotationBase));
                o.uv.zw = UVRotate(TRANSFORM_TEX(v.uv, _MaskTex), round(_RotationMask));

#ifdef _USE_VERTEX_COLOR
                o.color = v.color;
#endif

                return o;
            }

            
            half4 frag(v2f input) : SV_Target{
                half4 finalColor = (half4) 0;
                half4 vertexColor = TransformColorSpace(input.color);
                half4 albedo = tex2D(_BaseTex, input.uv.xy);
                half4 maskTex = tex2D(_MaskTex, input.uv.zw);

                half4 baseColor = TransformColorSpace(_BaseColor);
                
                #ifdef _BLEND_ALPHA
                    finalColor.rgb = albedo.rgb * vertexColor.rgb * baseColor.rgb;
                    finalColor.a = albedo.a * vertexColor.a * baseColor.a * maskTex.r; 
                #else
                    finalColor.rgb = albedo.rgb * albedo.a * baseColor.rgb * vertexColor.rgb * vertexColor.a;
                    finalColor.rbg *= maskTex.rgb * baseColor.a;
                    
                    finalColor.a = albedo.a * vertexColor.a * baseColor.a;
                #endif

                #ifdef _USE_LIGHT
                    BLEND_OVERLAY_3(finalColor.rgb)
                    finalColor.rgb = finalCol;
                #endif
                
                return finalColor;
            }

            ENDCG
        }
    }
    
    CustomEditor "VFXShaderEditor.NormalModeEditor"
}
