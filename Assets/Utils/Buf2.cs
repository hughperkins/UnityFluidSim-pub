using System.Runtime.InteropServices;
using UnityEngine;

namespace Utils {

public class Buf2<T> : IBuf2<T> {
    ComputeBuffer _computeBuffer;

    public ComputeBuffer GetComputeBuffer() {
        return _computeBuffer;
    }

    int _numEls;
    int _stride;

    int _width;
    int _height;

    T[,] _data;

    public T[,] Data {
        get {
            return _data;
        }
    }

    public Buf2(int width, int height) {
        this._width = width;
        this._height = height;
        this._numEls = width * height;
        _stride = Marshal.SizeOf<T>();
        _computeBuffer = new ComputeBuffer(_numEls, _stride);
        _data = new T[width, height];
    }

    public int Offset {
        get {
            return 0;
        }
    }

    public Vector2Int ResVec2 {
        get{
            return new Vector2Int(_width, _height);
        }
    }

    public int[] ResInts {
        get{
            return new int[]{_width, _height};
        }
    }

    public int ResX {
        get {
            return _width;
        }
    }

    public int ResY {
        get {
            return _height;
        }
    }

    public void ToGPU() {
        _computeBuffer.SetData(_data);
    }

    public void FromGPU() {
        _computeBuffer.GetData(_data);
    }

    public void ApplyShader(ComputeShader shader, int kernel, string name) {
        shader.SetBuffer(kernel, name, _computeBuffer);
    }

    public T this[int i, int j] {
        get {
            return _data[i, j];
        }
        set {
            _data[i, j] = value;
        }
    }

    public void GetData(T[,] tgt) {
        CheckDimensions(tgt);
        _computeBuffer.GetData(tgt);
    }

    void CheckDimensions(T[,] datat) {
        if(_data.Rank != 2) {
            throw new System.Exception($"data rank {_data.Rank} should be 2");
        }
        if(_data.GetLength(0) != _width) {
            throw new System.Exception($"data length 0 {_data.GetLength(0)} should be {_width}");
        }
        if(_data.GetLength(1) != _height) {
            throw new System.Exception($"data length 1 {_data.GetLength(1)} should be {_height}");
        }
    }

    public void SetData(T[,] data) {
        CheckDimensions(data);
        this._data = data;
        ToGPU();
    }

    public void Release() {
        if (_computeBuffer == null) return;
        _computeBuffer.Release();
        _computeBuffer = null;
    }
}

} // namespace LiquidShader
