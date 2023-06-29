#ifndef FUNC_UTILS_INCLUDED
#define FUNC_UTILS_INCLUDED


#ifdef _USE_EDITOR_LIGHT

// 方向光 使用 Ps 的叠加
#define BLEND_OVERLAY(clr) half4 finalCol = clr;

#define BLEND_OVERLAY_3(clr) half3 finalCol = clr;

#else

// 方向光 使用 Ps 的叠加
#define BLEND_OVERLAY(clr) half4 finalCol = _SpriteColor *(_DirectionLightColor.r + (clr - _DirectionLightColor.r) * (1 + _DirectionLightColor.g));

#define BLEND_OVERLAY_3(clr) half3 finalCol = _SpriteColor *(_DirectionLightColor.r + (clr - _DirectionLightColor.r) * (1 + _DirectionLightColor.g));

#endif

#define BLEND_OVERLAY_CUSTOM(clr, spriteColor) half4 finalCol = spriteColor *(_DirectionLightColor.r + (clr - _DirectionLightColor.r) * (1 + _DirectionLightColor.g));

#define BLEND_OVERLAY_CUSTOM_3(clr, spriteColor) half3 finalCol = spriteColor *(_DirectionLightColor.r + (clr - _DirectionLightColor.r) * (1 + _DirectionLightColor.g));

#endif // FUNC_UTILS_INCLUDED