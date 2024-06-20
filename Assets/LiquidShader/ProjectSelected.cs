using LiquidShader.Types;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace LiquidShader {

[RequireComponent(typeof(DivergenceCalculator))]
public class ProjectSelected : MonoBehaviour {
    /*
     * clear divergence on selected cells
     */
    ComputeShader _computeShader;
    DivergenceCalculator _divergenceCalculator;
    LiquidShaderRenderer _liquidShaderRenderer; // to get simulation state

    void OnEnable() {
        _divergenceCalculator = GetComponent<DivergenceCalculator>();
        _computeShader = Resources.Load<ComputeShader>("LiquidShader/ProjectSelected");
        _liquidShaderRenderer = GetComponent<LiquidShaderRenderer>();
    }

    public void RunProjection() {
        var simulationState = _liquidShaderRenderer.simulationState;

        var kernel = _computeShader.FindKernel("ProjectSelected");

        _computeShader.SetInt("_simResX", simulationState.simResX);
        _computeShader.SetInt("_simResY", simulationState.simResY);

        _computeShader.SetBuffer(kernel, "_horizVel", simulationState.uBuf.GetComputeBuffer());
        _computeShader.SetBuffer(kernel, "_vertVel", simulationState.vBuf.GetComputeBuffer());
        _computeShader.SetBuffer(kernel, "_selected", simulationState.selected.GetComputeBuffer());
        _computeShader.SetBuffer(kernel, "_isFluid", simulationState.sBuf.GetComputeBuffer());
        _computeShader.SetBuffer(kernel, "_debug", simulationState.debugBuf.GetComputeBuffer());

        _computeShader.Dispatch(
            kernel,
            (simulationState.simResX + 8 - 1) / 8,
            (simulationState.simResY + 8 - 1) / 8,
            1);

        _divergenceCalculator.CalcDivergence(simulationState);
    }

    #if UNITY_EDITOR
    [CustomEditor(typeof(ProjectSelected))]
    class ProjectSelectedEditor : Editor {
        public override void OnInspectorGUI() {
            base.OnInspectorGUI();
            var projectSelected = (ProjectSelected)target;
            if (GUILayout.Button("Clear divergence on selected")) {
                projectSelected.RunProjection();
            }
        }
    }
    #endif
}

} // namespace LiquidShader
