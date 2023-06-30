using System;
using System.Collections.Generic;
using UnityEngine;
using Random = System.Random;

namespace Game
{
    public class UIMeshText
    {
        private GameObject gameObj;
        private MeshFilter meshFilter;
        private MeshRenderer meshRenderer;
        private Vector3[] vertices;
        private Vector2[] uvs;
        private Color[] colors;
        private int[] triangles;

        private Mesh mesh;
        private UIMeshVertPool vp;

        private FontSpriteHelper spriteHelper;
        private bool m_isDrawUving = false;
        private Vector2 m_rectVec = Vector2.zero;
        //private Dictionary<List<int>, UIMeshInfo> _requireList;
        private RecycleArray<UIMeshInfo> _requireList;
        private RectTransform rectTransform;
        private bool bShow = false;
        private bool needRefresh = false;
        private bool dynamic = false;
        //private bool uvNeedRebuild = false;
        private float critScale = 1.5f;
        private Vector3 globalScale;
        private int tmpIndex;
        private Vector3 tmpPos;
        
        private Vector3 _playerPos;
        private bool _posDirty;
        private Vector3 _initPos;
        private int _randomXValue;
        private Random _random;
        private DamageAnimCurveIns _curveProfile;
        private List<Vector2> _offsets;
        private Vector2 _canvasScale = Vector2.one;
        private int _offsetIndex;
        
        private readonly Rect _rectZero = Rect.zero;
        private readonly Color _colorWhite = Color.white;

        public GameObject GameObject => gameObj;

        public UIMeshText(Transform parent, string name, Font font, int vertCount, int meshInfoCount, Vector3 globalScale)
        {
            InitMeshObject(name, parent);
            InitMeshComponent(font.material);
            InitVertices(vertCount);

            dynamic = font.dynamic;
            spriteHelper = new FontSpriteHelper(font, dynamic);
            spriteHelper.CritScale = critScale;
            spriteHelper.PreloadCharacterInfo();
            _requireList = new RecycleArray<UIMeshInfo>(meshInfoCount);
            _requireList.Prefill();
            
            _random = new Random();
            this.gameObj.SetActive(false);
            this.globalScale = globalScale;
            
            if (dynamic)
            {
                Font.textureRebuilt += OnFontTextureRebuild;
            }
            // else if (meshRenderer.sharedMaterial.shader == null)
            // {
            //     meshRenderer.sharedMaterial.shader = UnityEngine.UI.Text.defaultGraphicMaterialText.shader;
            // }
        }

        public void Clear()
        {
            var startIndex = _requireList.Start;
            var endIndex = _requireList.End;
            for (var i = endIndex - 1; i >= startIndex; i--)
            {
                var animation = _requireList[i].MeshAnimation;
                animation.Clear();
            }

            RefreshMesh();
            _requireList.Clear();
            vp.Clear();
            needRefresh = true;
        }

        public void SetInitPos(Vector3 pos)
        {
            _initPos = pos;
        }

        public Vector3 InitPos => _initPos;
        
        public void SetPlayerPos(Vector3 pos)
        {
            _playerPos = pos;
        }

        public void SetCurveProfile(DamageAnimCurveIns profile)
        {
            _curveProfile = profile;
        }

        public void SetOffsets(List<Vector2> offsets)
        {
            _offsets = offsets;
        }

        public void SetCanvasScale(Vector2 scale)
        {
            _canvasScale = scale;
        }
        public void SetRenderQueue(int nQueue)
        {
            meshRenderer.material.renderQueue = nQueue;
        }

        public void SetXRandomValue(int value)
        {
            _randomXValue = value;
        }

        public void SetCritScale(float scale)
        {
            critScale = scale;
        }

        public void Show(bool bShow)
        {
            if (this.bShow == bShow)
            {
                return;
            }
            this.bShow = bShow;
            this.gameObj.SetActive(bShow);
        }

        private void InitMeshObject(string name, Transform parent)
        {
            gameObj = new GameObject {name = name, layer = LayerMask.NameToLayer("UI")};
            gameObj.transform.SetParent(parent);
            
            gameObj.transform.localScale = Vector3.one;
            gameObj.transform.localPosition = Vector3.zero;
        }

        private void InitMeshComponent(Material meshMaterial)
        {
            rectTransform = gameObj.AddComponent<RectTransform>();
            rectTransform.localScale = Vector3.one;
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.anchoredPosition = Vector2.zero;
            rectTransform.pivot = Vector2.zero;
            rectTransform.sizeDelta = Vector2.zero;
            
            meshFilter = gameObj.AddComponent<MeshFilter>();
            meshRenderer = gameObj.AddComponent<MeshRenderer>();
            meshRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            meshRenderer.lightProbeUsage = UnityEngine.Rendering.LightProbeUsage.Off;
            meshRenderer.reflectionProbeUsage = UnityEngine.Rendering.ReflectionProbeUsage.Off;
            meshRenderer.receiveShadows = false;
            meshRenderer.sharedMaterial = meshMaterial;
            mesh = new Mesh {name = gameObj.name};
            meshFilter.mesh = mesh;
        }

        private void InitVertices(int vertCount)
        {
            vertices = new Vector3[vertCount];
            uvs = new Vector2[vertCount];
            triangles = new int[(vertCount - 2) * 3];
            colors = new Color[vertCount];
            vp = new UIMeshVertPool(vertCount);
        }

        public void OnFontTextureRebuild(Font changedFont)
        {
            if (changedFont != spriteHelper.getFont)
                return;
            RebuildMesh();
        }

        private void ReDrawVerts(int vertIndex, float vertPosx, float vertPosy, Color vertColor, float vertAngle = 0)
        {
            vertices[vertIndex].x = vertPosx;
            vertices[vertIndex].y = vertPosy;
            //colors[vertIndex] = vertColor;
        }

        private void ReDrawUVs(int vertIndex, float uvVectorx, float uvVectory)
        {
            uvs[vertIndex].x = uvVectorx;
            uvs[vertIndex].y = uvVectory;
        }

        /// <summary>
        /// 通过旋转矩阵对顶点进行旋转，等同于修改unity中的rotation
        /// </summary>
        /// <param name="angle"></param>
        /// <param name="vertX"></param>
        /// <param name="vertY"></param>
        private void RotationZ(float angle, ref float vertX, ref float vertY)
        {
            float x, y;
            float CosAngle = Mathf.Cos(angle * Mathf.Deg2Rad);
            float SinAngle = Mathf.Sin(angle * Mathf.Deg2Rad);
            x = vertX * CosAngle - SinAngle * vertY;
            y = vertX * SinAngle + CosAngle * vertY;
            vertX = x;
            vertY = y;
        }

        /// <summary>
        /// 绘制一个正方形，占用4个顶点。根据顶点下标，绘制位置，图集宽高，uv位置绘制。
        /// </summary>
        /// <param name="vertIndex">顶点下标，不是4的倍数会从4的倍数开始绘制</param>
        /// <param name="drawRect">xy为绘制起点(左下角)，宽高代表绘制大小与缩放,使用世界坐标</param>
        /// <param name="uvRect">xy是左下角uv坐标值。根据这个值算出四个点的uv</param>
        private void DrawMesh(int vertIndex, Rect drawRect, Rect uvRect, Color drawColor, float vertAngle = 0)
        {
            if (vertIndex % 4 != 0)
            {
                vertIndex = vertIndex >> 2 << 2;
            }
            int vertStartIndex = vertIndex;
            int triglesIndex = vertIndex * 6 / 4;
            var zero = 0f;

            ReDrawVerts(vertIndex, zero, zero, drawColor, vertAngle);
            ReDrawUVs(vertIndex, zero, zero);

            ReDrawVerts(++vertIndex, zero, zero, drawColor, vertAngle);
            ReDrawUVs(vertIndex, zero, zero);

            ReDrawVerts(++vertIndex, zero, zero, drawColor, vertAngle);
            ReDrawUVs(vertIndex, zero, zero);

            ReDrawVerts(++vertIndex, zero, zero, drawColor, vertAngle);
            ReDrawUVs(vertIndex, zero, zero);

            SetTriangle(triglesIndex, vertStartIndex);
        }

        private void SetTriangle(int triangleIndex, int vertIndex)
        {
            triangles[triangleIndex] = vertIndex;
            triangles[triangleIndex + 1] = vertIndex + 1;
            triangles[triangleIndex + 2] = vertIndex + 2;

            triangles[triangleIndex + 3] = vertIndex;
            triangles[triangleIndex + 4] = vertIndex + 2;
            triangles[triangleIndex + 5] = vertIndex + 3;
        }

        void SetColor(int index, Color color)
        {
            colors[index + 0] = color;
            colors[index + 1] = color;
            colors[index + 2] = color;
            colors[index + 3] = color;
        }

        void SetAlpha(int index, float alpha)
        {
            colors[index + 0].a = alpha;
            colors[index + 1].a = alpha;
            colors[index + 2].a = alpha;
            colors[index + 3].a = alpha;
        }

        //构建Meag
        public void RebuildMesh()
        {
            if (m_isDrawUving)
            {
                return;
            }
            /*var blocks = vp.GetBlocks;
            var charList = vp.GetCharList;

            for (int i = 0; i < blocks.Length; i++)
            {
                if (blocks[i] == -1)
                {
                    CharacterInfo ch;
                    if (!spriteHelper.GetCharacterInfo(charList[i], out ch))
                    {
                        uvNeedRebuild = true;
                        return;
                    }
                    SetUv(i << 2, ch);
                }
            }*/
            needRefresh = true;
        }

        private void SetUv(int index, Vector2[] uvSources)
        {
            //矩形的四个顶点
            /*var uvTopLeft = ch.uvTopLeft;
            var uvTopRight = ch.uvTopRight;
            var uvBottomRight = ch.uvBottomRight;
            var uvBottomLeft = ch.uvBottomLeft;*/

            uvs[index + 1] = uvSources[0];
            uvs[index + 2] = uvSources[1];
            uvs[index + 3] = uvSources[2];
            uvs[index + 0] = uvSources[3];
        }

        public void Update()
        {
            if (!this.bShow)
            {
                return;
            }

            var dt = Time.deltaTime;
            if (dt < float.Epsilon) return;

            if (needRefresh)
            {
                RefreshMesh();
                needRefresh = false;
            }

            var startIndex = _requireList.Start;
            var endIndex = _requireList.End;
            for (var i = endIndex - 1; i >= startIndex; i--)
            {
                var animation = _requireList[i].MeshAnimation;
                if (animation.IsEnd) continue;
                
                animation.Update(dt, _playerPos);
                
                if (animation.IsEnd)
                {
                    animation.Clear();
                }
            }

            /*if (null == _requireList || dynamic == false)
            {
                return;
            }

            var e = _requireList.GetEnumerator();
            while (e.MoveNext())
            {
                spriteHelper.RequestCharactersInTexture(e.Current.Value.outPutString);
            }
            e.Dispose();

            if (uvNeedRebuild)
            {
                uvNeedRebuild = false;
                RebuildMesh();
            }*/
        }

        private void RefreshMesh()
        {
            var len = vp.Range;
            if (len < 0) len = 0;

            mesh.vertices = vertices;
            mesh.uv = uvs;
            mesh.SetTriangles(triangles, vp.Start, len, 0);
            mesh.colors = colors;
        }

        public void RefreshDynamicPos(UIMeshInfo meshText, Vector3 pos, float scale, Color color, bool isCrit = false)
        {
            if (meshText == null) return;
            
            meshText.pos = pos;
            meshText.color = color;
            if (meshText.charLen != 0)
            {
                pos.x -= meshText.offset.x * (scale / meshText.scale) * 0.5f;
                pos.y += meshText.offset.y * (scale / meshText.scale) * 0.5f;
            }
            
            m_isDrawUving = true;

            tmpIndex = 0;
            tmpPos = pos;
            RefreshVertInfo(meshText, meshText.StartVertIndex, meshText.EndVertIndex, scale, color, isCrit);
            RefreshVertInfo(meshText, meshText.Start2VertIndex, meshText.End2VertIndex, scale, color, isCrit);
            needRefresh = true;
            m_isDrawUving = false;
        }

        private void RefreshVertInfo(UIMeshInfo meshText, int startIndex, int endIndex, float scale, Color color, bool isCrit = false)
        {
            if (startIndex == -1) return;

            //UnityEngine.Debug.LogError($"str = {meshText.outPutString}, start = {startIndex}, end = {endIndex}");
            if (startIndex > endIndex)
            {
                var scaleX = scale * globalScale.x;
                var scaleY = scale * globalScale.y;

                tmpIndex = 0;
                for (int i = startIndex; i >= endIndex; i--, tmpIndex++)
                {
                    int verticeStartIndex = i << 2;
                    var chIndex = tmpIndex / 4;

                    var ch = meshText.chars[tmpIndex];
                    spriteHelper.TryGetBounds(ch, out var bounds, isCrit);
                
                    SetVert(verticeStartIndex, tmpPos, bounds[0], bounds[1], bounds[2], bounds[3], scaleX, scaleY);
                    SetAlpha(verticeStartIndex, color.a);
                    tmpPos.x += bounds[4] * scaleX;
                }
            }
            else
            {
                var scaleX = scale * globalScale.x;
                var scaleY = scale * globalScale.y;
                
                tmpIndex = 0;
                for (int i = startIndex; i <= endIndex; i++, tmpIndex++)
                {
                    int verticeStartIndex = i << 2;
                    var chIndex = tmpIndex / 4;

                    var ch = meshText.chars[tmpIndex];
                    spriteHelper.TryGetBounds(ch, out var bounds, isCrit);
                
                    SetVert(verticeStartIndex, tmpPos, bounds[0], bounds[1], bounds[2], bounds[3], scaleX, scaleY);
                    SetAlpha(verticeStartIndex, color.a);
                    tmpPos.x += bounds[4] * scaleX;
                }
            }
        }

        public void DoAnimation(UIMeshInfo meshInfo, float yOffset = 120, DamageFloatDir dir = DamageFloatDir.Up, Vector3 dirValue = default, float dirOffset = 0f)
        {
            if (meshInfo == null)
            {
                return;
            }

            if (meshInfo.IsInValid())
            {
                UnityEngine.Debug.LogError($"mesh vert index is invalid, start = {meshInfo.StartVertIndex}, end = {meshInfo.EndVertIndex}");
                return;
            }
            
            var animation = meshInfo.MeshAnimation;
            animation.DoAnimation(meshInfo, this, _curveProfile, yOffset, dir, dirValue, dirOffset);
            animation.Update(0, _playerPos);
        }

        public void DrawTextUVs(out UIMeshInfo uiMeshInfo, string showString, Vector3 pos, Vector3 worldPos, float scale, Color color, bool isCrit = false, int fontOffset = -1)
        {
            uiMeshInfo = null;
            if (!vp.CheckVert(showString.Length))
            {
                return;
            }

            if (!_requireList.Predict(1))
            {
                return;
            }

            if (_offsets.Count > 0)
            {
                var offset = _offsets[_offsetIndex];
                pos.x += offset.x*_canvasScale.x;
                pos.y += offset.y*_canvasScale.y;
                _offsetIndex++;
                if (_offsetIndex >= _offsets.Count) _offsetIndex = 0;
            }

            var finalFontOffset = fontOffset * 20;
            UIMeshInfo meshInfo = GenerateText(showString, finalFontOffset);
            meshInfo.pos = pos;
            meshInfo.scale = scale;
            meshInfo.color = color;
            meshInfo.isCrit = isCrit;
            meshInfo.FontOffset = finalFontOffset;
            uiMeshInfo = meshInfo;
            
            if (dynamic)
                spriteHelper.RequestCharactersInTexture(showString);

            int vertIndex = 0;
            bool bSuccess = true;
            var offsets= GetTextRect(showString, scale, isCrit, meshInfo.FontOffset, out bSuccess);
            if (!bSuccess)
            {
                UnityEngine.Debug.LogError($"DrawDynamicTextUVs uv error={showString}");
                return;
            }

            meshInfo.offset = offsets;
            pos.x -= offsets.x * 0.5f;
            pos.y += offsets.y * 0.5f;

            var startVertIndex = -1;
            var endVertIndex = -1;
            var start2VertIndex = -1;
            var end2VertIndex = -1;
            
            var len = showString.Length;
            var oldState = false;
            
            var scaleX = scale * globalScale.x;
            var scaleY = scale * globalScale.y;
            for (var i = 0; i < len; i++)
            {
                var convertChar = meshInfo.chars[i];
                vertIndex = vp.GetVert(convertChar);
                if (startVertIndex == -1)
                {
                    oldState = vp.Reverse;
                    startVertIndex = vertIndex;
                }

                if (oldState != vp.Reverse && start2VertIndex == -1)
                {
                    endVertIndex = vp.Reverse ? vp.Length - 1 : 0;

                    start2VertIndex = vertIndex;
                }
                
                if (i == len - 1)
                {
                    if (start2VertIndex == -1)
                    {
                        endVertIndex = vertIndex;
                    }
                    else
                    {
                        end2VertIndex = vertIndex;
                    }
                }

                if (vertIndex == -1)
                {
                    return;
                }

                int verticeStartIndex = vertIndex << 2;
                int triangleStartIndex = vertIndex * 6;
                
                /*CharacterInfo ch;
                if (!spriteHelper.GetCharacterInfo(convertChar, out ch))
                {
                    //uvNeedRebuild = true;
                    continue;
                }*/

                spriteHelper.TryGetUvs(convertChar, out var uvSource);
                spriteHelper.TryGetBounds(convertChar, out var bounds, isCrit);

                SetVert(verticeStartIndex, pos, bounds[0],bounds[1], bounds[2], bounds[3], scaleX, scaleY);
                SetUv(verticeStartIndex, uvSource);
                
                SetColor(verticeStartIndex, color);
                SetTriangle(triangleStartIndex, verticeStartIndex);
                pos.x += bounds[4] * scaleX;
            }

            meshInfo.StartVertIndex = startVertIndex;
            meshInfo.EndVertIndex = endVertIndex;
            meshInfo.Start2VertIndex = start2VertIndex;
            meshInfo.End2VertIndex = end2VertIndex;

            needRefresh = true;
        }

        private UIMeshInfo GenerateText(string originStr, int fontOffset)
        {
            UIMeshInfo info = _requireList.Get();
            info.SetInfo(originStr, fontOffset);
            info.playerPos = _playerPos;
            return info;
        }

        private void SetVert(int index, Vector3 pos, float minX, float minY, float maxX, float maxY, float scaleX, float scaleY)
        {
            minX *= scaleX;
            maxX *= scaleX;
            minY *= scaleY;
            maxY *= scaleY;

            var xPos = pos.x;
            var yPos = pos.y;
            
            vertices[index + 1].x = xPos + minX;
            vertices[index + 1].y = yPos + maxY;
            
            vertices[index + 2].x = xPos + maxX;
            vertices[index + 2].y = yPos + maxY;
            
            vertices[index + 3].x = xPos + maxX;
            vertices[index + 3].y = yPos + minY;
            
            vertices[index].x = xPos + minX;
            vertices[index].y = yPos + minY;
        }

        public Vector2 GetTextRect(string showString, float scale,  bool isCrit, int fontOffset, out bool bSuccess)
        {
            float length = 0;
            float height = 0;
            bSuccess = true;

            var scaleX = scale * globalScale.x;
            var scaleY = scale * globalScale.y;
            
            for (var i = 0; i < showString.Length; i++)
            {
                //CharacterInfo ch;
                var convertChar = GetFontCharOffset(showString[i], fontOffset);
                /*bSuccess = spriteHelper.GetCharacterInfo(convertChar, out ch);
                if (!bSuccess)
                {
                    break;
                }*/

                spriteHelper.TryGetAdvanceAndHeight(convertChar,
                    out var advance, out var glyHeight);

                if (isCrit)
                {
                    length += advance * critScale * scaleX;
                    float fAbsHeight = glyHeight * scaleY;
                    height = height > fAbsHeight ? height : fAbsHeight;
                }
                else
                {
                    length += advance * scaleX;
                    float fAbsHeight = glyHeight * scaleY;
                    height = height > fAbsHeight ? height : fAbsHeight;
                }
            }
            m_rectVec.x = length;
            m_rectVec.y = height;
            return m_rectVec;

        }
        public void RefreshVertPos(UIMeshInfo uiMeshInfo, Vector3 pos, float scale, Color color, bool isCrit = false)
        {
            RefreshDynamicPos(uiMeshInfo, pos, scale, color, isCrit);
        }

        public void ReleaseVert(UIMeshInfo meshInfo)
        {
            if (meshInfo != null)
            {
                _requireList.Release(meshInfo);
                
                if (meshInfo.IsInValid()) return;
                
                ReleaseVertInfo(meshInfo.StartVertIndex, meshInfo.EndVertIndex);
                ReleaseVertInfo(meshInfo.Start2VertIndex, meshInfo.End2VertIndex);
                meshInfo.Clear();

                //indexArrPools.Release(vertArray);
                needRefresh = true;
            }
        }

        private void ReleaseVertInfo(int startIndex, int endIndex)
        {
            if (startIndex == -1) return;
            if (startIndex > endIndex)
            {
                for (int i = startIndex; i >= endIndex; i--)
                {
                    vp.ReleaseVert(i);
                    DrawMesh(i * 4, _rectZero, _rectZero, _colorWhite);
                }
            }
            else
            {
                for (int i = startIndex; i <= endIndex; i++)
                {
                    vp.ReleaseVert(i);
                    DrawMesh(i * 4, _rectZero, _rectZero, _colorWhite);
                }
            }
        }

        public void DrawDamage(out UIMeshInfo uiMeshInfo, string showString, Vector3 screenPos, Vector3 worldPos, float scale, Color color, bool isCrit = false, int fontOffset = -1)
        {
            DrawTextUVs(out uiMeshInfo, showString, screenPos, worldPos, scale, color, isCrit, fontOffset);
        }

        public static char GetFontCharOffset(char ch, int fontOffset)
        {
            if (fontOffset < 0) return ch;

            var v = ch - 46;
            if (v < 0) v = 0;
            return (char)(v + fontOffset);
        }

        public UIMeshVertPool VertPool => vp;
        public RecycleArray<UIMeshInfo> MeshInfo => _requireList;
    }
}