using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.TestTools.Utils;

using LiquidShader;
using Utils;

public class TestCopy
{
    [Test]
    public void TestCopyFloatsSimple() {
        Buf2<float> srcBuf = new Buf2<float>(1, 5);
        Buf2<float> destBuf = new Buf2<float>(1, 5);
        float[,] inputs = new float[,]{{1.3f, 2.8f, 0.3f, 4.9f, -2.111f}};
        srcBuf.SetData(inputs);
        float[,] outputs = new float[1, 5];
        // destBuffer.SetData(outputs);
        CopyBuffer copyBuffer = new CopyBuffer();
        copyBuffer.CopyFloats(5, 1, srcBuf, destBuf);
        destBuf.GetData(outputs);

        for(int i = 0; i < 5; i++) {
            Debug.Log($"{i} {outputs[0, i]}");
            Assert.That(outputs[0, i], Is.EqualTo(inputs[0, i]).Using(FloatEqualityComparer.Instance));
        }
    }

    [Test]
    public void TestCopyFloat4sSimple() {
        int numInputs = 5;
        Buf2<Vector4> srcBuffer = new Buf2<Vector4>(1, numInputs);
        Buf2<Vector4> destBuffer = new Buf2<Vector4>(1, numInputs);
        Vector4[,] inputs = new Vector4[,]{{
            new Vector4(0.1f, 0.4f, -1.2f, 0),
            new Vector4(1.1f, 0.4f, -1.2f, 1),
            new Vector4(0.1f, 3.4f, -1.2f, 1),
            new Vector4(0.1f, 0.4f, -5.2f, 0),
            new Vector4(5.1f, 3.4f, -2.2f, 4),
            }
        };
        srcBuffer.SetData(inputs);
        Vector4[,] outputs = new Vector4[1, numInputs];
        // destBuffer.SetData(outputs);
        CopyBuffer copyBuffer = new CopyBuffer();
        copyBuffer.CopyFloat4S(srcBuffer, destBuffer);
        destBuffer.GetData(outputs);

        for(int i = 0; i < numInputs; i++) {
            Debug.Log($"{i} {outputs[0, i]}");
            // Assert.That(outputs[i], Is.EqualTo(inputs[i]).Using(FloatEqualityComparer.Instance));
            TestUtils.AssertEqual(inputs[0, i], outputs[0, i]);
        }
    }
}
