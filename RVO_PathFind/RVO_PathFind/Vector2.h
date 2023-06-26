#ifndef RVO_VECTOR2_H_
#define RVO_VECTOR2_H_

#include <cmath>
#include <ostream>

namespace RVO {

	class Vector2 {
	public:

		inline Vector2() : x_(0.0f), y_(0.0f) { }

		inline Vector2(float x, float y) : x_(x), y_(y) { }

		inline float x() const { return x_; }

		inline float y() const { return y_; }

		inline Vector2 operator-() const
		{
			return Vector2(-x_, -y_);
		}

		inline float operator*(const Vector2 &vector) const
		{
			return x_ * vector.x() + y_ * vector.y();
		}

		inline Vector2 operator*(float s) const
		{
			return Vector2(x_ * s, y_ * s);
		}

		inline Vector2 operator/(float s) const
		{
			const float invS = 1.0f / s;

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

		Vector2 &operator*=(float s)
		{
			x_ *= s;
			y_ *= s;

			return *this;
		}

		Vector2 &operator/=(float s)
		{
			const float invS = 1.0f / s;
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
		float x_;
		float y_;
	};

	inline Vector2 operator*(float s, const Vector2 &vector)
	{
		return Vector2(s * vector.x(), s * vector.y());
	}

	inline std::ostream &operator<<(std::ostream &os,
											   const Vector2 &vector)
	{
		os << "(" << vector.x() << "," << vector.y() << ")";

		return os;
	}

	inline float abs(const Vector2 &vector)
	{
		return std::sqrt(vector * vector);
	}

	inline float absSq(const Vector2 &vector)
	{
		return vector * vector;
	}

	inline float det(const Vector2 &vector1, const Vector2 &vector2)
	{
		return vector1.x() * vector2.y() - vector1.y() * vector2.x();
	}

	inline Vector2 normalize(const Vector2 &vector)
	{
		return vector / abs(vector);
	}
}

#endif /* RVO_VECTOR2_H_ */
