using System;
using LiquidShader.Types;
using UnityEngine;
using Utils;

namespace LiquidShader {

enum RenderType {
    Design,
    DesignCheck,
    CellActive,
    ColorSource,
    Color,
    // VelSrc,
    Velocity,
    VelocityHSV,
    MovingTexture,
    VelU,
    VelV,
    Divergence,
    ChunkMaxDivergence,
    ChunkMaxDivL2,
    Debug,
}

enum ArrowVelocityType {
    CellCenter,
    U,
    V
}

[RequireComponent(typeof(ProjectCellByCell))]
[RequireComponent(typeof(AdvectCellByCell))]
[RequireComponent(typeof(RenderArrows))]
[RequireComponent(typeof(RenderDivergenceSpiral))]
[RequireComponent(typeof(RenderSelected))]
public class Rendering : MonoBehaviour {
    [Header("Grid")]
    [SerializeField] bool showGrid = false;
    [SerializeField] bool renderStaticTextureGrid;
    [SerializeField] bool renderRandomTextureGrid;
    [SerializeField] bool renderWavesGrid;
    [SerializeField] bool renderGridSolidColor;
    [SerializeField][Range(0.5f, 20)] float gridBoundarySpeed = 10;
    [SerializeField][Range(0.1f, 10)] float gridBoundaryWavelength = 4;
    [SerializeField][Range(0.05f, 0.6f)] float gridBoundaryThickness = 0.3f;

    [Header("Centers")]
    [SerializeField] RenderType renderType = RenderType.Color;

    [Header("General")]
    [SerializeField] bool showVelocityOverColor = false;
    [SerializeField] bool renderVelBuffers = false;
    [SerializeField] int renderResX = 1920;
    [SerializeField] int renderResY = 1080;

    [SerializeField] Setup setup;

    [Serializable] struct Setup {
        public ButtonGroupExclusiveSelected buttonGroupExclusiveSelected;
        // public ColorWheel colorWheel;
        public Texture waterTexture;
    }

    ProjectCellByCell _projectCellByCell;
    AdvectCellByCell _advectCellByCell;
    RenderArrows _renderArrows;
    RenderDivergenceSpiral _renderDivergenceSpiral;

    ComputeShader _renderingCentersShader;
    ComputeShader _renderingVelocityCentersShader;
    ComputeShader _renderingGridShader;

    RenderType _lastRenderType = RenderType.Color;

    RenderTexture _renderTexture1;
    RenderTexture _renderTexture2;
    RenderTexture _renderTexture3;
    RenderTexture _renderTextureWater;

    // DesignLoader _designLoader;
    LiquidShaderRenderer _parent;
    CopyBuffer _copyBuffer;
    TwoIntoOne _twoIntoOne;
    RenderSelected _renderSelected;

    public bool IsSplitScreen {
        get {
            return renderVelBuffers;
        }
    }

    void OnEnable() {
        _projectCellByCell = GetComponent<ProjectCellByCell>();
        _advectCellByCell = GetComponent<AdvectCellByCell>();
        _renderArrows = GetComponent<RenderArrows>();
        _renderDivergenceSpiral = GetComponent<RenderDivergenceSpiral>();
        _renderSelected = GetComponent<RenderSelected>();

        _renderingCentersShader = Resources.Load<ComputeShader>("LiquidShader/RenderingCenters");
        _renderingVelocityCentersShader = Resources.Load<ComputeShader>("LiquidShader/RenderingVelocityCenters");
        _renderingGridShader = Resources.Load<ComputeShader>("LiquidShader/RenderingGrid");

        // _designLoader = GetComponent<DesignLoader>();
        _parent = GetComponent<LiquidShaderRenderer>();

        _copyBuffer = new CopyBuffer();
        _twoIntoOne = new TwoIntoOne();

        _renderTexture1 = new RenderTexture(renderResX, renderResY, 24) {
            enableRandomWrite = true
        };
        _renderTexture1.Create();

        _renderTexture2 = new RenderTexture(renderResX, renderResY, 24) {
            enableRandomWrite = true
        };
        _renderTexture2.Create();

        _renderTexture3 = new RenderTexture(renderResX, renderResY, 24) {
            enableRandomWrite = true
        };
        _renderTexture3.Create();
    }

    void Start() {
        SyncRenderTypeToButtons();
    }

    public int[] RenderRes{
        get {
            return new []{renderResX, renderResY};
        }
    }

    void SyncRenderTypeToButtons() {
        setup.buttonGroupExclusiveSelected.ShowButtonSelected(renderType.ToString());
        _lastRenderType = renderType;
    }

    void Update() {
        if(Input.GetKeyDown(KeyCode.S)) {
            renderType = RenderType.Color;
            Debug.Log("Smoke");
        }
        if(Input.GetKeyDown(KeyCode.V)) {
            renderType = RenderType.Velocity;
            Debug.Log("Velocity");
        }
        if(Input.GetKeyDown(KeyCode.C)) {
            renderType = RenderType.ColorSource;
            Debug.Log("ColorSources");
        }
        // if(Input.GetKeyDown(KeyCode.M)) {
        //     renderType = RenderType.VelSrc;
        //     Debug.Log("VelocitySources");
        // }

        if (_lastRenderType != renderType) {
            SyncRenderTypeToButtons();
        }
    }

    public void ChooseColorSrc() {
        renderType = RenderType.ColorSource;
    }
    public void ChooseDesign() {
        renderType = RenderType.Design;
    }
    public void ChooseCellActive() {
        renderType = RenderType.CellActive;
    }
    public void ChooseColor() {
        renderType = RenderType.Color;
    }
    public void ChooseVelocity() {
        renderType = RenderType.Velocity;
    }
    // public void ChooseVelSrc() {
    //     renderType = RenderType.VelSrc;
    // }
    public void ChooseMovingTexture() {
        renderType = RenderType.MovingTexture;
    }
    public void ChooseVelocityHSV() {
        renderType = RenderType.VelocityHSV;
    }
    public void ChooseVelU() {
        renderType = RenderType.VelU;
    }
    public void ChooseVelV() {
        renderType = RenderType.VelV;
    }
    public void ChooseDivergence() {
        renderType = RenderType.Divergence;
    }
    public void ChooseMaxChunkDivergence() {
        renderType = RenderType.ChunkMaxDivergence;
    }
    public void ChooseMaxChunkDivL2() {
        renderType = RenderType.ChunkMaxDivL2;
    }
    public void ChooseCheckDesign() {
        renderType = RenderType.DesignCheck;
    }
    public void ChooseDebug() {
        renderType = RenderType.Debug;
    }

    public void RenderVelocity(RenderTexture renderTexture, SimulationState simulationState, bool hsv, bool onlyRenderFixed) {
        var shader = _renderingVelocityCentersShader;
        var kernelName = hsv ? "RenderVelocityHSV" : "RenderVelocity";
        var kernel = shader.FindKernel(kernelName);
        shader.SetBuffer(kernel, "_isFluid", simulationState.sBuf.GetComputeBuffer());
        shader.SetBuffer(kernel, "_horizVel", simulationState.uBuf.GetComputeBuffer());
        shader.SetBuffer(kernel, "_vertVel", simulationState.vBuf.GetComputeBuffer());
        shader.SetTexture(kernel, "_renderTexture", renderTexture);
        shader.SetBool("_onlyRenderFixed", onlyRenderFixed);
        shader.SetInts("_simRes", simulationState.SimResInts);
        shader.SetInts("_renderRes", RenderRes);
        shader.Dispatch(kernel, renderResX / 8, renderResY / 8, 1);
    }

    public void RenderMovingTexture(RenderTexture renderTexture, SimulationState simulationState) {
        var shader = _renderingVelocityCentersShader;
        var kernel = shader.FindKernel("RenderMovingTexture");
        shader.SetBuffer(kernel, "_horizVel", simulationState.uBuf.GetComputeBuffer());
        shader.SetBuffer(kernel, "_vertVel", simulationState.vBuf.GetComputeBuffer());
        shader.SetTexture(kernel, "_renderTexture", renderTexture);
        shader.SetInts("_simRes", simulationState.SimResInts);
        shader.SetInts("_renderRes", RenderRes);
        shader.SetBuffer(kernel, "_texPos", simulationState.borderTexPos.GetComputeBuffer());
        shader.SetTexture(kernel, "_waterTexture", setup.waterTexture);
        shader.Dispatch(kernel, renderResX / 8, renderResY / 8, 1);
    }

    public void RenderGridView(RenderTexture renderTexture, SimulationState simulationState) {
        var shader = _renderingGridShader;
        var kernel = shader.FindKernel("RenderGridView");
        shader.SetBuffer(kernel, "_isFluid", simulationState.sBuf.GetComputeBuffer());
        shader.SetBuffer(kernel, "_horizVel", simulationState.uBuf.GetComputeBuffer());
        shader.SetBuffer(kernel, "_vertVel", simulationState.vBuf.GetComputeBuffer());
        shader.SetBuffer(kernel, "_floatsToRender", simulationState.divergenceBuf.GetComputeBuffer());
        shader.SetBuffer(kernel, "_texPos", simulationState.borderTexPos.GetComputeBuffer());
        shader.SetTexture(kernel, "_renderTexture", renderTexture);
        shader.SetFloat("_gridBoundarySpeed", gridBoundarySpeed);
        shader.SetFloat("_gridBoundaryFrequency", 1 / gridBoundaryWavelength);
        shader.SetFloat("_gridBoundaryThickness", gridBoundaryThickness);
        shader.SetInts("_simRes", simulationState.SimResInts);
        shader.SetInts("_renderRes", RenderRes);
        shader.SetFloat("_time", Time.time);
        shader.SetBool("_renderStaticTexture", renderStaticTextureGrid);
        shader.SetBool("_renderRandomTexture", renderRandomTextureGrid);
        shader.SetBool("_renderWaves", renderWavesGrid);
        shader.SetBool("_renderSolidColor", renderGridSolidColor);
        shader.SetTexture(kernel, "_waterTexture", setup.waterTexture);
        shader.Dispatch(kernel, (renderResX + 8 - 1) / 8, (renderResY + 8 - 1) / 8, 1);
    }
    void UpdateTexPos(SimulationState simulationState) {
        var shader = _renderingGridShader;
        var kernel = shader.FindKernel("UpdateTexPos");
        shader.SetBuffer(kernel, "_texPos", simulationState.borderTexPos.GetComputeBuffer());
        shader.SetInts("_simRes", simulationState.SimResInts);
        shader.SetBuffer(kernel, "_horizVel", simulationState.uBuf.GetComputeBuffer());
        shader.SetBuffer(kernel, "_vertVel", simulationState.vBuf.GetComputeBuffer());
        shader.SetBuffer(kernel, "_debug", simulationState.debugBuf.GetComputeBuffer());
        shader.SetFloat(
            "_deltaTime", Time.deltaTime);
        shader.Dispatch(
            kernel,
            (simulationState.simResX + 8 - 1) / 8,
            (simulationState.simResY + 8 - 1) / 8,
            1);
    }


    public void RenderFloat3S(RenderTexture renderTexture, Buf2<Vector3> buf) {
        var kernel = _renderingCentersShader.FindKernel("RenderFloat3s");
        _renderingCentersShader.SetBuffer(kernel, "_float3sToRender", buf.GetComputeBuffer());
        _renderingCentersShader.SetTexture(kernel, "_renderTexture", renderTexture);
        _renderingCentersShader.SetInts("_simRes", buf.ResInts);
        _renderingCentersShader.SetInts("_renderRes", RenderRes);
        _renderingCentersShader.Dispatch(kernel, renderResX / 8, renderResY / 8, 1);
    }

    public void RenderFloat4S(RenderTexture renderTexture, Buf2<Vector4> buf) {
        var kernel = _renderingCentersShader.FindKernel("RenderFloat4s");
        _renderingCentersShader.SetBuffer(kernel, "_float4sToRender", buf.GetComputeBuffer());
        _renderingCentersShader.SetTexture(kernel, "_renderTexture", renderTexture);
        _renderingCentersShader.SetInts("_simRes", buf.ResInts);
        _renderingCentersShader.SetInts("_renderRes", RenderRes);
        _renderingCentersShader.Dispatch(kernel, renderResX / 8, renderResY / 8, 1);
    }

    public void RenderFloats(RenderTexture renderTexture, IBuf2<float> buf) {
        var kernel = _renderingCentersShader.FindKernel("RenderFloats");
        _renderingCentersShader.SetBuffer(kernel, "_horizVel", buf.GetComputeBuffer());
        _renderingCentersShader.SetInt("_uOffset", buf.Offset);
        _renderingCentersShader.SetTexture(kernel, "_renderTexture", renderTexture);
        _renderingCentersShader.SetInts("_simRes", buf.ResInts);
        _renderingCentersShader.SetInts("_renderRes", RenderRes);
        _renderingCentersShader.Dispatch(kernel, (renderResX + 8 - 1) / 8, (renderResY + 8 - 1) / 8, 1);
    }

    public void RenderVelU(RenderTexture renderTexture, Buf2<float> buf) {
        var shader = _renderingVelocityCentersShader;
        var kernel = shader.FindKernel("RenderVelU");
        shader.SetBuffer(kernel, "_horizVel", buf.GetComputeBuffer());
        shader.SetTexture(kernel, "_renderTexture", renderTexture);
        shader.SetInts("_simRes", buf.ResInts);
        shader.SetInts("_renderRes", RenderRes);
        shader.Dispatch(kernel, (renderResX + 8 - 1) / 8, (renderResY + 8 - 1) / 8, 1);
    }

    public void RenderVelV(RenderTexture renderTexture, Buf2<float> buf) {
        var shader = _renderingVelocityCentersShader;
        var kernel = shader.FindKernel("RenderVelV");
        shader.SetBuffer(kernel, "_vertVel", buf.GetComputeBuffer());
        shader.SetTexture(kernel, "_renderTexture", renderTexture);
        shader.SetInts("_simRes", buf.ResInts);
        shader.SetInts("_renderRes", RenderRes);
        shader.Dispatch(kernel, (renderResX + 8 - 1) / 8, (renderResY + 8 - 1) / 8, 1);
    }

    public void RenderInts(RenderTexture renderTexture, Buf2<int> buf) {
        var kernel = _renderingCentersShader.FindKernel("RenderInts");
        _renderingCentersShader.SetBuffer(kernel, "_isFluid", buf.GetComputeBuffer());
        _renderingCentersShader.SetTexture(kernel, "_renderTexture", renderTexture);
        _renderingCentersShader.SetInts("_simRes", buf.ResInts);
        _renderingCentersShader.SetInts("_renderRes", RenderRes);
        _renderingCentersShader.Dispatch(kernel, renderResX / 8, renderResY / 8, 1);
    }

    public void RenderBools(RenderTexture renderTexture, Buf2<int> buf) {
        var kernel = _renderingCentersShader.FindKernel("RenderBools");
        _renderingCentersShader.SetBuffer(kernel, "_isFluid", buf.GetComputeBuffer());
        _renderingCentersShader.SetTexture(kernel, "_renderTexture", renderTexture);
        _renderingCentersShader.SetInts("_simRes", buf.ResInts);
        _renderingCentersShader.SetInts("_renderRes", RenderRes);
        _renderingCentersShader.Dispatch(kernel, renderResX / 8, renderResY / 8, 1);
    }

    void CheckDesign(RenderTexture renderTexture, SimulationState simulationState) {
        var kernel = _renderingCentersShader.FindKernel("CheckDesign");
        _renderingCentersShader.SetBuffer(kernel, "_horizVel", simulationState.uBuf.GetComputeBuffer());
        _renderingCentersShader.SetBuffer(kernel, "_vertVel", simulationState.vBuf.GetComputeBuffer());
        _renderingCentersShader.SetBuffer(kernel, "_isFluid", simulationState.sBuf.GetComputeBuffer());
        _renderingCentersShader.SetBuffer(kernel, "_velocitySources", simulationState.velocitySourcesBuf.GetComputeBuffer());
        _renderingCentersShader.SetTexture(kernel, "_renderTexture", renderTexture);
        _renderingCentersShader.SetInts("_simRes", simulationState.SimResInts);
        _renderingCentersShader.SetInts("_renderRes", RenderRes);
        _renderingCentersShader.Dispatch(kernel, renderResX / 8, renderResY / 8, 1);
    }

    void RenderCurrentCell(RenderTexture renderTexture, SimulationState simulationState, Vector3 color, int[] lastUpdatePos) {
        var shader = _renderingGridShader;
        var kernel = shader.FindKernel("RenderCurrentCell");
        shader.SetInts("_simRes", simulationState.SimResInts);
        shader.SetInt("_updateX", lastUpdatePos[0]);
        shader.SetInt("_updateY", lastUpdatePos[1]);
        shader.SetInts("_renderRes", RenderRes);
        shader.SetVector("_currentCellColor", color);
        shader.SetTexture(kernel, "_renderTexture", renderTexture);
        shader.Dispatch(kernel, renderResX / 8, renderResY / 8, 1);
    }

    void RenderSimulationState(RenderTexture renderTexture, SimulationState simulationState) {
        // setup.colorWheel.show = false;
        if (simulationState == null) {
            return;
        }
        switch(renderType) {
            case RenderType.Velocity:
                // setup.colorWheel.show = true;
                // setup.colorWheel.colorWheelType = ColorWheelType.Axis2;
                RenderVelocity(renderTexture, simulationState, hsv: false, onlyRenderFixed: false);
                break;
            case RenderType.VelocityHSV:
                // setup.colorWheel.show = true;
                // setup.colorWheel.colorWheelType = ColorWheelType.HSV;
                RenderVelocity(renderTexture, simulationState, hsv: true, onlyRenderFixed: false);
                break;
            case RenderType.MovingTexture:
                RenderMovingTexture(renderTexture, simulationState);
                break;
            case RenderType.CellActive:
                RenderBools(renderTexture, simulationState.sBuf);
                break;
            case RenderType.ColorSource:
                RenderFloat4S(renderTexture, simulationState.colorSourcesBuf);
                break;
            case RenderType.Design:
                RenderFloat3S(renderTexture, simulationState.velocitySourcesBuf);
                break;
            // case RenderType.VelocitySources:
            //     RenderFloat3s(simulationState, simulationState.);
            //     break;
            case RenderType.Color:
                RenderFloat4S(renderTexture, simulationState.mBuf);
                if (showVelocityOverColor) {
                    // setup.colorWheel.show = true;
                    // setup.colorWheel.colorWheelType = ColorWheelType.HSV;
                    RenderVelocity(renderTexture, simulationState, hsv: true, onlyRenderFixed: true);
                }
                break;
            case RenderType.VelU:
                RenderVelU(renderTexture, simulationState.uBuf);
                break;
            case RenderType.VelV:
                RenderVelV(renderTexture, simulationState.vBuf);
                break;
            case RenderType.Divergence:
                RenderFloats(renderTexture, simulationState.divergenceBuf);
                break;
            case RenderType.ChunkMaxDivergence:
                RenderFloats(renderTexture, simulationState.chunkDivergenceBuffer);
                break;
            case RenderType.ChunkMaxDivL2:
                RenderFloats(renderTexture, simulationState.chunkDivergenceL2Buf);
                break;
            case RenderType.DesignCheck:
                CheckDesign(renderTexture, simulationState);
                break;
            case RenderType.Debug:
                RenderFloat3S(renderTexture, simulationState.debugBuf);
                break;
        }

        if(showGrid) {
            RenderGridView(renderTexture, simulationState);
        }

        if (showGrid || renderType == RenderType.MovingTexture) {
            UpdateTexPos(simulationState);
        }

        var speedDeltaTime = _parent.speed * _parent.simDeltaTime;
        _renderDivergenceSpiral.Render(renderTexture, simulationState, _parent.speed, _parent.simDeltaTime, RenderRes);
        _renderArrows.Render(renderTexture, simulationState, speedDeltaTime: speedDeltaTime, RenderRes);

        _renderSelected.Render(renderTexture, simulationState, RenderRes);

        if(_parent.projectSolver == ProjectionSolver.CellByCell) {
            RenderCurrentCell(renderTexture, simulationState, new Vector3(0, 0.5f, 1), _projectCellByCell.cellByCell.LastUpdatePos);
        }

        if(_parent.advecter == Advecter.CellByCell) {
            RenderCurrentCell(renderTexture, simulationState, new Vector3(0, 0.8f, 1), _advectCellByCell.cellByCell.LastUpdatePos);
        }
    }

    void OnRenderImage(RenderTexture src, RenderTexture  dest) {
        var simulationState = _parent.simulationState;

        if(!_parent.initialized) {
            return;
        }

        RenderSimulationState(_renderTexture1, simulationState);

        if(renderVelBuffers) {
            // back up current buffers (we arent using u3, v3 in this mode)
            _copyBuffer.CopyFloats(simulationState.simResX, simulationState.simResY, simulationState.uBuf, simulationState.u3Buf);
            _copyBuffer.CopyFloats(simulationState.simResX, simulationState.simResY, simulationState.vBuf, simulationState.v3Buf);

            // copy in new buffers
            _copyBuffer.CopyFloats(simulationState.simResX, simulationState.simResY, simulationState.u2Buf, simulationState.uBuf);
            _copyBuffer.CopyFloats(simulationState.simResX, simulationState.simResY, simulationState.v2Buf, simulationState.vBuf);

            // render the new buffers, from u and v
            RenderSimulationState(_renderTexture2, simulationState);

            // copy back current buffers
            _copyBuffer.CopyFloats(simulationState.simResX, simulationState.simResY, simulationState.u3Buf, simulationState.uBuf);
            _copyBuffer.CopyFloats(simulationState.simResX, simulationState.simResY, simulationState.v3Buf, simulationState.vBuf);

            // RenderFloats(renderTexture2, simulationState.u3Buf);

            // squish the two render textures into texture 1
            _twoIntoOne.RunTwoToOne(_renderTexture1, _renderTexture2, _renderTexture3);
            Graphics.Blit(_renderTexture3, dest);
        } else {
            Graphics.Blit(_renderTexture1, dest);
        }
    }
}

} // namespace LiquidShader
