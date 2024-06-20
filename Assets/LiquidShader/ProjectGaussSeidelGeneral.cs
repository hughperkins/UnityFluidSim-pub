using LiquidShader.Types;
using UnityEngine;
using Utils;

namespace LiquidShader {

public class ProjectGaussSeidelGeneral : MonoBehaviour {
    [Range(1, 500)][SerializeField] public int solverIterations = 100;

    ComputeShader _computeShader;

    Pooling _pooling;
    CopyBuffer _copyBuffer;

    void Awake() {
        Debug.Log("Project.Awake");
        _computeShader = (ComputeShader)Resources.Load("LiquidShader/ProjectGaussSeidelGeneral");
        Debug.Log($"computeshader {_computeShader}");

        _pooling = GetComponent<Pooling>();

        _copyBuffer = new CopyBuffer();
    }

    void RunPass(SimulationState simulationState, float simDeltaTime, float speed, bool debug, int[] passOffset, bool useDivergenceChunks,
                 bool saveDivergence) {
        var divergenceChunkSizeLog2 = (int)(Mathf.Log(Pooling.ChunkSize) / Mathf.Log(2));
        if((int)Mathf.Pow(2, divergenceChunkSizeLog2) != Pooling.ChunkSize) {
            throw new System.Exception($"Invalid divergence chunk size: should be power of 2");
        }

        var kernel = _computeShader.FindKernel("ProjectGaussSeidelGeneral");
        _computeShader.SetInt("_simResX", simulationState.simResX);
        _computeShader.SetInt("_simResY", simulationState.simResY);

        _computeShader.SetInts("_passOffset", passOffset);

        _computeShader.SetBool("_useDivergenceChunks", useDivergenceChunks);
        _computeShader.SetInt("_divergenceChunkSizeLog2", divergenceChunkSizeLog2);
        _computeShader.SetBuffer(kernel, "_divergence", simulationState.divergenceBuf.GetComputeBuffer());
        _computeShader.SetBuffer(kernel, "_divergenceChunks", simulationState.chunkDivergenceBuffer.GetComputeBuffer());

        _computeShader.SetBuffer(kernel, "_horizVel", simulationState.uBuf.GetComputeBuffer());
        _computeShader.SetBuffer(kernel, "_vertVel", simulationState.vBuf.GetComputeBuffer());

        _computeShader.SetBuffer(kernel, "_isFluid", simulationState.sBuf.GetComputeBuffer());
        _computeShader.SetBuffer(kernel, "_debug", simulationState.debugBuf.GetComputeBuffer());
        _computeShader.SetInt("_saveDivergence", saveDivergence ? 1 : 0);
        // we halve the number of horizontal and vertical threads because each subpass only processes one quarter of all cells
        _computeShader.Dispatch(kernel, (simulationState.simResX + 8 * 2 - 1) / 8 / 2, (simulationState.simResY + 8 * 2 - 1) / 8 / 2, 1);
    }

    public void RunProject(
        SimulationState simulationState, float simDeltaTime, float speed, bool debug = false
    ) {
        /*
        So, in this one, we update one of 4 possible grids of cells that are not touching, each grid
        comprising of cells in rows and columns with a row or column of unprocessed cells in between
        each adjacent pair of processed cells

        Coloring of each pass similar to as follows:
        |1|2|1|2|
        |3|4|3|4|
        |1|2|1|2|

        We'll just iterate over passI in {0, 1} and passJ in {0, 1} for each complete pass
        */


        for(var it = 0; it < solverIterations; it++) {
            var t = (float)it / (float)(solverIterations - 1);
            var divergenceThreshold = (1 - t) * _pooling.startDivergenceThreshold + t * _pooling.endDivergenceThreshold;
            for(var passI = 0; passI < 2; passI++) {
                for(var passJ = 0; passJ < 2; passJ++) {
                    RunPass(
                        simulationState, simDeltaTime, speed, debug, new int[]{passI, passJ},
                        _pooling.useDivergenceChunks && it > 0,
                        it == solverIterations - 1 || _pooling.useDivergenceChunks);
                }
            }

            if(_pooling.useDivergenceChunks || it == solverIterations - 1) {
                _pooling.AbsThresholdFloats(simulationState.divergenceBuf, simulationState.chunkDivergenceBuffer, threshold: divergenceThreshold);
            }

            if(_pooling.runMaxAbs) {
                _pooling.MaxAbsFloats(simulationState.divergenceBuf, simulationState.chunkDivergenceBuffer);
                _pooling.MaxAbsFloats(simulationState.chunkDivergenceBuffer, simulationState.chunkDivergenceL2Buf);
                _pooling.MaxAbsFloats(simulationState.chunkDivergenceL2Buf, simulationState.chunkDivergenceL3Buf);
                _copyBuffer.CopyFloats(1, 1, simulationState.chunkDivergenceL2Buf, simulationState.uBuf);
            }
        }
    }
}

} // namespace LiquidShader
