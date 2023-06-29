#ifndef RVO_VECTOR2_H_
#define RVO_VECTOR2_H_

#include <cmath>
#include <ostream>
#include "../libfixmath/fix16.hpp"

namespace RVO {

	class Vector2 {
	public:

		inline Vector2() : x_(Fix16::zero), y_(Fix16::zero) { }

		inline Vector2(Fix16 x, Fix16 y) : x_(x), y_(y) { }

		inline Fix16 x() const { return x_; }

		inline Fix16 y() const { return y_; }

		inline Vector2 operator-() const
		{
			return Vector2(-x_, -y_);
		}

		inline Fix16 operator*(const Vector2 &vector) const
		{
			return x_ * vector.x() + y_ * vector.y();
		}

		inline Vector2 operator*(Fix16 s) const
		{
			return Vector2(x_ * s, y_ * s);
		}

		inline Vector2 operator/(Fix16 s) const
		{
			const Fix16 invS = Fix16::one / s;

			return Vector2(x_ * invS, y_ * invS);
		}

		inline Vector2 operator+(const Vector2 &vector) const
		{
			return Vector2(x_ + vector.x(), y_ + vector.y());
		}

		inline Vector2 operator-(const Vector2 &vector) const
		{
			return Vector2(x_ - vector.x(), y_ - vector.y());
		}

		inline bool operator==(const Vector2 &vector) const
		{
			return x_ == vector.x() && y_ == vector.y();
		}

		inline bool operator!=(const Vector2 &vector) const
		{
			return x_ != vector.x() || y_ != vector.y();
		}

		Vector2 &operator*=(Fix16 s)
		{
			x_ *= s;
			y_ *= s;

			return *this;
		}

		Vector2 &operator/=(Fix16 s)
		{
			const Fix16 invS = Fix16::one / s;
			x_ *= invS;
			y_ *= invS;

			return *this;
		}

		inline Vector2 &operator+=(const Vector2 &vector)
		{
			x_ += vector.x();
			y_ += vector.y();

			return *this;
		}

		inline Vector2 &operator-=(const Vector2 &vector)
		{
			x_ -= vector.x();
			y_ -= vector.y();

			return *this;
		}

	private:
		Fix16 x_;
		Fix16 y_;
	};

	inline Vector2 operator*(Fix16 s, const Vector2 &vector)
	{
		return Vector2(s * vector.x(), s * vector.y());
	}

	/*inline std::ostream &operator<<(std::ostream &os,
											   const Vector2 &vector)
	{
		os << "(" << vector.x() << "," << vector.y() << ")";

		return os;
	}*/

	inline Fix16 abs(const Vector2 &vector)
	{
		Fix16 value = vector * vector;
		return value.sqrt();
	}

	inline Fix16 absSq(const Vector2 &vector)
	{
		return vector * vector;
	}

	inline Fix16 det(const Vector2 &vector1, const Vector2 &vector2)
	{
		return vector1.x() * vector2.y() - vector1.y() * vector2.x();
	}

	inline Vector2 normalize(const Vector2 &vector)
	{
		if (vector.x() == 0 && vector.y() == 0) return vector;

		Fix16 magnitude = abs(vector);
		if (magnitude > 0)
		{
			return vector / magnitude;
		}

		Vector2 vector1 = (vector * Fix16::_100);
		return vector1 / abs(vector1);
	}
}

#endif /* RVO_VECTOR2_H_ */
