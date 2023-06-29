Shader "Custom/UI/UI-ImgWipeEffect"
{
	Properties
	{
	    _MainTex ("Texture", 2D) = "white" { }
	    _Factor ("Factor", Float) = 1
	    _Threshold("Threshold", Float)=0.3
	    _MaxFactor ("MaxFactor", Float) = 200
	}
	SubShader
	{
		// No culling or depth
		Tags { "QUEUE"="Transparent" "IGNOREPROJECTOR"="true" "RenderType"="Transparent" }
		Blend SrcAlpha OneMinusSrcAlpha
        LOD 100

		Pass
		{
			CGPROGRAM
			#pragma vertex vert_img
			#pragma fragment frag
			
			#include "UnityCG.cginc"
			
			sampler2D _MainTex;
			float _Factor;
			float _Threshold;
			float4 _MainTex_ST;
			float _MaxFactor;

			fixed4 frag (v2f_img i) : SV_Target
			{
		        _MainTex_ST.xy = _Factor;
		        _MainTex_ST.zw = (_Factor -1.0)*-0.5;
		         fixed4 col1 = tex2D(_MainTex, i.uv);
                fixed4 col = tex2D(_MainTex, i.uv*_MainTex_ST.xy+_MainTex_ST.zw);
	            float f = step(col.w, _Threshold);
	            float f2 = 1.0 - step(_Factor, _MaxFactor);
	            float finalAlpha = clamp(f + f2, 0, 1);
	            
                return fixed4(col.xyz*(1.0-f)*(1-finalAlpha), finalAlpha);
			}
			ENDCG
		}
	}
}
