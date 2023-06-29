#ifndef __libfixmath_fix16_hpp__
#define __libfixmath_fix16_hpp__

#include "fix16_.h"

class Fix16 {
private:
	Fix16(const fix16_t inValue) { value = inValue; }

public:
	fix16_t value;

	Fix16() { value = 0; }
	Fix16(const Fix16& inValue) { value = inValue.value; }

	Fix16& operator=(const Fix16& rhs) { value = rhs.value;             return *this; }
	Fix16& operator=(const fix16_t rhs) { value = rhs;                   return *this; }

	Fix16& operator+=(const Fix16& rhs) { value = fix16_add(value, rhs.value); return *this; }
	Fix16& operator+=(const fix16_t rhs) { value = fix16_add(value, rhs); return *this; }

	Fix16& operator-=(const Fix16& rhs) { value = fix16_sub(value, rhs.value); return *this; }
	Fix16& operator-=(const fix16_t rhs) { value = fix16_sub(value, rhs); return *this; }

	Fix16& operator*=(const Fix16& rhs) { value = fix16_mul(value, rhs.value); return *this; }
	Fix16& operator*=(const fix16_t rhs) { value = fix16_mul(value, rhs); return *this; }

	Fix16& operator/=(const Fix16& rhs) { value = fix16_div(value, rhs.value); return *this; }
	Fix16& operator/=(const fix16_t rhs) { value = fix16_div(value, rhs); return *this; }

	const Fix16 operator+(const Fix16& other) const { Fix16 ret = *this; ret += other; return ret; }
	const Fix16 operator+(const fix16_t other) const { Fix16 ret = *this; ret += other; return ret; }

	const Fix16 sadd(const Fix16& other)  const { Fix16 ret = fix16_add(value, other.value);             return ret; }
	const Fix16 sadd(const fix16_t other) const { Fix16 ret = fix16_add(value, other);                   return ret; }

	const Fix16 operator-(const Fix16& other) const { Fix16 ret = *this; ret -= other; return ret; }
	const Fix16 operator-(const fix16_t other) const { Fix16 ret = *this; ret -= other; return ret; }
	const Fix16 operator-() const { return -value; }

	const Fix16 ssub(const Fix16& other)  const { Fix16 ret = fix16_add(value, -other.value);             return ret; }
	const Fix16 ssub(const fix16_t other) const { Fix16 ret = fix16_add(value, -other);                   return ret; }

	const Fix16 operator*(const Fix16& other) const { Fix16 ret = *this; ret *= other; return ret; }
	const Fix16 operator*(const fix16_t other) const { Fix16 ret = *this; ret *= other; return ret; }

	const Fix16 smul(const Fix16& other)  const { Fix16 ret = fix16_mul(value, other.value);             return ret; }
	const Fix16 smul(const fix16_t other) const { Fix16 ret = fix16_mul(value, other);                   return ret; }

	const Fix16 operator/(const Fix16& other) const { Fix16 ret = *this; ret /= other; return ret; }
	const Fix16 operator/(const fix16_t other) const { Fix16 ret = *this; ret /= other; return ret; }

	const Fix16 sdiv(const Fix16& other)  const { Fix16 ret = fix16_div(value, other.value);             return ret; }
	const Fix16 sdiv(const fix16_t other) const { Fix16 ret = fix16_div(value, other);                   return ret; }

	int operator==(const Fix16& other)  const { return (value == other.value); }
	int operator==(const fix16_t other) const { return (value == other); }

	int operator!=(const Fix16& other)  const { return (value != other.value); }
	int operator!=(const fix16_t other) const { return (value != other); }

	int operator<=(const Fix16& other)  const { return (value <= other.value); }
	int operator<=(const fix16_t other) const { return (value <= other); }

	int operator>=(const Fix16& other)  const { return (value >= other.value); }
	int operator>=(const fix16_t other) const { return (value >= other); }

	int operator< (const Fix16& other)  const { return (value < other.value); }
	int operator< (const fix16_t other) const { return (value < other); }

	int operator> (const Fix16& other)  const { return (value > other.value); }
	int operator> (const fix16_t other) const { return (value > other); }

	Fix16 sqrt() const { return Fix16(fix16_sqrt_fromLut(value)); }
	static Fix16 Abs(Fix16 v) { return Fix16(fix_abs(v.value)); }
	static Fix16 FromRaw(fix16_t t) { return Fix16(t); }

	static Fix16 Min(Fix16 a, Fix16 b) { return a.value > b.value ? b : a; }
	static Fix16 Max(Fix16 a, Fix16 b) { return a.value < b.value ? b : a; }

	static Fix16 one;
	static Fix16 zero;
	static Fix16 half;
	static Fix16 infinity;
	static Fix16 two;
	static Fix16 _100;
	static Fix16 epsilon;
};

#endif
