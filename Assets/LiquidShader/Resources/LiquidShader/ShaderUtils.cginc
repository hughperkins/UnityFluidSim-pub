float max3(float a, float b, float c) {
    return max(a, max(b, c));
}

float minaxes(float3 v) {
    return min(min(v.x, v.y), v.z);
}

float maxaxes(float3 v) {
    return max(max(v.x, v.y), v.z);
}

float2 getRayBoxIntersection(float3 rayStart, float3 rayDirection, float3 boundsMin, float3 boundsMax) {
    float3 invDir = 1 / rayDirection;
    float3 t1 = (boundsMin - rayStart) * invDir;
    float3 t2 = (boundsMax - rayStart) * invDir;
    float3 t1a = min(t1, t2);
    float3 t2b = max(t1, t2);
    float t1max = maxaxes(t1a);
    float t2min = minaxes(t2b);
    return float2(t1max, t2min);
}

// from https://stackoverflow.com/questions/12964279/whats-the-origin-of-this-glsl-rand-one-liner
float rand(float2 co){
    return frac(sin(dot(co.xy ,float2(12.9898,78.233))) * 43758.5453);
}

float4 colorFromObjectPos(float3 objectPos) {
    /*
    Take position within object, -0.5 <= x/y/z <= 0.5, and change that
    into a color, one color channel per axis.
    */
    float margin = 0;
    float4 col = float4(0, 0, 0, 1);
    if(objectPos.x >= -5 + margin && objectPos.x <= 5 - margin) {
        col.x = objectPos.x + 0.5;
    }
    if(objectPos.y >= -5 + margin && objectPos.y <= 5 - margin) {
        col.y = objectPos.y + 0.5;
    }
    if(objectPos.z >= -5 + margin && objectPos.z <= 5 - margin) {
        col.z = objectPos.z + 0.5;
    }
    return col;
}

float3 HSVtoRGB(float3 hsv) {
    float h = min(1, max(0, hsv.x));
    float s = hsv.y;
    float v = hsv.z;

    float c = v * s;
    float x = c * (1.0 - abs(fmod(h * 6.0, 2.0) - 1.0));
    float m = v - c;

    float3 col;

    if (h < 0.16666) {
        col = float3(c, x, 0.0);
    } else if (h < 0.33333) {
        col = float3(x, c, 0.0);
    } else if (h < 0.5) {
        col = float3(0.0, c, x);
    } else if (h < 0.66666) {
        col = float3(0.0, x, c);
    } else if (h < 0.83333) {
        col = float3(x, 0.0, c);
    } else {
        col = float3(c, 0.0, x);
    }

    return col + m;
}
