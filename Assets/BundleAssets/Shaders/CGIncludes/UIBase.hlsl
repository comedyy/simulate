#ifndef UI_BASE_LIB
#define UI_BASE_LIB

#include "UnityCG.cginc"
#include "UnityUI.cginc"

sampler2D _MainTex;
half4 _MainTex_ST;
half4 _BaseColor;
float4 _ClipRect;

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

#if defined(UNITY_UI_CLIP_RECT) || defined(_THIRD_D_UI_CLIP)
    float2 clipPosition : TEXCOORD5;
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

half CalcClipAlpha(float2 position){
    return UnityGet2DClipping(position.xy, _ClipRect);
}

float2 GetClipPosition(float4 positionOS){
#ifdef UNITY_UI_CLIP_RECT
    return positionOS.xy;
#endif

#ifdef _THIRD_D_UI_CLIP
    return mul(unity_ObjectToWorld, positionOS).xy;
#endif


}

half4 SampleAlbedo(float2 uv){
    half4 albedo;
    albedo = tex2D(_MainTex,TRANSFORM_TEX(uv, _MainTex));
    return albedo;
}

v2f VFXBase_vert(a2v v){
    v2f o;
    
    o.vertex = UnityObjectToClipPos(v.vertex);
    o.uv.xy = v.uv;
    
#ifdef _USE_VERTEX_COLOR
    o.color = v.color;
#endif

#if defined(UNITY_UI_CLIP_RECT) || defined(_THIRD_D_UI_CLIP)
    o.clipPosition = GetClipPosition(v.vertex);
#endif
    
    return o;
}

#endif  // end UIBase_BASE_LIB
