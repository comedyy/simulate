#ifndef __libfixmath_fix16_hpp__
#define __libfixmath_fix16_hpp__

#include "fix16.h"

class Fix16 {
public:
	fix16_t value;

	Fix16() { value = 0; }
	Fix16(const Fix16& inValue) { value = inValue.value; }
	Fix16(const fix16_t inValue) { value = inValue; }

	Fix16& operator=(const Fix16& rhs) { value = rhs.value;             return *this; }
	Fix16& operator=(const fix16_t rhs) { value = rhs;                   return *this; }

	Fix16& operator+=(const Fix16& rhs) { value = fix16_sadd(value, rhs.value); return *this; }
	Fix16& operator+=(const fix16_t rhs) { value = fix16_sadd(value, rhs); return *this; }

	Fix16& operator-=(const Fix16& rhs) { value = fix16_ssub(value, rhs.value); return *this; }
	Fix16& operator-=(const fix16_t rhs) { value = fix16_ssub(value, rhs); return *this; }

	Fix16& operator*=(const Fix16& rhs) { value = fix16_smul(value, rhs.value); return *this; }
	Fix16& operator*=(const fix16_t rhs) { value = fix16_smul(value, rhs); return *this; }

	Fix16& operator/=(const Fix16& rhs) { value = fix16_sdiv(value, rhs.value); return *this; }
	Fix16& operator/=(const fix16_t rhs) { value = fix16_sdiv(value, rhs); return *this; }

	const Fix16 operator+(const Fix16& other) const { Fix16 ret = *this; ret += other; return ret; }
	const Fix16 operator+(const fix16_t other) const { Fix16 ret = *this; ret += other; return ret; }

	const Fix16 sadd(const Fix16& other)  const { Fix16 ret = fix16_sadd(value, other.value);             return ret; }
	const Fix16 sadd(const fix16_t other) const { Fix16 ret = fix16_sadd(value, other);                   return ret; }

	const Fix16 operator-(const Fix16& other) const { Fix16 ret = *this; ret -= other; return ret; }
	const Fix16 operator-(const fix16_t other) const { Fix16 ret = *this; ret -= other; return ret; }
	const Fix16 operator-() const { return -value; }

	const Fix16 ssub(const Fix16& other)  const { Fix16 ret = fix16_sadd(value, -other.value);             return ret; }
	const Fix16 ssub(const fix16_t other) const { Fix16 ret = fix16_sadd(value, -other);                   return ret; }

	const Fix16 operator*(const Fix16& other) const { Fix16 ret = *this; ret *= other; return ret; }
	const Fix16 operator*(const fix16_t other) const { Fix16 ret = *this; ret *= other; return ret; }

	const Fix16 smul(const Fix16& other)  const { Fix16 ret = fix16_smul(value, other.value);             return ret; }
	const Fix16 smul(const fix16_t other) const { Fix16 ret = fix16_smul(value, other);                   return ret; }

	const Fix16 operator/(const Fix16& other) const { Fix16 ret = *this; ret /= other; return ret; }
	const Fix16 operator/(const fix16_t other) const { Fix16 ret = *this; ret /= other; return ret; }

	const Fix16 sdiv(const Fix16& other)  const { Fix16 ret = fix16_sdiv(value, other.value);             return ret; }
	const Fix16 sdiv(const fix16_t other) const { Fix16 ret = fix16_sdiv(value, other);                   return ret; }

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

	Fix16 sqrt() const { return Fix16(fix16_sqrt(value)); }
	Fix16 Abs() { return Fix16(fix_abs(value)); }

	static Fix16 Min(Fix16 a, Fix16 b) { return a.value > b.value ? b : a; }
	static Fix16 Max(Fix16 a, Fix16 b) { return a.value < b.value ? b : a; }

	static Fix16 one;
	static Fix16 zero;
	static Fix16 half;
};

Fix16 Fix16::one = Fix16(fix16_one);
Fix16 Fix16::zero = Fix16(0);
Fix16 Fix16::half = Fix16(fix16_half);

#endif
