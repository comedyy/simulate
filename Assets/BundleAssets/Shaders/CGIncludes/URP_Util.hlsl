#ifndef URP_UTIL
#define URP_UTIL


#define fixed half
#define fixed2 half2
#define fixed3 half3
#define fixed4 half4

inline float4 ComputeGrabScreenPos(float4 pos) {
#if UNITY_UV_STARTS_AT_TOP
    float scale = -1.0;
#else
    float scale = 1.0;
#endif
    float4 o = pos * 0.5f;
    o.xy = float2(o.x, o.y * scale) + o.w;
#ifdef UNITY_SINGLE_PASS_STEREO
    o.xy = TransformStereoScreenSpaceTex(o.xy, pos.w);
#endif
    o.zw = pos.zw;
    return o;
}

#endif