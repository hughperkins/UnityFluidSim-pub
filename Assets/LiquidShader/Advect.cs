using LiquidShader.Types;
using UnityEngine;
using Utils;

namespace LiquidShader {

public class Advect : MonoBehaviour {
    ComputeShader _advectShader;
    ComputeShader _advectFloat4Shader;

    int _kAdvectFloat4;
    int _kAdvectVelocity;

    CopyBuffer _copyBuffer;

    public ComputeShader TestingGetShader() {
        return _advectShader;
    }

    void OnEnable() {
        _copyBuffer = new CopyBuffer();
        _advectShader = (ComputeShader)Resources.Load("LiquidShader/Advect");
        _advectFloat4Shader = (ComputeShader)Resources.Load("LiquidShader/AdvectFloat4");
    }

    public void AdvectVelocity(SimulationState simulationState, float simDeltaTime, float speed) {
        var shader = _advectShader;
        var kernel = shader.FindKernel("AdvectVelocity");
        shader.SetBuffer(kernel, "_horizVel", simulationState.uBuf.GetComputeBuffer());
        shader.SetBuffer(kernel, "_vertVel", simulationState.vBuf.GetComputeBuffer());
        shader.SetBuffer(kernel, "_newHorizVel", simulationState.u23BufParent.GetComputeBuffer());
        shader.SetBuffer(kernel, "_newVertVel", simulationState.v23BufParent.GetComputeBuffer());
        shader.SetBuffer(kernel, "_isFluid", simulationState.sBuf.GetComputeBuffer());
        shader.SetFloat("_deltaTime", simDeltaTime);
        shader.SetFloat("_speedDeltaTime", simDeltaTime * speed);
        shader.SetInt("_simResX", simulationState.simResX);
        shader.SetInt("_simResY", simulationState.simResY);
        shader.Dispatch(kernel, (simulationState.simResX + 8 - 1) / 8, (simulationState.simResY + 8 - 1) / 8, 1);
        _copyBuffer.CopyFloats(simulationState.simResX, simulationState.simResY, simulationState.u2Buf, simulationState.uBuf);
        _copyBuffer.CopyFloats(simulationState.simResX, simulationState.simResY, simulationState.v2Buf, simulationState.vBuf);
    }

    public void AdvectFloat4(SimulationState simulationState, float simDeltaTime, float speed, Buf2<Vector4> target, Buf2<Vector4> targetCopy) {
        var shader = _advectFloat4Shader;
        var kernel = shader.FindKernel("AdvectFloat4");
        shader.SetBuffer(kernel, "_isFluid", simulationState.sBuf.GetComputeBuffer());
        shader.SetBuffer(kernel, "_srcFloat4", target.GetComputeBuffer());
        shader.SetBuffer(kernel, "_destFloat4", targetCopy.GetComputeBuffer());
        shader.SetBuffer(kernel, "_horizVel", simulationState.uBuf.GetComputeBuffer());
        shader.SetBuffer(kernel, "_vertVel", simulationState.vBuf.GetComputeBuffer());
        shader.SetBuffer(kernel, "_debug", simulationState.debugBuf.GetComputeBuffer());
        shader.SetFloat("_deltaTime", simDeltaTime);
        shader.SetFloat("_speedDeltaTime", simDeltaTime * speed);
        shader.SetInt("_simResX", simulationState.simResX);
        shader.SetInt("_simResY", simulationState.simResY);
        shader.Dispatch(kernel, (simulationState.simResX + 8 - 1) / 8, (simulationState.simResY + 8 - 1) / 8, 1);

        _copyBuffer.CopyFloat4S(targetCopy, target);
    }
}

} // namespace LiquidShader
