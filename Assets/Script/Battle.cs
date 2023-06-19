using System;

public class Battle
{
    BattleWorld _world;
    CheckSumMgr _checkSumMgr;
    public CheckSumMgr CheckSumMgr => _checkSumMgr;

    LocalFrame _localFrame;

    public Battle(float tick, float pingSec, bool randomFixedCount, bool usePlaybackInput, int i,  DumpServer _dumpServer, int userCount)
    {
        _localFrame = new LocalFrame();

        if(usePlaybackInput)
        {
            _localFrame.LoadPlayBackInfo();
        }
        else if(_dumpServer != null)
        {
            _localFrame.Init(tick, _dumpServer._transLayer.Send);
            _dumpServer._transLayer.Init(_dumpServer.AddMessage, _localFrame.OnReceive);
        }

        _checkSumMgr = new CheckSumMgr();
        _world = new BattleWorld("new " + i, _checkSumMgr, randomFixedCount, tick, _localFrame, i, userCount);
    }

    public bool IsEnd => _world.IsEnd;

    public void Update(float pingSec)
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