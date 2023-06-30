using System.Diagnostics;
using System.Collections.Generic;
using UnityEngine;

namespace Game
{
    public class FontSpriteHelper
    {
        private Rect uvRect;
        private Font font;
        bool isDynamic;
        public Font getFont => font;
        
        //Dictionary<char, float[]> dictBounds = new Dictionary<char, float[]>(); 
        private float[] cirtUv = new float[5];

        public float CritScale;
        private const int MaxSize = 140;

        Dictionary<char, CharacterInfo> m_dicCharacter;
        Dictionary<char, CharacterInfo> characterTable
        {
            get
            {
                if (m_dicCharacter == null)
                    m_dicCharacter = new Dictionary<char, CharacterInfo>();
                return m_dicCharacter;
            }
        }

        private CharacterInfo[] characterArray;
        private Vector2[][] characterUvArray;
        private float[][] _bounds;
        private float[][] _boundsCrit;
        private int[] _advance;
        private float[] _glyphHeight;

        public void RequestCharactersInTexture(string characters)
        {
            getFont.RequestCharactersInTexture(characters);
        }

        private void CacheBounds(char ch, CharacterInfo chInfo, bool isCrit)
        {
            var index = (int) ch;
            float[] boundInfo = null;
            
            if (isCrit)
            {
                boundInfo = _boundsCrit[index];
                if (boundInfo == null)
                {
                    boundInfo = new float[5];
                    boundInfo[0] = chInfo.minX * CritScale;
                    boundInfo[1] = chInfo.minY * CritScale;
                    boundInfo[2] = chInfo.maxX * CritScale;
                    boundInfo[3] = chInfo.maxY * CritScale;
                    boundInfo[4] = chInfo.advance * CritScale;
                    _boundsCrit[index] = boundInfo;
                }
            }
            else
            {
                boundInfo = _bounds[index];
                if (boundInfo == null)
                {
                    boundInfo = new float[5];
                    boundInfo[0] = chInfo.minX;
                    boundInfo[1] = chInfo.minY;
                    boundInfo[2] = chInfo.maxX;
                    boundInfo[3] = chInfo.maxY;
                    boundInfo[4] = chInfo.advance;
                    _bounds[index] = boundInfo;
                } 
            }
        }

        private void CacheAdvanceHeight(char ch, CharacterInfo chInfo)
        {
            _advance[ch] = chInfo.advance;
            _glyphHeight[ch] = Mathf.Abs(chInfo.glyphHeight);
        }

        public void TryGetAdvanceAndHeight(char ch, out int advance, out float glyphHeight)
        {
            advance = _advance[ch];
            glyphHeight = _glyphHeight[ch];
        }

        public void TryGetBounds(char ch, out float[] bounds, bool isCrit = false)
        {
            if (isCrit)
            {
                bounds = _boundsCrit[ch];
            }
            else
            {
                bounds = _bounds[ch];
            }
        }

        public void TryGetUvs(char ch, out Vector2[] uvs)
        {
            uvs = characterUvArray[ch];
        }

        public FontSpriteHelper(Font fontInfo, bool isDynamic)
        {
            font = fontInfo;
            if (font.dynamic != isDynamic)
            {
               UnityEngine.Debug.Log("字体类型不符合，请修改！");
            }
            this.isDynamic = isDynamic;
        }

        public void PreloadCharacterInfo()
        {
            if (isDynamic) return;
            if (characterArray != null) return;

            characterArray = new CharacterInfo[MaxSize];
            _bounds = new float[MaxSize][];
            _boundsCrit = new float[MaxSize][];
            _advance = new int[MaxSize];
            _glyphHeight = new float[MaxSize];
            characterUvArray = new Vector2[MaxSize][];
            
            foreach (var charInfo in font.characterInfo)
            {
                var ch = (char) charInfo.index;
                font.GetCharacterInfo(ch, out var info);
                characterArray[charInfo.index] = info;

                CacheBounds(ch, info, false);
                CacheBounds(ch, info, true);
                CacheAdvanceHeight(ch, info);
                
                var uvs = new Vector2[4];
                uvs[0] = charInfo.uvTopLeft;
                uvs[1] = charInfo.uvTopRight;
                uvs[2] = charInfo.uvBottomRight;
                uvs[3] = charInfo.uvBottomLeft;
                characterUvArray[charInfo.index] = uvs;
            }
        }

        internal bool GetCharacterInfo(char c, out CharacterInfo info)
        {
            if (isDynamic)
            {
                return font.GetCharacterInfo(c, out info);
            }
            else
            {
                info = characterArray[c];
                /*if (!characterTable.TryGetValue(c, out info))
                {
                    if (font.GetCharacterInfo(c, out info))
                        characterTable.Add(c, info);
                    else
                        UnityEngine.Debug.LogError($"静态字体没有文字{c}，请修复");
                }
                return true;*/

                return true;
            }
        }
    }

}