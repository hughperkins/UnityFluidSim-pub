using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.TestTools.Utils;

using LiquidShader;
using Utils;

class TestBuf2 {
    // float requiresArray(float[,] tgt) {
    //     return tgt[1, 2];
    // }

    [Test]
    public void Test1(){
        Buf2<float> cf = new Buf2<float>(5, 7);
        Buf2<Vector3> cf3 = new Buf2<Vector3>(5, 7);
        Buf2<int> ci = new Buf2<int>(5, 7);
        Buf2<Vector3Int> ci3 = new Buf2<Vector3Int>(5, 7);
        Buf2<Vector4> cf4 = new Buf2<Vector4>(5, 7);

        float input = 3.5f;
        float input2 = 5.7f;
        float input3 = 222;
        cf[2, 4] = input;
        cf[1, 2] = input2;
        cf[4, 6] = 222;
        Assert.AreEqual(input, cf[2, 4]);
        Assert.AreEqual(input2, cf[1, 2]);
        Assert.AreEqual(input3, cf[4, 6]);

        // requiresArray(cf);

        cf.ToGPU();
        cf[2, 4] = 123f;
        cf[4, 6] = 124f;
        Assert.AreEqual(123, cf[2, 4]);
        Assert.AreEqual(124, cf[4, 6]);

        cf.FromGPU();
        Assert.AreEqual(input, cf[2, 4]);
        Assert.AreEqual(input3, cf[4, 6]);
    }

    [Test]
    public void Testf3(){
        Buf2<Vector3> cf3 = new Buf2<Vector3>(5, 7);

        Vector3 in1 = new Vector3(0.1f, 2.2f, 5.4f);
        Vector3 in2 = new Vector3(3.2f, 5.1f, 3.9f);

        cf3[1, 4] = in1;
        cf3[2, 3] = in2;

        Assert.AreEqual(in1, cf3[1, 4]);
        Assert.AreEqual(in2, cf3[2, 3]);

        cf3.ToGPU();
        Vector3 in1a = new Vector3(0, 123f, 0);
        Vector3 in2a = new Vector3(0, 0, 124f);
        cf3[1, 4] = in1a;
        cf3[2, 3] = in2a;
        Assert.AreEqual(in1a, cf3[1, 4]);
        Assert.AreEqual(in2a, cf3[2, 3]);

        cf3.FromGPU();
        Assert.AreEqual(in1, cf3[1, 4]);
        Assert.AreEqual(in2, cf3[2, 3]);
    }
}
