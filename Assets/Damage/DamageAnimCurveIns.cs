using UnityEngine;

namespace Game
{
    public class DamageAnimCurveIns
    {
        private DamageAnimCurveProfile _curveProfile;

        private AnimationCurve _localScale;
        private AnimationCurve _localPositionY;
        private AnimationCurve _alpha;

        public float Duration => _curveProfile.Time;
        
        public float EvaluateScale(float time)
        {
            return _localScale.Evaluate(time);
        }
        
        public float EvaluateAlpha(float time)
        {
            return _alpha.Evaluate(time);
        }
        
        public float EvaluatePositionY(float time)
        {
            return _localPositionY.Evaluate(time);
        }

        public void Clear()
        {
            _curveProfile = null;
            _localPositionY = null;
            _localScale = null;
            _alpha = null;
        }

        public void SetCurve(DamageAnimCurveProfile curveProfile)
        {
            _curveProfile = curveProfile;

            foreach (var animCurve in curveProfile.Curves)
            {
                if (animCurve.Type == DamageAnimCurveProfile.AnimCurveType.ColorA)
                {
                    _alpha = animCurve.Curve;
                }
                else if (animCurve.Type == DamageAnimCurveProfile.AnimCurveType.LocalPosY)
                {
                    _localPositionY = animCurve.Curve;
                }
                else if (animCurve.Type == DamageAnimCurveProfile.AnimCurveType.LocalScaleX)
                {
                    _localScale = animCurve.Curve;
                }
            }
        }
    }
}