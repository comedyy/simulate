#ifndef RVO_DEFINITIONS_H_
#define RVO_DEFINITIONS_H_

#include <algorithm>
#include <cmath>
#include <cstddef>
#include <limits>
#include <vector>

#include "Vector2.h"

namespace RVO {
	class Agent;
	class Obstacle;
	class RVOSimulator;

	inline Fix16 distSqPointLineSegment(const Vector2 &a, const Vector2 &b,
										const Vector2 &c)
	{
		const Fix16 r = ((c - a) * (b - a)) / absSq(b - a);

		if (r < Fix16::zero) {
			return absSq(c - a);
		}
		else if (r > Fix16::one) {
			return absSq(c - b);
		}
		else {
			return absSq(c - (a + r * (b - a)));
		}
	}

	inline Fix16 leftOf(const Vector2 &a, const Vector2 &b, const Vector2 &c)
	{
		return det(a - c, b - a);
	}

	inline Fix16 sqr(Fix16 a)
	{
		return a * a;
	}
}

#endif /* RVO_DEFINITIONS_H_ */
