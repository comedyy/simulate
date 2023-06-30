using UnityEngine;
    public enum HitAniColorStage
    {
        Stage1,
        Stage2,
        Stage3,
        None
    }
namespace Game.Battle.Mono.Hero
{
    public class HeroHitColor : MonoBehaviour
    {
        [SerializeField] private SkinnedMeshRenderer _skinnedMeshRenderer;
        private bool _isHit = false;
        private bool _isNewHit = false;
        private HitAniColorStage _hitState = HitAniColorStage.None;
        private Color _hitColor = new Color(1f, 0f, 0f);
        private float _lastRecordTime = 0;
        private Color _nowColor;
        private Color LitColor;
        private Material _material;

        public void Start()
        {
            _material = _skinnedMeshRenderer.material;
            LitColor = _material.GetColor("_HitColor");

        }

        public void HeroHit()
        {
            if (_isHit)
            {
                _isNewHit = true;
            }

            _isHit = true;
        }

        public void Update()
        {
            if (_isHit)
            {
                if (_isNewHit)
                {
                    _hitState = HitAniColorStage.Stage1;
                    _lastRecordTime = Time.time;
                    SetMatColor(_hitColor);
                    _isNewHit = false;
                }
                else
                {
                    var timeDiff = Time.time - _lastRecordTime;

                    if (_hitState == HitAniColorStage.None)
                    {
                        _hitState = HitAniColorStage.Stage1;
                    }

                    if (_hitState == HitAniColorStage.Stage1 )
                    {
                        _hitState = HitAniColorStage.Stage2;
                        SetMatColor(_hitColor);
                        _lastRecordTime = Time.time;
                    }
                    else if (_hitState == HitAniColorStage.Stage2 && timeDiff > 0.05f)
                    {
                        _hitState = HitAniColorStage.Stage3;
                        _lastRecordTime = Time.time;
                    }
                    else if (_hitState == HitAniColorStage.Stage3 )
                    {
                        if (timeDiff > 0.2)
                        {
                            _hitState = HitAniColorStage.None;
                            SetMatColor(Color.white);
                            _isHit = false;
                            _isNewHit = false;
                        }
                        else
                        {
                            SetMatColor( Color.Lerp(_nowColor, Color.white, Time.deltaTime * 10));
                        }
                    }
                }
            }
        }

        private void SetMatColor(Color color)
        {
            _nowColor = color;
            _material.SetColor("_BaseColor", color);

            if (color == Color.white)
            {
                _material.SetColor("_HitColor", LitColor);
            }
            else
            {
                _material.SetColor("_HitColor", color);
            }
        }
    }
}
