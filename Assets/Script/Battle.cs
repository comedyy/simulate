using System;

public class Battle
{
    BattleWorld _world;
    CheckSumMgr _checkSumMgr;
    public CheckSumMgr CheckSumMgr => _checkSumMgr;

    LocalFrame _localFrame;

    public Battle(float tick, bool randomFixedCount, bool usePlaybackInput, int i, IGameSocket socket, int userCount)
    {
        _localFrame = new LocalFrame();
        _checkSumMgr = new CheckSumMgr();

        if(usePlaybackInput)
        {
            _localFrame.LoadPlayBackInfo();
        }
        else if(socket != null)
        {
            _localFrame.Init(tick, socket, _checkSumMgr, i);
        }

        _world = new BattleWorld("new " + i, _checkSumMgr, randomFixedCount, tick, _localFrame, i, userCount);
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