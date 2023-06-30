using System.Collections.Generic;
using UnityEngine;

namespace Game
{
    public class UIMeshAnimation
    {
        private UIMeshText _meshText;
        private float _elapsedTime;
        private float _offset;
        private Vector3 _startPos;
        private Vector3 _endPos;
        private UIMeshInfo _meshInfo;
        private DamageAnimCurveIns _curveProfile;
        private DamageFloatDir _dir;
        private Vector3 _dirValue;
        private float _dirOffset;
        private float _duration;
        public bool IsEnd;
        public void DoAnimation(UIMeshInfo meshInfo, UIMeshText uiMeshText, DamageAnimCurveIns profile, 
            float offset, DamageFloatDir dir = DamageFloatDir.Up,
            Vector3 dirValue = default, float dirOffset = 0f)
        {
            _meshInfo = meshInfo;
            _meshText = uiMeshText;
            _elapsedTime = 0;
            _startPos = meshInfo.pos;
            _endPos = _startPos;
            _curveProfile = profile;
            _endPos.y += offset;
            _offset = offset;
            _dir = dir;
            _dirValue = dirValue.normalized;
            _dirOffset = dirOffset;
            _duration = _curveProfile.Duration;
            IsEnd = false;
        }

        public void Update(float dt, Vector3 nowPlayerPos)
        {
            var pos = _startPos;
            pos.y += _offset;
            
            var y = _curveProfile.EvaluatePositionY(_elapsedTime);
            pos.y += y;

            if (_dir == DamageFloatDir.TargetDir)
            {
                float factor = _elapsedTime / _duration;
                if (factor > 1f) factor = 1f;
                pos.x += factor * _dirOffset * _dirValue.x;
                pos.y += factor * _dirOffset * _dirValue.y;
            }

            var scale = _meshInfo.scale;
            var s = _curveProfile.EvaluateScale(_elapsedTime);
            scale *= s;

            var color = _meshInfo.color;
            color.a *= _curveProfile.EvaluateAlpha(_elapsedTime);

            var playerPos = _meshInfo.playerPos;
            pos.x += (nowPlayerPos.x - playerPos.x);
            pos.y += (nowPlayerPos.y - playerPos.y);

            //pos += (nowPlayerPos - _meshInfo.playerPos);
            _meshText.RefreshVertPos(_meshInfo, pos, scale, color, _meshInfo.isCrit);
            _elapsedTime += dt;
            IsEnd = _elapsedTime >= _duration;
        }

        public void Clear()
        {
            if (_meshInfo != null)
            {
                _meshText.ReleaseVert(_meshInfo);
            }

            _meshInfo = null;
            _meshText = null;
            _curveProfile = null;
            IsEnd = true;
        }
    }
}