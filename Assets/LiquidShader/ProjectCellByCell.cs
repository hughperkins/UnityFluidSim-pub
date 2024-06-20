using LiquidShader.Types;
using Utils;
using UnityEngine;

namespace LiquidShader {

[RequireComponent(typeof(ProjectSingleCell))]
public class ProjectCellByCell : MonoBehaviour {
    public CellByCell cellByCell;

    ProjectSingleCell _projectSingleCell;

    public void Reset() {
        cellByCell.Reset();
    }

    void OnEnable() {
        cellByCell = new CellByCell();
        _projectSingleCell = GetComponent<ProjectSingleCell>();
    }

    public void RunProject(SimulationState simulationState) {
        _projectSingleCell.ProjectCell(simulationState, cellByCell.updateX, cellByCell.updateY);
        cellByCell.Inc(simulationState);
    }
}

} // namespace LiquidShader
