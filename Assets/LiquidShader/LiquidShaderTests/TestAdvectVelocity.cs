using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

using LiquidShader;
using LiquidShader.Types;

public class TestAdvectVelocity
{
    [Test]
    public void TestAdvectVelocitySimple() {
        SimulationState simulationState = new SimulationState(5, 5);
        float[,] u = TestUtils.FT(new float[,] {
            {-1, -1, 0.2f, 100, -1, },
            {-1, -1, -1.3f, 20, 200, },
            {-1, -1, -2, -1, -1, },
            {-1, -1, -1, 10, 100, },
            {-1, -1, -1, 1000, 10000, }, 
        });
        float[,] v = TestUtils.FT(new float[,] {
            {0, 0, 0, 0, 0,},
            {0, 0, 0, 0, 0,},
            {0, 0, 0, 0, 0,},
            {0, 0, 0, 0, 0,},
            {0, 0, 0, 0, 0,},
        });
        int[,] s = TestUtils.FT(new int[,] {
            {1, 1, 1, 1, 1},
            {1, 1, 1, 1, 1},
            {1, 1, 1, 1, 1},
            {1, 1, 1, 1, 1},
            {1, 1, 1, 1, 1},
        });
        TestUtils.Print2D(u);
        TestUtils.Print2D(v);
        TestUtils.Print2D(s);

        Advect advect = new Advect();
        simulationState.uBuf.SetData(u);
        simulationState.vBuf.SetData(v);
        simulationState.sBuf.SetData(s);
        advect.AdvectVelocity(simulationState, 1, 1);
        float[,] u2 = new float[5, 5];
        float[,] v2 = new float[5, 5];
        simulationState.uBuf.GetData(u2);
        simulationState.vBuf.GetData(v2);

        TestUtils.Print2D(u2);
        TestUtils.Print2D(v2);

        Assert.AreEqual(-1.3f, u2[1,3], 0.001f);
        Assert.AreEqual(-1f, u2[2,2], 0.001f);
        Assert.AreEqual(0.7f * 20 + 0.3f * 200, u2[2,3], 0.001f);
        // Assert.AreEqual(u2[0,2], 0.8f * 0.2f + 0.2f * 100, 0.001f);
    }

    static IEnumerable<TestCaseData> TestAdvectVelocityExamples() {
        yield return new TestCaseData(new object[]{
            TestUtils.FT(new float[,] {
                {-1, -1, 0.2f, 100, -1, },
                {-1, -1, -1.3f, 20, 200, },
                {-1, -1, -2, -1, -1, },
                {-1, -1, -1, 10, 100, },
                {-1, -1, -1, 1000, 10000, },
            }),
            TestUtils.FT(new float[,] {
                {0, 0, 0, 0, 0,},
                {0, 0, 0, 0, 0,},
                {0, 0, 0, 0, 0,},
                {0, 0, 0, 0, 0,},
                {0, 0, 0, 0, 0,},
            }),
            TestUtils.FT(new int[,] {
                {1, 1, 1, 1, 1, },
                {1, 1, 1, 1, 1, },
                {1, 1, 1, 1, 1, },
                {1, 1, 1, 1, 1, },
                {1, 1, 1, 1, 1, },
            }),
            TestUtils.FT(new float[,] {
                {-1, -1, 0.2f, 100, -1},
                {-1, -1.3f, 74, 0, 200},
                {-1, -2, -1, -1, -1},
                {-1, -1, 10, 0, 100},
                {-1, -1, -1, 1000, 10000},
            }),
            TestUtils.FT(new float[,] {
                {0, 0, 0, 0, 0,},
                {0, 0, 0, 0, 0,},
                {0, 0, 0, 0, 0,},
                {0, 0, 0, 0, 0,},
                {0, 0, 0, 0, 0,},
            }),
        });

        /*
            x_u = 0  x_s=0  x_u=1  x_s=1 x_u=2 x_sx=2
                            lck    0     lck
                    u with same x, and one greater x locked
                    lower u not locked

            conversely, to check if u[x] locked, need to check the same x
            and x - 1, ie
            
            s[x] locks u[x] and u[x + 1]
            
            u[x] is locked by s[x] and s[x - 1]
        */
        yield return new TestCaseData(new object[]{
            TestUtils.FT(new float[,] {
                {-1, -1, 0.2f, 100, -1, },
                {-1, -1, -1.3f, 20, 200, },
                {-1, -1, -2, -1, -1, },
                {-1, -1, -1, 10, 100, },
                {-1, -1, -1, 1000, 10000, },
            }),
            TestUtils.FT(new float[,] {
                {0, 0, 0, 0, 0,},
                {0, 0, 0, 0, 0,},
                {0, 0, 0, 0, 0,},
                {0, 0, 0, 0, 0,},
                {0, 0, 0, 0, 0,},
            }),
            TestUtils.FT(new int[,] {
                {1, 1, 1, 1, 1, },
                {1, 1, 0, 1, 1, },
                {1, 1, 0, 0, 1, },
                {1, 1, 0, 1, 1, },
                {1, 1, 1, 1, 1, },
            }),
            TestUtils.FT(new float[,] {
                {-1, -1,     0.2f,  100, -1},
                {-1, -1.3f, -1.3f,    20, 200},
                {-1, -2f,      -2,   -1, -1},
                {-1, -1,       -1,    10, 100},
                {-1, -1,       -1, 1000, 10000},
            }),
            TestUtils.FT(new float[,] {
                {0, 0, 0, 0, 0,},
                {0, 0, 0, 0, 0,},
                {0, 0, 0, 0, 0,},
                {0, 0, 0, 0, 0,},
                {0, 0, 0, 0, 0,},
            }),
        });
    }

    [Test]
    [TestCaseSource("TestAdvectVelocityExamples")]
    public void TestAdvectVelocityParametrized(float[,] u, float[,] v, int[,] s, float[,] exp_u, float[,] exp_v) {
        int resX = u.GetLength(0);
        int resY = u.GetLength(1);
        SimulationState simulationState = new SimulationState(resX, resY);
        // int[,] s = new int[resX, resY];
        // TestUtils.Fill(s, 1);
        TestUtils.Print2D(u);
        TestUtils.Print2D(v);
        TestUtils.Print2D(s);

        Advect advect = new Advect();
        simulationState.uBuf.SetData(u);
        simulationState.vBuf.SetData(v);
        simulationState.sBuf.SetData(s);
        advect.AdvectVelocity(simulationState, 1, 1);
        float[,] u2 = new float[resX, resY];
        float[,] v2 = new float[resX, resY];
        simulationState.uBuf.GetData(u2);
        simulationState.vBuf.GetData(v2);

        TestUtils.Print2D(u2);
        TestUtils.Print2D(v2);

        TestUtils.AssertEqual(exp_u, u2);
        TestUtils.AssertEqual(exp_v, v2);
    }

    static IEnumerable<TestCaseData> TestCellCenterVelocityExamples() {
        yield return new TestCaseData(new object[]{
            TestUtils.FT(new float[,] {
                {-1, -2, -3},
                {2, -4, 0},
            }),
            TestUtils.FT(new float[,] {
                {0.5f, 1.5f, 2.5f},
                {4.5f, 2.5f, 5.5f},
            }),
            TestUtils.FT(new float[,] {
                {-1.5f, -2.5f, -1.5f},
                {-1, -2, 0},
            }),
            TestUtils.FT(new float[,] {
                {0.25f, 0.75f, 1.25f},
                {2.5f, 2f, 4f},
            }),
        });
    }

    [Test]
    [TestCaseSource("TestCellCenterVelocityExamples")]
    public void TestCellCenterVelocity(float[,] u, float[,] v, float[,] exp_u, float[,] exp_v) {
        TestUtils.Print2D(u);
        TestUtils.Print2D(v);

        int resX = u.GetLength(0);
        int resY = u.GetLength(1);
        SimulationState simulationState = new SimulationState(resX, resY);

        Advect advect = new Advect();
        simulationState.uBuf.SetData(u);
        simulationState.vBuf.SetData(v);
        ComputeShader shader = advect.TestingGetShader();
        int kernel = shader.FindKernel("kGetCellCenterVelocity");
        ComputeBuffer uBuffer = new ComputeBuffer(resX * resY, sizeof(float));
        ComputeBuffer vBuffer = new ComputeBuffer(resX * resY, sizeof(float));
        ComputeBuffer destBuffer = new ComputeBuffer(resX * resY, sizeof(float) * 3);
        // ComputeBuffer v2Buffer = new ComputeBuffer(resX * resY, sizeof(float));
        uBuffer.SetData(u);
        vBuffer.SetData(v);
        shader.SetBuffer(kernel, "u", uBuffer);
        shader.SetBuffer(kernel, "v", vBuffer);
        shader.SetInt("simResX", resX);
        shader.SetInt("simResY", resY);
        shader.SetFloat("speedDeltaTime", 1);
        shader.SetBuffer(kernel, "destFloat3", destBuffer);
        shader.Dispatch(kernel, resX, resY, 1);
        // advect.AdvectVelocity(simulationState, 1, 1);
        // float[,] u2 = new float[resX, resY];
        Vector3[,] dest = new Vector3[resX, resY];
        // simulationState.uBuffer.GetData(u2);
        destBuffer.GetData(dest);

        float[,] u2 = new float[resX, resY];
        float[,] v2 = new float[resX, resY];
        float[,] w2 = new float[resX, resY];

        TestUtils.Vector3ArrayToComponents(dest, out u2, out v2, out w2);

        TestUtils.Print2D(u2);
        TestUtils.Print2D(v2);

        TestUtils.AssertEqual(exp_u, u2);
        TestUtils.AssertEqual(exp_v, v2);

        uBuffer.Release();
        vBuffer.Release();
        destBuffer.Release();
        // v2Buffer.Release();
    }

    static IEnumerable<TestCaseData> TestInterpolateVelocityLeftExamples() {
        /*
               v[0, 1]         v[1, 1]
        u[0,0] s[0, 0] u[1, 0] s[1, 0]
               v[0, 0]         v[1, 0]
        
        mix vs: [0,0], left, up, left-up
        */
        yield return new TestCaseData(new object[]{
            TestUtils.FT(new float[,] {
                {-1, -2, -3},
                {2, -4, 0},
            }),
            TestUtils.FT(new float[,] {
                {0.5f, 1.5f, 2.5f},
                {4.5f, 2.5f, 5.5f},
            }),
            TestUtils.FT(new float[,] {
                {-1, -2, -3},
                {2, -4, 0},
            }),
            TestUtils.FT(new float[,] {
                {0.125f, 0.5f, 1f},
                {1.25f, 2.25f, 3f},
            }),
        });
    }

    [Test]
    [TestCaseSource("TestInterpolateVelocityLeftExamples")]
    public void TestInterpolateVelocityLeft(float[,] u, float[,] v, float[,] exp_u, float[,] exp_v) {
        TestUtils.Print2D(u);
        TestUtils.Print2D(v);

        int resX = u.GetLength(0);
        int resY = u.GetLength(1);
        SimulationState simulationState = new SimulationState(resX, resY);

        Advect advect = new Advect();
        simulationState.uBuf.SetData(u);
        simulationState.vBuf.SetData(v);
        ComputeShader shader = advect.TestingGetShader();
        int kernel = shader.FindKernel("kInterpolateVelocityLeft");
        ComputeBuffer uBuffer = new ComputeBuffer(resX * resY, sizeof(float));
        ComputeBuffer vBuffer = new ComputeBuffer(resX * resY, sizeof(float));
        ComputeBuffer destBuffer = new ComputeBuffer(resX * resY, sizeof(float) * 3);
        // ComputeBuffer v2Buffer = new ComputeBuffer(resX * resY, sizeof(float));
        uBuffer.SetData(u);
        vBuffer.SetData(v);
        shader.SetBuffer(kernel, "u", uBuffer);
        shader.SetBuffer(kernel, "v", vBuffer);
        shader.SetInt("simResX", resX);
        shader.SetInt("simResY", resY);
        shader.SetFloat("speedDeltaTime", 1);
        shader.SetBuffer(kernel, "destFloat3", destBuffer);
        shader.Dispatch(kernel, resX, resY, 1);
        // advect.AdvectVelocity(simulationState, 1, 1);
        // float[,] u2 = new float[resX, resY];
        Vector3[,] dest = new Vector3[resX, resY];
        // simulationState.uBuffer.GetData(u2);
        destBuffer.GetData(dest);

        float[,] u2 = new float[resX, resY];
        float[,] v2 = new float[resX, resY];
        float[,] w2 = new float[resX, resY];

        TestUtils.Vector3ArrayToComponents(dest, out u2, out v2, out w2);

        TestUtils.Print2D(u2);
        TestUtils.Print2D(v2);

        TestUtils.AssertEqual(exp_u, u2);
        TestUtils.AssertEqual(exp_v, v2);

        uBuffer.Release();
        vBuffer.Release();
        destBuffer.Release();
        // v2Buffer.Release();
    }

    static IEnumerable<TestCaseData> TestInterpolateVelocityBottomExamples() {
        /*
               v[0, 1]         v[1, 1]
        u[0,0] s[0, 0] u[1, 0] s[1, 0]
               v[0, 0]         v[1, 0]
        
        mix us: [0,0], right, down, right-down
        */
        yield return new TestCaseData(new object[]{
            TestUtils.FT(new float[,] {
                {-1, -2, -3},
                {2, -4, 0},
            }),
            TestUtils.FT(new float[,] {
                {0.5f, 1.5f, 2.5f},
                {4.5f, 2.5f, 5.5f},
            }),
            TestUtils.FT(new float[,] {
                {-1.25f, -2.25f, -0.75f},
                {-0.5f, -1, 0},
            }),
            TestUtils.FT(new float[,] {
                {0.5f, 1.5f, 2.5f},
                {4.5f, 2.5f, 5.5f},
            }),
        });
    }

    [Test]
    [TestCaseSource("TestInterpolateVelocityBottomExamples")]
    public void TestInterpolateVelocityBottom(float[,] u, float[,] v, float[,] exp_u, float[,] exp_v) {
        TestUtils.Print2D(u);
        TestUtils.Print2D(v);

        int resX = u.GetLength(0);
        int resY = u.GetLength(1);
        SimulationState simulationState = new SimulationState(resX, resY);

        Advect advect = new Advect();
        simulationState.uBuf.SetData(u);
        simulationState.vBuf.SetData(v);
        ComputeShader shader = advect.TestingGetShader();
        int kernel = shader.FindKernel("kInterpolateVelocityBottom");
        ComputeBuffer uBuffer = new ComputeBuffer(resX * resY, sizeof(float));
        ComputeBuffer vBuffer = new ComputeBuffer(resX * resY, sizeof(float));
        ComputeBuffer destBuffer = new ComputeBuffer(resX * resY, sizeof(float) * 3);
        // ComputeBuffer v2Buffer = new ComputeBuffer(resX * resY, sizeof(float));
        uBuffer.SetData(u);
        vBuffer.SetData(v);
        shader.SetBuffer(kernel, "u", uBuffer);
        shader.SetBuffer(kernel, "v", vBuffer);
        shader.SetInt("simResX", resX);
        shader.SetInt("simResY", resY);
        shader.SetFloat("speedDeltaTime", 1);
        shader.SetBuffer(kernel, "destFloat3", destBuffer);
        shader.Dispatch(kernel, resX, resY, 1);
        // advect.AdvectVelocity(simulationState, 1, 1);
        // float[,] u2 = new float[resX, resY];
        Vector3[,] dest = new Vector3[resX, resY];
        // simulationState.uBuffer.GetData(u2);
        destBuffer.GetData(dest);

        float[,] u2 = new float[resX, resY];
        float[,] v2 = new float[resX, resY];
        float[,] w2 = new float[resX, resY];

        TestUtils.Vector3ArrayToComponents(dest, out u2, out v2, out w2);

        TestUtils.Print2D(u2);
        TestUtils.Print2D(v2);

        TestUtils.AssertEqual(exp_u, u2);
        TestUtils.AssertEqual(exp_v, v2);

        uBuffer.Release();
        vBuffer.Release();
        destBuffer.Release();
        // v2Buffer.Release();
    }

    static IEnumerable<TestCaseData> TestInterpolateFractionalPosFloatExamples() {
        yield return new TestCaseData(new object[]{
            TestUtils.FT(new float[,] {
                {-1, -2, -3},
                {2, -4, 1},
            }),
            TestUtils.FT(new Vector2[,] {
                {new Vector2(0.5f, 1.0f), new Vector2(2.2f, 0.3f), new Vector2(1.8f, 1.4f)},
                {new Vector2(1.6f, 1.8f), new Vector2(1, 1), new Vector2(0, 1)},
            }),
            TestUtils.FT(new float[,] {
                {
                    (0.5f * (-1) + 0.5f * (-2)),  // -1.5
                    (0.8f * (1)) * 0.7f + (0.8f * (-3)) * 0.3f, // -0.16
                    (0.2f * (-2) + 0.8f * (-3)) * 0.6f  // -1.68
                },
                {
                    // -2.4f,
                    0.2f * (0.4f * (-2) + 0.6f * (-3)), // -0.52
                    -2,
                    -1
                },
            }),
        });
    }

    [Test]
    [TestCaseSource("TestInterpolateFractionalPosFloatExamples")]
    public void TestInterpolateFractionalPosFloat(float[,] source, Vector2[,] fractionalPos, float[,] exp_interp) {
        // - source array (float) => map to u
        // - fractional pos (float2) => map to srcFloat3.xy

        // - output (float) => map to u2

        TestUtils.Print2D(source);
        float[,] fractionalPosX;
        float[,] fractionalPosY;
        TestUtils.Vector2ArrayToComponents(fractionalPos, out fractionalPosX, out fractionalPosY);
        TestUtils.Print2D(fractionalPosX);
        TestUtils.Print2D(fractionalPosY);

        int resX = source.GetLength(0);
        int resY = source.GetLength(1);
        SimulationState simulationState = new SimulationState(resX, resY);

        Advect advect = new Advect();
        // simulationState.uBuffer.SetData(source);
        // simulationState.vBuffer.SetData(v);
        ComputeShader shader = advect.TestingGetShader();
        int kernel = shader.FindKernel("kInterpolateFractionalPosFloat");
        ComputeBuffer uBuffer = new ComputeBuffer(resX * resY, sizeof(float));
        ComputeBuffer srcFloat3Buffer = new ComputeBuffer(resX * resY, sizeof(float) * 3);
        ComputeBuffer u2Buffer = new ComputeBuffer(resX * resY, sizeof(float));
        // ComputeBuffer v2Buffer = new ComputeBuffer(resX * resY, sizeof(float));
        uBuffer.SetData(source);
        Vector3[,] fractionalPosV3 = new Vector3[resX, resY];
        for(int i = 0; i < resX; i++) {
            for(int j = 0; j < resY; j++) {
                Vector2 v2 = fractionalPos[i, j];
                fractionalPosV3[i, j] = new Vector3(v2.x, v2.y, 0);
            }
        }
        srcFloat3Buffer.SetData(fractionalPosV3);
        shader.SetBuffer(kernel, "u", uBuffer);
        shader.SetBuffer(kernel, "u2", u2Buffer);
        shader.SetInt("simResX", resX);
        shader.SetInt("simResY", resY);
        shader.SetFloat("speedDeltaTime", 1);
        shader.SetBuffer(kernel, "srcFloat3", srcFloat3Buffer);
        shader.Dispatch(kernel, resX, resY, 1);
        // advect.AdvectVelocity(simulationState, 1, 1);
        // float[,] u2 = new float[resX, resY];
        float[,] u2 = new float[resX, resY];
        // simulationState.uBuffer.GetData(u2);
        u2Buffer.GetData(u2);
        TestUtils.Print2D(u2);

        TestUtils.AssertEqual(exp_interp, u2);

        uBuffer.Release();
        srcFloat3Buffer.Release();
        u2Buffer.Release();
        // v2Buffer.Release();
    }
}
