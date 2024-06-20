using LiquidShader.Types;
using UnityEngine;
using Utils;

namespace LiquidShader {

enum NoiseType {
    Velocity,
    Color
}

enum NoiseOp {
    Add,
    Mul
}

// enum DecayType {
//     Linear,
//     Squared
// }

public class NoiseAdder: MonoBehaviour {
    [SerializeField] bool enableNoise = true;
    [SerializeField] NoiseType noiseType = NoiseType.Color;
    [SerializeField] NoiseOp noiseOp = NoiseOp.Mul;
    [SerializeField] bool sqrtNoise = true;
    [Range(-10f, 0.0f)] [SerializeField] float logNoiseAmount = -3;
    // [SerializeField] DecayType decayType = DecayType.Linear;
    [SerializeField][Range(-10, 0)] float logDecay = -1;

    ComputeShader _shader;

    void Awake() {
        this._shader = (ComputeShader)Resources.Load("LiquidShader/AddNoise");
    }

    public void AddNoise(SimulationState simulationState) {
        if (!enableNoise) return;
        var noiseAmount = Mathf.Exp(logNoiseAmount);
        if (noiseType == NoiseType.Velocity) {
            // noise u
            AddNoiseFloats(
                frame: simulationState.frame,
                noiseAmount: noiseAmount,
                seed: 0,
                simResX: simulationState.simResX,
                simResY: simulationState.simResY,
                s: simulationState.sBuf,
                tgt: simulationState.uBuf
            );
            // noise v
            AddNoiseFloats(
                frame: simulationState.frame,
                noiseAmount: noiseAmount,
                seed: 100,
                simResX: simulationState.simResX,
                simResY: simulationState.simResY,
                s: simulationState.sBuf,
                tgt: simulationState.vBuf
            );
        } else if (noiseType == NoiseType.Color) {
            AddNoiseFloat4s(
                frame: simulationState.frame,
                noiseAmount: noiseAmount,
                seed: 100,
                simResX: simulationState.simResX,
                simResY: simulationState.simResY,
                s: simulationState.sBuf,
                tgt: simulationState.mBuf
            );
        }
    }

    void AddNoiseFloats(int frame, float noiseAmount, int seed, int simResX, int simResY, Buf2<int> s, Buf2<float> tgt) {
        var kernel = _shader.FindKernel("AddNoiseFloats");
        _shader.SetBuffer(kernel, "_isFluid", s.GetComputeBuffer());
        _shader.SetBuffer(kernel, "_tgt", tgt.GetComputeBuffer());
        _shader.SetInt("_simResX", simResX);
        _shader.SetInt("_simResY", simResY);
        _shader.SetInt("_frame", frame);
        _shader.SetInt("_seed", seed);
        _shader.SetFloat("_noiseAmount", noiseAmount);
        _shader.Dispatch(kernel, (simResX + 8 - 1) / 8, (simResY + 8 - 1 ) / 8, 1);
    }
    void AddNoiseFloat4s(int frame, float noiseAmount, int seed, int simResX, int simResY, Buf2<int> s, Buf2<Vector4> tgt) {
        var kernel = _shader.FindKernel("AddNoiseFloat4s");
        _shader.SetBuffer(kernel, "_isFluid", s.GetComputeBuffer());
        _shader.SetBuffer(kernel, "_tgtFloat4s", tgt.GetComputeBuffer());
        _shader.SetInt("_simResX", simResX);
        _shader.SetInt("_simResY", simResY);
        _shader.SetInt("_frame", frame);
        _shader.SetInt("_seed", seed);
        _shader.SetBool("_addNoise", noiseOp == NoiseOp.Add);
        _shader.SetBool("_sqrtNoise", sqrtNoise);
        _shader.SetFloat("_noiseAmount", noiseAmount);
        var decay = Mathf.Exp(logDecay);
        _shader.SetFloat("_decay", decay);
        // _shader.SetBool("_decaySquared", decayType == DecayType.Squared);
        _shader.Dispatch(kernel, (simResX + 8 - 1) / 8, (simResY + 8 - 1 ) / 8, 1);
    }

}

} // namespace LiquidShader
