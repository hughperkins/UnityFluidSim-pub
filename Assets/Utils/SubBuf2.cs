/*
Cut sub-buffer from large main buffer
Idea is to reduce the number of "uavs" sent to computeshader kernel, and avoid the error
"too many uavs send to computeshader kernel" (or similar)
*/

using UnityEngine;

namespace Utils {

public class SubBuf2<T> : IBuf2<T> {
    Buf2<T> _parent;
    int _offset;
    int _resX;
    int _resY;

    public ComputeBuffer GetComputeBuffer() {
        return _parent.GetComputeBuffer();
    }

    public int Offset {
        get {
            return _offset;
        }
    }

    public int ResX {
        get {
            return _resX;
        }
    }

    public int ResY {
        get {
            return _resY;
        }
    }

    public Vector2Int ResVec2 {
        get{
            return new Vector2Int(_resX, _resY);
        }
    }

    public int[] ResInts {
        get{
            return new int[]{_resX, _resY};
        }
    }

    public Buf2<T> Parent {
        get {
            return _parent;
        }
    }

    public SubBuf2(Buf2<T> parent, int offset, int resX, int resY) {
        this._parent = parent;
        this._offset = offset;
        this._resX = resX;
        this._resY = resY;
    }

    public void SetData(T[,] data) {
        var parentData = _parent.Data;
        for(var i = 0; i < _resX; i++) {
            for(var j = 0; j < _resY; j++) {
                parentData[i, j] = data[i, j];
            }
        }
        _parent.GetComputeBuffer().SetData(_parent.Data, _offset, _offset, _resX * _resY);
    }
}

} // namespace LiquidShader
