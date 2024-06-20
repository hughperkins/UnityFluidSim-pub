using System;
using UnityEngine;

namespace Utils {
public class DisplayConfig {

    readonly Vector3 _bottomLeft;
    readonly Vector3 _topRight;

    public float ScreenWidth { get; }
    public float ScreenHeight { get; }

    public Vector3 BottomLeft{ get{ return _bottomLeft; } }
    public Vector3 TopRight{ get{ return _topRight; } }

    public float SLeft { get; }
    public float SRight { get; }
    public float SBottom { get; }
    public float STop { get; }

    public DisplayConfig(Camera cam) {
        var mainCamera = cam;
        if( mainCamera == null ) {
            throw new Exception( "mainCamera should not be null" );
        }
        // ScreenWidth = (mainCamera.orthographicSize * 2.0f) * mainCamera.aspect;
        // ScreenHeight = mainCamera.orthographicSize * 2.0f;
        // Debug.Log($"screenwidth {ScreenWidth} screenheight {ScreenHeight}");

        _bottomLeft = cam.ViewportToWorldPoint(new Vector3(0, 0, cam.nearClipPlane));
        _topRight = cam.ViewportToWorldPoint(new Vector3(1, 1, cam.nearClipPlane));

        SLeft = _bottomLeft.x;
        SRight = _topRight.x;
        SBottom = _bottomLeft.y;
        STop = _topRight.y;

        ScreenWidth = SRight - SLeft;
        ScreenHeight = STop - SBottom;
        // Debug.Log($"screenwidth {ScreenWidth} screenheight {ScreenHeight} left {SLeft} right {SRight}");
    }
}
}
