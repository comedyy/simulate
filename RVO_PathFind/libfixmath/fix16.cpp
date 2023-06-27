#include "fix16.hpp"

Fix16 Fix16::one = Fix16(fix16_one);
Fix16 Fix16::zero = Fix16(0);
Fix16 Fix16::half = Fix16(fix16_half);
Fix16 Fix16::infinity = Fix16(fix16_maximum);
Fix16 Fix16::two = Fix16(fix16_one * 2);
Fix16 Fix16::_100 = Fix16(fix16_one * 100);
Fix16 Fix16::epsilon = Fix16(fix16_eps);

