APP_SHORT_COMMANDS := true
APP_STL := gnustl_static

APP_CPPFLAGS += -std=c++11
APP_CPPFLAGS += -Wno-error=format-security
APP_CPPFLAGS += -Wno-literal-suffix


APP_BUILD_SCRIPT := Android.mk
APP_PLATFORM := android-19
APP_ABI := armeabi-v7a arm64-v8a

APP_OPTIM := release
#APP_CFLAGS += -O3
