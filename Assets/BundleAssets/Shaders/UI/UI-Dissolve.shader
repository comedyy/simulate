Shader "Custom/UI/UI-Dissolve"
{
	Properties
	{
		_MainTex ("MainTex", 2D) = "white" {}  //基础纹理
		_NoiseTex ("NoiseTex", 2D) = "white" {} //溶解纹理
		_Threshold("Threshold", float) = 1.3 //阈值
		
		_NoiseCentrePos("NoiseCentrePos", Vector) = (0.5, 0.5, 0) //起始点
		[HDR]_FirstBoarderColor("FirstBoarderColor", Color) = (1, 1, 1, 1) //边缘颜色
		[HDR]_SecondBoarderColor("SecondBoarderColor", Color) = (1, 1, 1, 1) //边缘颜色
		
		_ColorFactor("ColorFactor", Range(0, 1)) = 0.7
		_DissoveEdge("DissoveEdge", Range(0, 1)) = 0.8
	}
	SubShader
	{
		// No culling or depth
		
		Tags { "QUEUE"="Transparent" "IGNOREPROJECTOR"="true" "RenderType"="Transparent" "PreviewType" = "Plane" "CanUseSpriteAtlas"="true" }
		Blend SrcAlpha OneMinusSrcAlpha
        LOD 100

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"
			
			sampler2D _MainTex;
			sampler2D _NoiseTex;
			float _Threshold;
			float4 _MainTex_TexelSize;
			float4 _MainTex_ST;
			float4 _NoiseTex_ST;
			fixed2 _NoiseCentrePos;
			float4 _FirstBoarderColor;
			float4 _SecondBoarderColor;
			float _ColorFactor;
			float _DissoveEdge;
			
			struct v2f
            {
                float4 vertex   : SV_POSITION;
                fixed4 color : COLOR;
                float4 uv  : TEXCOORD0;
                UNITY_VERTEX_OUTPUT_STEREO
            };
			
			v2f vert (appdata_base v)
			{
				v2f o;
				//UNITY_SETUP_INSTANCE_ID(v);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);  // 实例化处理

				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv.xy = TRANSFORM_TEX(v.texcoord, _MainTex);
				o.uv.zw = TRANSFORM_TEX(v.texcoord, _NoiseTex);
				return o;

			}

		     fixed4 frag (v2f i) : COLOR
			{
				fixed4 col = tex2D(_MainTex, i.uv.xy);
				fixed4 nTex = tex2D(_NoiseTex, i.uv.zw);

				// 设置溶解的方向
				half dissove = nTex.r;
				half dist = distance(_NoiseCentrePos, i.uv.zw);
				dissove = dissove + dist;
				col.a *= step(_Threshold, dissove);
				clip(col.a - 0.01);

				// 处理颜色部分
				fixed t = _Threshold / dissove;
				half lerpEdge = sign(t - _ColorFactor - _DissoveEdge);
				fixed3 edgeColor = lerp(_FirstBoarderColor.rgb, _SecondBoarderColor.rgb, saturate(lerpEdge));
				half lerpOut = sign(t - _ColorFactor);
				fixed3 colorOut = lerp(col.rgb, edgeColor.rgb, saturate(lerpOut));

				return fixed4(colorOut, col.a);
				return col;
			}
			ENDCG
		}
	}
}
