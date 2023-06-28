using System.Runtime.InteropServices;
using Unity.Jobs;

namespace Game.Battle.CommonLib
{
    [StructLayout(LayoutKind.Sequential)]
    public struct AgentVector2
    {
        public int x;
        public int y;
    }

    public class MSPathSystem
    {
#if UNITY_EDITOR || UNITY_STANDLONE_WIN || UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX
        const string kFindPathDllName = "RVO_PathFind";
#elif (UNITY_IOS || UNITY_IPHONE)
        const string kFindPathDllName = "__Internal";
#else
        const string kFindPathDllName = "libProject";
#endif

        // [DllImport(kFindPathDllName)]
        // public static extern void InitSystem(float timeStep, float neighborDist, int maxNeighbors, float timeHorizon, 
        //     float timeHorizonObst, float radius, float maxSpeed);

        /// <summary>
        ///  初始化
        /// </summary>
        [DllImport(kFindPathDllName)]
        public static extern void InitSystem(int id);

        /// <summary>
        /// 关闭系统,每个关卡战斗结束后进行调用
        /// </summary>
        [DllImport(kFindPathDllName)]
        public static extern void Shutdown(int id);

        /// <summary>
        /// 添加Agent
        /// </summary>
        /// <param name="type"> 0: 地面   1: 飞行 </param>
        /// <param name="posX"></param>
        /// <param name="posY"></param>
        /// <param name="neighborDist"></param>
        /// <param name="maxNeighbors"></param>
        /// <param name="timeHorizon"></param>
        /// <param name="timeHorizonObst"></param>
        /// <param name="radius"></param>
        /// <param name="maxSpeed"> </param>
        /// <param name="velocityX"> 初始x加速度, 默认都为0. </param>
        /// <param name="velocityY"> 初始z加速度, 默认都为0. </param>
        /// <returns></returns>
        [DllImport(kFindPathDllName)]
        public static extern int AddAgent(int id, int posX, int posY, int neighborDist, int maxNeighbors, int timeHorizon, 
            int timeHorizonObst, int radius, int maxSpeed, int mass, int velocityX, int velocityY, int entityId);

        /// <summary>
        /// 移除Agent
        /// </summary>
        /// <param name="type"></param>
        /// <param name="agentIndex"></param>
        [DllImport(kFindPathDllName)]
        public static extern void RemoveAgent(int id, int agentIndex);

        /// <summary>
        /// 获取Agent位置
        /// </summary>
        /// <param name="type"></param>
        /// <param name="agentIndex"></param>
        /// <param name="position"></param>
        [DllImport(kFindPathDllName)]
        public static extern void GetAgentPosition(int id, int agentIndex, ref AgentVector2 position);

                /// 获取Agent位置
        /// </summary>
        /// <param name="type"></param>
        /// <param name="agentIndex"></param>
        /// <param name="position"></param>
        [DllImport(kFindPathDllName)]
        public static extern void GetAgentDir(int id, int agentIndex, ref AgentVector2 position);

        /// <summary>
        /// 设置单帧流逝时间. Time.deltaTime
        /// </summary>
        /// <param name="timeStep"></param>
        [DllImport(kFindPathDllName)]
        public static extern void SetTimeStep(int id, int timeStep);

        /// <summary>
        /// 设置Agent加速度
        /// </summary>
        /// <param name="type"></param>
        /// <param name="agentIndex"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        [DllImport(kFindPathDllName)]
        public static extern void SetAgentVelocityPref(int id, int agentIndex, int x, int y);

#if false
        /// <summary>
        /// 主循环Update中进行调用
        /// </summary>
        [DllImport(kFindPathDllName)]
        public static extern void DoStep(int id);
#else
        struct JobStep : IJobParallelFor
        {
            public int max;
            internal int id;

            public void Execute(int index)
            {
                doStepNeighborAndVelocity(id, index, max);
            }
        }

        /// <summary>
        /// 主循环Update中进行调用
        /// </summary>
        public static void DoStep(int id)
        {
            UnityEngine.Profiling.Profiler.BeginSample("DoStepBuildTree");
            DoStepBuildTree(id);
            UnityEngine.Profiling.Profiler.EndSample();

            // 分两个线程
            UnityEngine.Profiling.Profiler.BeginSample("DoStepNeighbor");
            var job = new JobStep()
            {
                id = id,
                max = 2,
            };
            job.Schedule(2, 1).Complete();
            UnityEngine.Profiling.Profiler.EndSample();

            UnityEngine.Profiling.Profiler.BeginSample("DoStepUpdate");
            DoStepUpdate(id);
            UnityEngine.Profiling.Profiler.EndSample();
        }
        /// <summary>
        /// 主循环Update中进行调用
        /// </summary>
        [DllImport(kFindPathDllName)]
        static extern void DoStepBuildTree(int id);
        /// <summary>
        /// 主循环Update中进行调用
        /// </summary>
        [DllImport(kFindPathDllName)]
        static extern void doStepNeighborAndVelocity(int id, int index, int max);
        /// <summary>
        /// 主循环Update中进行调用
        /// </summary>
        [DllImport(kFindPathDllName)]
        static extern void DoStepUpdate(int id);
#endif

        /// <summary>
        /// 用来查询当前位置的单位数量
        /// </summary>
        [DllImport(kFindPathDllName)]
        unsafe private static extern int GetNearByAgents(int id, int x, int y, void* ptr, int size, int range);

        public unsafe static int GetNearByAgents(int id, int x, int y, Unity.Collections.NativeArray<int> nativeByteArray, int searchRange)
        {
            void* ptr = Unity.Collections.LowLevel.Unsafe.NativeArrayUnsafeUtility.GetUnsafePtr(nativeByteArray);
            return GetNearByAgents(id, x, y, ptr, nativeByteArray.Length, searchRange);
        }

        
        /// </summary>
        /// <param name="type"></param>
        /// <param name="agentIndex"></param>
        /// <param name="position"></param>
        [DllImport(kFindPathDllName)]
        unsafe public static extern int GetNeighbour(int id, int agentIndex, void* ptr);
        public unsafe static int GetAgentNeighbor(int id, int index, Unity.Collections.NativeArray<int> nativeByteArray)
        {
            void* ptr = Unity.Collections.LowLevel.Unsafe.NativeArrayUnsafeUtility.GetUnsafePtr(nativeByteArray);
            return GetNeighbour(id, index, ptr);
        }

    }
}
