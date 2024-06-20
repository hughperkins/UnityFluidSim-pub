using System;
using LiquidShader.Types;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utils;
// ReSharper disable CompareOfFloatsByEqualityOperator

namespace LiquidShader {

public enum DrawType {
    Velocity,
    Color,
    S1,
    RemoveDivergence
    // S0
}

public enum DrawDirection {
    Left,
    Right,
    Up,
    Down,
    Drag,
    Freeze
}

public enum DrawColor {
    Green,
    Red,
    Blue,
    Erase
}

[RequireComponent(typeof(Rendering))]
public class Draw : MonoBehaviour {
    [Header("Draw")]
    [SerializeField] [Range(0.5f, 50f)] float drawRadius = 5;
    [SerializeField] bool colorPersist = false;
    [SerializeField] bool velocityPersist = false;

    [SerializeField] public DrawType drawType = DrawType.Velocity;
    [SerializeField] DrawDirection drawDirection = DrawDirection.Right;
    // [SerializeField] Color drawColor = new Color(0, 1, 0, 1);
    [SerializeField] DrawColor drawColor = DrawColor.Green;

    public void SetColorPersist(bool v) {
        colorPersist = v;
    }

    DrawType _oldDrawType;
    DrawDirection _oldDrawDirection;
    DrawColor _oldDrawColor;
    bool _oldColorPersist;
    bool _oldVelocityPersist;
    float _oldDrawRadius;
    // [SerializeField] bool updateDraw = false;

    [SerializeField] Setup setup;

    // [Header("Setup")]
    [Serializable] struct Setup {
        public Toggle toggleVelocityPersist;
        public Toggle toggleColorPersist;
        public Slider brushSizeSlider;
        public TMP_Text bushSizeTex;
        public ButtonGroupExclusiveSelected buttonGroupExclusiveSelected;
    }

    float _lastRelX = 0;
    float _lastRelY = 0;

    Rendering _rendering;
    ComputeShader _shader;

    void Awake() {
        this._shader = Resources.Load<ComputeShader>("LiquidShader/Draw");
        _rendering = GetComponent<Rendering>();
    }

    void Start() {
        SyncButtonsFromSerializeFields();
    }

    void SyncButtonsFromSerializeFields() {
        if (drawType == DrawType.Color) {
            setup.buttonGroupExclusiveSelected.ShowButtonSelected(drawColor.ToString());
        } else if (drawType == DrawType.Velocity) {
            setup.buttonGroupExclusiveSelected.ShowButtonSelected(drawDirection.ToString());
        } else {
            setup.buttonGroupExclusiveSelected.ShowButtonSelected(drawType.ToString());
        }
        _oldDrawColor = drawColor;
        _oldDrawType = drawType;
        _oldDrawDirection = drawDirection;
    }

    void Update() {
        if (_oldDrawColor != drawColor || _oldDrawType != drawType || _oldDrawDirection != drawDirection) {
            SyncButtonsFromSerializeFields();
        }
        if (_oldColorPersist != colorPersist) {
            setup.toggleColorPersist.isOn = colorPersist;
            _oldColorPersist = colorPersist;
        }
        if (_oldVelocityPersist != velocityPersist) {
            setup.toggleVelocityPersist.isOn = velocityPersist;
            _oldVelocityPersist = velocityPersist;
        }
        colorPersist = setup.toggleColorPersist.isOn;
        velocityPersist = setup.toggleVelocityPersist.isOn;

        if (_oldDrawRadius != drawRadius) {
            setup.brushSizeSlider.value = drawRadius;
            _oldDrawRadius = drawRadius;
            setup.bushSizeTex.text = drawRadius.ToString("F1");
        }
        drawRadius = setup.brushSizeSlider.value;
    }

    public void RunDrawing(SimulationState simulationState) {
        if(Input.GetKey(KeyCode.LeftCommand)) return;
        if (ClickFilter.HitUI()) return;

        if(Input.GetMouseButtonDown(0)) {
            _lastRelX = Input.mousePosition.x / Screen.width;
            if(_rendering.IsSplitScreen) {
                _lastRelX *= 2;
            }
            _lastRelY = Input.mousePosition.y / Screen.height;
        }
        if(Input.GetMouseButton(0)) {
            var relX = Input.mousePosition.x / Screen.width;
            var relY = Input.mousePosition.y / Screen.height;
            if(_rendering.IsSplitScreen) {
                relX *= 2;
            }
            var simX = (int)(relX * simulationState.simResX);
            var simY = (int)(relY * simulationState.simResY);
            if(simX >= 0 && simY >= 0 && simX < simulationState.simResX && simY < simulationState.simResY) {
                var tgtPos = new Vector2Int(simX, simY);
                if(drawType == DrawType.Velocity) {
                    var newVel = new Vector2(0, 0);
                    switch(drawDirection) {
                        case DrawDirection.Right:
                            newVel = new Vector2(1, 0);
                            break;
                        case DrawDirection.Left:
                            newVel = new Vector2(-1, 0);
                            break;
                        case DrawDirection.Up:
                            newVel = new Vector2(0, 1);
                            break;
                        case DrawDirection.Down:
                            newVel = new Vector2(0, -1);
                            break;
                        case DrawDirection.Freeze:
                            newVel = new Vector2(0, 0);
                            break;
                        case DrawDirection.Drag:
                            if(relX != _lastRelX || relY != _lastRelY) {
                                var direction = new Vector2(relX - _lastRelX, relY - _lastRelY);
                                if(direction.magnitude > 0.01f) {
                                    direction.Normalize();
                                    newVel = direction;
                                    _lastRelX = relX;
                                    _lastRelY = relY;
                                } else {
                                    return;
                                }
                            } else {
                                return;
                            }
                            break;
                    }
                    DrawVelU(tgt: simulationState.uBuf, drawRadius: drawRadius, drawValue: newVel.x, tgtPos,
                        simulationState: simulationState, colorDrawLambda: 1, drawPersistVel: velocityPersist);
                    DrawVelV(tgt: simulationState.vBuf, drawRadius: drawRadius, drawValue: newVel.y, tgtPos,
                        simulationState: simulationState, colorDrawLambda: 1, drawPersistVel: velocityPersist);
                    if(velocityPersist) {
                        var permTgtPos = tgtPos - new Vector2(0.0f, 0.0f);
                        DrawFloat4S(
                            tgt: simulationState.colorSourcesBuf, drawRadius: drawRadius, drawColor: Color.black, tgtPos,
                            simulationState: simulationState, colorDrawLambda: 1);
                        DrawFloat4S(
                            tgt: simulationState.mBuf, drawRadius: drawRadius, drawColor: Color.black, tgtPos,
                            simulationState: simulationState, colorDrawLambda: 1);
                        DrawInts(tgt: simulationState.sBuf, drawRadius: drawRadius - 1, drawValue: 0, permTgtPos,
                            simulationState: simulationState, colorDrawLambda: 1);
                    }
                } else if(drawType == DrawType.Color) {
                    Color color = Color.white;
                    switch (drawColor) {
                        case DrawColor.Blue:
                            color = Color.blue;
                            break;
                        case DrawColor.Green:
                            color = Color.green;
                            break;
                        case DrawColor.Red:
                            color = Color.red;
                            break;
                        case DrawColor.Erase:
                            color = new Color(0, 0, 0,0 );
                            break;
                    }
                    DrawFloat4S(
                        tgt: simulationState.mBuf, drawRadius: drawRadius, drawColor: color, tgtPos,
                        simulationState: simulationState, colorDrawLambda: 1);
                    if (colorPersist) {
                        DrawFloat4S(
                            tgt: simulationState.colorSourcesBuf, drawRadius: drawRadius, drawColor: color, tgtPos,
                            simulationState: simulationState, colorDrawLambda: 1);
                    }
                } else if(drawType == DrawType.S1) {
                    DrawInts(
                        tgt: simulationState.sBuf, drawRadius: drawRadius, 1, tgtPos,
                        simulationState: simulationState, colorDrawLambda: 1);
                    DrawFloat4S(
                        tgt: simulationState.colorSourcesBuf, drawRadius: drawRadius, drawColor: new Color(0, 0, 0, 0), tgtPos,
                        simulationState: simulationState, colorDrawLambda: 1);
                }
            }
        }
    }

    public void SetDrawRight() {
        drawDirection = DrawDirection.Right;
        drawType = DrawType.Velocity;
    }
    public void SetDrawLeft() {
        drawDirection = DrawDirection.Left;
        drawType = DrawType.Velocity;
    }
    public void SetDrawUp() {
        drawDirection = DrawDirection.Up;
        drawType = DrawType.Velocity;
    }
    public void SetDrawDown() {
        drawDirection = DrawDirection.Down;
        drawType = DrawType.Velocity;
    }
    public void SetDrawDrag() {
        drawDirection = DrawDirection.Drag;
        drawType = DrawType.Velocity;
    }

    public void SetDrawRed() {
        drawColor = DrawColor.Red;
        drawType = DrawType.Color;
    }
    public void SetDrawGreen() {
        drawColor = DrawColor.Green;
        drawType = DrawType.Color;
    }
    public void SetDrawBlue() {
        drawColor = DrawColor.Blue;
        drawType = DrawType.Color;
    }
    public void SetDrawErase() {
        drawColor = DrawColor.Erase;
        drawType = DrawType.Color;
    }
    public void SetDrawS1() {
        drawType = DrawType.S1;
    }
    public void SetDrawS0() {
        drawDirection = DrawDirection.Freeze;
        drawType = DrawType.Velocity;
    }
    public void SetDrawRemoveDivergence() {
        drawType = DrawType.RemoveDivergence;
    }

    public void DrawFloat4S(
        Buf2<Vector4> tgt, float drawRadius, Vector4 drawColor, Vector2 tgtPos, SimulationState simulationState,
        float colorDrawLambda
    ) {
        var kernel = _shader.FindKernel("DrawFloat4");
        _shader.SetBuffer(kernel, "_tgtFloat4", tgt.GetComputeBuffer());
        // shader.SetVector("_tgtPos", (Vector2)tgtPos);
        // shader.SetInts("_tgtPos", new int[]{tgtPos.x, tgtPos.y});
        _shader.SetVector("_tgtPos", tgtPos);
        _shader.SetInt("_simResX", tgt.ResX);
        _shader.SetInt("_simResY", tgt.ResY);
        _shader.SetFloat("_drawRadius", drawRadius);
        _shader.SetVector("_drawColor", drawColor);
        _shader.SetBuffer(kernel, "_isFluid", simulationState.sBuf.GetComputeBuffer());
        _shader.SetFloat("_colorDrawLambda", colorDrawLambda);
        _shader.Dispatch(kernel, (tgt.ResX + 8 - 1) / 8, (tgt.ResY + 8 - 1 ) / 8, 1);
    }

    public void DrawFloats(
        Buf2<float> tgt, float drawRadius, float drawValue, Vector2 tgtPos, SimulationState simulationState,
        float colorDrawLambda
    ) {
        var kernel = _shader.FindKernel("DrawFloat");
        _shader.SetBuffer(kernel, "_tgtFloat", tgt.GetComputeBuffer());
        // shader.SetVector("_tgtPos", (Vector2)tgtPos);
        // shader.SetInts("_tgtPos", new int[]{tgtPos.x, tgtPos.y});
        _shader.SetVector("_tgtPos", tgtPos);
        _shader.SetInt("_simResX", tgt.ResX);
        _shader.SetInt("_simResY", tgt.ResY);
        _shader.SetFloat("_drawRadius", drawRadius);
        _shader.SetFloat("_drawFloatValue", drawValue);
        _shader.SetBuffer(kernel, "_isFluid", simulationState.sBuf.GetComputeBuffer());
        _shader.SetFloat("_colorDrawLambda", colorDrawLambda);
        _shader.Dispatch(kernel, (tgt.ResX + 8 - 1) / 8, (tgt.ResY + 8 - 1 ) / 8, 1);
    }

    public void DrawVelU(
        Buf2<float> tgt, float drawRadius, float drawValue, Vector2 tgtPos, SimulationState simulationState,
        float colorDrawLambda, bool drawPersistVel
    ) {
        var kernel = _shader.FindKernel("DrawVelU");
        _shader.SetBuffer(kernel, "_tgtFloat", tgt.GetComputeBuffer());
        // shader.SetVector("_tgtPos", (Vector2)tgtPos);
        // shader.SetInts("_tgtPos", new int[]{tgtPos.x, tgtPos.y});
        _shader.SetVector("_tgtPos", tgtPos);
        _shader.SetInt("_simResX", tgt.ResX);
        _shader.SetInt("_simResY", tgt.ResY);
        _shader.SetFloat("_drawRadius", drawRadius);
        _shader.SetBool("_drawPersistVel", drawPersistVel);
        _shader.SetFloat("_drawFloatValue", drawValue);
        _shader.SetBuffer(kernel, "_isFluid", simulationState.sBuf.GetComputeBuffer());
        _shader.SetFloat("_colorDrawLambda", colorDrawLambda);
        _shader.Dispatch(kernel, (tgt.ResX + 8 - 1) / 8, (tgt.ResY + 8 - 1 ) / 8, 1);
    }

    public void DrawVelV(
        Buf2<float> tgt, float drawRadius, float drawValue, Vector2 tgtPos, SimulationState simulationState,
        float colorDrawLambda, bool drawPersistVel
    ) {
        var kernel = _shader.FindKernel("DrawVelV");
        _shader.SetBuffer(kernel, "_tgtFloat", tgt.GetComputeBuffer());
        // shader.SetVector("_tgtPos", (Vector2)tgtPos);
        // shader.SetInts("_tgtPos", new int[]{tgtPos.x, tgtPos.y});
        _shader.SetVector("_tgtPos", tgtPos);
        _shader.SetInt("_simResX", tgt.ResX);
        _shader.SetInt("_simResY", tgt.ResY);
        _shader.SetFloat("_drawRadius", drawRadius);
        _shader.SetBool("_drawPersistVel", drawPersistVel);
        _shader.SetFloat("_drawFloatValue", drawValue);
        _shader.SetBuffer(kernel, "_isFluid", simulationState.sBuf.GetComputeBuffer());
        _shader.SetFloat("_colorDrawLambda", colorDrawLambda);
        _shader.Dispatch(kernel, (tgt.ResX + 8 - 1) / 8, (tgt.ResY + 8 - 1 ) / 8, 1);
    }

    public void DrawInts(
        Buf2<int> tgt, float drawRadius, int drawValue, Vector2 tgtPos, SimulationState simulationState,
        float colorDrawLambda
    ) {
        var kernel = _shader.FindKernel("DrawInt");
        _shader.SetBuffer(kernel, "_tgtInt", tgt.GetComputeBuffer());
        // shader.SetVector("_tgtPos", (Vector2)tgtPos);
        // shader.SetInts("_tgtPos", new int[]{tgtPos.x, tgtPos.y});
        _shader.SetVector("_tgtPos", tgtPos);
        _shader.SetInt("_simResX", tgt.ResX);
        _shader.SetInt("_simResY", tgt.ResY);
        _shader.SetFloat("_drawRadius", drawRadius);
        _shader.SetInt("_drawIntValue", drawValue);
        // shader.SetBuffer(kernel, "_isFluid", simulationState.sBuf.GetComputeBuffer());
        _shader.SetFloat("_colorDrawLambda", colorDrawLambda);
        _shader.Dispatch(kernel, (tgt.ResX + 8 - 1) / 8, (tgt.ResY + 8 - 1 ) / 8, 1);
    }
}

} // namespace LiquidShader
