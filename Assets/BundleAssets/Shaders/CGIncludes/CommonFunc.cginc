#ifndef COMMON_FUNC
#define COMMON_FUNC

#include "ColorSpace.cginc"

#define sample_tex2D(textureName, coord2) 	tex2D(textureName, coord2)

half4 tex2D_Custom(sampler2D mainTexture, float2 uv)
{
#ifdef UNITY_COLORSPACE_GAMMA
	half4 color = sample_tex2D(mainTexture, uv);
#else
	half4 color = sample_tex2D(mainTexture, uv);
	#if LINEAR_TO_GAMMA_SPACE
		color.rgb = LinearToGammaSpace_Custom(color.rgb);
	#endif
#endif

	return color;
}

half4 TransformColorSpace(half4 color)
{
#if !UNITY_COLORSPACE_GAMMA && LINEAR_TO_GAMMA_SPACE
	color.rgb = LinearToGammaSpace_Custom(color.rgb);
#endif
	return color;
}

half3 TransformColorSpace(half3 color)
{
#if !UNITY_COLORSPACE_GAMMA && LINEAR_TO_GAMMA_SPACE
	color = LinearToGammaSpace_Custom(color.rgb);
#endif
	return color;
}

half TransformColorSpace(half value)
{
#if !UNITY_COLORSPACE_GAMMA && LINEAR_TO_GAMMA_SPACE
	value = LinearToGammaSpace_Custom(value);
#endif
	return value;
}

#endif // COMMON_FUNC
