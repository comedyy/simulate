
#ifndef UBER_FBX_LIB
#define UBER_FBX_LIB

#include "UnityCG.cginc"
#include "CommonFunc.cginc"

sampler2D _BaseTex;
half4 _BaseTex_ST;
half4 _BaseColor;
half _BaseAlpha;

#ifdef _ALPHATEST_ON

half _AlphaTest;
half _AlphaTestThreshold;

#endif


// 扭曲
#ifdef _UBER_FBX_DISTURTION

sampler2D _Dist_DisplacementTex;
half4 _Dist_DisplacementTex_ST;
half4 _Dist_Speed;
half4 _Dist_Magnitude;

#endif

// 流光
#ifdef _UBER_FBX_FLOW

sampler2D _Flow_Mask;
half4 _Flow_Mask_ST;
sampler2D _Flow_TexOne;
half4 _Flow_TexOne_ST;
sampler2D _Flow_TexTwo;
half4 _Flow_TexTwo_ST;
half4 _Flow_ColorOne;
half _Flow_PowerOne;
half4 _Flow_ParamsOne; // xy = speed    z = duration    w = interval

half4 _Flow_ColorTwo;
half _Flow_PowerTwo;
half4 _Flow_ParamsTwo; // xy = speed    z = duration    w = interval

#endif

// 溶解
#ifdef _UBER_FBX_DISSOLVE

sampler2D _Diss_CutMaskTex;
half4 _Diss_CutMaskTex_ST;
half _Diss_LineWidth;
half4 _Diss_LineColor;
half _Diss_Amount;


#endif


struct a2v{
    float4 vertex : POSITION;
    half4 color : COLOR;
    half2 uv : TEXCOORD0;
};

struct v2f{
    float4 vertex : SV_POSITION;
    half2 uv : TEXCOORD0;
    half4 color : TEXCOORD1;
    half4 baseColor : TEXCOORD2;
#ifdef _UBER_FBX_DISSOLVE
    half4 dissLineColor : TEXCOORD3;
#endif
    half4 flowColorOne : TEXCOORD4;
    half4 flowColorTwo : TEXCOORD5;
    
};

#ifdef _UBER_FBX_FLOW



inline half3 Flow_GetColor(half2 uv, half4 flowParams){
    half2 uvSpeed = flowParams.xy;
    half idleTime = flowParams.z;
    half durationTime = flowParams.w;
    
    float internal = (idleTime + durationTime) * 1.0f;
    float curLoopTime = idleTime <=0 ? _Time.y : fmod(_Time.y, internal);
    float actionTime = curLoopTime - idleTime;
    
    // 计算UV,采样
    float actionPercentage = actionTime / durationTime;
    uv += uvSpeed * actionPercentage;
    half4 flowColor = tex2D(_Flow_TexOne, uv);
    
    half3 color = actionPercentage > 0 ? flowColor.xyz : half3(0,0,0);
    return color;
}

inline float Flow_GetActionPercentage(half4 flowParams){
    half idleTime = flowParams.z;
    half durationTime = flowParams.w;
    
    float internal = (idleTime + durationTime) * 1.0f;
    float curLoopTime = idleTime <=0 ? _Time.y : fmod(_Time.y, internal);
    float actionTime = max(curLoopTime - idleTime, 0);
    
    // 计算UV,采样
    float actionPercentage = actionTime / durationTime;
    return actionPercentage;
}

inline half4 Flow_GetColorOne(half2 uv, half4 flowParams){
    half2 uvSpeed = flowParams.xy;
    float actionPercentage = Flow_GetActionPercentage(flowParams);
    uv += uvSpeed * actionPercentage;
    
    uv.x = uv.x % 1;
    uv.y = uv.y % 1;
    
    half4 flowColor = tex2D(_Flow_TexOne, TRANSFORM_TEX(uv, _Flow_TexOne));
    
    half4 color = actionPercentage > 0 ? flowColor : half4(0,0,0, 0);
    return color;
}

inline half4 Flow_GetColorTwo(half2 uv, half4 flowParams){
    half2 uvSpeed = flowParams.xy;
    float actionPercentage = Flow_GetActionPercentage(flowParams);
    uv += uvSpeed * actionPercentage;
    
    uv.x = uv.x % 1;
    uv.y = uv.y % 1;
    
    half4 flowColor = tex2D(_Flow_TexTwo, TRANSFORM_TEX(uv, _Flow_TexTwo));
    
    half4 color = actionPercentage > 0 ? flowColor : half4(0,0,0, 1);
    return color;
}

inline half3 Flow_Calc(half2 uv, half4 flowColorOne, half4 flowColorTwo, out half flowAlpha){
    half4 flowColor = Flow_GetColorOne(uv, _Flow_ParamsOne);
    half4 flowMask = tex2D(_Flow_Mask, TRANSFORM_TEX(uv, _Flow_Mask));
    flowColor.rgb = flowMask.r * _Flow_PowerOne * flowColorOne.rgb * flowColor.r;
    
    flowAlpha = flowMask.r * flowColor.r;
    
    // 使用 B 通道，二次控制流光 
    #ifdef _UBER_FBX_FLOW_USE2Channel
    
        flowColorTwo = Flow_GetColorTwo(uv, flowColorTwo);
        flowColor.rgb *= flowMask.r *  _Flow_PowerTwo * _Flow_ColorTwo.rgb * flowColorTwo.r;
        
        flowAlpha *= flowColorTwo.r;
    #endif
    
    return flowColor;
}

#endif


#ifdef _UBER_FBX_DISTURTION

inline half2 Dist_Calc(half2 uv, half2 speed, half2 magnitude, half2 offset){
    half2 distUV = half2(uv.x + _Time.x * speed.x, uv.y + _Time.x * speed.y);
    half2 dispTex = tex2D(_Dist_DisplacementTex, TRANSFORM_TEX(distUV, _Dist_DisplacementTex)).xy;
    // 映射到 -1 与 1之间
    dispTex = (dispTex * 2 - 1);
    dispTex *= magnitude + offset;
    
    // 把扭曲贴图采样的值，当作UV 返回
    return dispTex;
}

#endif

#ifdef _UBER_FBX_DISSOLVE

inline half Diss_Calc(half2 uv, half lineWidth, half amount){
    half4 cutMaskTex = tex2D(_Diss_CutMaskTex, TRANSFORM_TEX(uv, _Diss_CutMaskTex));
    half cutValue = cutMaskTex.a - amount; 
    clip(cutValue);
    
    half t = cutValue < lineWidth ? cutValue / lineWidth + 0.001 : 0;
    return t;
}

#endif


v2f uber_vert(a2v v){
    v2f o;
    
    o.vertex = UnityObjectToClipPos(v.vertex);
    o.uv = v.uv;
    o.color = v.color;
    o.baseColor = TransformColorSpace(_BaseColor);

#ifdef _UBER_FBX_DISSOLVE
    o.dissLineColor = TransformColorSpace(_Diss_LineColor);
#endif

#ifdef _UBER_FBX_FLOW
    o.flowColorOne = TransformColorSpace(_Flow_ColorOne);
#endif
    
    return o;
}

half4 uber_frag(v2f i) : SV_Target {
    half4 finalColor;
    
    half2 uv = i.uv;
    
    // 如果开启扭曲
    #if _UBER_FBX_DISTURTION
        uv += Dist_Calc(uv, _Dist_Speed.xy, _Dist_Magnitude.xy, _Dist_Magnitude.zw);    
    #endif
    
    
    
    #ifdef _UBER_FBX_FLOW
        #ifdef _UBER_FBX_FLOW_USEBASETEX
            finalColor = tex2D(_BaseTex,TRANSFORM_TEX(uv, _BaseTex));
        #else
            finalColor = half4(0,0,0,1); 
        #endif
        
    #else 
        finalColor = tex2D(_BaseTex,TRANSFORM_TEX(uv, _BaseTex));
    #endif
    
    
    // 如果开启流光
    #ifdef _UBER_FBX_FLOW
        half flowAlpha;
        finalColor.rgb += Flow_Calc(i.uv, i.flowColorOne, i.flowColorTwo, flowAlpha);
        #ifndef _UBER_FBX_FLOW_USEBASETEX
            finalColor.a *= flowAlpha;
        #endif
    #endif
    
    
    finalColor.rgb *= i.baseColor;
    finalColor.a *= _BaseAlpha;
    
    #if _ALPHATEST_ON
        clip(finalColor.a - _AlphaTestThreshold);
    #endif
    
    // 如果开启溶解
    #ifdef _UBER_FBX_DISSOLVE
        half dissT = Diss_Calc(i.uv, _Diss_LineWidth, _Diss_Amount);
        finalColor.rgb = lerp(finalColor.rgb, i.dissLineColor.rgb, dissT);
    #endif
    
    finalColor *= i.color;
    
    return finalColor;
}



#endif  // end UBER_FBX_LIB
