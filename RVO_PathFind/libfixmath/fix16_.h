#ifndef __libfixmath_fix16_h__
#define __libfixmath_fix16_h__

#ifdef __cplusplus
extern "C"
{
#endif

/* These options may let the optimizer to remove some calls to the functions.
 * Refer to http://gcc.gnu.org/onlinedocs/gcc/Function-Attributes.html
 */
#ifndef FIXMATH_FUNC_ATTRS
# ifdef __GNUC__
#   if __GNUC__ > 4 || (__GNUC__ == 4 && __GNUC_MINOR__ > 6)
#     define FIXMATH_FUNC_ATTRS __attribute__((leaf, nothrow, const))
#   else
#     define FIXMATH_FUNC_ATTRS __attribute__((nothrow, const))
#   endif
# else
#   define FIXMATH_FUNC_ATTRS
# endif
#endif

#ifdef __KERNEL__
#include <linux/types.h>
#else
#include <stdint.h>
#endif

typedef int64_t fix16_t;

static const fix16_t FOUR_DIV_PI  = 0x145F3;            /*!< Fix16 value of 4/PI */
static const fix16_t _FOUR_DIV_PI2 = 0xFFFF9840;        /*!< Fix16 value of -4/PI² */
static const fix16_t X4_CORRECTION_COMPONENT = 0x399A; 	/*!< Fix16 value of 0.225 */
static const fix16_t PI_DIV_4 = 0x0000C90F;             /*!< Fix16 value of PI/4 */
static const fix16_t THREE_PI_DIV_4 = 0x00025B2F;       /*!< Fix16 value of 3PI/4 */

static const fix16_t fix16_maximum  = 0x7FFFFFFF; /*!< the maximum value of fix16_t */
static const fix16_t fix16_minimum  = 0x80000000; /*!< the minimum value of fix16_t */
static const fix16_t fix16_overflow = 0x80000000; /*!< the value used to indicate overflows when FIXMATH_NO_OVERFLOW is not specified */

static const fix16_t fix16_pi  = 205887;     /*!< fix16_t value of pi */
static const fix16_t fix16_e   = 178145;     /*!< fix16_t value of e */
static const fix16_t fix16_one = 0x00010000; /*!< fix16_t value of 1 */
static const fix16_t fix16_half = 0x00008000; /*!< fix16_t value of 1 */
static const fix16_t fix16_eps = 1;          /*!< fix16_t epsilon */

/* Conversion functions between fix16_t and float/integer.
 * These are inlined to allow compiler to optimize away constant numbers
 */
static inline fix16_t fix16_from_int(int a)     { return a * fix16_one; }

/* Macro for defining fix16_t constant values.
   The functions above can't be used from e.g. global variable initializers,
   and their names are quite long also. This macro is useful for constants
   springled alongside code, e.g. F16(1.234).

   Note that the argument is evaluated multiple times, and also otherwise
   you should only use this for constant values. For runtime-conversions,
   use the functions above.
*/
#define F16(x) ((fix16_t)(((x) >= 0) ? ((x) * 65536.0 + 0.5) : ((x) * 65536.0 - 0.5)))

static inline fix16_t fix16_abs(fix16_t x)
    { return x < 0 ? -x : x; }
static inline fix16_t fix16_floor(fix16_t x)
	{ return (x & 0xFFFF0000UL); }
static inline fix16_t fix16_ceil(fix16_t x)
	{ return (x & 0xFFFF0000UL) + (x & 0x0000FFFFUL ? fix16_one : 0); }
static inline fix16_t fix16_min(fix16_t x, fix16_t y)
	{ return (x < y ? x : y); }
static inline fix16_t fix16_max(fix16_t x, fix16_t y)
	{ return (x > y ? x : y); }
static inline fix16_t fix16_clamp(fix16_t x, fix16_t lo, fix16_t hi)
	{ return fix16_min(fix16_max(x, lo), hi); }

/* Subtraction and addition with (optional) overflow detection. */
#ifdef FIXMATH_NO_OVERFLOW

static inline fix16_t fix16_add(fix16_t inArg0, fix16_t inArg1) { return (inArg0 + inArg1); }
static inline fix16_t fix16_sub(fix16_t inArg0, fix16_t inArg1) { return (inArg0 - inArg1); }

#else

extern fix16_t fix16_add(fix16_t a, fix16_t b) FIXMATH_FUNC_ATTRS;
extern fix16_t fix16_sub(fix16_t a, fix16_t b) FIXMATH_FUNC_ATTRS;

/* Saturating arithmetic */
extern fix16_t fix16_sadd(fix16_t a, fix16_t b) FIXMATH_FUNC_ATTRS;
extern fix16_t fix16_ssub(fix16_t a, fix16_t b) FIXMATH_FUNC_ATTRS;

#endif

/*! Multiplies the two given fix16_t's and returns the result.
*/
extern fix16_t fix16_mul(fix16_t inArg0, fix16_t inArg1) FIXMATH_FUNC_ATTRS;

/*! Divides the first given fix16_t by the second and returns the result.
*/
extern fix16_t fix16_div(fix16_t inArg0, fix16_t inArg1) FIXMATH_FUNC_ATTRS;

#ifndef FIXMATH_NO_OVERFLOW
/*! Performs a saturated multiplication (overflow-protected) of the two given fix16_t's and returns the result.
*/
extern fix16_t fix16_smul(fix16_t inArg0, fix16_t inArg1) FIXMATH_FUNC_ATTRS;

/*! Performs a saturated division (overflow-protected) of the first fix16_t by the second and returns the result.
*/
extern fix16_t fix16_sdiv(fix16_t inArg0, fix16_t inArg1) FIXMATH_FUNC_ATTRS;
#endif

/*! Returns the square root of the given fix16_t.
*/
extern fix16_t fix16_sqrt(fix16_t inValue) FIXMATH_FUNC_ATTRS;
extern fix16_t fix16_sqrt_fromLut(fix16_t inValue) FIXMATH_FUNC_ATTRS;

/*! Returns the square of the given fix16_t.
*/
static inline fix16_t fix16_sq(fix16_t x)
	{ return fix16_mul(x, x); }

/*! Returns the exponent (e^) of the given fix16_t.
*/
extern fix16_t fix16_exp(fix16_t inValue) FIXMATH_FUNC_ATTRS;

/*! Returns the natural logarithm of the given fix16_t.
 */
extern fix16_t fix16_log(fix16_t inValue) FIXMATH_FUNC_ATTRS;

/*! Returns the base 2 logarithm of the given fix16_t.
 */
extern fix16_t fix16_log2(fix16_t x) FIXMATH_FUNC_ATTRS;

/*! Returns the saturated base 2 logarithm of the given fix16_t.
 */
extern fix16_t fix16_slog2(fix16_t x) FIXMATH_FUNC_ATTRS;

static inline uint32_t fix_abs(fix16_t in)
{
    if(in == fix16_minimum)
    {
        // minimum negative number has same representation as
        // its absolute value in unsigned
        return 0x80000000;
    }
    else
    {
        return ((in >= 0)?(in):(-in));
    }
}


/** Helper macro for F16C. Replace token with its number of characters/digits. */
#define FIXMATH_TOKLEN(token) ( sizeof( #token ) - 1 )

/** Helper macro for F16C. Handles pow(10, n) for n from 0 to 8. */
#define FIXMATH_CONSTANT_POW10(times) ( \
  (times == 0) ? 1ULL \
        : (times == 1) ? 10ULL \
            : (times == 2) ? 100ULL \
                : (times == 3) ? 1000ULL \
                    : (times == 4) ? 10000ULL \
                        : (times == 5) ? 100000ULL \
                            : (times == 6) ? 1000000ULL \
                                : (times == 7) ? 10000000ULL \
                                    : 100000000ULL \
)


/** Helper macro for F16C, the type uint64_t is only used at compile time and
 *  shouldn't be visible in the generated code.
 *
 * @note We do not use fix16_one instead of 65536ULL, because the
 *       "use of a const variable in a constant expression is nonstandard in C".
 */
#define FIXMATH_CONVERT_MANTISSA(m) \
( (unsigned) \
    ( \
        ( \
            ( \
                (uint64_t)( ( ( 1 ## m ## ULL ) - FIXMATH_CONSTANT_POW10(FIXMATH_TOKLEN(m)) ) * FIXMATH_CONSTANT_POW10(5 - FIXMATH_TOKLEN(m)) ) \
                * 100000ULL * 65536ULL \
            ) \
            + 5000000000ULL /* rounding: + 0.5 */ \
        ) \
        / \
        10000000000LL \
    ) \
)


#define FIXMATH_COMBINE_I_M(i, m) \
( \
    ( \
        (    i ) \
        << 16 \
    ) \
    | \
    ( \
        FIXMATH_CONVERT_MANTISSA(m) \
        & 0xFFFF \
    ) \
)


/** Create int16_t (Q16.16) constant from separate integer and mantissa part.
 *
 * Only tested on 32-bit ARM Cortex-M0 / x86 Intel.
 *
 * This macro is needed when compiling with options like "--fpu=none",
 * which forbid all and every use of Fix16 and related types and
 * would thus make it impossible to have fix16_t constants.
 *
 * Just replace uses of F16() with F16C() like this:
 *   F16(123.1234) becomes F16C(123,1234)
 *
 * @warning Specification of any value outside the mentioned intervals
 *          WILL result in undefined behavior!
 *
 * @note Regardless of the specified minimum and maximum values for i and m below,
 *       the total value of the number represented by i and m MUST be in the interval
 *       ]-32768.00000:32767.99999[ else usage with this macro will yield undefined behavior.
 *
 * @param i Signed integer constant with a value in the interval ]-32768:32767[.
 * @param m Positive integer constant in the interval ]0:99999[ (fractional part/mantissa).
 */
#define F16C(i, m) \
( (fix16_t) \
    ( \
      (( #i[0] ) == '-') \
        ? -FIXMATH_COMBINE_I_M((unsigned)( ( (i) * -1) ), m) \
        : FIXMATH_COMBINE_I_M((unsigned)i, m) \
    ) \
)

#ifdef __cplusplus
}
#include "fix16.hpp"
#endif

#endif
