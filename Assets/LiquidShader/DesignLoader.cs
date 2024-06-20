using System;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using Utils;

namespace LiquidShader {

public class DesignLoader : MonoBehaviour {
    [SerializeField] Texture2D fixedVelocitiesTex;
    [SerializeField] Texture2D colorSourcesTex;

    Texture2D ResizeTexture(int destResY, Texture2D texture) {
        var destResX = (destResY * 16) / 9;
        if (destResX == 853) destResX = 854;
        var newTexture = new Texture2D(destResX, destResY);
        var newPixels = newTexture.GetPixels(0);
        for (var j = 0; j < destResY; j++) {
            for (var i = 0; i < destResX; i++) {
                var destIdx = j * destResX + i;
                var srcU = (float)i / destResX;
                var srcV = (float)j / destResY;
                var value = texture.GetPixelBilinear(srcU, srcV);
                newPixels[destIdx] = value;
            }
        }
        newTexture.SetPixels(newPixels, 0);
        newTexture.Apply();
        return newTexture;
    }

    public void LoadDesign(
            int resY,
            out int[,] s, out float[,] u, out float[,] v, out Vector4[,] colorSources,
            out Vector3[,] velocitySources) {
        var srcResX = fixedVelocitiesTex.width;
        var srcResY = fixedVelocitiesTex.height;
        var destResY = resY;
        var destResX = (16 * resY) / 9;
        if (destResX == 853) destResX = 854;
        // var destResX = destResX;
        // var destResY = destResY;
        u = new float[destResX, destResY];
        v = new float[destResX, destResY];
        s = new int[destResX, destResY];
        colorSources = new Vector4[destResX, destResY];
        velocitySources = new Vector3[destResX, destResY];
        // var fixedVelData = fixedVelocitiesTex.GetPixels();
        for(var j = 0; j < destResY; j++) {
            var jOffset = j * destResX;
            for(var i = 0; i < destResX; i++) {
                var offset = jOffset + i;
                var srcU = (float)i / destResX;
                var srcV = (float)j / destResY;
                var srcX = (int)(srcU * srcResX);
                var srcY = (int)(srcV * srcResY);
                var color = fixedVelocitiesTex.GetPixel(srcX, srcY);
                // var color = fixedVelData[offset];
                velocitySources[i, j] = new Vector4(color.r, color.g, color.b, color.a);
                if(color == Color.red) {
                    s[i, j] = 0;

                    v[i, j] = 1;
                    v[i, j + 1] = 1;
                } else if(color == Color.blue) {
                    s[i, j] = 0;

                    if(v[i, j] == 1) {
                        throw new Exception($"v is already 1 at {i} {j}");
                    }
                    if(v[i, j + 1] == 1) {
                        throw new Exception($"v is already 1 at {i} {j} + 1");
                    }
                    v[i, j] = -1;
                    v[i, j + 1] = -1;
                } else if( color == Color.black) {
                    s[i, j] = 0;

                //     v[i, j] = 0;
                //     if(j < simResY - 1) v[i, j + 1] = 0;
                } else {
                    // treat as white
                    s[i, j] = 1;
                }
            }
        }
        // var colorSourcesData = colorSourcesTex.GetPixels();
        for(var j = 1; j < destResY - 1; j++) {
            var jOffset = j * destResX;
            for(var i = 1; i < destResX - 1; i++) {
                var offset = jOffset + i;
                if(s[i, j] != 1) {
                    continue;
                }
                var srcU = (float)i / destResX;
                var srcV = (float)j / destResY;
                var srcX = (int)(srcU * srcResX);
                var srcY = (int)(srcV * srcResY);
                var csCol = colorSourcesTex.GetPixel(srcX, srcY);
                // var csCol = colorSourcesData[offset];
                if(csCol.a != 1) {
                    continue;
                }
                // only add on boundaries
                // bool addColor =
                //     s[i - 1, j] == 0 ||
                //     s[i + 1, j] == 0 ||
                //     s[i, j - 1] == 0 ||
                //     s[i, j + 1] == 0;
                // if(!addColor) {
                //     continue;
                // }
                colorSources[i, j] = new Vector4(csCol.r, csCol.g, csCol.b, csCol.a);
            }
        }
    }

}

} // namespace Liquid2Tex
