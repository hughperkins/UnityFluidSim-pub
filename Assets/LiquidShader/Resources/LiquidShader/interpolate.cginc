// ReSharper disable CppInconsistentNaming
// ReSharper disable CppParameterMayBeConst
inline void posfToPosiLambda(float2 posf, out int2 posi, out float2 lambdaF) {
    /*
    Given a float position into a cell, returns the integer position of the
    bottom left cell, and a float lambda positive offset into cells above and
    to the right

    For example, if input is (2.1, 3.7) then outputs will be:
    - pos = (2, 3)
    - lambda = (0.1, 0.7)
    */
    posi = (int2)posf;
    lambdaF = posf - posi;
}

inline float2 getCellCenterVelocity(Buf<float> u, Buf<float> v, int2 pos, int offset) {
    /*
    Given cell at offset offset, returns the velocity at the center of that cell,
    by interpolating from the velocity components on each edge.
    */
    float u1 = pos.x < _simResX - 1 ? u[R(offset)] : 0;
    float v1 = pos.y < _simResY - 1 ? v[U(offset)] : 0;
    return float2(
        (u[offset] + u1) / 2,
        (v[offset] + v1) / 2
    );
}

float4 interpolateFloat4(Buf<float4> src, int offset, float2 lambda) {
    /*
    Given integer offset to a cell, and a fractional lambda into cells above and to the
    right, returns interpolated value from those four cells
    */
    float2 oneMinusLambda = 1 - lambda;
    float4 bottom = oneMinusLambda.x * src[offset] + lambda.x * src[R(offset)];
    float4 top = oneMinusLambda.x * src[U(offset)] + lambda.x * src[U(R(offset))];
    float4 v = oneMinusLambda.y * bottom + lambda.y * top;
    return v;
}

float interpolateFloat(Buf<float> src, int2 pos, int offset, float2 lambda) {
    /*
    Given integer offset to a cell, and a fractional lambda into cells above and to the
    right, returns interpolated value from those four cells
    */
    float2 oneMinusLambda = 1 - lambda;
    bool hasNonTop = pos.y >= 0;
    bool hasNonRight = pos.x >= 0;
    bool hasTop = pos.y < _simResY - 1;
    bool hasRight = pos.x < _simResX - 1;
    float bottom = hasNonTop ? (
        hasNonRight ? oneMinusLambda.x * src[offset] : 0
    ) + (
        hasRight ? lambda.x * src[R(offset)] : 0
    ) : 0;
    float top = hasTop ? (
        (
            hasNonRight ? oneMinusLambda.x * src[U(offset)] : 0
        ) +
        (
            hasRight ? lambda.x * src[U(R(offset))] : 0
        )
    ) : 0;
    float v = oneMinusLambda.y * bottom + lambda.y * top;
    return v;
}

float interpolateFloatNoBoundsChecking(const Buf<float> src, int2 pos, int idx, float2 lambda) {
    /*
    Given integer idx to a cell, and a fractional lambda into cells above and to the
    right, returns interpolated value from those four cells
    */
    float2 oneMinusLambda = 1 - lambda;
    float valueBottom = oneMinusLambda.x * src[idx] + lambda.x * src[RIGHT(idx)];
    float valueTop = oneMinusLambda.x * src[UP(idx)] + lambda.x * src[UP(RIGHT(idx))];
    float value = oneMinusLambda.y * valueBottom + lambda.y * valueTop;
    return value;
}

float interpolateFractionalPosFloat(const Buf<float> src, float2 fracPos) {
    int2 pos = (int2)fracPos;
    float2 lambda = fracPos - pos;
    int idx = posiToIdx(pos);
    float valueInterp = interpolateFloat(src, pos, idx, lambda);
    return valueInterp;
}

float2 interpolateVelocityLeft(Buf<float> horizVel, Buf<float> vertVel, int2 pos, int idx) {
    /*
    returns u at offset, and average of the four vs around u
    */
    /*
            v[0, 1]         v[1, 1]
    u[0,0] s[0, 0] u[1, 0] s[1, 0]
            v[0, 0]         v[1, 0]

    the four v's around a u are to left and up
    */
    float vertVelBottomRight = vertVel[idx];
    bool hasLeft = pos.x > 0;
    bool hasUp = pos.y < _simResY - 1;
    float vertVelBottomLeft = hasLeft ? vertVel[L(idx)] : 0;
    float vertVelTopRight = hasUp ? vertVel[U(idx)] : 0;
    float vertVelTopLeft = hasLeft && hasUp ? vertVel[U(L(idx))] : 0;
    return float2(
        horizVel[idx],
        (vertVelBottomRight + vertVelBottomLeft + vertVelTopRight + vertVelTopLeft)  / 4
    );
}

float2 interpolateVelocityLeftNoBoundsCheck(Buf<float> horizVel, Buf<float> vertVel, int idx) {
    /*
    returns u at offset, and average of the four vs around u
    */
    /*
            v[0, 1]         v[1, 1]
    u[0,0] s[0, 0] u[1, 0] s[1, 0]
            v[0, 0]         v[1, 0]

    the four v's around a u are to left and up
    */
    float v_br = vertVel[idx];
    float v_bl = vertVel[L(idx)];
    float v_tr = vertVel[U(idx)];
    float v_tl = vertVel[U(L(idx))];
    return float2(
        horizVel[idx],
        (v_br + v_bl + v_tr + v_tl)  / 4
    );
}

float2 interpolateVelocityBottom(Buf<float> u, Buf<float> v, int2 pos, int offset) {
    /*
    returns v at offset, and average of the four us around v
            v[0, 2]         v[1, 2]
    v[0,1]  s[0, 1] u[1, 1] s[1, 1]
            v[0, 1]         v[1, 1]
    u[0,0] s[0, 0] u[1, 0] s[1, 0]
            v[0, 0]         v[1, 0]

    The four u's around a v are down and right
    */

    float u_tl = u[offset];
    bool hasRight = pos.x < _simResX - 1;
    bool hasDown = pos.y > 0;
    float u_bl = hasDown ? u[D(offset)] : 0;
    float u_tr = hasRight ? u[R(offset)] : 0;
    float u_br = hasDown && hasRight ? u[D(R(offset))] : 0;

    return float2(
        (u_tl + u_bl + u_tr + u_br)  / 4,
        v[offset]
    );
}
