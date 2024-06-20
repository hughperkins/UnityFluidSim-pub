using System;
using System.IO;
using UnityEngine;
using Utils;
using Newtonsoft.Json;

namespace LiquidShader.Types {

public class SimulationState {
    public readonly Buf2<int> sBuf;
    public readonly Buf2<float> uBuf;
    public readonly Buf2<float> vBuf;
    public readonly Buf2<float> divergenceBuf;

    public readonly Buf2<Vector2> texPosBase;
    public SubBuf2<Vector2> borderTexPos;
    public SubBuf2<Vector2> divergenceTexPos;

    public Buf2<int> selected;

    public readonly Buf2<Vector3> debugBuf;

    public readonly Buf2<Vector4> mBuf;
    public readonly Buf2<Vector4> colorSourcesBuf;

    public readonly Buf2<Vector3> velocitySourcesBuf;
    public readonly Buf2<Vector4> m2Buf;
    public readonly Buf2<float> u23BufParent;
    public readonly Buf2<float> v23BufParent;
    public readonly SubBuf2<float> u2Buf;
    public readonly SubBuf2<float> v2Buf;
    public readonly SubBuf2<float> u3Buf;
    public readonly SubBuf2<float> v3Buf;
    // public SubBuf2<float> uCellByCell;
    // public SubBuf2<float> vCellByCell;

    public readonly Buf2<float> chunkDivergenceBuffer;
    public readonly Buf2<float> chunkDivergenceL2Buf;
    public readonly Buf2<float> chunkDivergenceL3Buf;

    public readonly int simResX;
    public readonly int simResY;

    public Vector2Int SimResVec2 {
        get {
            return new Vector2Int(simResX, simResY);
        }
    }

    public int[] SimResInts {
        get {
            return new []{simResX, simResY};
        }
    }

    public void LoadState(string stateFolder) {
        LoadBuf(stateFolder, "s", sBuf);
        LoadBuf(stateFolder, "u", uBuf);
        LoadBuf(stateFolder, "v", vBuf);
        LoadBuf(stateFolder, "selected", selected);
        LoadBuf(stateFolder, "m", mBuf);
        LoadBuf(stateFolder, "colorSources", colorSourcesBuf);
        LoadBuf(stateFolder, "velocitySources", velocitySourcesBuf);
    }

    static void LoadBuf<T>(string stateFolder, string bufName, Buf2<T> buf) {
        var filePath = stateFolder + "/" + bufName + ".dat";

        var data = buf.Data;
        using var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read);
        using var r = new BinaryReader(fs);
        for (var i = 0; i < buf.ResX; i++) {
            for (var j = 0; j < buf.ResY; j++) {
                if (typeof(T) == typeof(int)) {
                    data[i, j] = (T)(object)r.ReadInt32();
                } else if (typeof(T) == typeof(float)) {
                    data[i, j] = (T)(object)r.ReadSingle();
                } else if (typeof(T) == typeof(Vector4)) {
                    var x = r.ReadSingle();
                    var y = r.ReadSingle();
                    var z = r.ReadSingle();
                    var w = r.ReadSingle();
                    var v4 = new Vector4(x, y, z, w);
                    data[i, j] = (T)(object)v4;
                } else if (typeof(T) == typeof(Vector3)) {
                    var x = r.ReadSingle();
                    var y = r.ReadSingle();
                    var z = r.ReadSingle();
                    var v3 = new Vector3(x, y, z);
                    data[i, j] = (T)(object)v3;
                } else {
                    throw new Exception("unhandled type " + typeof(T));
                }
            }
        }

        buf.ToGPU();
    }

    static void DumpBuf<T>(string stateFolder, string bufName, Buf2<T> buf) {
        var filePath = stateFolder + "/" + bufName + ".dat";
        buf.FromGPU();

        var data = buf.Data;
        using var fs = new FileStream(filePath, FileMode.Create);
        using var w = new BinaryWriter(fs);
        for (var i = 0; i < buf.ResX; i++) {
            for (var j = 0; j < buf.ResY; j++) {
                if (typeof(T) == typeof(int)) {
                    w.Write((int)(object)data[i, j]);
                } else if (typeof(T) == typeof(float)) {
                    w.Write((float)(object)data[i, j]);
                } else if (typeof(T) == typeof(Vector4)) {
                    var v3 = (Vector4)(object)data[i, j];
                    w.Write(v3.x);
                    w.Write(v3.y);
                    w.Write(v3.z);
                    w.Write(v3.w);
                } else if (typeof(T) == typeof(Vector3)) {
                    var v3 = (Vector3)(object)data[i, j];
                    w.Write(v3.x);
                    w.Write(v3.y);
                    w.Write(v3.z);
                } else {
                    throw new Exception("unhandled type " + typeof(T));
                }
            }
        }
    }

    public void SaveState(string stateFolder) {
        DumpBuf(stateFolder, "s", sBuf);
        DumpBuf(stateFolder, "u", uBuf);
        DumpBuf(stateFolder, "v", vBuf);
        DumpBuf(stateFolder, "selected", selected);
        DumpBuf(stateFolder, "m", mBuf);
        DumpBuf(stateFolder, "colorSources", colorSourcesBuf);
        DumpBuf(stateFolder, "velocitySources", velocitySourcesBuf);

        // public readonly Buf2<Vector4> mBuf;
        // public readonly Buf2<Vector4> colorSourcesBuf;
        //
        // public readonly Buf2<Vector3> velocitySourcesBuf;
}

    public int frame = 0;

    public SimulationState(int simResX, int simResY) {
        this.simResX = simResX;
        this.simResY = simResY;

        texPosBase = new Buf2<Vector2>(simResX * 2, simResY);
        texPosBase.ToGPU();
        borderTexPos = new SubBuf2<Vector2>(texPosBase, simResX * simResY * 0, simResX, simResY);
        divergenceTexPos = new SubBuf2<Vector2>(texPosBase, simResX * simResY * 1, simResX, simResY);

        selected = new Buf2<int>(simResX, simResY);

        uBuf = new Buf2<float>(simResX, simResY);
        vBuf = new Buf2<float>(simResX, simResY);

        u23BufParent = new Buf2<float>(simResX * 2, simResY);
        v23BufParent = new Buf2<float>(simResX * 2, simResY);

        u2Buf = new SubBuf2<float>(u23BufParent, simResX * simResY * 0, simResX, simResY);
        u3Buf = new SubBuf2<float>(u23BufParent, simResX * simResY * 1, simResX, simResY);

        v2Buf = new SubBuf2<float>(v23BufParent, simResX * simResY * 0, simResX, simResY);
        v3Buf = new SubBuf2<float>(v23BufParent, simResX * simResY * 1, simResX, simResY);

        divergenceBuf = new Buf2<float>(simResX, simResY);
        sBuf = new Buf2<int>(simResX, simResY);
        debugBuf = new Buf2<Vector3>(simResX, simResY);
        velocitySourcesBuf = new Buf2<Vector3>(simResX, simResY);
        chunkDivergenceBuffer = new Buf2<float>(
            (simResX + Pooling.ChunkSize - 1) / Pooling.ChunkSize,
            (simResY + Pooling.ChunkSize - 1) / Pooling.ChunkSize);
        // Debug.Log($"chunkDivergenceBuffer {chunkDivergenceBuffer.ResVec2} {chunkDivergenceBuffer.ResX} {chunkDivergenceBuffer.ResY}");
        chunkDivergenceL2Buf = new Buf2<float>(
            (chunkDivergenceBuffer.ResX + Pooling.ChunkSize - 1) / Pooling.ChunkSize,
            (chunkDivergenceBuffer.ResY + Pooling.ChunkSize - 1) / Pooling.ChunkSize);
        // Debug.Log($"chunkDivergenceL2Buf {chunkDivergenceL2Buf.ResVec2} {chunkDivergenceL2Buf.ResX} {chunkDivergenceL2Buf.ResY}");
        chunkDivergenceL3Buf = new Buf2<float>(
            (chunkDivergenceL2Buf.ResX + Pooling.ChunkSize - 1) / Pooling.ChunkSize,
            (chunkDivergenceL2Buf.ResY + Pooling.ChunkSize - 1) / Pooling.ChunkSize);
        // Debug.Log($"chunkDivergenceL3Buf {chunkDivergenceL3Buf.ResVec2} {chunkDivergenceL3Buf.ResX} {chunkDivergenceL3Buf.ResY}");
        // chunkDivergenceBuffer = new Buf2<float>(SimResX, SimResY);

        selected.ToGPU();

        mBuf = new Buf2<Vector4>(simResX, simResY);
        m2Buf = new Buf2<Vector4>(simResX, simResY);
        colorSourcesBuf = new Buf2<Vector4>(simResX, simResY);
        // Debug.Log("SimulationState constructor ran");
    }

    public void Destroy() {
        // Debug.Log("Simulation state destructor running");
        texPosBase.Release();

        uBuf.Release();
        vBuf.Release();
        u23BufParent.Release();
        v23BufParent.Release();
        sBuf.Release();
        divergenceBuf.Release();
        chunkDivergenceBuffer.Release();
        chunkDivergenceL2Buf.Release();
        chunkDivergenceL3Buf.Release();

        selected.Release();

        colorSourcesBuf.Release();
        velocitySourcesBuf.Release();
        mBuf.Release();
        m2Buf.Release();
        debugBuf.Release();
    }
}

} // namespace LiquidShader
