/*
* UI 高斯模糊
*/
Shader "Custom/UI/UI_GaussianBlur"
{
    Properties
    {
        [PerRendererData] _MainTex("Sprite Texture", 2D) = "white" {}
        _Blur("Blur",Range(0,1)) = 0.01
    }

    SubShader
    {
        Tags
        {
            "Queue" = "Transparent"
            "IgnoreProjector" = "True"
            "RenderType" = "Transparent"
            "PreviewType" = "Plane"
        }

        Cull Off
        Lighting Off
        ZWrite Off
        //Blend One OneMinusSrcAlpha
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
			#pragma multi_compile _ LINEAR_TO_GAMMA_SPACE
            
            //#pragma multi_compile_local _MODE_ONE __
            //#pragma multi_compile_local _MODE_TWO __
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            #include "../CGIncludes/CommonFunc.cginc"

            struct appdata_t
            {
                float4 vertex   : POSITION;
                float2 texcoord : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex   : SV_POSITION;
                float2 texcoord  : TEXCOORD0;
            };

            sampler2D _MainTex;
            half4 _MainTex_ST;
            float4 _MainTex_TexelSize;
            float _Blur;


            v2f vert(appdata_t IN)
            {
                v2f OUT;
                OUT.vertex = UnityObjectToClipPos(IN.vertex);
                OUT.texcoord = TRANSFORM_TEX(IN.texcoord, _MainTex);

                return OUT;
            }

            // 采样卷积
            half3 SampleSpriteTexture(float2 uv)
            {
                half3 color = half3(0, 0, 0);

                // 9次卷积
                color += (tex2D(_MainTex, uv + _MainTex_TexelSize.xy * float2(0, 1) * _Blur).rgb) * 0.118318f;
                color += (tex2D(_MainTex, uv + _MainTex_TexelSize.xy * float2(0, -1) * _Blur).rgb) * 0.118318f;
                color += (tex2D(_MainTex, uv + _MainTex_TexelSize.xy * float2(-1, 0) * _Blur).rgb) * 0.118318f;
                color += (tex2D(_MainTex, uv + _MainTex_TexelSize.xy * float2(1, 0) * _Blur).rgb) * 0.118318f;
                color += (tex2D(_MainTex, uv + _MainTex_TexelSize.xy * float2(0, 0) * _Blur).rgb) * 0.147761f;
                color += (tex2D(_MainTex, uv + _MainTex_TexelSize.xy * float2(-1, 1) * _Blur).rgb) * 0.0947416f;
                color += (tex2D(_MainTex, uv + _MainTex_TexelSize.xy * float2(1, 1) * _Blur).rgb) * 0.0947416f;
                color += (tex2D(_MainTex, uv + _MainTex_TexelSize.xy * float2(-1, -1) * _Blur).rgb) * 0.0947416f;
                color += (tex2D(_MainTex, uv + _MainTex_TexelSize.xy * float2(1, -1) * _Blur).rgb) * 0.0947416f;
                
                return color;
            }

            half4 frag(v2f IN) : SV_Target
            {
                half4 c = half4(SampleSpriteTexture(IN.texcoord), 1.0f);
                c.rgb = LinearToGammaSpace(c.rgb);
                return c;
            }
            ENDCG
        }
    }
}
