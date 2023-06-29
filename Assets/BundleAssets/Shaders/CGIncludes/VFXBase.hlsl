#ifndef VFX_BASE_LIB
#define VFX_BASE_LIB

#include "UnityCG.cginc"

sampler2D _BaseTex;
half4 _BaseTex_ST;
half4 _BaseColor;



struct a2v{
    float4 vertex : POSITION;
    
#ifdef _USE_VERTEX_COLOR
    half4 color : COLOR;
#endif
    
    float2 uv : TEXCOORD0;
};

struct v2f{
    float4 vertex : SV_POSITION;
    float4 uv : TEXCOORD0;
    
#ifdef _USE_VERTEX_COLOR
    half4 color : TEXCOORD1;
#endif
    
};

half3 RgbToHsv(half3 c)
{
    const half4 K = half4(0.0, -1.0 / 3.0, 2.0 / 3.0, -1.0);
    half4 p = lerp(half4(c.bg, K.wz), half4(c.gb, K.xy), step(c.b, c.g));
    half4 q = lerp(half4(p.xyw, c.r), half4(c.r, p.yzx), step(p.x, c.r));
    half d = q.x - min(q.w, q.y);
    const half e = 1.0e-4;
    return half3(abs(q.z + (q.w - q.y) / (6.0 * d + e)), d / (q.x + e), q.x);
}

half3 HsvToRgb(half3 c)
{
    const half4 K = half4(1.0, 2.0 / 3.0, 1.0 / 3.0, 3.0);
    half3 p = abs(frac(c.xxx + K.xyz) * 6.0 - K.www);
    return c.z * lerp(K.xxx, saturate(p - K.xxx), c.y);
}


half4 SampleAlbedo(float2 uv){
    half4 albedo;
    albedo = tex2D(_BaseTex,TRANSFORM_TEX(uv, _BaseTex));
    return albedo;
}


#endif  // end VFX_BASE_LIB
