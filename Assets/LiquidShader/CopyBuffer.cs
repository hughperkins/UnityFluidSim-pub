using System;
using UnityEngine;
using Utils;

namespace LiquidShader {

public class CopyBuffer {
    ComputeShader _copyShader;

    public CopyBuffer() {
        this._copyShader = (ComputeShader)Resources.Load("LiquidShader/Copy");
    }

    public void CopyFloat4S(Buf2<Vector4> src, Buf2<Vector4> dest) {
        if(src.ResX != dest.ResX) {
            throw new Exception($"width of src and dest mismach {src.ResX} vs {dest.ResX}");
        }
        if(src.ResY != dest.ResY) {
            throw new Exception($"height of src and dest mismach {src.ResY} vs {dest.ResY}");
        }
        var kernel = _copyShader.FindKernel("CopyFloat4s");
        _copyShader.SetBuffer(kernel, "_srcFloat4s", src.GetComputeBuffer());
        _copyShader.SetBuffer(kernel, "_destFloat4s", dest.GetComputeBuffer());
        _copyShader.SetInt("_simResX", src.ResX);
        _copyShader.SetInt("_simResY", src.ResY);
        _copyShader.Dispatch(kernel, (src.ResX + 8 - 1) / 8, (src.ResY + 8 - 1 ) / 8, 1);
    }

    public void CopyFloats(int simResX, int simResY, IBuf2<float> src, IBuf2<float> dest) {
        var kernel = _copyShader.FindKernel("CopyFloats");
        _copyShader.SetBuffer(kernel, "_srcFloats", src.GetComputeBuffer());
        _copyShader.SetBuffer(kernel, "_destFloats", dest.GetComputeBuffer());
        _copyShader.SetInt("_srcOffset", src.Offset);
        _copyShader.SetInt("_destOffset", dest.Offset);
        _copyShader.SetInt("_simResX", simResX);
        _copyShader.SetInt("_simResY", simResY);
        _copyShader.Dispatch(kernel, (simResX + 8 - 1) / 8, (simResY + 8 - 1 ) / 8, 1);
    }
}

} // namespace LiquidShader
