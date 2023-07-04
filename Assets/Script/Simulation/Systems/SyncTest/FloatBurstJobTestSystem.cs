using Unity.Burst;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;

public class FloatBurstJobTestSystem : JobComponentSystem
{
    EntityQuery _query;

    protected override void OnCreate()
    {
        base.OnCreate();
        _query = GetEntityQuery(typeof(TestFloatSync));
    }

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        var job = new CalFloatJob()
        {
            floatType = GetArchetypeChunkComponentType<TestFloatSync>(),
            randomType = GetArchetypeChunkComponentType<EntityRandom>(),
        };
        
        job.ScheduleParallel(_query, inputDeps).Complete();
        return default;
    }

    [BurstCompile]
    struct CalFloatJob : IJobChunk
    {
        internal ArchetypeChunkComponentType<TestFloatSync> floatType;
        internal ArchetypeChunkComponentType<EntityRandom> randomType;

        public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
        {
            var floats = chunk.GetNativeArray(floatType);
            var randoms = chunk.GetNativeArray(randomType);
            for(int i = 0; i < chunk.Count; i++)
            {
                var randomComp = randoms[i];
                ref var random = ref randomComp.random;

                var param1 = random.NextFp(-100000, 1000000);
                var param2 = random.NextFp(-100000, 1000000);
                var param3 = random.NextFp(-100000, 1000000);
                var param4 = random.NextFp(-100000, 1000000);
                var param5 = random.NextFp(-100000, 1000000);


                var div = (param1 + param2);

                var result = ((param3 - param4) * param5 / div);

                randoms[i] = randomComp;
                floats[i] = new TestFloatSync(){result = result};
            }
        }
    }
}