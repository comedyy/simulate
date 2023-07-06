export NDK_ROOT_DIR=C:/Progra~1/Unity/Hub/Editor/2019.4.32f1/Editor/Data/PlaybackEngines/AndroidPlayer/NDK

cd jni
cmd /c $NDK_ROOT_DIR/ndk-build.cmd
cd ..