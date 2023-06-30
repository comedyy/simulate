
using System.Collections.Generic;
using UnityEngine;

namespace Game
{
    public enum ColorEnum
    {
        White,
        Red,
        Green,
        ElementFire,
        ElementIce,
        ElementThunder,
        ElementWind,
        RecoverHp,
        Injure
    }

    public enum DamageFloatDir
    {
        Up,
        TargetDir
    }
    
    public interface IDamageNumManager
    {
        /// <summary>
        /// 伤害飘字
        /// <param name="damage">伤害值</param>
        /// <param name="worldPos">世界坐标</param>
        /// <param name="isCrit">是否暴击</param>
        /// <param name="color">颜色</param>
        /// <param name="hitPosition">受击位置</param>
        /// </summary>
        void ShowDamage(int damage, Vector3 worldPos, bool isCrit = false, ColorEnum color = ColorEnum.White, Vector3 hitPosition = default);
        
        /// <summary>
        /// 伤害飘字
        /// <param name="damage">伤害值</param>
        /// <param name="worldPos">世界坐标</param>
        /// <param name="isCrit">是否暴击</param>
        /// <param name="color">颜色</param>
        /// <param name="hitPosition">受击位置</param>
        /// </summary>
        void ShowDamage(long damage, Vector3 worldPos, bool isCrit = false, ColorEnum color = ColorEnum.White, Vector3 hitPosition = default);
        void ShowDamageParam(int damage, Vector3 worldPos, bool isCrit = false, ColorEnum color = ColorEnum.White, DamageFloatDir dir = DamageFloatDir.Up, Vector3 dirValue = default);
        void ShowDamageParam(long damage, Vector3 worldPos, bool isCrit = false, ColorEnum color = ColorEnum.White, DamageFloatDir dir = DamageFloatDir.Up, Vector3 dirValue = default);
        void ShowDamageParam(string damage, Vector3 worldPos, bool isCrit = false, ColorEnum color = ColorEnum.White, DamageFloatDir dir = DamageFloatDir.Up, Vector3 dirValue = default);
        void ShowDamageByScreenPos(int damage, Vector3 screenPos, Vector3 worldPos, bool isCrit = false, ColorEnum color = ColorEnum.White, DamageFloatDir dir = DamageFloatDir.Up, Vector3 dirValue = default);
        void ShowDamageByScreenPos(string damage, Vector3 screenPos, Vector3 worldPos, bool isCrit = false, ColorEnum color = ColorEnum.White, DamageFloatDir dir = DamageFloatDir.Up, Vector3 dirValue = default);
        void Clear();
        void ResetCollectTimes();
        
        float Scale { get; set; }
        Color GetColor(ColorEnum c, bool retEleColor = false);
        void SetVisible(bool set);
        void SetSkip(bool skip);
        bool IsVisible { get; }
        UIMeshVertPool VertPool { get; }
        RecycleArray<UIMeshInfo> MeshInfo { get; }
        uint CallTimes { get; }
    }
    
    public class DamageNumManager : MonoBehaviour, IDamageNumManager
    {
       public Font _font;
       private UIMeshText _textMesh;
       public DamageAnimCurveProfile _curveProfile;
       private DamageAnimCurveIns _curveIns;
       private float _dirOffset;
       private float _yOffset;
       private float _critScale;
       private float _randomXOffset;
       private Dictionary<uint, string> _damageCache;
       private bool _hide;
       private bool _skip;
       private uint _callTimes;
       private float _screenHalfWidth;
       private float _screenHalfHeight;
       private Color _defaultColor;

       private const int MeshInfoMaxCount = 2000;
       private const int VertMaxCount = 2000 * 8;

       public Transform damageLayer; // 父节点


       private static Color[] _colors = new[]
       {
           Color.white,
           Color.red,
           Color.green,
           // GameUtility.StringToHtmlColor("#84ea6b")
       };

        static DamageNumManager _instance;
       void Awake()
       {
            _instance = this;
            _instance.Initialize();
       }

       public static DamageNumManager Instance => _instance;

       public void Initialize()
       {
           var scale = 1;// GameCore.UI.GetScaleFactor();

        //    var configDefine = GameCore.DataTable.QueryRecord<ConfigDefine>(0);
           _dirOffset = 20;// configDefine.hurtFontOffset1;
           _critScale = 1.33f;//configDefine.hurtFontCritScale;
           _randomXOffset = 10;// configDefine.hurtFontLocation * scale;
           _yOffset = 80;// configDefine.hurtFontHeight * scale;

           _defaultColor = Color.white;
           _screenHalfWidth = Screen.width * 0.5f;
           _screenHalfHeight = Screen.height * 0.5f;
           _damageCache = new Dictionary<uint, string>();
           _hide = false;//Constant.GameSetting.GetSettingData(Constant.GameSetting.SettingId.HideAttackNum);

            var meshScale = new Vector3(scale, scale, 1);
            _textMesh = new UIMeshText(damageLayer, "uiDamageMesh", _font,
                VertMaxCount, MeshInfoMaxCount, meshScale);
            _textMesh.GameObject.transform.localScale = new Vector3(1f/scale, 1f/scale, 1);
            _textMesh.SetCritScale(_critScale);
            _textMesh.SetXRandomValue((int)_randomXOffset);
        
            Debug.Log($"Damage ui Scale {meshScale}");

            _curveIns = new DamageAnimCurveIns();
            _curveIns.SetCurve(_curveProfile);
            _textMesh.SetCurveProfile(_curveIns);
            _textMesh.SetOffsets(_curveProfile.Offsets);
            _textMesh.SetCanvasScale(new Vector2(scale, scale));

            
           
        //    BattleEvents.PlayerPos.Subscribe(GameEvent.GameEventBattlePlayerPos.EventId, OnPlayerPosChanged);
        //    BattleEvents.ShowDamage.Subscribe(GameEvent.BattleEventShowDamageEvent.EventId, EventShowDamage);
        //    GameCore.Event.Subscribe(ScreenOrientationChangeEventArgs.EventId, OnScreenOrientationChanged);
       }
       
       private void EventShowDamage()
       {
            ShowDamageNoRecursive(100, UnityEngine.Vector3.zero, false, ColorEnum.White, default);
       }

       private void OnScreenOrientationChanged()
       {
        //    CalculateScale();

        //    if (_textMesh != null)
        //    {
        //        var scale = GameCore.UI.GetScaleFactor();
        //        _textMesh.GameObject.transform.localScale = new Vector3(1f/scale, 1f/scale, 1);
        //        _textMesh.SetCanvasScale(new Vector2(scale, scale));
        //    }
       }

    //    private void CalculateScale()
    //    {
    //        var configDefine = GameCore.DataTable.QueryRecord<ConfigDefine>(0);
    //        var scale = GameCore.UI.GetScaleFactor();
    //        _randomXOffset = configDefine.hurtFontLocation * scale;
    //        _yOffset = configDefine.hurtFontHeight * scale;
    //        _screenHalfWidth = Screen.width * 0.5f;
    //        _screenHalfHeight = Screen.height * 0.5f;
    //    }

       private void ShowDamageNoRecursive(uint damage, Vector3 worldPos, bool isCrit = false, ColorEnum color = ColorEnum.White, Vector3 hitPosition = default)
       {
           var dir = hitPosition.Equals(default) ? DamageFloatDir.Up : DamageFloatDir.TargetDir;
           if (dir == DamageFloatDir.TargetDir)
           {
               hitPosition = ConvertDir(hitPosition);
           }
           
           var screenPos = Camera.main.WorldToScreenPoint(worldPos);
           if (!_damageCache.TryGetValue(damage, out var showString))
           {
               showString = damage.ToString();
               _damageCache[damage] = showString;
           }
           _callTimes++;
           screenPos.z = 0;
           _textMesh.DrawDamage(out var meshInfo, showString, screenPos, worldPos, Scale, GetColor(color), isCrit, GetFontIndexOffset(color));
           _textMesh.DoAnimation(meshInfo, _yOffset, dir, hitPosition, _dirOffset);
           _textMesh.Show(true);
       }
       
       public void ShowDamage(int damage, Vector3 worldPos, bool isCrit = false, ColorEnum color = ColorEnum.White, Vector3 hitPosition = default)
       {
           var dir = hitPosition.Equals(default) ? DamageFloatDir.Up : DamageFloatDir.TargetDir;
           if (dir == DamageFloatDir.TargetDir)
           {
               hitPosition = ConvertDir(hitPosition);
           }
           ShowDamageParam(damage, worldPos, isCrit, color, dir, hitPosition);
       }

       public void ShowDamage(long damage, Vector3 worldPos, bool isCrit = false, ColorEnum color = ColorEnum.White, Vector3 hitPosition = default)
       {
           var dir = hitPosition.Equals(default) ? DamageFloatDir.Up : DamageFloatDir.TargetDir;
           if (dir == DamageFloatDir.TargetDir)
           {
               hitPosition = ConvertDir(hitPosition);
           }
           ShowDamageParam(damage, worldPos, isCrit, color, dir, hitPosition);
       }

       public void ShowDamageParam(int damage, Vector3 worldPos, bool isCrit = false,  ColorEnum color = ColorEnum.White, DamageFloatDir dir = DamageFloatDir.Up, Vector3 dirValue = default)
       {
           ShowDamageParam((long)damage, worldPos, isCrit, color, dir, dirValue);
       }

       public void ShowDamageParam(long damage, Vector3 worldPos, bool isCrit = false, ColorEnum color = ColorEnum.White, DamageFloatDir dir = DamageFloatDir.Up, Vector3 dirValue = default)
       {
           var screenPos = Camera.main.WorldToScreenPoint(worldPos);
           ShowDamageByScreenPos(damage, screenPos, worldPos, isCrit, color, dir, dirValue);
       }

       public void ShowDamageParam(string damage, Vector3 worldPos, bool isCrit = false, ColorEnum color = ColorEnum.White, DamageFloatDir dir = DamageFloatDir.Up, Vector3 dirValue = default)
       {
           var screenPos = Camera.main.WorldToScreenPoint(worldPos);
   
           ShowDamageByScreenPos(damage, screenPos, worldPos, isCrit, color, dir, dirValue);
       }

       public void ShowDamageByScreenPos(int damage, Vector3 screenPos, Vector3 worldPos, bool isCrit = false, ColorEnum color = ColorEnum.White, DamageFloatDir dir = DamageFloatDir.Up, Vector3 dirValue = default)
       {
           ShowDamageByScreenPos((long)damage, screenPos, worldPos, isCrit, color, dir, dirValue);
       }

       public void ShowDamageByScreenPos(long damage, Vector3 screenPos, Vector3 worldPos, bool isCrit = false, ColorEnum color = ColorEnum.White, DamageFloatDir dir = DamageFloatDir.Up, Vector3 dirValue = default)
       {
           if (_hide) return;
           
           if (!_damageCache.TryGetValue((uint)damage, out var showString))
           {
               showString = damage.ToString();
               _damageCache[(uint)damage] = showString;
           }
           _callTimes++;
           screenPos.z = 0;
           _textMesh.DrawDamage(out var meshInfo, showString, screenPos, worldPos, Scale, GetColor(color), isCrit, GetFontIndexOffset(color));
           _textMesh.DoAnimation(meshInfo, _yOffset, dir, dirValue, _dirOffset);
           _textMesh.Show(true);
       }
       
       public void ShowDamageByScreenPos(string damage, Vector3 screenPos, Vector3 worldPos, bool isCrit = false, ColorEnum color = ColorEnum.White, DamageFloatDir dir = DamageFloatDir.Up, Vector3 dirValue = default)
       {
           if (_hide) return;

           _callTimes++;
           screenPos.z = 0;
           _textMesh.DrawDamage(out var meshInfo, damage, screenPos, worldPos, Scale, GetColor(color), isCrit, GetFontIndexOffset(color));
           _textMesh.DoAnimation(meshInfo, _yOffset, dir, dirValue, _dirOffset);
           _textMesh.Show(true);
       }

       public Color GetColor(ColorEnum c, bool retEleColor = false)
       {
           var index = (int) c;
           if (index < _colors.Length)
           {
               return _colors[(int)c];
           }

           if (retEleColor)
           {
               return _curveProfile.ElementColor[index - 3];
           }

           return _defaultColor;
       }

       public void SetVisible(bool set)
       {
           _hide = !set;
       }

       public void SetSkip(bool skip)
       {
           _skip = skip;
       }

       public bool IsVisible => !_hide;
       public UIMeshVertPool VertPool => _textMesh.VertPool;
       public RecycleArray<UIMeshInfo> MeshInfo => _textMesh.MeshInfo;
       public uint CallTimes => _callTimes;

       private int GetFontIndexOffset(ColorEnum c)
       {
           if (c < ColorEnum.ElementFire)
           {
               return 0;
           }
           
           return (int)c - 2;
       }

       private Vector3 ConvertDir(Vector3 dirValue)
       {
           dirValue = Camera.main.WorldToScreenPoint(dirValue);
           dirValue.x = (dirValue.x -_screenHalfWidth)/ _screenHalfWidth;
           dirValue.y = (dirValue.y - _screenHalfHeight) / _screenHalfHeight;
           dirValue.z = 0;
           return dirValue;
       }

       public void Clear()
       {
           _textMesh?.Clear();
           _damageCache.Clear();
           _textMesh?.Show(false);
       }

       public void ResetCollectTimes()
       {
           _callTimes = 0;
       }

       public float Scale { get; set; } = 1f;

       public void Update()
        {
            _textMesh?.Update();
        }

        private Vector3 _playerPos;
        // private void OnPlayerPosChanged(object sender, GameFrameworkEventArgs e)
        // {
        //     if (e is GameEvent.GameEventBattlePlayerPos playerPosEvent)
        //     {
        //          if (playerPosEvent.Init)
        //          {
        //              _playerPos = playerPosEvent.Pos;
        //          }
        //          var initScreenPos = GameCore.CameraManager.WorldCamera.WorldToScreenPoint(_playerPos);
        //          if (playerPosEvent.Init)
        //          {
        //              _textMesh.SetInitPos(initScreenPos);
        //          }

        //          _textMesh.SetPlayerPos(initScreenPos);
        //     }
        // }
    }
}