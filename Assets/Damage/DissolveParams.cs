using UnityEngine;

namespace TextureRendering
{
    public class DissolveParams : MonoBehaviour
    {
        [SerializeField]
        private float m_DissolveTime = 1;
        public float dissolveTime => m_DissolveTime;

        [SerializeField]
        private float m_DissolveScale = 2;
        public float dissolveScale => m_DissolveScale;

        [SerializeField]
        private float m_ModelHeight;
        public float modelHeight => m_ModelHeight;
    }
}
