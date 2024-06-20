#define L(idx) ((idx) - _simResY)
#define R(idx) ((idx) + _simResY)
#define D(idx) ((idx) - 1)
#define U(idx) ((idx) + 1)

#define  LEFT(idx) ((idx) - _simResY)
#define RIGHT(idx) ((idx) + _simResY)
#define  DOWN(idx) ((idx) - 1)
#define    UP(idx) ((idx) + 1)

inline int posiToIdx(int2 pos) {
    return pos.x * _simResY + pos.y;
}
