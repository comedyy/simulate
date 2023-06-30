using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game
{
    [CreateAssetMenu(fileName = "DamageAnimCurveProfile", menuName = "ScriptableObjects/DamageAnimCurveProfile")]
    public class DamageAnimCurveProfile : ScriptableObject
    {
        public enum AnimCurveType
        {
            None,
            LocalPosY,
            LocalScaleX,
            LocalScaleY,
            LocalScaleZ,
            ColorA
        }
        
        [Serializable]
        public class DamageAnimCurve
        {
            public AnimCurveType Type;
            public AnimationCurve Curve;
        }
        
        public List<DamageAnimCurve> Curves;
        public List<Color> ElementColor;
        public List<Vector2> Offsets;
        public float Time;
        public int Offset;
        public uint OffsetCount;
    }
}