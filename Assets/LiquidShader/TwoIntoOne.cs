using UnityEngine;

namespace LiquidShader {

public class TwoIntoOne {
    ComputeShader _shader;

    public TwoIntoOne() {
        _shader = Resources.Load<ComputeShader>("TwoIntoOne");
    }

    void SquashHoriz(RenderTexture tgt, RenderTexture src, bool toRight) {
        // squishes src horizontally to fit into half the texture tgt
        //
        // puts on left hand side, unless toRight is true, in which case right hand side
        var resX = tgt.width;
        var resY = tgt.height;
        if(src.width != resX) {
            throw new System.Exception($"target and source resolutions should match {src.width} vs {resX}");
        }
        if(src.height != resY) {
            throw new System.Exception($"target and source resolutions should match {src.height} vs {resY}");
        }
        var kernel = _shader.FindKernel("SquashHoriz");
        _shader.SetInts("_renderRes", new int[]{resX, resY});
        _shader.SetBool("_toRight", toRight);
        _shader.SetTexture(kernel, "_renderTexture", tgt);
        _shader.SetTexture(kernel, "_renderTexture2", src);
        _shader.Dispatch(kernel, resX / 8, resY / 8, 1);
    }

    // void _Merge(RenderTexture tgt, RenderTexture src) {
    //     // squishes src horizontally to fit into half the texture tg
    //     int resX = tgt.width;
    //     int resY = tgt.height;
    //     if(src.width != resX) {
    //         throw new System.Exception($"target and source resolutions should match {src.width} vs {resX}");
    //     }
    //     if(src.height != resY) {
    //         throw new System.Exception($"target and source resolutions should match {src.height} vs {resY}");
    //     }
    //     int kernel = shader.FindKernel("Merge");
    //     shader.SetInts("_renderRes", new int[]{resX, resY});
    //     shader.SetTexture(kernel, "_renderTexture", tgt);
    //     shader.SetTexture(kernel, "_renderTexture2", src);
    //     shader.Dispatch(kernel, resX / 8, resY / 8, 1);
    // }

    public void RunTwoToOne(RenderTexture lhs, RenderTexture rhs, RenderTexture tgt) {
        var resX = tgt.width;
        var resY = tgt.height;
        if(lhs.width != resX) {
            throw new System.Exception($"target and source resolutions should match {lhs.width} vs {resX}");
        }
        if(lhs.height != resY) {
            throw new System.Exception($"target and source resolutions should match {lhs.height} vs {resY}");
        }
        if(rhs.width != resX) {
            throw new System.Exception($"target and source resolutions should match {rhs.width} vs {resX}");
        }
        if(rhs.height != resY) {
            throw new System.Exception($"target and source resolutions should match {rhs.height} vs {resY}");
        }
        SquashHoriz(tgt, lhs, false);
        SquashHoriz(tgt, rhs, true);
    }
}

} // namespace LiquidShader
