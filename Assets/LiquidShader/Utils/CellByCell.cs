using LiquidShader.Types;

namespace Utils {

public class CellByCell {
    public int updateX = 1;
    public int updateY = 1;

    public int lastUpdateX = 1;
    public int lastUpdateY = 1;

    public void Reset() {
        updateX = 1;
        updateY = 1;

        lastUpdateX = 1;
        lastUpdateY = 1;
    }

    public int[] LastUpdatePos {
        get {
            return new int[]{ lastUpdateX, lastUpdateY };
        }
    }

    public int[] UpdatePos {
        get {
            return new int[]{ updateX, updateY };
        }
    }

    void CheckBounds(SimulationState simulationState) {
        // in case the resolution has changed recently
        if(updateX >= simulationState.simResX) {
            updateX = simulationState.simResX - 1;
        }
        if(updateY >= simulationState.simResY) {
            updateY = simulationState.simResY - 1;
        }
    }

    public void Inc(SimulationState simulationState) {
        CheckBounds(simulationState);

        lastUpdateX = updateX;
        lastUpdateY = updateY;

        updateX += 1;
        if(updateX >= simulationState.simResX - 1) {
            updateX = 1;
            updateY += 1;
            if(updateY >= simulationState.simResY - 1) {
                updateY = 1;
            }
        }
    }

    public void Dec(SimulationState simulationState) {
        CheckBounds(simulationState);

        lastUpdateX = updateX;
        lastUpdateY = updateY;

        updateX -= 1;
        if(updateX <= -1) {
            updateX = simulationState.simResX - 1;
            updateY -= 1;
            if(updateY <= -1) {
                updateY = simulationState.simResY - 1;
            }
        }

    }
}

} // namespace LiquidShader
