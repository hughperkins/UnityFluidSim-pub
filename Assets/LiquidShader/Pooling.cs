using System;
using UnityEngine;
using Utils;

namespace LiquidShader {

public class Pooling : MonoBehaviour {
    [SerializeField] public bool useDivergenceChunks = false;
    [SerializeField] public float startDivergenceThreshold = 0.3f;
    [SerializeField] public float endDivergenceThreshold = 0.01f;
    [SerializeField] public float noRunProjectDivergenceThreshold = 0.01f;
    public int chunkOverlap = 0;
    [SerializeField] public bool runMaxAbs = false;

    ComputeShader _shader;

    // these have to match what is in the shader
    public const int ChunkSize = 8;
    const int THREAD_BLOCK_SIZE = 8;

    private void Awake() {
        this._shader = Resources.Load<ComputeShader>("LiquidShader/Pooling");
    }

    public static int DivergenceChunkSizeLog2 {
        get{
            var divergenceChunkSizeLog2 = (int)(Mathf.Log(ChunkSize) / Mathf.Log(2));
            if((int)Mathf.Pow(2, divergenceChunkSizeLog2) != ChunkSize) {
                throw new Exception($"Invalid divergence chunk size: should be power of 2");
            }
            return divergenceChunkSizeLog2;
        }
    }

    public void MaxAbsFloats(Buf2<float> src, Buf2<float> dest) {
        if(dest.ResX != (src.ResX + ChunkSize - 1) / ChunkSize) {
            throw new Exception($"width of dest should be {ChunkSize} smaller than  dest {src.ResX} vs {dest.ResX}");
        }
        if(dest.ResY != (src.ResY + ChunkSize - 1) / ChunkSize) {
            throw new Exception($"width of dest should be {ChunkSize} smaller than  dest {src.ResY} vs {dest.ResY}");
        }
        var kernel = _shader.FindKernel("MaxAbsBuffer");
        _shader.SetBuffer(kernel, "_src", src.GetComputeBuffer());
        _shader.SetBuffer(kernel, "_dest", dest.GetComputeBuffer());
        _shader.SetInts("_simRes", new int[]{src.ResX, src.ResY});
        _shader.Dispatch(kernel, (dest.ResX + THREAD_BLOCK_SIZE - 1) / THREAD_BLOCK_SIZE, (dest.ResY + THREAD_BLOCK_SIZE - 1 ) / THREAD_BLOCK_SIZE, 1);
    }

    public void AbsThresholdFloats(Buf2<float> src, Buf2<float> dest, float threshold) {
        if(dest.ResX != (src.ResX + ChunkSize - 1) / ChunkSize) {
            throw new Exception($"width of dest should be {ChunkSize} smaller than  dest {src.ResX} vs {dest.ResX}");
        }
        if(dest.ResY != (src.ResY + ChunkSize - 1) / ChunkSize) {
            throw new Exception($"width of dest should be {ChunkSize} smaller than  dest {src.ResY} vs {dest.ResY}");
        }
        var kernel = _shader.FindKernel("AbsThresholdPooling");
        _shader.SetBuffer(kernel, "_src", src.GetComputeBuffer());
        _shader.SetBuffer(kernel, "_dest", dest.GetComputeBuffer());
        _shader.SetInts("_simRes", new int[]{src.ResX, src.ResY});
        _shader.SetFloat("_divergenceThreshold", threshold);
        _shader.SetInt("_chunkOverlap", chunkOverlap);
        _shader.Dispatch(kernel, (dest.ResX + THREAD_BLOCK_SIZE - 1) / THREAD_BLOCK_SIZE, (dest.ResY + THREAD_BLOCK_SIZE - 1 ) / THREAD_BLOCK_SIZE, 1);
    }
}

} // namespace LiquidShader
