using UnityEngine;
 
namespace Game.Battle.Mono.Hero
{
    public class MonsterHitColor : MonoBehaviour
    {
        [SerializeField] private SkinnedMeshRenderer _skinnedMeshRenderer;

        public float keepWhiteTime = 0.2f;
        public Color colorWhite;
        float recoveryTime = 0;

        Color c = Color.black;
        Material _material;
        
        public void Start()
        {
            _material = _skinnedMeshRenderer.material;
        }

        public void HeroHit()
        {
            _material.SetColor("_ColorHit", colorWhite);
            recoveryTime = Time.time + keepWhiteTime;
        }

        public void Update()
        {
            if(recoveryTime == 0) return;

            if(recoveryTime < Time.time)
            {
                recoveryTime = 0;
                _material.SetColor("_ColorHit", Color.black);
            }
            else
            {
                var percent = 1- (recoveryTime - Time.time) / keepWhiteTime;
                _material.SetColor("_ColorHit", Color.Lerp(colorWhite, Color.black, percent));
            }
        }
    }
}
