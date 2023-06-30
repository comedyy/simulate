using System;

namespace Game
{
    public class UIMeshVertPool
    {
        private int maxVertNum;
        private int length;
        private int index; // 结束索引
        private int startIndex = 0; //开始索引
        private bool reverse = false; // 是否逆向查找
        private int offset = 0; // 反方向查找空闲顶点时，需要跳过回收的定点数
        private int originOffset = 0; // 配合上面字段使用，恢复正常查找方向时，需要跳过的偏移量

        public UIMeshVertPool(int maxVert)
        {
            maxVertNum = maxVert;
            length = maxVert % 4 == 0 ? maxVert >> 2 : maxVert >> 2 + 1;
        }

        public int GetVert(char value)
        {
            var oldState = reverse;
            if (index == length)
            {
                if (startIndex == 0) return -1;
                
                reverse = true;

                if (!oldState)
                {
                    offset = index - startIndex;
                    originOffset = offset;
                }
            }
            else if (startIndex == 0)
            {
                if (index == length - 1) return -1;
                reverse = false;

                if (oldState)
                {
                    offset = index - startIndex;
                    originOffset = offset;
                }
            }

            if (reverse)
            {
                var nextIndex = startIndex - 1;
                if (nextIndex < 0)
                {
                    UnityEngine.Debug.LogError($"array size too short!, start = {Start}, end = {End}, range = {Range}");
                    return -1;
                }

                startIndex = nextIndex;
                if (startIndex < 0 || startIndex >= length)
                {
                    UnityEngine.Debug.LogError($"array size too short!, start = {Start}, end = {End}, range = {Range}");
                }
                
                return startIndex;
            }
            
            return index++;
        }

        public bool CheckVert(int count)
        {
            if (index - startIndex + count >= length)
            {
                return false;
            }

            return true;
        }

        public void ReleaseVert(int id)
        {
            if (reverse)
            {
                if (offset > 0)
                {
                    offset --;
                    if (offset == 0)
                    {
                        index = index - originOffset;
                    }
                }
                else index -= 1;
                
                if (index < 0)
                {
                    index = 0;
                }
                else if (index > length)
                {
                    index = length;
                }
            }
            else
            {
                if (offset > 0)
                {
                    offset --;
                    if (offset == 0)
                    {
                        startIndex = startIndex + originOffset;
                    }
                }
                else startIndex += 1;
                
                if (startIndex < 0)
                {
                    startIndex = 0;
                }
                else if (startIndex > length)
                {
                    startIndex = length;
                }
            }
        }

        public void PrintInfo()
        {
            //Debug.Log($"Tri len start= {Start}, end = {End}, len = {Range}, offset = {offset}, reverser = {reverse}");
        }

        public void Clear()
        {
            index = 0;
            startIndex = 0;
        }

        public int Range => (index - startIndex) * 6;

        public int Start => startIndex * 6;
        public int End => index * 6;
        public int Length => length;
        public int Index => index;
        public int StartIndex => startIndex;
        public bool Reverse => reverse;
    }
}