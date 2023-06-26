#ifndef RVO_DEFINITIONS_H_
#define RVO_DEFINITIONS_H_

#include <algorithm>
#include <cmath>
#include <cstddef>
#include <limits>
#include <vector>

#include "Vector2.h"

const float RVO_EPSILON = 0.00001f;

namespace RVO {
	class Agent;
	class Obstacle;
	class RVOSimulator;

	inline float distSqPointLineSegment(const Vector2 &a, const Vector2 &b,
										const Vector2 &c)
	{
		const float r = ((c - a) * (b - a)) / absSq(b - a);

		if (r < 0.0f) {
			return absSq(c - a);
		}
		else if (r > 1.0f) {
			return absSq(c - b);
		}
		else {
			return absSq(c - (a + r * (b - a)));
		}
	}

	inline float leftOf(const Vector2 &a, const Vector2 &b, const Vector2 &c)
	{
		return det(a - c, b - a);
	}

	inline float sqr(float a)
	{
		return a * a;
	}
}

#endif /* RVO_DEFINITIONS_H_ */
