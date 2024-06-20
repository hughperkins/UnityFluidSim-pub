using LiquidShader.Types;
using Utils;
using UnityEngine;

namespace LiquidShader {

[RequireComponent(typeof(DivergenceCalculator))]
public class ProjectSingleCell : MonoBehaviour {
    ComputeShader _computeShader;
    DivergenceCalculator _divergenceCalculator;

    void OnEnable() {
        _divergenceCalculator = GetComponent<DivergenceCalculator>();
        _computeShader = Resources.Load<ComputeShader>("LiquidShader/ProjectSingleCell");
    }

    public void ProjectCell(SimulationState simulationState, int cellX, int cellY) {
        var kernel = _computeShader.FindKernel("ProjectSingleCell");

        _computeShader.SetBuffer(kernel, "_horizVel", simulationState.uBuf.GetComputeBuffer());
        _computeShader.SetBuffer(kernel, "_vertVel", simulationState.vBuf.GetComputeBuffer());
        _computeShader.SetBuffer(kernel, "_isFluid", simulationState.sBuf.GetComputeBuffer());
        _computeShader.SetBuffer(kernel, "_debug", simulationState.debugBuf.GetComputeBuffer());

        _computeShader.SetInt("_simResX", simulationState.simResX);
        _computeShader.SetInt("_simResY", simulationState.simResY);
        _computeShader.SetInt("_updateX", cellX);
        _computeShader.SetInt("_updateY", cellY);

        _computeShader.Dispatch(kernel, 1, 1, 1);

        _divergenceCalculator.CalcDivergence(simulationState);
    }
}

} // namespace LiquidShader
