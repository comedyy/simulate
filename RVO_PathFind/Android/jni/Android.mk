LOCAL_PATH := $(call my-dir)
include $(CLEAR_VARS)

LOCAL_MODULE := libProject
LOCAL_LDLIBS     := -llog -landroid
LOCAL_CFLAGS    := -DANDROID_NDK

FILE_LIST1 := $(wildcard $(LOCAL_PATH)/../../libfixmath/*.cpp)
FILE_LIST := $(wildcard $(LOCAL_PATH)/../../RVO_PathFind/*.cpp)

$(warning $(FILE_LIST1))
$(warning $(FILE_LIST))

LOCAL_SRC_FILES := $(FILE_LIST:$(LOCAL_PATH)/%=%) $(FILE_LIST1:$(LOCAL_PATH)/%=%)
$(warning $(LOCAL_SRC_FILES))

LOCAL_C_INCLUDES := $(LOCAL_PATH)/../../RVO_PathFind/*.h,$(LOCAL_PATH)/../../libfixmath/*.h

include $(BUILD_SHARED_LIBRARY)
