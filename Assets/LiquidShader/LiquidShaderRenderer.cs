using LiquidShader.Types;
using UnityEngine;
using Utils;
using System.IO;
using Newtonsoft.Json;

namespace LiquidShader {
public enum ProjectionSolver {
    Jacobi,
    JacobiInPlace,
    GaussSeidelGeneral,
    GaussSeidelClean,
    CellByCell
}

public enum Advecter {
    CellByCell,
    Partials,
    Standard
}

[RequireComponent(typeof(Rendering))]
[RequireComponent(typeof(DesignLoader))]
[RequireComponent(typeof(NoiseAdder))]
[RequireComponent(typeof(ColorAdder))]
[RequireComponent(typeof(Draw))]
[RequireComponent(typeof(ProjectCellByCell))]
[RequireComponent(typeof(ProjectJacobi))]
[RequireComponent(typeof(ProjectJacobiInPlace))]
[RequireComponent(typeof(ProjectGaussSeidelGeneral))]
[RequireComponent(typeof(ProjectGaussSeidelClean))]
[RequireComponent(typeof(DivergenceCalculator))]
[RequireComponent(typeof(Advect))]
[RequireComponent(typeof(AdvectCellByCell))]
[RequireComponent(typeof(AdvectPartials))]
public class LiquidShaderRenderer : MonoBehaviour
{
    [Range(0.0001f, 0.05f)][SerializeField] public float simDeltaTime = 0.02f;
    [Range(0.1f, 200.0f)] [SerializeField] public float speed = 50;

    [SerializeField] bool loadDesign = true;
    [SerializeField] int noDesignResY = 80;
    [SerializeField] bool runProject = true;
    [SerializeField] bool runAdvectVelocity = true;

    [SerializeField] bool slowMotion = false;
    [SerializeField][Range(1, 10)] int frameSkip = 0;
    [SerializeField] bool stepFrames = false;
    [SerializeField] bool stepFrame = false;
    [SerializeField] bool reInit = false;

    [SerializeField] public ProjectionSolver projectSolver = ProjectionSolver.GaussSeidelGeneral;
    [SerializeField] public Advecter advecter = Advecter.Standard;

    [HideInInspector] public bool initialized = false;
    [HideInInspector] public SimulationState simulationState;

    DesignLoader _designLoader;
    NoiseAdder _noiseAdder;
    Draw _draw;

    int lastDesignResY = -1;
    bool lastLoadDesign = false;

    ProjectCellByCell _projectCellByCell;
    ProjectJacobi _projectJacobi;
    ProjectJacobiInPlace _projectJacobiInPlace;
    ProjectGaussSeidelGeneral _projectGaussSeidelGeneral;
    ProjectGaussSeidelClean _projectGaussSeidelClean;
    DivergenceCalculator _divergenceCalculator;

    ColorAdder _colorAdder;

    Advect _advect;
    AdvectPartials _advectPartials;
    AdvectCellByCell _advectCellByCell;

    ComputeBuffer _syncSrcBuffer;
    ComputeBuffer _syncDstBuffer;

    void OnEnable() {
        _designLoader = GetComponent<DesignLoader>();

        _projectCellByCell = GetComponent<ProjectCellByCell>();
        _projectJacobi = GetComponent<ProjectJacobi>();
        _projectJacobiInPlace = GetComponent<ProjectJacobiInPlace>();
        _projectGaussSeidelGeneral = GetComponent<ProjectGaussSeidelGeneral>();
        _projectGaussSeidelClean = GetComponent<ProjectGaussSeidelClean>();
        _divergenceCalculator = GetComponent<DivergenceCalculator>();

        _draw = GetComponent<Draw>();
        _noiseAdder = GetComponent<NoiseAdder>();
        _colorAdder = GetComponent<ColorAdder>();

        _advect = GetComponent<Advect>();
        _advectPartials = GetComponent<AdvectPartials>();
        _advectCellByCell = GetComponent<AdvectCellByCell>();

        _syncSrcBuffer = new ComputeBuffer(1, sizeof(int), ComputeBufferType.Counter);
        _syncDstBuffer = new ComputeBuffer(1, sizeof(int), ComputeBufferType.Counter);
    }

    public void Init() {
        Initialize();
    }

    void Start() {
        Initialize();
    }

    public static void Fill<T>(T [,] tgt, T value) {
        var xLen = tgt.GetLength(0);
        var yLen = tgt.GetLength(1);
        for(var x = 0; x < xLen; x++) {
            for(var y = 0; y < yLen; y++) {
                tgt[x, y] = value;
            }
        }
    }

    void NonSimUpdates() {
        if(reInit || simulationState == null || lastDesignResY != noDesignResY || lastLoadDesign != loadDesign) {
            if (noDesignResY < 1) {
                return;
            }
            reInit = false;
            lastDesignResY = noDesignResY;
            lastLoadDesign = loadDesign;
            Initialize();
        }

        _draw.RunDrawing(simulationState: simulationState);
    }

    void Initialize() {
        reInit = false;
        float[,] u;
        float[,] v;
        Vector4[,] colorSources;
        Vector3[,] velocitySources;
        int[,] s;
        Vector4[,] m;

        if(simulationState != null) {
            simulationState.Destroy();
            simulationState = null;
        }

        if(loadDesign) {
            _designLoader.LoadDesign(
                resY: noDesignResY,
                velocitySources: out velocitySources,
                u: out u, v: out v,  colorSources: out colorSources, s: out s);

            simulationState = new SimulationState(u.GetLength(0), u.GetLength(1));
            Debug.Log($"Sim dimensions {simulationState.simResX}x{simulationState.simResY}");
            m = new Vector4[simulationState.simResX, simulationState.simResY];

            simulationState.mBuf.SetData(m);
            simulationState.uBuf.SetData(u);
            simulationState.vBuf.SetData(v);
            simulationState.sBuf.SetData(s);
            simulationState.colorSourcesBuf.SetData(colorSources);
            simulationState.velocitySourcesBuf.SetData(velocitySources);
            noDesignResY = simulationState.simResY;
            lastDesignResY = noDesignResY;
        } else {
            var resX = (noDesignResY * 16) / 9;
            var resY = noDesignResY;
            simulationState = new SimulationState(resX, noDesignResY);
            u = new float[resX, resY];
            s = new int[resX, resY];
            // m = new Vector4[resX, resY];
            Fill(s, 1);
            for(var x = 0; x < resX; x++) {
                s[x, 0] = 0;
                s[x, resY - 1] = 0;
            }
            for(var y = 0; y < resY; y++) {
                s[0, y] = 0;
                s[resX - 1, y] = 0;
            }
            velocitySources = new Vector3[resX, resY];
            colorSources = new Vector4[resX, resY];
            simulationState.mBuf.SetData(colorSources);
            simulationState.uBuf.SetData(u);
            simulationState.vBuf.SetData(u);
            simulationState.sBuf.SetData(s);
            simulationState.colorSourcesBuf.SetData(colorSources);
            simulationState.velocitySourcesBuf.SetData(velocitySources);
        }

        if (!this.isActiveAndEnabled) return;
        if (_advectCellByCell == null) return;
        if (_advectCellByCell.cellByCell == null) return;
        _advectCellByCell.cellByCell.Reset();
        _projectCellByCell.Reset();

        initialized = true;
    }

    void Simulate() {
        _colorAdder.AddColor(simulationState: simulationState, simDeltaTime: simDeltaTime, speed: speed);
        _noiseAdder.AddNoise(simulationState: simulationState);
        if(runAdvectVelocity) {
            switch(advecter) {
                case Advecter.Standard:
                    _advect.AdvectVelocity(simulationState, simDeltaTime: simDeltaTime, speed: speed);
                    break;
                case Advecter.Partials:
                    _advectPartials.AdvectVelocity(simulationState, simDeltaTime: simDeltaTime, speed: speed);
                    break;
                case Advecter.CellByCell:
                    _advectCellByCell.AdvectVelocity(simulationState, simDeltaTime: simDeltaTime, speed: speed);
                    break;
            }
        }
        if(runProject) {
            switch(projectSolver) {
                case ProjectionSolver.Jacobi:
                    _projectJacobi.RunProject(
                        simulationState: simulationState, simDeltaTime: simDeltaTime, speed: speed
                    );
                    break;
                case ProjectionSolver.JacobiInPlace:
                    _projectJacobiInPlace.RunProject(
                        simulationState: simulationState, simDeltaTime: simDeltaTime, speed: speed
                    );
                    break;
                case ProjectionSolver.GaussSeidelGeneral:
                    _projectGaussSeidelGeneral.RunProject(
                        simulationState: simulationState, simDeltaTime: simDeltaTime, speed: speed
                    );
                    break;
                case ProjectionSolver.GaussSeidelClean:
                    _projectGaussSeidelClean.RunProject(simulationState: simulationState);
                    break;
                case ProjectionSolver.CellByCell:
                    _projectCellByCell.RunProject(simulationState: simulationState);
                    break;
            }
        }
        _advect.AdvectFloat4(simulationState, simDeltaTime: simDeltaTime, speed: speed, target: simulationState.mBuf, targetCopy: simulationState.m2Buf);
        simulationState.frame += 1;
    }

    void Update() {
        NonSimUpdates();
        if(!slowMotion || stepFrames) {
            if(!stepFrames || stepFrame) {
                stepFrame = false;
                Simulate();
            }
        }
    }

    void FixedUpdate() {
        if(slowMotion && !stepFrames && simulationState != null) {
            simulationState.frame += 1;
            if (simulationState.frame % (frameSkip + 1) == 0) {
                Simulate();
            }
        }
        _divergenceCalculator.CalcDivergence(simulationState: simulationState);
    }

    void OnApplicationQuit() {
        Debug.Log("LiquidShaderRender OnApplicationQuit");
        Destroy();
    }

    void Destroy() {
        Debug.Log("LiquidShaderRender Destroy");
        if(simulationState != null) {
            simulationState.Destroy();
            simulationState = null;
        }
        if(_syncSrcBuffer != null) {
            _syncSrcBuffer.Release();
            _syncDstBuffer.Release();
            _syncSrcBuffer = null;
            _syncDstBuffer = null;
        }
    }

    class Meta {
        public int simResX;
        public int simResY;
        public int frame;
    }

    public void SaveState(string stateFolder) {
        var meta = new Meta(){
            frame=simulationState.frame, simResX=simulationState.simResX, simResY=simulationState.simResY
        };
        var metaString = JsonConvert.SerializeObject(meta);
        var metaFilepath = stateFolder + "/meta.json";
        using var writer = new StreamWriter(metaFilepath);
        writer.Write((metaString));
        simulationState.SaveState(stateFolder);
    }
    public void LoadState(string stateFolder) {
        var metaFilepath = stateFolder + "/meta.json";
        using var reader = new StreamReader(metaFilepath);
        var metaString = reader.ReadToEnd();
        var meta = JsonConvert.DeserializeObject<Meta>(metaString);
        if (meta.simResX != simulationState.simResX || meta.simResY != simulationState.simResY) {
            simulationState = new SimulationState(meta.simResX, meta.simResY);
        }
        simulationState.frame = meta.frame;
        simulationState.LoadState(stateFolder);
    }
}
}
