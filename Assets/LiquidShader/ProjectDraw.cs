using LiquidShader.Types;
using Utils;
using UnityEngine;

namespace LiquidShader {

[RequireComponent(typeof(Draw))]
[RequireComponent(typeof(Rendering))]
[RequireComponent(typeof(LiquidShaderRenderer))]
[RequireComponent(typeof(ProjectSingleCell))]
public class ProjectDraw : MonoBehaviour {
    Draw _draw;
    Rendering _rendering;
    LiquidShaderRenderer _liquidShaderRenderer;
    ProjectSingleCell _projectSingleCell;

    void OnEnable() {
        _draw = GetComponent<Draw>();
        _rendering = GetComponent<Rendering>();
        // needed to get the simulation state for mouse drawing
        _liquidShaderRenderer = GetComponent<LiquidShaderRenderer>();
        _projectSingleCell = GetComponent<ProjectSingleCell>();
    }

    void Update() {
        if (_draw.drawType != DrawType.RemoveDivergence) return;
        if (!Input.GetMouseButtonDown(0) || ClickFilter.HitUI() || Input.GetKey(KeyCode.LeftCommand)) return;
        var relX = Input.mousePosition.x / Screen.width;
        var relY = Input.mousePosition.y / Screen.height;
        if(_rendering.IsSplitScreen) {
            relX *= 2;
        }
        var simulationState = _liquidShaderRenderer.simulationState;
        var simX = (int)(relX * simulationState.simResX);
        var simY = (int)(relY * simulationState.simResY);
        if (simX >= 0 && simY >= 0 && simX < simulationState.simResX && simY < simulationState.simResY) {
            _projectSingleCell.ProjectCell(simulationState, simX, simY);
        }
    }
}

} // namespace LiquidShader
