#include "fix16_.h"
#include "int64.h"


inline fix16_t fix16_mul(fix16_t inArg0, fix16_t inArg1)
{
	int64_t product = inArg0 * inArg1;
	return product >> 16;
}

inline fix16_t fix16_div(fix16_t a, fix16_t b)
{
	if (b == 0) return 0;
	return (a << 16) / b;
}
