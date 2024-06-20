using System;
using UnityEngine;
using NUnit.Framework;

public class TestUtils {
    public static void Fill<T>(T [,] tgt, T value) {
        int X = tgt.GetLength(0);
        int Y = tgt.GetLength(1);
        for(int x = 0; x < X; x++) {
            for(int y = 0; y < Y; y++) {
                tgt[x, y] = value;
            }
        }
    }

    public static T[,] Transpose<T>(T[,] orig) {
        /*
        expects inputs [H][W]
        outputs [W][H]
        */
        int H = orig.GetLength(0);
        int W = orig.GetLength(1);
        // int W = orig_H;
        // int H = orig_W;
        T[,] transposed = new T[W, H];
        for(int x = 0; x < W; x++) {
            for(int y = 0; y < H; y++) {
                transposed[x, y] = orig[y, x];
            }
        }
        return transposed;
    }

    public static T[,] FlipUD<T>(T[,] orig) {
        /*
        expects axes [W][H]
        */
        int W = orig.GetLength(0);
        int H = orig.GetLength(1);
        T[,] flipped = new T[W, H];
        for(int x = 0; x < W; x++) {
            for(int y = 0; y < H; y++) {
                flipped[x, H - 1 - y] = orig[x, y];
            }
        }
        return flipped;
    }

    public static T[,] FT<T>(T[,] orig) {
        /*
        expects [H][W], with higher h being towards bottom
        outputs [W][H], with higher h being towards top
        */
        return FlipUD(Transpose(orig));
    }

    public static void AssertEqual(float[,] expected, float[,] tgt, float tolerance = 0.001f) {
        int resX = tgt.GetLength(0);
        int resY = tgt.GetLength(1);
        Assert.AreEqual(expected.GetLength(0), resX);
        Assert.AreEqual(expected.GetLength(1), resY);
        for(int x = 0; x < resX; x++) {
            for(int y = 0; y < resY; y++) {
                try{
                    Assert.AreEqual(expected[x, y], tgt[x, y], tolerance);
                } catch(Exception e) {
                    Debug.Log($"x {x} y {y}");
                    throw e;
                }
            }
        }
    }

    public static void AssertEqual(Vector4 expected, Vector4 tgt, float tolerance = 0.001f) {
        float distance = (expected - tgt).magnitude;
        Assert.LessOrEqual(distance, tolerance);
    }

    public static void AssertEqual(Vector3 expected, Vector3 tgt, float tolerance = 0.001f) {
        float distance = (expected - tgt).magnitude;
        Assert.LessOrEqual(distance, tolerance);
    }

    public static void AssertEqual(Vector3[,] expected, Vector3[,] tgt, float tolerance = 0.001f) {
        int resX = tgt.GetLength(0);
        int resY = tgt.GetLength(1);
        Assert.AreEqual(expected.GetLength(0), resX);
        Assert.AreEqual(expected.GetLength(1), resY);
        for(int x = 0; x < resX; x++) {
            for(int y = 0; y < resY; y++) {
                try{
                    AssertEqual(expected[x, y], tgt[x, y]);
                } catch(Exception e) {
                    Debug.Log($"x {x} y {y}");
                    throw e;
                }
            }
        }
    }

    public static void AssertEqual(Vector4[,] expected, Vector4[,] tgt, float tolerance = 0.001f) {
        int resX = tgt.GetLength(0);
        int resY = tgt.GetLength(1);
        Assert.AreEqual(expected.GetLength(0), resX);
        Assert.AreEqual(expected.GetLength(1), resY);
        for(int x = 0; x < resX; x++) {
            for(int y = 0; y < resY; y++) {
                try{
                    AssertEqual(expected[x, y], tgt[x, y]);
                } catch(Exception e) {
                    Debug.Log($"x {x} y {y}");
                    throw e;
                }
            }
        }
    }

    public static void Print2D<T>(T[,] tgt) {
        int W = tgt.GetLength(0);
        int H = tgt.GetLength(1);
        string output = "";
        for(int y = H - 1; y >=0; y--) {
            string row = "";
            for(int x = 0; x < W; x++) {
                row += tgt[x, y] + ", ";
            }
            output += row + "\n";
        }
        Debug.Log(output);
    }

    public static void Vector3ArrayToComponents(Vector3[,] src, out float[,] x, out float[,] y, out float[,] z) {
        int W = src.GetLength(0);
        int H = src.GetLength(1);
        x = new float[W, H];
        y = new float[W, H];
        z = new float[W, H];
        for(int j = 0; j < H; j++) {
            for(int i = 0; i < W; i++) {
                Vector3 v = src[i, j];
                x[i, j] = v.x;
                y[i, j] = v.y;
                z[i, j] = v.z;
            }
        }
    }

    public static void Vector2ArrayToComponents(Vector2[,] src, out float[,] x, out float[,] y) {
        int W = src.GetLength(0);
        int H = src.GetLength(1);
        x = new float[W, H];
        y = new float[W, H];
        for(int j = 0; j < H; j++) {
            for(int i = 0; i < W; i++) {
                Vector3 v = src[i, j];
                x[i, j] = v.x;
                y[i, j] = v.y;
            }
        }
    }
}
