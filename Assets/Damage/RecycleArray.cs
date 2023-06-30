using System;

namespace Game
{
    public class RecycleArray<T> where T : new()
    {
        private readonly T[] _data;
        private readonly int _size;
        private int _startIndex;
        private int _endIndex;
        private bool _reverse;
        private int _offset; // 反方向查找空闲顶点时，需要跳过回收的定点数
        private int _originOffset; // 配合上面字段使用，恢复正常查找方向时，需要跳过的偏移量

        public int Range => _endIndex - _startIndex;
        public int Start => _startIndex;

        public int End => _endIndex;

        public RecycleArray(int size)
        {
            if (size <= 0)
            {
                throw new ArgumentException("size must larger than 0");
            }
            
            _size = size;
            _data = new T[size];
            _startIndex = 0;
            _endIndex = 0;
        }

        public void Prefill()
        {
            for (var i = 0; i < _size; i++)
            {
                if (_data[i] == null)
                {
                    _data[i] = new T();
                }
            }
        }

        public void Clear()
        {
            _endIndex = 0;
            _startIndex = 0;
        }

        public T Get()
        {
            if (_endIndex == _size)
            {
                if (_startIndex == 0) return default;
                
                if (!_reverse)
                {
                    _offset = _endIndex - _startIndex;
                    _originOffset = _offset;
                }

                _reverse = true;
            }
            else if (_startIndex == 0)
            {
                if (_endIndex == _size) return default;
                if (_reverse)
                {
                    _offset = _endIndex - _startIndex;
                    _originOffset = _offset;
                }
                
                _reverse = false;
            }
            
            if (_reverse)
            {
                var nextIndex = _startIndex - 1;
                if (nextIndex < 0)
                {
                    UnityEngine.Debug.LogError($"array size too short!, start = {Start}, end = {End}, range = {Range}");
                    return default;
                }

                _startIndex = nextIndex;
                if (_startIndex < 0 || _startIndex >= _size)
                {
                    UnityEngine.Debug.LogError($"exception, index = {_startIndex}, size={_size}");
                }
                
                return _data[_startIndex];
            }

            if (_endIndex < 0 || _endIndex >= _size)
            {
                UnityEngine.Debug.LogError("exception, index = {_startIndex}, size={_size}");
            }

            return _data[_endIndex++];
        }

        public void Release(T t)
        {
            if (_reverse)
            {
                if (_offset > 0)
                {
                    _offset --;
                    if (_offset == 0)
                    {
                        _endIndex = _endIndex - _originOffset;
                    }
                }
                else _endIndex -= 1;
            }
            else
            {
                if (_offset > 0)
                {
                    _offset --;
                    if (_offset == 0)
                    {
                        _startIndex = _startIndex + _originOffset;
                    }
                }
                else _startIndex += 1;
            }
        }
        
        public bool Predict(int count)
        {
            if (_endIndex - _startIndex + count >= _size)
            {
                return false;
            }

            return true;
        }

        public void SetData(int index, T data)
        {
            _data[index] = data;
        }
        
        public T this[int index]
        {
            get
            {
                if ((uint) index >= (uint) _size)
                    throw new ArgumentOutOfRangeException();
                
                return _data[index];
            }
           set
            {
                if ((uint) index >= (uint) _size)
                    throw new ArgumentOutOfRangeException();
                
                _data[index] = value;
            }
        }
    }
}