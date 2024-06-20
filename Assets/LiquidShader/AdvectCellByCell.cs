using LiquidShader.Types;
using UnityEngine;
using Utils;

namespace LiquidShader {

enum AdvectDirection {
    FromCenter,
    ToCenter,
    ToEdges
}

enum UpdateDirections {
    U,
    V,
    Both
}

public class AdvectCellByCell : MonoBehaviour {
    [SerializeField] AdvectDirection advectDirection = AdvectDirection.FromCenter;
    [SerializeField] bool doubleBuffered = false;
    [SerializeField] bool removeSourceVelocity = false;
    // [SerializeField] UpdateDirections updateDirections = UpdateDirections.U;

    public CellByCell cellByCell;
    CopyBuffer _copyBufer;

    ComputeShader _advectShader;

    public ComputeShader TestingGetShader() {
        return _advectShader;
    }

    void Awake() {
        cellByCell = new CellByCell();
        _copyBufer = new CopyBuffer();

        _advectShader = (ComputeShader)Resources.Load("LiquidShader/AdvectCellByCell");
    }

    public void AdvectVelocity(SimulationState simulationState, float simDeltaTime, float speed) {
        string kernelName;
        switch(advectDirection) {
            case AdvectDirection.FromCenter:
                kernelName = doubleBuffered ?
                    removeSourceVelocity ?
                        "AdvectVelocityFromCenterDoubleBufferedRemoveSrcVel"
                        : "AdvectVelocityFromCenterDoubleBuffered"
                    : "AdvectVelocityFromCenter";
                break;
            case AdvectDirection.ToCenter:
            case AdvectDirection.ToEdges:
            default:
                throw new System.Exception($"unhandled advectDirection {advectDirection}");
        }
        if(cellByCell.updateX == 0 && cellByCell.updateY == 0) {
            // initialize at start of sweep
            _copyBufer.CopyFloats(simulationState.simResX, simulationState.simResY, simulationState.uBuf, simulationState.u2Buf);
            _copyBufer.CopyFloats(simulationState.simResX, simulationState.simResY, simulationState.vBuf, simulationState.v2Buf);
        }
        var kernel = _advectShader.FindKernel(kernelName);
        _advectShader.SetBuffer(kernel, "_horizVel", simulationState.uBuf.GetComputeBuffer());
        _advectShader.SetBuffer(kernel, "_vertVel", simulationState.vBuf.GetComputeBuffer());
        _advectShader.SetBuffer(kernel, "_newHorizVel", simulationState.u2Buf.GetComputeBuffer());
        _advectShader.SetBuffer(kernel, "_newVertVel", simulationState.v2Buf.GetComputeBuffer());
        _advectShader.SetBuffer(kernel, "_isFluid", simulationState.sBuf.GetComputeBuffer());
        _advectShader.SetBuffer(kernel, "_debug", simulationState.debugBuf.GetComputeBuffer());
        _advectShader.SetFloat("_deltaTime", simDeltaTime);
        _advectShader.SetFloat("_speedDeltaTime", simDeltaTime * speed);
        _advectShader.SetInt("_updateX", cellByCell.updateX);
        _advectShader.SetInt("_updateY", cellByCell.updateY);
        _advectShader.SetInt("_simResX", simulationState.simResX);
        _advectShader.SetInt("_simResY", simulationState.simResY);
        _advectShader.Dispatch(kernel, 1, 1, 1);

        cellByCell.Inc(simulationState);

        if(cellByCell.updateX == 0 && cellByCell.updateY == 0) {
            // copy back at end of sweep
            _copyBufer.CopyFloats(simulationState.simResX, simulationState.simResY, simulationState.u2Buf, simulationState.uBuf);
            _copyBufer.CopyFloats(simulationState.simResX, simulationState.simResY, simulationState.v2Buf, simulationState.vBuf);
        }
    }
}

} // namespace LiquidShader
