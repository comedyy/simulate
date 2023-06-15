

using Unity.Entities;

public static class SystemExternsion
{
    public static T GetSingletonObject<T>(this ComponentSystemBase system)
    {
        return system.EntityManager.GetComponentObject<T>(system.GetSingletonEntity<T>());
    }
}