using System.Collections.Generic;
using Unity.Entities;

public class CustomSystems3 : ComponentSystemGroup
{
    public override void SortSystemUpdateList( )
    {
        var backup = new List<ComponentSystemBase>();
        backup.AddRange( m_systemsToUpdate );
        base.SortSystemUpdateList(  );
        m_systemsToUpdate.Clear(  );
        m_systemsToUpdate.AddRange( backup );
    }
}