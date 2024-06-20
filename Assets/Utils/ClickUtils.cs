using LiquidShader;
using LiquidShader.Types;
using UnityEngine;

namespace Utils {

public class ClickUtils {
    public static Vector2Int? GetClickSimPos(Rendering rendering, SimulationState simulationState) {
        var relX = Input.mousePosition.x / Screen.width;
        var relY = Input.mousePosition.y / Screen.height;
        if (rendering.IsSplitScreen) {
            relX *= 2;
        }
        var simX = (int)(relX * simulationState.simResX);
        var simY = (int)(relY * simulationState.simResY);
        if (simX >= 0 && simY >= 0 && simX < simulationState.simResX && simY < simulationState.simResY) {
            var tgtPos = new Vector2Int(simX, simY);
            return tgtPos;
        }
        return null;
    }
}

}
