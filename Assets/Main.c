#include <stdio.h>
#include <stdint.h>
#include <stdbool.h>
#include <string.h>

// --- Sage Type Definitions ---
typedef uint8_t   u8;
typedef uint16_t  u16;
typedef uint32_t  u32;
typedef uint64_t  u64;
typedef int8_t    i8;
typedef int16_t   i16;
typedef int32_t   i32;
typedef int64_t   i64;
typedef float     f32;
typedef double    f64;
typedef bool      b8;
typedef char      c8;
typedef const char* str;
typedef void      none;
// -----------------------------

// --- Console Module (C Emulation) ---
void _Console_PrintLine_Str(str text) { printf("%s\n", text); }
void _Console_PrintLine_I32(i32 val)  { printf("%d\n", val); }
void _Console_PrintLine_F32(f32 val)  { printf("%f\n", val); }

#define Console_PrintLine(x) _Generic((x), \
    char*: _Console_PrintLine_Str, \
    const char*: _Console_PrintLine_Str, \
    i32: _Console_PrintLine_I32, \
    f32: _Console_PrintLine_F32 \
)(x)

// Used module: Console
none main()
{
	Console_PrintLine("Hello World!");
}
