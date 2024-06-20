using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

using LiquidShader;
using LiquidShader.Types;

public class TestProjectJacobi
{
    GameObject gameObject;
    ProjectJacobi project;

    [SetUp]
    public void SetUp() {
        gameObject = new GameObject("Testing");
        project = gameObject.AddComponent<ProjectJacobi>();
    }

    [TearDown]
    public void TearDwon() {
        GameObject.DestroyImmediate(gameObject);
    }

    static IEnumerable<TestCaseData> TestProject1Examples() {
        yield return new TestCaseData(new object[]{
            /*
            s   0    1     1      1      0
            u 1    3    5     5      3
            d   -    2    0      -2     -
        delta -   -   -2+0   +0-2    -   
            */
            TestUtils.FT(new float[,] {
                {0, 0, 0, 0, 0, },
                {1, 3, 5, 5, 3, },
                {0, 0, 0, 0, 0, },
            }),
            TestUtils.FT(new float[,] {
                {0, 0, 0, 0, 0, },
                {0, 0, 0, 0, 0,},
                {0, 0, 0, 0, 0, },
            }),
            TestUtils.FT(new int[,] {
                {0, 0, 0, 0, 0, },
                {0, 1, 1, 1, 0, },
                {0, 0, 0, 0, 0, },
            }),
            TestUtils.FT(new float[,] {
                {0, 0, 0, 0, 0, },
                {
                    1,
                    3,         // locked
                    5 - 2 / 2 + 0, // 4
                    5 - 0 - 2 / 2, // 4
                    3
                },
                {0, 0, 0, 0, 0, },
            }),
            TestUtils.FT(new float[,] {
                {0, 0, 0, 0, 0, },
                {0, 0, 0, 0, 0, },
                {0, 0, 0, 0, 0,},
            }),
        });

        yield return new TestCaseData(new object[]{
            /*
            s   0    1     1      1      0
            u 1    3    5     5      3
            d   -    2    0      -2     -
        delta -   -   -2+0   +0-2    -   
            */
            TestUtils.FT(new float[,] {
                {0, 0, 0, 0, 0, },
                {1, -1, 1, -1, 1, },
                {1,  1, -1, 1, -1 },
                {1, -1, 1, -1, 1, },
                {0, 0, 0, 0, 0, }
            }),
            TestUtils.FT(new float[,] {
                {-1, 1, -1, 1, -1, },
                {1, -1, 1, -1, 1, },
                {1,  1, -1, 1, -1 },
                {1, -1, 1, -1, 1, },
                {-1, 1, -1, 1, -1, },
            }),
            TestUtils.FT(new int[,] {
                {0, 0, 0, 0, 0, },
                {0, 1, 1, 1, 0, },
                {0, 1, 1, 1, 0, },
                {0, 1, 1, 1, 0, },
                {0, 0, 0, 0, 0, },
            }),
            /*
            s_sum
                0, 1, 1, 1, 0
                1, 2, 3, 2, 1
                1, 3, 4, 3, 1
                1, 2, 3, 2, 1
                0, 1, 1, 1, 0
            disp
                0,  0,  0,  0, 0
                0, -4,  4, -4, 0
                0,  4, -4,  4, 0
                0, -4,  4, -4, 0
                0,  0,  0,  0, 0
            disp/sum
                0,  0,       0,    0, 0
                0, -2,    1.33,   -2, 0
                0,  1.33,   -1, 1.33, 0
                0   -2,   1.33,   -2, 0
                0,  0,       0,    0, 0
            */
            TestUtils.FT(new float[,] {
                {0, 0, 0, 0, 0, },
                {
                    1,
                    -1, // because left is fixed
                    1 + ((-2) - (1.3333f)) / 2, // 0.666
                    -1 + ((1.3333f) - (-2f)) / 2, // 0.666
                    1,
                },
                {1, 1, 0.16666f, -0.16666f, -1},  // this plus following outputs copied from output :P
                {1, -1, -0.6666f, 0.6666f, 1 },
                {0,0,0,0,0}
            }),
            TestUtils.FT(new float[,] {
                {-1, 1, -1, 1, -1}, // this plus following outputs copied from output :P
                {1, 0.6666f, -0.166666f, 0.6666f, 1 },
                {1, -0.6666f, 0.16666f, -0.6666f, -1},
                {1, -1, 1, -1, 1, },
                {-1, 1, -1, 1, -1},
            }),
        });
    }

    [Test]
    [TestCaseSource("TestProject1Examples")]
    public void TestProject1(float[,] u, float[,] v, int[,] s, float[,] exp_u, float[,] exp_v) {
        int resX = u.GetLength(0);
        int resY = u.GetLength(1);
        SimulationState simulationState = new SimulationState(resX, resY);
        TestUtils.Print2D(u);
        TestUtils.Print2D(v);
        TestUtils.Print2D(s);

        // Project project = new Project();
        simulationState.uBuf.SetData(u);
        simulationState.vBuf.SetData(v);
        simulationState.sBuf.SetData(s);
        project.solverIterations = 1;
        project.RunProject(simulationState, 1, 1, debug: true);
        float[,] u_new = new float[resX, resY];
        float[,] v_new = new float[resX, resY];
        simulationState.uBuf.GetData(u_new);
        simulationState.vBuf.GetData(v_new);

        TestUtils.Print2D(u_new);
        TestUtils.Print2D(v_new);

        TestUtils.AssertEqual(exp_u, u_new);
        TestUtils.AssertEqual(exp_v, v_new);
    }
}
