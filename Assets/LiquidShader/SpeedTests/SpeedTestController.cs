using System.Collections;
using UnityEngine;
using System.Diagnostics;
using Utils;

namespace LiquidShader {

public class SpeedTestController : MonoBehaviour
{
    [SerializeField] bool doSync = true;
    [SerializeField] bool doCopyBuffer = false;
    [SerializeField] int numIts = 100;

    ComputeBuffer _syncSrcBuffer;
    ComputeBuffer _syncDstBuffer;

    CopyBuffer _copyBuffer;
    Pooling _maxAbsBuffer;

    // Start is called before the first frame update
    void Start()
    {
        _copyBuffer = new CopyBuffer();
        _maxAbsBuffer = new Pooling();

        _syncSrcBuffer = new ComputeBuffer(1, sizeof(int), ComputeBufferType.Counter);
        _syncDstBuffer = new ComputeBuffer(1, sizeof(int), ComputeBufferType.Counter);
    }

    void Sync() {
        ComputeBuffer.CopyCount(_syncSrcBuffer, _syncDstBuffer, 0);
    }

    void RunMaxOnce(Buf2<float> someBuf, Buf2<float> dest) {
        _maxAbsBuffer.MaxAbsFloats(someBuf, dest);
        if(doSync) {
            Sync();
        }
        if(doCopyBuffer) {
            _copyBuffer.CopyFloats(1, 1, dest, someBuf);
        }
    }

    void RunMaxI() {
        UnityEngine.Debug.Log("start");
        var simResY = 480;
        var simResX = (simResY * 16) / 9;
        // Vector2Int simRes = new Vector2Int(simResX, simResY);
        var someBuf = new Buf2<float>(simResX, simResY);
        var chunksResX = (simResX + 8 - 1) / 8;
        var chunksResY = (simResY + 8 - 1) / 8;
        var dest = new Buf2<float>(chunksResX, chunksResY);
        // float startTime = Time.time;
        var stopwatch = new Stopwatch();
        // UnityEngine.Debug.Log($"start {stopwatch.}");
        stopwatch.Start();
        // int numIts = 100;
        for(var i = 0; i < numIts; i++) {
            // Debug.Log(".");
            RunMaxOnce(someBuf, dest);
        }
        stopwatch.Stop();
        // float endTime = Time.time;
        // float elapsed = endTime - startTime;
        float elapsedMills = stopwatch.ElapsedMilliseconds;
        var averageMills = elapsedMills / (float)numIts;
        UnityEngine.Debug.Log($"End total {elapsedMills}ms averarge {averageMills}ms");
        // UnityEngine.Debug.Log($"End {Time.time}");

        someBuf.Release();
        dest.Release();
    }

    IEnumerator WaitingTask() {
        for(var i = 0; i < 10; i++) {
            UnityEngine.Debug.Log(".");
            yield return new WaitForSeconds(0.2f);
        }
    }

    IEnumerator WaitForTask() {
        UnityEngine.Debug.Log("Start waiting for task");
        yield return StartCoroutine(WaitingTask());
        UnityEngine.Debug.Log("Finished waiting for task");
    }

    public void BtnRunMax() {
        RunMaxI();
        // StartCoroutine(WaitForTask());
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

} // namespace LiquidShader
