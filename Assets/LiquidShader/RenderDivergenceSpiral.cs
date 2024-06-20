using LiquidShader.Types;
using UnityEngine;
using Utils;

namespace LiquidShader {
public class RenderDivergenceSpiral : MonoBehaviour {
    [SerializeField][Range(0.01f, 1.0f)] float spiralRadius = 1;
    [SerializeField][Range(0.01f, 3.0f)] float rotationSpeed = 1;
    [SerializeField][Range(0.01f, 3.0f)] float radialSpeed = 1;
    [SerializeField] bool renderPositive = true;
    [SerializeField] bool renderNegative = true;
    [SerializeField] bool render = false;
    [SerializeField] Texture waterTexture;

    ComputeShader _renderDivergenceSpiralShader;

    void OnEnable() {
        _renderDivergenceSpiralShader = Resources.Load<ComputeShader>("LiquidShader/RenderDivergenceSpiral");
    }

    public void RenderSpiral(RenderTexture renderTexture, SimulationState simulationState, float speedDeltaTime, int[] renderRes) {
        if (!render) return;
        // Debug.Log("render divergencespiral");
        var shader = _renderDivergenceSpiralShader;
        var kernel = shader.FindKernel("Render");
        shader.SetBuffer(kernel, "_texPosBase", simulationState.texPosBase.GetComputeBuffer());
        shader.SetInt("_divergenceTexPosOffset", simulationState.divergenceTexPos.Offset);
        shader.SetBuffer(kernel, "_divergence", simulationState.divergenceBuf.GetComputeBuffer());
        shader.SetTexture(kernel, "_renderTexture", renderTexture);
        shader.SetTexture(kernel, "_waterTexture", waterTexture);
        shader.SetInts("_simRes", simulationState.SimResInts);
        shader.SetBool("_renderPositive", renderPositive);
        shader.SetBool("_renderNegative", renderNegative);
        shader.SetInts("_renderRes", renderRes);
        shader.SetFloat("_speedDeltaTime", speedDeltaTime);
        shader.SetFloat("_spiralRadius", spiralRadius);
        shader.SetFloat("_rotationSpeed", rotationSpeed);
        shader.SetFloat("_radialSpeed", radialSpeed);
        shader.Dispatch(kernel, (renderRes[0] + 8 - 1) / 8, (renderRes[1] + 8 - 1) / 8, 1);
    }

    void UpdateDivergenceTexPos(SimulationState simulationState, float deltaTime, int[] renderRes) {
        var shader = _renderDivergenceSpiralShader;
        var kernel = shader.FindKernel("UpdateDivergenceTexPos");
        shader.SetBuffer(kernel, "_texPosBase", simulationState.texPosBase.GetComputeBuffer());
        shader.SetInt("_divergenceTexPosOffset", simulationState.divergenceTexPos.Offset);
        shader.SetBuffer(kernel, "_divergence", simulationState.divergenceBuf.GetComputeBuffer());
        shader.SetFloat("_deltaTime", deltaTime);
        shader.SetInts("_renderRes", renderRes);
        shader.SetInts("_simRes", simulationState.SimResInts);
        shader.Dispatch(
            kernel,
            (simulationState.simResX + 8 - 1) / 8,
            (simulationState.simResY + 8 - 1) / 8,
            1);
    }

    public void Render(RenderTexture renderTexture, SimulationState simulationState, float speed, float deltaTime, int[] renderRes) {
        float speedDeltaTime = speed * deltaTime;
        RenderSpiral(renderTexture, simulationState, speedDeltaTime, renderRes);
        UpdateDivergenceTexPos(simulationState, deltaTime, renderRes);
    }
}
}
