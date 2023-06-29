Shader "Custom/UI/Erosion"
{
    Properties
    {
        _MainTex ("Main Texture", 2D) = "white" {}
        [HDR]_BaseColor ("Base Color", Color) = (1,1,1,1)
        
        _Alpha ("Alpha", float) = 0
        
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
        _ErosionTex ("Erosion Texture", 2D) = "white" {}
        _InnerRingErosionColor ("Inner Ring Erosion Color", Color) = (1,1,1,1)
        [F4VectorDrawer(UMove_f,10,10,  VMove_f,10,10, InnerRingErosion_f,0,0.99,  H,0,0)]
        _FlowParams ("Flow Params", Vector) = (0, 1, 0, 0)
        
        _ErosionSoftValue ("Erosion Soft Value", Range(0, 6)) = 0
        [Toggle(_BLEND_ALPHA)]_BlendAlpha("Blend Alpha", float)= 0
        [HideInInspector][Toggle(_USE_LIGHT)]_UseLight("Use Light", float)= 0

        [Enum(Rotation)]_RotationBase("Rotation Base", float) = 0
        [Enum(Rotation)]_RotationMask("Rotation Mask", float) = 0
        [Enum(Rotation)]_RotationErosionTex("Rotation Erosion Tex", float) = 0
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
            
            #pragma shader_feature _ UNITY_UI_CLIP_RECT _THIRD_D_UI_CLIP
            #pragma shader_feature __ _USE_UI_PARTICLE_UV2
            
            #define _USE_VERTEX_COLOR 1
            
            #include "../CGIncludes/Texture_Rotation.cginc"
            #include "../CGIncludes/UIBase.hlsl"
            
            #pragma vertex vert
            #pragma fragment frag
            
            sampler2D _MaskTex;
            half4 _MaskTex_ST;
            
            sampler2D _ErosionTex;
            half4 _ErosionTex_ST;
            
            half4 _FlowParams;
            half _ErosionSoftValue;
            
            half4 _InnerRingErosionColor;
            
            half _Alpha;

            float _RotationBase;
            float _RotationMask;
            float _RotationErosionTex;
            
            struct Erosion_a2v{
                float4 vertex : POSITION;
                
            #ifdef _USE_VERTEX_COLOR
                half4 color : COLOR;
            #endif
                
                half2 uv : TEXCOORD0;
                
                #ifdef _USE_UI_PARTICLE_UV2
                    half4 uv2 : TEXCOORD1;
                    half4 uv3 : TEXCOORD2;
                #else
                    half4 uv2 : TEXCOORD1;
                #endif
                
            };
            
            struct Erosion_v2f{
                float4 vertex : SV_POSITION;
                half2 uv : TEXCOORD0;
                half4 uv2 : TEXCOORD1;
                
            #ifdef _USE_VERTEX_COLOR
                half4 color : TEXCOORD2;
            #endif
            
            #if defined(UNITY_UI_CLIP_RECT) || defined(_THIRD_D_UI_CLIP)
                float2 clipPosition : TEXCOORD5;
            #endif
                
                float4 uvs : TEXCOORD6;
            };
            
            
            half2 GetFlowUV(Erosion_v2f input){
                half2 uv;
                
                uv.xy = _Time.y * _FlowParams.xy + input.uv;
                
                return uv;
            }
            
            Erosion_v2f vert(Erosion_a2v input){
                Erosion_v2f o;
                
                o.vertex = UnityObjectToClipPos(input.vertex);
                o.uv = input.uv;
                #ifdef _USE_UI_PARTICLE_UV2
                    o.uv2 = half4(input.uv2.xy, input.uv3.xy);
                #else
                    o.uv2 = input.uv2;
                #endif

                o.uvs.xy = UVRotate(TRANSFORM_TEX(input.uv, _MaskTex), round(_RotationMask));
                o.uvs.zw = UVRotate(TRANSFORM_TEX(input.uv, _ErosionTex), round(_RotationErosionTex));
                
            #ifdef _USE_VERTEX_COLOR
                o.color = input.color;
            #endif
            
            #if defined(UNITY_UI_CLIP_RECT) || defined(_THIRD_D_UI_CLIP)
                o.clipPosition = GetClipPosition(input.vertex);
            #endif
                
                return o;
            }
            
            half4 frag(Erosion_v2f input) : SV_Target{
                half4 finalColor = (half4) 0;
                half4 vertexColor = input.color;
                half2 uv = input.uv;
                
                half4 albedo = tex2D(_MainTex, UVRotate(TRANSFORM_TEX(GetFlowUV(input), _MainTex), round(_RotationBase)));
                half4 maskTex = tex2D(_MaskTex, input.uvs.xy);
                half4 erosionTex = tex2D(_ErosionTex, input.uvs.zw);
                
                half uv2Lerp = lerp(_ErosionSoftValue, -1.5, input.uv2.z);
                half erosionValue = (erosionTex.r * erosionTex.a * _ErosionSoftValue) - uv2Lerp;
                erosionValue = clamp(erosionValue, 0, 1);
                
                #ifdef _BLEND_ALPHA
                    finalColor.rgb = albedo.rgb * vertexColor.rgb * _BaseColor.rgb;
                    finalColor.a = albedo.a * vertexColor.a * _BaseColor.a * maskTex.a * erosionValue;
                #else
                    finalColor.rgb = albedo.rgb * albedo.a * _BaseColor.rgb * vertexColor.rgb * erosionValue;
                    
                    finalColor.rbg *= maskTex.rgb * _BaseColor.a;
                    finalColor.a = albedo.a * _BaseColor.a * vertexColor.a * erosionValue;
                #endif
                
                half t = step(finalColor.a, _FlowParams.z);
                finalColor.rgb = lerp(finalColor.rgb, _InnerRingErosionColor.rgb, t);
                
                clip(finalColor.a - 0.001);
                
                #if defined(UNITY_UI_CLIP_RECT) || defined(_THIRD_D_UI_CLIP)
                    finalColor.a *= CalcClipAlpha(input.clipPosition);
                #endif
                
                //finalColor = half4(1,0,0, 1);
                
                return finalColor;
            }

            ENDCG
        }
    }
    
    CustomEditor "VFXShaderEditor.ErosionEditor"
}
