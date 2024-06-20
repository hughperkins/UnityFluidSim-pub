using System;
using LiquidShader.Types;
using UnityEngine;
using Utils;

namespace LiquidShader {
public class RenderArrows : MonoBehaviour {
    [SerializeField] bool cellArrows = false;
    [SerializeField] bool staggeredArrows = false;
    [SerializeField] bool scaleArrowsWithSpeed = false;
    [SerializeField][Range(0f, 1f)] float arrowPushPullLambda = 0;
    // [SerializeField] bool arrowsInbound = false;
    // [SerializeField] bool arrowsCentered = false;
    [SerializeField] ArrowVelocityType arrowVelocityType = ArrowVelocityType.CellCenter;

    ComputeShader _renderingArrowsShader;

    void OnEnable() {
        _renderingArrowsShader = Resources.Load<ComputeShader>("LiquidShader/RenderingArrows");
    }

    void RenderArrowsSpecificKernel(
        string kernelName, RenderTexture renderTexture, SimulationState simulationState, float speedDeltaTime, int[] renderRes
    ) {
        var shader = _renderingArrowsShader;
        var kernel = shader.FindKernel(kernelName);
        shader.SetBuffer(kernel, "_isFluid", simulationState.sBuf.GetComputeBuffer());
        shader.SetBuffer(kernel, "_horizVel", simulationState.uBuf.GetComputeBuffer());
        shader.SetBuffer(kernel, "_vertVel", simulationState.vBuf.GetComputeBuffer());
        shader.SetBuffer(kernel, "_debug", simulationState.debugBuf.GetComputeBuffer());
        shader.SetFloat("_arrowPushPullLambda", arrowPushPullLambda);
        // shader.SetBool("_arrowsCentered", arrowsCentered);
        shader.SetBuffer(kernel, "_floatsToRender", simulationState.divergenceBuf.GetComputeBuffer());
        shader.SetTexture(kernel, "_renderTexture", renderTexture);
        shader.SetInts("_simRes", simulationState.SimResInts);
        shader.SetInts("_renderRes", renderRes);
        shader.SetBool("_arrowsForU", arrowVelocityType == ArrowVelocityType.U);
        shader.SetBool("_arrowsForV", arrowVelocityType == ArrowVelocityType.V);
        shader.SetFloat("_time", Time.time);
        shader.SetFloat("_speedDeltaTime", scaleArrowsWithSpeed ? speedDeltaTime : 1);
        shader.Dispatch(kernel, (simulationState.simResX + 8 - 1) / 8, (simulationState.simResY + 8 - 1) / 8, 1);
    }

    public void Render(RenderTexture renderTexture, SimulationState simulationState, float speedDeltaTime, int[] renderRes) {
        if (cellArrows) {
            RenderArrowsSpecificKernel("RenderArrows", renderTexture, simulationState, speedDeltaTime, renderRes);
        }
        if (staggeredArrows) {
            RenderArrowsSpecificKernel("RenderStaggeredArrows", renderTexture, simulationState, speedDeltaTime, renderRes);
        }
    }
}
}
