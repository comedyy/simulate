#ifndef TEXTURE_ROTATION_INCLUDE
#define TEXTURE_ROTATION_INCLUDE


static const float2 _Rots[4] = {
    float2(1.0, 0.0),       // 不旋转
    float2(0.0, 1.0),       // 顺时针 90
    float2(0.0, -1.0),      // 逆时针 90
    float2(-1.0, 0.0),      // 180
};

inline float2 UVRotate(float2 uv, float rotIndex) {
    float2x2 rot = {
        _Rots[rotIndex].x, -_Rots[rotIndex].y,
        _Rots[rotIndex].y, _Rots[rotIndex].x
    };
    return mul(rot, uv - 0.5) + 0.5;
}


#endif      // TEXTURE_ROTATION_INCLUDE