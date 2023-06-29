Shader "Custom/UI/UVFlow_CustomData"
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
        _MaskTotalTex ("Mask Total Texture", 2D) = "white" {}
        
        [F4VectorDrawer(BaseUMove_f,10,10, BaseVMove_f,10,10, MaskUMove_f,10,10,  MaskVMove_f,10,10)]
        _FlowParams ("Flow Params", Vector) = (0, 1, 0, 0)
        
        [F4VectorDrawer(Bias_f,1,5,  Scale_f,0,10, Power,0,10,  H,0,0)]
        _FresnelParams ("Fresnel Params", Vector) = (0, 1, 0, 0)
        
        [Toggle(_USE_UV2)]_UseUV2("UseUV2", float)= 0
        [Toggle(_LOOP)]_Loop("Loop", float)= 0
        [Toggle(_FRESNEL)]_Fresnel("Fresnel", float) = 0
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
            #pragma shader_feature _ _USE_UV2
            #pragma shader_feature _ _LOOP
            #pragma shader_feature _ _FRESNEL
            
            #pragma shader_feature _ UNITY_UI_CLIP_RECT
            #pragma shader_feature _ _THIRD_D_UI_CLIP
            #pragma shader_feature __ _USE_UI_PARTICLE_UV2
            
            
            #define _USE_VERTEX_COLOR 1
            
            #include "../CGIncludes/UIBase.hlsl"
            
            #pragma vertex vert
            #pragma fragment frag
            
            sampler2D _MaskTex; 
            half4 _MaskTex_ST;
            
            sampler2D _MaskTotalTex;
            half4 _MaskTotalTex_ST;
            
            half4 _FlowParams;
            
            #ifdef _FRESNEL
                half4 _FresnelParams;
            #endif
            
            
            struct UVFlow_a2v{
                float4 vertex : POSITION;
                
            #ifdef _USE_VERTEX_COLOR
                half4 color : COLOR;
            #endif
            
             #ifdef _FRESNEL
                float3 normalOS : NORMAL;
            #endif
            
            #ifdef _LOOP
                half2 uv : TEXCOORD0;
            #else
                half2 uv : TEXCOORD0;
                
                #ifdef _USE_UI_PARTICLE_UV2
                    half4 uv2 : TEXCOORD1;
                    half4 uv3 : TEXCOORD2;
                #else
                    half4 uv2 : TEXCOORD1;
                #endif
            #endif
                
            };
            
            
            struct UVFlow_v2f{
                float4 vertex : SV_POSITION;
                
                
            #ifdef _LOOP
                float2 uv : TEXCOORD0;
            #else
                float2 uv : TEXCOORD0;
                float4 uv2 : TEXCOORD1;
            #endif
                
            #ifdef _USE_VERTEX_COLOR
                half4 color : TEXCOORD2;
            #endif
            
            #ifdef _FRESNEL
                float3 normalWS : TEXCOORD3;
                float3 positionWS : TEXCOORD4;
            #endif
            
            #if defined(UNITY_UI_CLIP_RECT) || defined(_THIRD_D_UI_CLIP)
                float2 clipPosition : TEXCOORD5;
            #endif
                
            };
            
            float2 GetFlowUV(UVFlow_v2f input){
                float2 uv = (float2) 0;
            #ifdef _LOOP
                uv.x = _Time.y * _FlowParams.x;
                uv.y = _Time.y * _FlowParams.y;
                
                uv += input.uv;
            #elif _USE_UV2
                uv = input.uv + input.uv2.xy;
            #else
                uv = input.uv;
            #endif    
            
                return uv;
            }
            
            UVFlow_v2f vert(UVFlow_a2v input){
                UVFlow_v2f o;
                
                o.vertex = UnityObjectToClipPos(input.vertex);
                
            #ifdef _LOOP
                o.uv = input.uv;
            #else
                o.uv = input.uv;
                
                #ifdef _USE_UI_PARTICLE_UV2
                    o.uv2 = half4(input.uv2.xy, input.uv3.xy);
                #else
                    o.uv2 = input.uv2;
                #endif
            #endif
                
            #ifdef _USE_VERTEX_COLOR
                o.color = input.color;
            #endif
            
            #ifdef _FRESNEL
                o.normalWS = UnityObjectToWorldNormal(input.normalOS);
                o.positionWS = mul(unity_ObjectToWorld, input.vertex).xyz;
            #endif
            
            #if defined(UNITY_UI_CLIP_RECT) || defined(_THIRD_D_UI_CLIP)
                o.clipPosition = GetClipPosition(input.vertex);
            #endif
                
                return o;
            }
            
            half4 frag(UVFlow_v2f input) : SV_Target{
                half4 finalColor = (half4) 0;
                half4 vertexColor = input.color;
                float2 uv = input.uv;
                
                half4 albedo = SampleAlbedo(GetFlowUV(input));
                half4 maskTex = tex2D(_MaskTex,TRANSFORM_TEX(uv, _MaskTex) + _FlowParams.zw * _Time.y);
                
                half4 maskTotalTex = tex2D(_MaskTotalTex,TRANSFORM_TEX(uv, _MaskTotalTex));
                
                #ifdef _BLEND_ALPHA
                    finalColor.rgb = albedo.rgb * vertexColor.rgb * _BaseColor.rgb;
                    finalColor.a = albedo.a * vertexColor.a * _BaseColor.a * maskTex.a; 
                    
                #else
                    finalColor.rgb = albedo.rgb * albedo.a * _BaseColor.rgb * vertexColor.rgb * vertexColor.a;
                    
                    finalColor.rbg *= maskTex.rgb * _BaseColor.a;
                    finalColor.a = albedo.a * vertexColor.a * _BaseColor.a;
                #endif
                
                #ifdef _FRESNEL
                    float3 viewDirWS = normalize(UnityWorldSpaceViewDir(input.positionWS));
                    float3 normalWS = input.normalWS;
                    float NDotV = dot(normalWS, viewDirWS);
                    float fresnel = _FresnelParams.x + (_FresnelParams.y * pow(1 - NDotV, _FresnelParams.z));
                    finalColor.rgb *= fresnel;
                #endif
                
                finalColor *= maskTotalTex;
                
                #if defined(UNITY_UI_CLIP_RECT) || defined(_THIRD_D_UI_CLIP)
                    finalColor.a *= CalcClipAlpha(input.clipPosition);
                #endif
                
                return finalColor;
            }

            ENDCG
        }
    }
    
    CustomEditor "VFXShaderEditor.UVFlowEditor"
}
