
using UnityEngine;

public class SimpleJoystick : MonoBehaviour
{
    Vector2 _lastTouchPos;
    Vector2 _currentTouchPos;
    Vector2 _dir;
    bool _isTouch;
    static SimpleJoystick _instance;
    public static SimpleJoystick Instance
    {
        get{
            if(_instance == null)
            {
                _instance = new GameObject("SimipleJobstick").AddComponent<SimpleJoystick>();
            }

            return _instance;
        }
    }

    public void Update()
    {
        bool isTouch = Input.GetMouseButton(0);
        if(!isTouch)
        {
            _isTouch = false;
            return;
        }

        Vector2 mousePos = Input.mousePosition;
        if(isTouch && !_isTouch)
        {
            _lastTouchPos = mousePos;
            _isTouch = true;
            return;
        }

        _currentTouchPos = mousePos;
        _dir =  mousePos - _lastTouchPos;
    }

    public bool GetDir(out float dir)
    {
        dir = default;

        if(!_isTouch) return false;

        dir = (Vector2.SignedAngle(_dir, Vector2.up) + 360) % 360;
        Debug.LogError(_dir + " " + dir);

        return true;
    }

    void OnGUI()
    {
        var lstTouchPos = new Vector2(_lastTouchPos.x, Screen.height - _lastTouchPos.y);
        GUI.Box(new Rect(lstTouchPos- new Vector2(10, 10), new Vector2(20, 20)), "");

        var currentTouchPos = new Vector2(_currentTouchPos.x, Screen.height - _currentTouchPos.y);
        GUI.Box(new Rect(currentTouchPos - new Vector2(10, 10), new Vector2(20, 20)), "x");
        
    }
}