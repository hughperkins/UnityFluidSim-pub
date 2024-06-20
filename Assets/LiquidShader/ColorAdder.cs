using LiquidShader.Types;
using UnityEngine;
using Utils;

namespace LiquidShader {

public class ColorAdder: MonoBehaviour {
    [SerializeField] bool hardSetColor = false;
    [Range(0.1f, 10.0f)] [SerializeField] float colorAddSpeed = 0.1f;
    // [Range(0.0f, 1.0f)][SerializeField] float colorAddNoise = 0.0f;

    ComputeShader _shader;

    void Awake() {
        this._shader = (ComputeShader)Resources.Load("LiquidShader/AddColor");
    }

    public void AddColor(SimulationState simulationState, float simDeltaTime, float speed) {
        var kernel = _shader.FindKernel("AddColor");
        _shader.SetBuffer(kernel, "_horizVel", simulationState.uBuf.GetComputeBuffer());
        _shader.SetBuffer(kernel, "_vertVel", simulationState.vBuf.GetComputeBuffer());
        _shader.SetBuffer(kernel, "_isFluid", simulationState.sBuf.GetComputeBuffer());
        _shader.SetFloat("_speedDeltaTime", simDeltaTime * speed);
        _shader.SetInt("_simResX", simulationState.simResX);
        _shader.SetInt("_simResY", simulationState.simResY);
        _shader.SetFloat("_colorAddSpeed", colorAddSpeed);
        _shader.SetBool("_colorSet", hardSetColor);
        _shader.SetBuffer(kernel, "_m", simulationState.mBuf.GetComputeBuffer());
        _shader.SetBuffer(kernel, "_colorSources", simulationState.colorSourcesBuf.GetComputeBuffer());
        _shader.Dispatch(kernel,(simulationState.simResX + 8 - 1) / 8, (simulationState.simResY + 8 -1) / 8, 1);
    }
}

} // namespace LiquidShader
