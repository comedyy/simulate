using System;

public class Battle
{
    BattleWorld _world;
    CheckSumMgr _checkSumMgr;
    public CheckSumMgr CheckSumMgr => _checkSumMgr;

    LocalFrame _localFrame;

    public Battle(fp tick, bool randomFixedCount, bool usePlaybackInput, int worldIndex, IGameSocket socket, int userCount, bool userAutoMove)
    {
        _localFrame = new LocalFrame();
        _checkSumMgr = new CheckSumMgr();

        if(usePlaybackInput)
        {
            _localFrame.LoadPlayBackInfo(true);
        }
        else if(socket != null)
        {
            _localFrame.Init(tick, socket, _checkSumMgr, worldIndex);
        }
        else
        {
            _localFrame.LoadPlayBackInfo(false);
        }

        _world = new BattleWorld("new " + worldIndex, _checkSumMgr, randomFixedCount, tick, _localFrame, worldIndex, userCount, userAutoMove);
    }

    public bool IsEnd => _world.IsEnd;

    public void Update()
    {
        if(!_world.IsEnd)
        {
            _world.Update();
        }

        _localFrame.Update();
    }

    internal void SavePlayback()
    {
        _localFrame.SavePlayback();
    }

    internal void Dispose()
    {
        _world.Dispose();
    }
}