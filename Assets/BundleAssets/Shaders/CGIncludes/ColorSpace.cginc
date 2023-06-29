#ifndef COLOR_SPACE
#define COLOR_SPACE

inline float GammaToLinearSpace_Custom (float value)
{
	if (value <= 0.04045F)
		return value / 12.92F;
	else if (value < 1.0F)
		return pow((value + 0.055F)/1.055F, 2.4F);
	else
		return pow(value, 2.2F);
}

inline half3 GammaToLinearSpace_Custom (half3 sRGB)
{
	return sRGB * (sRGB * (sRGB * 0.305306011h + 0.682171111h) + 0.012522878h);
}

inline float LinearToGammaSpace_Custom(float value)
{
	if (value < 0.0f)
		return 0.0f;
	else if (value <= 0.0031308f)
		return 12.92f * value;
	else if (value < 1.0f)
		return 1.055f * pow(value, 0.4166667f) - 0.055f;
	else
		return pow(value, 0.45454545f);
}

inline half3 LinearToGammaSpace_Custom(half3 linRGB)
{
	linRGB = max(linRGB, half3(0.0h, 0.0h, 0.0h));
	return max(1.055h * pow(linRGB, 0.416666667f) - 0.055h, 0.0h);
}

#endif
