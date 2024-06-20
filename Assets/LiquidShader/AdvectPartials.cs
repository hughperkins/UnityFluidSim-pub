using LiquidShader.Types;
using UnityEngine;

namespace LiquidShader {

public class AdvectPartials : MonoBehaviour {
    /*
    Use the discrete formulation of (\v \cdot \nabla)\v to handle projection.
    */
    ComputeShader _advectShader;

    // int kAdvectFloat4;
    int _kAdvectVelocity;

    CopyBuffer _copyBuffer;

    public ComputeShader TestingGetShader() {
        return _advectShader;
    }

    void Awake() {
        _copyBuffer = new CopyBuffer();
        _advectShader = (ComputeShader)Resources.Load("LiquidShader/AdvectPartials");

        // kAdvectFloat4 = AdvectShader.FindKernel("AdvectFloat4");
        _kAdvectVelocity = _advectShader.FindKernel("AdvectVelocity");
    }

    public void AdvectVelocity(SimulationState simulationState, float simDeltaTime, float speed) {
        // Debug.Log("AdvectVelocity");
        var kernel = _kAdvectVelocity;
        _advectShader.SetBuffer(kernel, "_horizVel", simulationState.uBuf.GetComputeBuffer());
        _advectShader.SetBuffer(kernel, "_vertVel", simulationState.vBuf.GetComputeBuffer());
        _advectShader.SetBuffer(kernel, "_newHorizVel", simulationState.u23BufParent.GetComputeBuffer());
        _advectShader.SetBuffer(kernel, "_newVertVel", simulationState.v23BufParent.GetComputeBuffer());
        _advectShader.SetBuffer(kernel, "_isFluid", simulationState.sBuf.GetComputeBuffer());
        _advectShader.SetBuffer(kernel, "_debug", simulationState.debugBuf.GetComputeBuffer());
        _advectShader.SetFloat("_deltaTime", simDeltaTime);
        _advectShader.SetFloat("_speedDeltaTime", simDeltaTime * speed);
        _advectShader.SetInt("_simResX", simulationState.simResX);
        _advectShader.SetInt("_simResY", simulationState.simResY);
        _advectShader.Dispatch(kernel, (simulationState.simResX + 8 - 1) / 8, (simulationState.simResY + 8 - 1) / 8, 1);
        _copyBuffer.CopyFloats(simulationState.simResX, simulationState.simResY, simulationState.u2Buf, simulationState.uBuf);
        _copyBuffer.CopyFloats(simulationState.simResX, simulationState.simResY, simulationState.v2Buf, simulationState.vBuf);
    }

    // public void AdvectFloat4(SimulationState simulationState, float simDeltaTime, float speed, Buf2<Vector4> target, Buf2<Vector4> targetCopy) {
    //     int kernel = kAdvectFloat4;
    //     AdvectShader.SetBuffer(kernel, "_isFluid", simulationState.sBuf.GetComputeBuffer());
    //     AdvectShader.SetBuffer(kernel, "_srcFloat4", target.GetComputeBuffer());
    //     AdvectShader.SetBuffer(kernel, "_destFloat4", targetCopy.GetComputeBuffer());
    //     AdvectShader.SetBuffer(kernel, "_horizVel", simulationState.uBuf.GetComputeBuffer());
    //     AdvectShader.SetBuffer(kernel, "_vertVel", simulationState.vBuf.GetComputeBuffer());
    //     AdvectShader.SetFloat("_deltaTime", simDeltaTime);
    //     AdvectShader.SetFloat("_speedDeltaTime", simDeltaTime * speed);
    //     AdvectShader.SetInt("_simResX", simulationState.SimResX);
    //     AdvectShader.SetInt("_simResY", simulationState.SimResY);
    //     AdvectShader.Dispatch(kernel, (simulationState.SimResX + 8 - 1) / 8, (simulationState.SimResY + 8 - 1) / 8, 1);

    //     copyBuffer.CopyFloat4s(targetCopy, target);
    // }
}

} // namespace LiquidShader
