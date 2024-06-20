using LiquidShader.Types;
using UnityEngine;

namespace LiquidShader {

[RequireComponent(typeof(Pooling))]
public class DivergenceCalculator : MonoBehaviour {
    ComputeShader _computeShader;

    Pooling _pooling;

    void OnEnable() {
        _computeShader = (ComputeShader)Resources.Load("LiquidShader/Divergence");

        _pooling = GetComponent<Pooling>();
    }

    public void CalcDivergence(SimulationState simulationState) {
        var kernel = _computeShader.FindKernel("CalcDivergence");

        _computeShader.SetInt("_simResX", simulationState.simResX);
        _computeShader.SetInt("_simResY", simulationState.simResY);

        _computeShader.SetBuffer(kernel, "_horizVel", simulationState.uBuf.GetComputeBuffer());
        _computeShader.SetBuffer(kernel, "_vertVel", simulationState.vBuf.GetComputeBuffer());
        _computeShader.SetBuffer(kernel, "_isFluid", simulationState.sBuf.GetComputeBuffer());
        _computeShader.SetBuffer(kernel, "_divergence", simulationState.divergenceBuf.GetComputeBuffer());
        _computeShader.Dispatch(kernel, (simulationState.simResX + 8 - 1) / 8, (simulationState.simResY + 8 - 1) / 8, 1);

        _pooling.AbsThresholdFloats(simulationState.divergenceBuf, simulationState.chunkDivergenceBuffer,
            threshold: _pooling.noRunProjectDivergenceThreshold);
        _pooling.AbsThresholdFloats(simulationState.chunkDivergenceBuffer, simulationState.chunkDivergenceL2Buf,
            threshold: _pooling.noRunProjectDivergenceThreshold);
    }
}

} // namespace LiquidShader
