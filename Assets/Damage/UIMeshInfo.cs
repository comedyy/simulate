using System.Collections.Generic;
using UnityEngine;

namespace Game
{
    public class UIMeshInfo
    {
        //public int originStringCode;
        //public string outPutString;
        public Vector2 offset;

        public Vector3 pos;
        public float scale;
        public Color color;
        public Vector3 playerPos;
        public bool isCrit;
        public int FontOffset;
        public UIMeshAnimation MeshAnimation;
        public char[] chars;
        public int charLen;

        public int StartVertIndex;
        public int EndVertIndex;
        public int Start2VertIndex;
        public int End2VertIndex;

        private static int MaxLen = 15;

        public UIMeshInfo()
        {
            MeshAnimation = new UIMeshAnimation();
            chars = new char[MaxLen];
        }

        public void SetInfo(string originStr, int fontOffset)
        {
            charLen = originStr.Length;
            for (int i = 0; i < originStr.Length && i < MaxLen; i++)
            {
                var ch = originStr[i];
                ch = UIMeshText.GetFontCharOffset(ch, fontOffset);
                chars[i] = ch;
            }
            //outPutString = originStr;
            //originStringCode = originStr.GetHashCode();
        }

        public void Clear()
        {
            //outPutString = string.Empty;
            Start2VertIndex = -1;
            End2VertIndex = -1;
            StartVertIndex = -1;
            EndVertIndex = -1;
        }

        public bool IsInValid()
        {
            return StartVertIndex == -1 || EndVertIndex == -1;
        }
    }
}