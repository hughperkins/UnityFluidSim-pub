using UnityEngine;

namespace Utils {

public class ArrayUtils {
    public static void Print2D<T>(T[,] tgt) {
        var w = tgt.GetLength(0);
        var h = tgt.GetLength(1);
        var output = "";
        for(var y = h - 1; y >=0; y--) {
            var row = "";
            for(var x = 0; x < w; x++) {
                row += tgt[x, y] + ", ";
            }
            output += row + "\n";
        }
        Debug.Log(output);
    }
}

} // namespace LiquidShader
