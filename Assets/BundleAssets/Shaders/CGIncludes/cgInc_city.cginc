#ifndef CG_INC_CITY
#define CG_INC_CITY

float _LightTime;
float4 _lightFlashingKeyFrames[15];
float _lightFlashingKeyFramesAmount;
float _lightFlashingTime[60];


float evaluate(float time, float4 keyframe0, float4 keyframe1)
{
            	float t = (time  - keyframe0.x) / (keyframe1.x - keyframe0.x);
                float dt = keyframe1.x - keyframe0.x;
                float m0 = keyframe0.z * dt;
                float m1 = keyframe1.w * dt;
                float t2 = t * t;
                float t3 = t2 * t;
                float a = 2 * t3 - 3 * t2 + 1;
                float b = t3 - 2 * t2 + t;
                float c = t3 - t2;
                float d = -2 * t3 + 3 * t2;
                float value = a * keyframe0.y + b * m0 + c * m1 + d * keyframe1.y;
                return saturate(value);
}

float GetLightFlashingValue(float timeY){
	float time = fmod(timeY, _LightTime) / _LightTime;
                float currentVector = _lightFlashingTime[floor(time * 60)];
	currentVector = min(currentVector, _lightFlashingKeyFramesAmount - 1);
                float nextVector = currentVector+1;
                float l = evaluate(time,_lightFlashingKeyFrames[floor(currentVector)],_lightFlashingKeyFrames[floor(nextVector)] );
	return l;
}

#endif