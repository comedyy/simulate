Shader "Custom/UI/CircleWipe"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_Radius ("Radius", Float) = 0
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
			float _Radius;
			float4 _MainTex_TexelSize;

			fixed4 frag (v2f_img i) : SV_Target
			{
				// get the source color of screen
				fixed4 col = tex2D(_MainTex, i.uv);
				//col.w = 0;
				//return col;
				// center pixel
				half2 center = _MainTex_TexelSize.zw / 2;
				half2 pixelPos = i.uv * _MainTex_TexelSize.zw;
				// distance to the center by pixel
				half dist = distance(center, pixelPos);
				// get the radius by pixels
				half radius = _Radius * _MainTex_TexelSize.z;

				float f = step(dist, radius);
				return fixed4(f, f, f, 1 - f);
			}
			ENDCG
		}
	}
}
