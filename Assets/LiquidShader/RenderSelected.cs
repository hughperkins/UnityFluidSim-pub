using System;
using LiquidShader.Types;
using UnityEngine;
using UnityEngine.UIElements;
using Utils;

namespace LiquidShader {
public class RenderSelected : MonoBehaviour {
    [SerializeField] Color selectedColor = new Color(0, 0.5f, 1);
    [SerializeField] bool enable = false;

    LiquidShaderRenderer _liquidShaderRenderer;
    Rendering _rendering;

    ComputeShader _cellSelectionShader;
    ComputeShader _renderingSelectedCellsShader;

    void OnEnable() {
        _cellSelectionShader = Resources.Load<ComputeShader>("LiquidShader/CellSelection");
        _renderingSelectedCellsShader = Resources.Load<ComputeShader>("LiquidShader/RenderingSelectedCells");
        _rendering = GetComponent<Rendering>();
        _liquidShaderRenderer = GetComponent<LiquidShaderRenderer>();
    }

    public void Render(
        RenderTexture renderTexture, SimulationState simulationState, int[] renderRes
    ) {
        if (!enable) return;
        var shader = _renderingSelectedCellsShader;
        var kernel = shader.FindKernel("RenderSelectedCells");
        shader.SetInts("_simRes", simulationState.SimResInts);
        shader.SetInts("_renderRes", renderRes);
        shader.SetVector("_currentCellColor", selectedColor);
        shader.SetBuffer(kernel, "_selected", simulationState.selected.GetComputeBuffer());
        shader.SetTexture(kernel, "_renderTexture", renderTexture);
        shader.Dispatch(
            kernel,
            (renderRes[0] + 8 - 1) / 8,
            (renderRes[1] + 8 - 1) / 8,
            1);
    }

    public void SetSelected(
        SimulationState simulationState, int[] pos, int selected
    ) {
        if (!enable) return;
        var shader = _cellSelectionShader;
        var kernel = shader.FindKernel("SetSelected");
        shader.SetInts("_simRes", simulationState.SimResInts);
        shader.SetInts("_tgtPos", pos);
        shader.SetInt("_newVal", selected);
        shader.SetBuffer(kernel, "_selected", simulationState.selected.GetComputeBuffer());
        shader.Dispatch(kernel, 1, 1, 1);
    }

    void Update() {
        var simulationState = _liquidShaderRenderer.simulationState;
        if (Input.GetKey(KeyCode.LeftCommand) && Input.GetMouseButtonDown(0)) {
            if (ClickFilter.HitUI()) return;

            var simPos = ClickUtils.GetClickSimPos(_rendering, simulationState);
            var simPosInts = new int[] {
                simPos?.x ?? 0, simPos?.y ?? 0
            };
            Debug.Log($"set selected {simPos} -1");
            SetSelected(simulationState, simPosInts, -1);
        } else if (Input.GetKey(KeyCode.LeftCommand) && Input.GetKeyDown(KeyCode.Escape)) {
            Debug.Log("clearing selected");
            var clearedSelected = new int[simulationState.simResX, simulationState.simResY];
            simulationState.selected.SetData(clearedSelected);
        }
    }
}
}
