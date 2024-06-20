// using LiquidShaderColo;
using UnityEngine;
using Utils;
using SimulationState = LiquidShader.Types.SimulationState;

namespace LiquidShader {

public class ProjectGaussSeidelClean : MonoBehaviour {
    [Range(1, 500)][SerializeField] public int solverIterations = 100;
    [SerializeField] bool stepSubPasses = false;

    ComputeShader _computeShader;
    int[] _subPassPos;
    bool _lastStepSubPasses;

    RenderSelected _renderSelected;
    LiquidShaderRenderer _liquidShaderRenderer;

    void OnEnable() {
        _computeShader = (ComputeShader)Resources.Load("LiquidShader/ProjectGaussSeidelClean");
        _subPassPos = new []{0, 0};
        _renderSelected = GetComponent<RenderSelected>();
        _liquidShaderRenderer = GetComponent<LiquidShaderRenderer>();
    }

    void RunPass(SimulationState simulationState, int[] passOffset) {
        var kernel = _computeShader.FindKernel("ProjectGaussSeidel");
        _computeShader.SetInt("_simResX", simulationState.simResX);
        _computeShader.SetInt("_simResY", simulationState.simResY);
        _computeShader.SetInts("_passOffset", passOffset);
        _computeShader.SetBuffer(kernel, "_horizVel", simulationState.uBuf.GetComputeBuffer());
        _computeShader.SetBuffer(kernel, "_vertVel", simulationState.vBuf.GetComputeBuffer());

        _computeShader.SetBuffer(kernel, "_isFluid", simulationState.sBuf.GetComputeBuffer());
        // halve the number of horizontal and vertical threads because each sub-pass only processes one quarter of all cells
        _computeShader.Dispatch(
            kernel,
            (simulationState.simResX + 8 * 2 - 1) / 8 / 2,
            (simulationState.simResY + 8 * 2 - 1) / 8 / 2,
            1);
    }

    void IncrementSubPassPos() {
        _subPassPos[1] += 1;
        if (_subPassPos[1] == 2) {
            _subPassPos[1] = 0;
            _subPassPos[0] += 1;
        }
        if (_subPassPos[0] == 2) {
            _subPassPos = new int[] {
                0, 0
            };
        }
    }

    void LoadSelectedCells() {
        var simulationState = _liquidShaderRenderer.simulationState;
        var simRes = simulationState.SimResInts;
        var newSelected = new int[simRes[0], simRes[1]];
        for (var i = _subPassPos[0]; i < simRes[0]; i += 2) {
            for (var j = _subPassPos[1]; j < simRes[1]; j += 2) {
                newSelected[i, j] = 1;
            }
        }
        simulationState.selected.SetData(newSelected);
    }

    void ClearSelectedCells() {
        var simulationState = _liquidShaderRenderer.simulationState;
        var simRes = simulationState.SimResInts;
        var newSelected = new int[simRes[0], simRes[1]];
        simulationState.selected.SetData(newSelected);
    }

    void RunSubPass(SimulationState simulationState) {
        RunPass(simulationState, _subPassPos);
        IncrementSubPassPos();
        LoadSelectedCells();
    }

    void RunIterations(SimulationState simulationState) {
        for (var it = 0; it < solverIterations; it++) {
            for (var passI = 0; passI < 2; passI++) {
                for (var passJ = 0; passJ < 2; passJ++) {
                    RunPass(simulationState, new[] {
                        passI, passJ
                    });
                }
            }
        }
    }

    void Update() {
        if (_lastStepSubPasses != stepSubPasses) {
            _lastStepSubPasses = stepSubPasses;
            if (stepSubPasses) {
                LoadSelectedCells();
            } else {
                ClearSelectedCells();
            }
        }
    }

    public void RunProject(SimulationState simulationState) {
        if (stepSubPasses) {
            RunSubPass(simulationState);
        } else {
            RunIterations(simulationState);
        }
    }
}

} // namespace LiquidShader
