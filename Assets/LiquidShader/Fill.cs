using UnityEngine;
using Utils;

namespace LiquidShader {

public class Fill {
    ComputeShader _copyShader;

    public Fill() {
        this._copyShader = (ComputeShader)Resources.Load("LiquidShader/Fill");
    }

    public void FillFloats(int simResX, int simResY, IBuf2<float> tgt, float value) {
        var kernel = _copyShader.FindKernel("FillFloats");
        _copyShader.SetBuffer(kernel, "_tgtFloats", tgt.GetComputeBuffer());
        _copyShader.SetInt("_tgtOffset", tgt.Offset);
        _copyShader.SetFloat("_valueFloat", value);
        _copyShader.SetInt("_simResX", simResX);
        _copyShader.SetInt("_simResY", simResY);
        _copyShader.Dispatch(kernel, (simResX + 8 - 1) / 8, (simResY + 8 - 1 ) / 8, 1);
    }
}

} // namespace LiquidShader
