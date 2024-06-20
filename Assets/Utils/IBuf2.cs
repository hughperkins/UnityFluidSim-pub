using UnityEngine;

namespace Utils {

public interface IBuf2<T> {
    int Offset { get; }
    int ResX { get; }
    int ResY { get; }
    Vector2Int ResVec2 { get; }
    int[] ResInts { get; }
    ComputeBuffer GetComputeBuffer();
}

} // namespace LiquidShader
