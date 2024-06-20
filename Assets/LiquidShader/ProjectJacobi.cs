using LiquidShader.Types;
using UnityEngine;
using Utils;

namespace LiquidShader {

[RequireComponent(typeof(Pooling))]
public class ProjectJacobi : MonoBehaviour {
    [Range(1, 500)][SerializeField] public int solverIterations = 100;

    ComputeShader _computeShader;

    Pooling _pooling;

    CopyBuffer _copyBuffer;

    void Awake() {
        Debug.Log("Project.Awake");
        _computeShader = (ComputeShader)Resources.Load("LiquidShader/ProjectJacobi");
        Debug.Log($"computeshader {_computeShader}");

        _pooling = GetComponent<Pooling>();

        _copyBuffer = new CopyBuffer();
    }

    public void RunProject(
        SimulationState simulationState, float simDeltaTime, float speed, bool debug = false
    ) {
        var kernel1 = _computeShader.FindKernel("Project1");
        var kernel2 = _computeShader.FindKernel("Project2");

        for(var it = 0; it < solverIterations; it++) {
            var t = (float)it / (float)(solverIterations - 1);
            var divergenceThreshold = (1 - t) * _pooling.startDivergenceThreshold + t * _pooling.endDivergenceThreshold;
            var kernel = kernel1;
            _computeShader.SetInt("_simResX", simulationState.simResX);
            _computeShader.SetInt("_simResY", simulationState.simResY);

            _computeShader.SetBool("_useDivergenceChunks", _pooling.useDivergenceChunks && it > 0);
            _computeShader.SetInt("_divergenceChunkSizeLog2", Pooling.DivergenceChunkSizeLog2);
            _computeShader.SetBuffer(kernel, "_divergence", simulationState.divergenceBuf.GetComputeBuffer());
            _computeShader.SetBuffer(kernel, "_divergenceChunks", simulationState.chunkDivergenceBuffer.GetComputeBuffer());

            _computeShader.SetBuffer(kernel, "_horizVel", simulationState.uBuf.GetComputeBuffer());
            _computeShader.SetBuffer(kernel, "_vertVel", simulationState.vBuf.GetComputeBuffer());

            _computeShader.SetBuffer(kernel, "_u23", simulationState.u23BufParent.GetComputeBuffer());
            _computeShader.SetBuffer(kernel, "_v23", simulationState.v23BufParent.GetComputeBuffer());

            _computeShader.SetInt("_u2Offset", simulationState.u2Buf.Offset);
            _computeShader.SetInt("_u3Offset", simulationState.u3Buf.Offset);
            _computeShader.SetInt("_v2Offset", simulationState.v2Buf.Offset);
            _computeShader.SetInt("_v3Offset", simulationState.v3Buf.Offset);

            _computeShader.SetBuffer(kernel, "_isFluid", simulationState.sBuf.GetComputeBuffer());
            _computeShader.SetBuffer(kernel, "_debug", simulationState.debugBuf.GetComputeBuffer());
            _computeShader.SetInt("_saveDivergence", it == solverIterations - 1 || _pooling.useDivergenceChunks ? 1 : 0);
            _computeShader.Dispatch(kernel, (simulationState.simResX + 8 - 1) / 8, (simulationState.simResY + 8 - 1) / 8, 1);

            kernel = kernel2;
            _computeShader.SetInt("_simResX", simulationState.simResX);
            _computeShader.SetInt("_simResY", simulationState.simResY);

            _computeShader.SetBool("_useDivergenceChunks", _pooling.useDivergenceChunks && it > 0);
            _computeShader.SetInt("_divergenceChunkSizeLog2", Pooling.DivergenceChunkSizeLog2);
            _computeShader.SetBuffer(kernel, "_divergenceChunks", simulationState.chunkDivergenceBuffer.GetComputeBuffer());

            _computeShader.SetBuffer(kernel, "_horizVel", simulationState.uBuf.GetComputeBuffer());
            _computeShader.SetBuffer(kernel, "_vertVel", simulationState.vBuf.GetComputeBuffer());
            _computeShader.SetBuffer(kernel, "_u23", simulationState.u23BufParent.GetComputeBuffer());
            _computeShader.SetBuffer(kernel, "_v23", simulationState.v23BufParent.GetComputeBuffer());

            _computeShader.SetInt("_u2Offset", simulationState.u2Buf.Offset);
            _computeShader.SetInt("_u3Offset", simulationState.u3Buf.Offset);
            _computeShader.SetInt("_v2Offset", simulationState.v2Buf.Offset);
            _computeShader.SetInt("_v3Offset", simulationState.v3Buf.Offset);

            _computeShader.SetBuffer(kernel, "_isFluid", simulationState.sBuf.GetComputeBuffer());
            _computeShader.Dispatch(kernel2, (simulationState.simResX + 8 - 1) / 8, (simulationState.simResY + 8 - 1) / 8, 1);

            if(_pooling.useDivergenceChunks || it == solverIterations - 1) {
                _pooling.AbsThresholdFloats(simulationState.divergenceBuf, simulationState.chunkDivergenceBuffer, threshold: divergenceThreshold);
            }
        }
    }
}

} // namespace LiquidShader
